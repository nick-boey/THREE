using System.Diagnostics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace THREE;

[Serializable]
public class ArcballControls : IDisposable
{
    private bool controlKey;
    private bool shiftKey;

    public ArcballControls(IControlsContainer glControl, Camera camera, Scene scene = null)
    {
        this.glControl = glControl;
        this.scene = scene;

        _center.x = 0;
        _center.y = 0;

        SetCamera(camera);

        if (this.scene != null) this.scene.Add(_gizmos);
        InitializeMouseActions();

        glControl.MouseWheel += OnWheel;
        glControl.MouseDown += OnPointerDown;
        glControl.MouseUp += OnPointerCancel;
        glControl.SizeChanged += Control_SizeChanged;

        glControl.KeyDown += OnKeyDown;
        glControl.KeyUp += OnKeyUp;
        stopWatch.Start();
    }

    /**
     * Set default mouse actions
     */
    private void InitializeMouseActions()
    {
        SetMouseAction("PAN", "LEFT", "CTRL");
        SetMouseAction("PAN", "RIGHT");

        SetMouseAction("ROTATE", "LEFT");

        SetMouseAction("ZOOM", "WHEEL");
        SetMouseAction("ZOOM", "MIDDLE");

        SetMouseAction("FOV", "WHEEL", "SHIFT");
        SetMouseAction("FOV", "MIDDLE", "SHIFT");
    }


    /**
     * Set a new mouse action by specifying the operation to be performed and a mouse/key combination. In case of conflict, replaces the existing one
     * @param {String} operation The operation to be performed ('PAN', 'ROTATE', 'ZOOM', 'FOV)
     * @param {*} mouse A mouse button (0, 1, 2) or 'WHEEL' for wheel notches
     * @param {*} key The keyboard modifier ('CTRL', 'SHIFT') or null if key is not needed
     * @returns {Boolean} True if the mouse action has been successfully added, false otherwise
     */
    private bool SetMouseAction(string operation, string mouse, string key = null)
    {
        var state = STATE.NONE;

        if (!OperationInput.Contains(operation) || !MouseInput.Contains(mouse) || !KeyInput.Contains(key))
            //invalid parameters
            return false;

        if (mouse == "WHEEL")
            if (operation != "ZOOM" && operation != "FOV")
                //cannot associate 2D operation to 1D input
                return false;

        switch (operation)
        {
            case "PAN":

                state = STATE.PAN;
                break;

            case "ROTATE":

                state = STATE.ROTATE;
                break;

            case "ZOOM":

                state = STATE.SCALE;
                break;

            case "FOV":

                state = STATE.FOV;
                break;
        }

        var action = new MouseAction
        {
            operation = operation,
            mouse = mouse,
            key = key,
            state = state
        };

        for (var i = 0; i < mouseActions.Count; i++)
            if (mouseActions[i].mouse == action.mouse && mouseActions[i].key == action.key)
            {
                mouseActions.Insert(i, action);
                return true;
            }

        mouseActions.Add(action);
        return true;
    }

    private void OnKeyUp(object sender, KeyboardKeyEventArgs e)
    {
        if (e.Key == Keys.LeftControl || e.Key == Keys.RightControl)
            controlKey = false;
        if (e.Key == Keys.LeftShift || e.Key == Keys.RightShift)
            shiftKey = true;
    }

    private void OnKeyDown(object sender, KeyboardKeyEventArgs e)
    {
        if (e.Key == Keys.LeftControl || e.Key == Keys.RightControl)
            controlKey = false;
        if (e.Key == Keys.LeftShift || e.Key == Keys.RightShift)
            shiftKey = true;
    }


    private void SetCenter(float x, float y)
    {
        _center.x = x;
        _center.y = y;
    }

    private void OnPointerCancel(object sender, MouseEventArgs e)
    {
        _touchStart.Clear();
        _touchCurrent.Clear();
        _input = INPUT.NONE;
    }


    private void DrawGrid()
    {
        if (scene != null)
        {
            var color = Color.Hex(0x888888);
            var multiplier = 3;
            float size, divisions, maxLength, tick;

            if (camera is OrthographicCamera)
            {
                var width = camera.CameraRight - camera.Left;
                var height = camera.Bottom - camera.Top;

                maxLength = Math.Max(width, height);
                tick = maxLength / 20;

                size = maxLength / camera.Zoom * multiplier;
                divisions = size / tick * camera.Zoom;
            }
            else //if (this.camera is PerspectiveCamera)
            {
                var distance = camera.Position.DistanceTo(_gizmos.Position);
                var halfFovV = MathUtils.DEG2RAD * camera.Fov * 0.5f;
                var halfFovH = (float)Math.Atan(camera.Aspect * Math.Tan(halfFovV));

                maxLength = (float)Math.Tan(Math.Max(halfFovV, halfFovH)) * distance * 2;
                tick = maxLength / 20;

                size = maxLength * multiplier;
                divisions = size / tick;
            }

            if (_grid == null)
            {
                _grid = new GridHelper((int)size, (int)divisions, 0x888888);
                _grid.Position.Copy(_gizmos.Position);
                _gridPosition.Copy(_grid.Position);
                _grid.Quaternion.Copy(camera.Quaternion);
                _grid.RotateX((float)Math.PI * 0.5f);

                scene.Add(_grid);
            }
        }
    }

    /**
     * Update a PointerEvent inside current pointerevents array
     * @param {PointerEvent} event
     */
    private void UpdateTouchEvent(MouseEventArgs e)
    {
        for (var i = 0; i < _touchCurrent.Count; i++)
            if (_touchCurrent[i].Equals(e))
            {
                _touchCurrent.Insert(i, e);
                break;
            }
    }

    private void DisposeGrid()
    {
        if (_grid != null && scene != null)
        {
            scene.Remove(_grid);
            _grid = null;
        }
    }

    public void Control_SizeChanged(object sender, ResizeEventArgs e)
    {
        var scale = (_gizmos.Scale.X + _gizmos.Scale.Y + _gizmos.Scale.Z) / 3;
        _tbRadius = CalculateTbRadius(camera);

        var newRadius = _tbRadius / scale;
        var curve = new EllipseCurve(0, 0, newRadius, newRadius);
        var points = curve.GetPoints(_curvePts);
        var curveGeometry = new BufferGeometry().SetFromPoints(points);


        foreach (var gizmo in _gizmos.Children) gizmo.Geometry = curveGeometry;
        //this._gizmos.children[gizmo].geometry = curveGeometry;
        if (ChangeEvent != null)
            ChangeEvent();

        //this.dispatchEvent(_changeEvent);
    }

    /**
     * Calculate the distance between two pointers
     * @param {PointerEvent} p0 The first pointer
     * @param {PointerEvent} p1 The second pointer
     * @returns {number} The distance between the two pointers
     */
    private float CalculatePointersDistance(MouseEventArgs p0, MouseEventArgs p1)
    {
        return (float)Math.Sqrt(Math.Pow(p1.X - p0.X, 2) + Math.Pow(p1.Y - p0.Y, 2));
    }


    /**
     * Set gizmos visibility
     * @param {Boolean} value Value of gizmos visibility
     */
    public void SetGizmosVisible(bool value)
    {
        _gizmos.Visible = value;

        if (ChangeEvent != null)
            ChangeEvent();
    }

    /**
     * Set gizmos radius factor and redraws gizmos
     * @param {Float} value Value of radius factor
     */
    public void SetTbRadius(float value)
    {
        radiusFactor = value;
        _tbRadius = CalculateTbRadius(camera);

        var curve = new EllipseCurve(0, 0, _tbRadius, _tbRadius);
        var points = curve.GetPoints(_curvePts);
        var curveGeometry = new BufferGeometry().SetFromPoints(points);


        foreach (var gizmo in _gizmos.Children) gizmo.Geometry = curveGeometry;

        if (ChangeEvent != null)
            ChangeEvent();
    }

    public void SetCamera(Camera camera)
    {
        camera.LookAt(target);
        camera.UpdateMatrix();

        //setting state
        if (camera is PerspectiveCamera)
        {
            _fov0 = camera.Fov;
            _fovState = camera.Fov;
        }

        _cameraMatrixState0.Copy(camera.Matrix);
        _cameraMatrixState.Copy(_cameraMatrixState0);
        _cameraProjectionState.Copy(camera.ProjectionMatrix);
        _zoom0 = camera.Zoom;
        _zoomState = _zoom0;

        _initialNear = camera.Near;
        _nearPos0 = camera.Position.DistanceTo(target) - camera.Near;
        _nearPos = _initialNear;

        _initialFar = camera.Far;
        _farPos0 = camera.Position.DistanceTo(target) - camera.Far;
        _farPos = _initialFar;

        _up0.Copy(camera.Up);
        _upState.Copy(camera.Up);

        this.camera = camera;
        this.camera.UpdateProjectionMatrix();

        //making gizmos
        _tbRadius = CalculateTbRadius(camera);
        MakeGizmos(target, _tbRadius);
    }

    /**
     * Creates the rotation gizmos matching trackball center and radius
     * @param {Vector3} tbCenter The trackball center
     * @param {number} tbRadius The trackball radius
     */
    private void MakeGizmos(Vector3 tbCenter, float tbRadius)
    {
        var curve = new EllipseCurve(0, 0, tbRadius, tbRadius);
        var points = curve.GetPoints(_curvePts);

        //geometry
        var curveGeometry = new BufferGeometry().SetFromPoints(points);

        //material
        var curveMaterialX = new LineBasicMaterial
            { Color = Color.Hex(0xff8080), Fog = true, Transparent = true, Opacity = 0.6f };
        var curveMaterialY = new LineBasicMaterial
            { Color = Color.Hex(0x80ff80), Fog = true, Transparent = true, Opacity = 0.6f };
        var curveMaterialZ = new LineBasicMaterial
            { Color = Color.Hex(0x8080ff), Fog = true, Transparent = true, Opacity = 0.6f };

        //line
        var gizmoX = new Line(curveGeometry, curveMaterialX);
        var gizmoY = new Line(curveGeometry, curveMaterialY);
        var gizmoZ = new Line(curveGeometry, curveMaterialZ);

        var rotation = (float)Math.PI * 0.5f;
        gizmoX.Rotation.X = rotation;
        gizmoY.Rotation.Y = rotation;


        ; //setting state
        _gizmoMatrixState0 = Matrix4.Identity().SetPosition(tbCenter);
        _gizmoMatrixState.Copy(_gizmoMatrixState0);

        if (camera.Zoom != 1)
        {
            //adapt gizmos size to camera zoom
            var size = 1 / camera.Zoom;
            _scaleMatrix.MakeScale(size, size, size);
            _translationMatrix.MakeTranslation(-tbCenter.X, -tbCenter.Y, -tbCenter.Z);

            _gizmoMatrixState.PreMultiply(_translationMatrix).PreMultiply(_scaleMatrix);
            _translationMatrix.MakeTranslation(tbCenter.X, tbCenter.Y, tbCenter.Z);
            _gizmoMatrixState.PreMultiply(_translationMatrix);
        }

        _gizmoMatrixState.Decompose(_gizmos.Position, _gizmos.Quaternion, _gizmos.Scale);

        _gizmos.Clear();

        _gizmos.Add(gizmoX);
        _gizmos.Add(gizmoY);
        _gizmos.Add(gizmoZ);
    }

    /**
     * Perform animation for focus operation
     * @param {Number} time Instant in which this function is called as performance.now()
     * @param {Vector3} point Point of interest for focus operation
     * @param {Matrix4} cameraMatrix Camera matrix
     * @param {Matrix4} gizmoMatrix Gizmos matrix
     */
    public void OnnFocusAnim(float time, Vector3 point, Matrix4 cameraMatrix, Matrix4 gizmoMatrix)
    {
        if (_timeStart == -1)
            //animation start
            _timeStart = time;

        if (_state == STATE.ANIMATION_FOCUS)
        {
            var deltaTime = time - _timeStart;
            var animTime = deltaTime / focusAnimationTime;

            _gizmoMatrixState.Copy(gizmoMatrix);

            if (animTime >= 1)
            {
                //animation end

                _gizmoMatrixState.Decompose(_gizmos.Position, _gizmos.Quaternion, _gizmos.Scale);

                Focus(point, ScaleFactor);

                _timeStart = -1;
                UpdateTbState(STATE.IDLE, false);
                ActivateGizmos(false);

                if (ChangeEvent != null)
                    ChangeEvent();
            }
            else
            {
                var amount = EaseOutCubic(animTime);
                var size = 1 - amount + ScaleFactor * amount;

                _gizmoMatrixState.Decompose(_gizmos.Position, _gizmos.Quaternion, _gizmos.Scale);
                Focus(point, size, amount);

                if (ChangeEvent != null)
                    ChangeEvent();
                //const self = this;
                //this._animationId = window.requestAnimationFrame(function(t) {

                //	self.onFocusAnim(t, point, cameraMatrix, gizmoMatrix.clone());

                //} );
            }
        }
        else
        {
            //interrupt animation

            _animationId = -1;
            _timeStart = -1;
        }
    }

    /**
     * Perform animation for rotation operation
     * @param {Number} time Instant in which this function is called as performance.now()
     * @param {Vector3} rotationAxis Rotation axis
     * @param {number} w0 Initial angular velocity
     */
    public void OnRotationAnim(float time, Vector3 rotationAxis, float w0)
    {
        if (_timeStart == -1)
        {
            //animation start
            _anglePrev = 0;
            _angleCurrent = 0;
            _timeStart = time;
        }

        if (_state == STATE.ANIMATION_ROTATE)
        {
            //w = w0 + alpha * t
            var deltaTime = (time - _timeStart) / 1000;
            var w = w0 + -dampingFactor * deltaTime;

            if (w > 0)
            {
                //tetha = 0.5 * alpha * t^2 + w0 * t + tetha0
                _angleCurrent = 0.5f * -dampingFactor * (float)Math.Pow(deltaTime, 2) + w0 * deltaTime + 0;
                ApplyTransformMatrix(Rotate(rotationAxis, _angleCurrent));

                if (ChangeEvent != null)
                    ChangeEvent();

                //const self = this;
                //this._animationId = window.requestAnimationFrame(function(t) {

                //	self.onRotationAnim(t, rotationAxis, w0);

                //} );
            }
            else
            {
                _animationId = -1;
                _timeStart = -1;

                UpdateTbState(STATE.IDLE, false);
                ActivateGizmos(false);

                if (ChangeEvent != null)
                    ChangeEvent();
            }
        }
        else
        {
            //interrupt animation

            _animationId = -1;
            _timeStart = -1;

            if (_state != STATE.ROTATE)
            {
                ActivateGizmos(false);
                if (ChangeEvent != null)
                    ChangeEvent();
            }
        }
    }

    /**
     * Rotate the camera around an axis passing by trackball's center
     * @param {Vector3} axis Rotation axis
     * @param {number} angle Angle in radians
     * @returns {Object} Object with 'camera' field containing transformation matrix resulting from the operation to be applied to the camera
     */
    private Transformation Rotate(Vector3 axis, float angle)
    {
        var point = _gizmos.Position; //rotation center
        _translationMatrix.MakeTranslation(-point.X, -point.Y, -point.Z);
        _rotationMatrix.MakeRotationAxis(axis, -angle);

        //rotate camera
        _m4_1.MakeTranslation(point.X, point.Y, point.Z);
        _m4_1.Multiply(_rotationMatrix);
        _m4_1.Multiply(_translationMatrix);

        SetTransformationMatrices(_m4_1);

        return _transformation;
    }

    /**
     * Compute the easing out cubic function for ease out effect in animation
     * @param {Number} t The absolute progress of the animation in the bound of 0 (beginning of the) and 1 (ending of animation)
     * @returns {Number} Result of easing out cubic at time t
     */
    private float EaseOutCubic(float t)
    {
        return 1 - (float)Math.Pow(1 - t, 3);
    }

    /**
     * Make rotation gizmos more or less visible
     * @param {Boolean} isActive If true, make gizmos more visible
     */
    private void ActivateGizmos(bool isActive)
    {
        var gizmoX = _gizmos.Children[0];
        var gizmoY = _gizmos.Children[1];
        var gizmoZ = _gizmos.Children[2];

        if (isActive)
        {
            gizmoX.Material.Opacity = 1;
            gizmoY.Material.Opacity = 1;
            gizmoZ.Material.Opacity = 1;
        }
        else
        {
            gizmoX.Material.Opacity = 0.6f;
            gizmoY.Material.Opacity = 0.6f;
            gizmoZ.Material.Opacity = 0.6f;
        }
    }

    /**
     * Update the trackball FSA
     * @param {STATE} newState New state of the FSA
     * @param {Boolean} updateMatrices If matriices state should be updated
     */
    private void UpdateTbState(STATE newState, bool updateMatrices)
    {
        _state = newState;
        if (updateMatrices) UpdateMatrixState();
    }

    /**
     * Update camera and gizmos state
     */
    private void UpdateMatrixState()
    {
        //update camera and gizmos state
        _cameraMatrixState.Copy(camera.Matrix);
        _gizmoMatrixState.Copy(_gizmos.Matrix);

        if (camera is OrthographicCamera)
        {
            _cameraProjectionState.Copy(camera.ProjectionMatrix);
            camera.UpdateProjectionMatrix();
            _zoomState = camera.Zoom;
        }
        else if (camera is PerspectiveCamera)
        {
            _fovState = camera.Fov;
        }
    }

    /**
     * Focus operation consist of positioning the point of interest in front of the camera and a slightly zoom in
     * @param {Vector3} point The point of interest
     * @param {Number} size Scale factor
     * @param {Number} amount Amount of operation to be completed (used for focus animations, default is complete full operation)
     */
    private void Focus(Vector3 point, float size, float amount = 1)
    {
        //move center of camera (along with gizmos) towards point of interest
        _offset.Copy(point).Sub(_gizmos.Position).MultiplyScalar(amount);
        _translationMatrix.MakeTranslation(_offset.X, _offset.Y, _offset.Z);

        _gizmoMatrixStateTemp.Copy(_gizmoMatrixState);
        _gizmoMatrixState.PreMultiply(_translationMatrix);
        _gizmoMatrixState.Decompose(_gizmos.Position, _gizmos.Quaternion, _gizmos.Scale);

        _cameraMatrixStateTemp.Copy(_cameraMatrixState);
        _cameraMatrixState.PreMultiply(_translationMatrix);
        _cameraMatrixState.Decompose(camera.Position, camera.Quaternion, camera.Scale);

        //apply zoom
        if (EnableZoom) ApplyTransformMatrix(Scale(size, _gizmos.Position));

        _gizmoMatrixState.Copy(_gizmoMatrixStateTemp);
        _cameraMatrixState.Copy(_cameraMatrixStateTemp);
    }

    private Transformation Scale(float size, Vector3 point, bool scaleGizmos = true)
    {
        _scalePointTemp.Copy(point);
        var sizeInverse = 1 / size;

        if (camera is OrthographicCamera)
        {
            //camera zoom
            camera.Zoom = _zoomState;
            camera.Zoom *= size;

            //check min and max zoom
            if (camera.Zoom > MaxZoom)
            {
                camera.Zoom = MaxZoom;
                sizeInverse = _zoomState / MaxZoom;
            }
            else if (camera.Zoom < MinZoom)
            {
                camera.Zoom = MinZoom;
                sizeInverse = _zoomState / MinZoom;
            }

            camera.UpdateProjectionMatrix();

            _v3_1.SetFromMatrixPosition(_gizmoMatrixState); //gizmos position

            //scale gizmos so they appear in the same spot having the same dimension
            _scaleMatrix.MakeScale(sizeInverse, sizeInverse, sizeInverse);
            _translationMatrix.MakeTranslation(-_v3_1.X, -_v3_1.Y, -_v3_1.Z);

            _m4_2.MakeTranslation(_v3_1.X, _v3_1.Y, _v3_1.Z).Multiply(_scaleMatrix);
            _m4_2.Multiply(_translationMatrix);


            //move camera and gizmos to obtain pinch effect
            _scalePointTemp.Sub(_v3_1);

            var amount = _scalePointTemp.Clone().MultiplyScalar(sizeInverse);
            _scalePointTemp.Sub(amount);

            _m4_1.MakeTranslation(_scalePointTemp.X, _scalePointTemp.Y, _scalePointTemp.Z);
            _m4_2.PreMultiply(_m4_1);

            SetTransformationMatrices(_m4_1, _m4_2);

            return _transformation;
        }

        if (camera is PerspectiveCamera)
        {
            _v3_1.SetFromMatrixPosition(_cameraMatrixState);
            _v3_2.SetFromMatrixPosition(_gizmoMatrixState);

            //move camera
            var distance = _v3_1.DistanceTo(_scalePointTemp);
            var amount = distance - distance * sizeInverse;

            //check min and max distance
            var newDistance = distance - amount;
            if (newDistance < MinDistance)
            {
                sizeInverse = MinDistance / distance;
                amount = distance - distance * sizeInverse;
            }
            else if (newDistance > MaxDistance)
            {
                sizeInverse = MaxDistance / distance;
                amount = distance - distance * sizeInverse;
            }

            _offset.Copy(_scalePointTemp).Sub(_v3_1).Normalize().MultiplyScalar(amount);

            _m4_1.MakeTranslation(_offset.X, _offset.Y, _offset.Z);


            if (scaleGizmos)
            {
                //scale gizmos so they appear in the same spot having the same dimension
                var pos = _v3_2;

                distance = pos.DistanceTo(_scalePointTemp);
                amount = distance - distance * sizeInverse;
                _offset.Copy(_scalePointTemp).Sub(_v3_2).Normalize().MultiplyScalar(amount);

                _translationMatrix.MakeTranslation(pos.X, pos.Y, pos.Z);
                _scaleMatrix.MakeScale(sizeInverse, sizeInverse, sizeInverse);

                _m4_2.MakeTranslation(_offset.X, _offset.Y, _offset.Z).Multiply(_translationMatrix);
                _m4_2.Multiply(_scaleMatrix);

                _translationMatrix.MakeTranslation(-pos.X, -pos.Y, -pos.Z);

                _m4_2.Multiply(_translationMatrix);
                SetTransformationMatrices(_m4_1, _m4_2);
            }
            else
            {
                SetTransformationMatrices(_m4_1);
            }

            return _transformation;
        }

        return null;
    }

    /**
     * Set camera fov
     * @param {Number} value fov to be setted
     */
    public void SetFov(float value)
    {
        if (camera is PerspectiveCamera)
        {
            camera.Fov = MathUtils.Clamp(value, minFov, maxFov);
            camera.UpdateProjectionMatrix();
        }
    }

    /**
     * Set values in transformation object
     * @param {Matrix4} camera Transformation to be applied to the camera
     * @param {Matrix4} gizmos Transformation to be applied to gizmos
     */
    private void SetTransformationMatrices(Matrix4 camera = null, Matrix4 gizmos = null)
    {
        if (camera != null)
        {
            if (_transformation.Camera != null)
                _transformation.Camera.Copy(camera);
            else
                _transformation.Camera = (Matrix4)camera.Clone();
        }
        else
        {
            _transformation.Camera = null;
        }

        if (gizmos != null)
        {
            if (_transformation.gizmos != null)
                _transformation.gizmos.Copy(gizmos);
            else
                _transformation.gizmos = (Matrix4)gizmos.Clone();
        }
        else
        {
            _transformation.gizmos = null;
        }
    }

    /**
     * Rotate camera around its direction axis passing by a given point by a given angle
     * @param {Vector3} point The point where the rotation axis is passing trough
     * @param {Number} angle Angle in radians
     * @returns The computed transormation matix
     */
    public Transformation ZRotate(Vector3 point, float angle)
    {
        _rotationMatrix.MakeRotationAxis(_rotationAxis, angle);
        _translationMatrix.MakeTranslation(-point.X, -point.Y, -point.Z);

        _m4_1.MakeTranslation(point.X, point.Y, point.Z);
        _m4_1.Multiply(_rotationMatrix);
        _m4_1.Multiply(_translationMatrix);

        _v3_1.SetFromMatrixPosition(_gizmoMatrixState).Sub(point); //vector from rotation center to gizmos position
        _v3_2.Copy(_v3_1).ApplyAxisAngle(_rotationAxis, angle); //apply rotation
        _v3_2.Sub(_v3_1);

        _m4_2.MakeTranslation(_v3_2.X, _v3_2.Y, _v3_2.Z);

        SetTransformationMatrices(_m4_1, _m4_2);
        return _transformation;
    }

    public Raycaster GetRaycaster()
    {
        return _raycaster;
    }

    /**
     * Unproject the cursor on the 3D object surface
     * @param {Vector2} cursor Cursor coordinates in NDC
     * @param {Camera} camera Virtual camera
     * @returns {Vector3} The point of intersection with the model, if exist, null otherwise
     */
    public Vector3 UnprojectOnObj(Vector2 cursor, Camera camera)
    {
        _raycaster.near = camera.Near;
        _raycaster.far = camera.Far;
        _raycaster.SetFromCamera(cursor, camera);

        var intersect = _raycaster.IntersectObjects(scene.Children, true);

        for (var i = 0; i < intersect.Count; i++)
            if (intersect[i].Object3D.Uuid != _gizmos.Uuid && intersect[i].Face != null)
                return intersect[i].Point.Clone();

        return null;
    }

    /**
     * Unproject the cursor on the trackball surface
     * @param {Camera} camera The virtual camera
     * @param {Number} cursorX Cursor horizontal coordinate on screen
     * @param {Number} cursorY Cursor vertical coordinate on screen
     * @param {HTMLElement} canvas The canvas where the renderer draws its output
     * @param {number} tbRadius The trackball radius
     * @returns {Vector3} The unprojected point on the trackball surface
     */
    private Vector3 UnprojectOnTbSurface(Camera camera, float cursorX, float cursorY, float tbRadius)
    {
        if (camera is OrthographicCamera)
        {
            _v2_1.Copy(GetCursorPosition(cursorX, cursorY));
            _v3_1.Set(_v2_1.X, _v2_1.Y, 0);

            var x2 = (float)Math.Pow(_v2_1.X, 2);
            var y2 = (float)Math.Pow(_v2_1.Y, 2);
            var r2 = (float)Math.Pow(_tbRadius, 2);

            if (x2 + y2 <= r2 * 0.5)
                //intersection with sphere
                _v3_1.SetZ((float)Math.Sqrt(r2 - (x2 + y2)));
            else
                //intersection with hyperboloid
                _v3_1.SetZ(r2 * 0.5f / (float)Math.Sqrt(x2 + y2));

            return _v3_1;
        }

        // if (camera.type == 'PerspectiveCamera' ) {
        //unproject cursor on the near plane
        _v2_1.Copy(GetCursorNDC(cursorX, cursorY));

        _v3_1.Set(_v2_1.X, _v2_1.Y, -1);
        _v3_1.ApplyMatrix4(camera.ProjectionMatrixInverse);

        var rayDir = _v3_1.Clone().Normalize(); //unprojected ray direction
        var cameraGizmoDistance = camera.Position.DistanceTo(_gizmos.Position);
        var radius2 = (float)Math.Pow(tbRadius, 2);

        //	  camera
        //		|\
        //		| \
        //		|  \
        //	h	|	\
        //		| 	 \
        //		| 	  \
        //	_ _ | _ _ _\ _ _  near plane
        //			l

        var h = _v3_1.Z;
        var l = (float)Math.Sqrt(Math.Pow(_v3_1.X, 2) + Math.Pow(_v3_1.Y, 2));

        if (l == 0)
        {
            //ray aligned with camera
            rayDir.Set(_v3_1.X, _v3_1.Y, tbRadius);
            return rayDir;
        }

        var m = h / l;
        var q = cameraGizmoDistance;

        /*
         * calculate intersection point between unprojected ray and trackball surface
         *|y = m * x + q
         *|x^2 + y^2 = r^2
         *
         * (m^2 + 1) * x^2 + (2 * m * q) * x + q^2 - r^2 = 0
         */
        var a = (float)Math.Pow(m, 2) + 1;
        var b = 2 * m * q;
        var c = (float)Math.Pow(q, 2) - radius2;
        var delta = (float)Math.Pow(b, 2) - 4 * a * c;

        if (delta >= 0)
        {
            //intersection with sphere
            _v2_1.SetX((-b - (float)Math.Sqrt(delta)) / (2 * a));
            _v2_1.SetY(m * _v2_1.X + q);

            var angle = MathUtils.RAD2DEG * _v2_1.Angle();

            if (angle >= 45)
            {
                //if angle between intersection point and X' axis is >= 45°, return that point
                //otherwise, calculate intersection point with hyperboloid

                var rayLength = (float)Math.Sqrt(Math.Pow(_v2_1.X, 2) + Math.Pow(cameraGizmoDistance - _v2_1.Y, 2));
                rayDir.MultiplyScalar(rayLength);
                rayDir.Z += cameraGizmoDistance;
                return rayDir;
            }
        }

        //intersection with hyperboloid
        /*
         *|y = m * x + q
         *|y = (1 / x) * (r^2 / 2)
         *
         * m * x^2 + q * x - r^2 / 2 = 0
         */

        a = m;
        b = q;
        c = -radius2 * 0.5f;
        delta = (float)Math.Pow(b, 2) - 4 * a * c;
        _v2_1.SetX((-b - (float)Math.Sqrt(delta)) / (2 * a));
        _v2_1.SetY(m * _v2_1.X + q);

        var rayLength1 = (float)Math.Sqrt(Math.Pow(_v2_1.X, 2) + Math.Pow(cameraGizmoDistance - _v2_1.Y, 2));

        rayDir.MultiplyScalar(rayLength1);
        rayDir.Z += cameraGizmoDistance;
        return rayDir;
    }


    /**
     * Unproject the cursor on the plane passing through the center of the trackball orthogonal to the camera
     * @param {Camera} camera The virtual camera
     * @param {Number} cursorX Cursor horizontal coordinate on screen
     * @param {Number} cursorY Cursor vertical coordinate on screen
     * @param {HTMLElement} canvas The canvas where the renderer draws its output
     * @param {Boolean} initialDistance If initial distance between camera and gizmos should be used for calculations instead of current (Perspective only)
     * @returns {Vector3} The unprojected point on the trackball plane
     */
    public Vector3 UnprojectOnTbPlane(Camera camera, float cursorX, float cursorY, bool initialDistance = false)
    {
        if (camera is OrthographicCamera)
        {
            _v2_1.Copy(GetCursorPosition(cursorX, cursorY));
            _v3_1.Set(_v2_1.X, _v2_1.Y, 0);

            return _v3_1.Clone();
        }

        if (camera is PerspectiveCamera)
        {
            _v2_1.Copy(GetCursorNDC(cursorX, cursorY));

            //unproject cursor on the near plane
            _v3_1.Set(_v2_1.X, _v2_1.Y, -1);
            _v3_1.ApplyMatrix4(camera.ProjectionMatrixInverse);

            var rayDir = _v3_1.Clone().Normalize(); //unprojected ray direction

            //	  camera
            //		|\
            //		| \
            //		|  \
            //	h	|	\
            //		| 	 \
            //		| 	  \
            //	_ _ | _ _ _\ _ _  near plane
            //			l

            var h = _v3_1.Z;
            var l = (float)Math.Sqrt(Math.Pow(_v3_1.X, 2) + Math.Pow(_v3_1.Y, 2));
            float cameraGizmoDistance;

            if (initialDistance)
                cameraGizmoDistance = _v3_1.SetFromMatrixPosition(_cameraMatrixState0)
                    .DistanceTo(_v3_2.SetFromMatrixPosition(_gizmoMatrixState0));
            else
                cameraGizmoDistance = camera.Position.DistanceTo(_gizmos.Position);

            /*
             * calculate intersection point between unprojected ray and the plane
             *|y = mx + q
             *|y = 0
             *
             * x = -q/m
             */
            if (l == 0)
            {
                //ray aligned with camera
                rayDir.Set(0, 0, 0);
                return rayDir;
            }

            var m = h / l;
            var q = cameraGizmoDistance;
            var x = -q / m;

            var rayLength = (float)Math.Sqrt(Math.Pow(q, 2) + Math.Pow(x, 2));
            rayDir.MultiplyScalar(rayLength);
            rayDir.Z = 0;
            return rayDir;
        }

        return null;
    }

    /**
     * Calculate the cursor position inside the canvas x/y coordinates with the origin being in the center of the canvas
     * @param {Number} x Cursor horizontal coordinate within the canvas
     * @param {Number} y Cursor vertical coordinate within the canvas
     * @param {HTMLElement} canvas The canvas where the renderer draws its output
     * @returns {Vector2} Cursor position inside the canvas
     */
    private Vector2 GetCursorPosition(float cursorX, float cursorY)
    {
        _v2_1.Copy(GetCursorNDC(cursorX, cursorY));
        _v2_1.X *= (camera.CameraRight - camera.Left) * 0.5f;
        _v2_1.Y *= (camera.Top - camera.Bottom) * 0.5f;
        return _v2_1.Clone();
    }

    /**
     * Calculate the cursor position in NDC
     * @param {number} x Cursor horizontal coordinate within the canvas
     * @param {number} y Cursor vertical coordinate within the canvas
     * @param {HTMLElement} canvas The canvas where the renderer draws its output
     * @returns {Vector2} Cursor normalized position inside the canvas
     */
    private Vector2 GetCursorNDC(float cursorX, float cursorY)
    {
        var canvasRect = glControl.ClientRectangle;
        _v2_1.SetX((cursorX - canvasRect.Left) / canvasRect.Width * 2 - 1);
        _v2_1.SetY((canvasRect.Bottom - cursorY) / canvasRect.Height * 2 - 1);
        return _v2_1.Clone();
    }

    /**
     * Apply a transformation matrix, to the camera and gizmos
     * @param {Object} transformation Object containing matrices to apply to camera and gizmos
     */
    private void ApplyTransformMatrix(Transformation transformation)
    {
        if (transformation.Camera != null)
        {
            _m4_1.Copy(_cameraMatrixState).PreMultiply(transformation.Camera);
            _m4_1.Decompose(camera.Position, camera.Quaternion, camera.Scale);
            camera.UpdateMatrix();

            //update camera up vector
            if (_state == STATE.ROTATE || _state == STATE.ZROTATE || _state == STATE.ANIMATION_ROTATE)
                camera.Up.Copy(_upState).ApplyQuaternion(camera.Quaternion);
        }

        if (transformation.gizmos != null)
        {
            _m4_1.Copy(_gizmoMatrixState).PreMultiply(transformation.gizmos);
            _m4_1.Decompose(_gizmos.Position, _gizmos.Quaternion, _gizmos.Scale);
            _gizmos.UpdateMatrix();
        }

        if (_state == STATE.SCALE || _state == STATE.FOCUS || _state == STATE.ANIMATION_FOCUS)
        {
            _tbRadius = CalculateTbRadius(camera);

            if (AdjustNearFar)
            {
                var cameraDistance = camera.Position.DistanceTo(_gizmos.Position);

                var bb = new Box3();
                bb.SetFromObject(_gizmos);
                var sphere = new Sphere();
                bb.GetBoundingSphere(sphere);

                var adjustedNearPosition = Math.Max(_nearPos0, sphere.Radius + sphere.Center.Length());
                var regularNearPosition = cameraDistance - _initialNear;

                var minNearPos = Math.Min(adjustedNearPosition, regularNearPosition);
                camera.Near = cameraDistance - minNearPos;


                var adjustedFarPosition = Math.Min(_farPos0, -sphere.Radius + sphere.Center.Length());
                var regularFarPosition = cameraDistance - _initialFar;

                var minFarPos = Math.Min(adjustedFarPosition, regularFarPosition);
                camera.Far = cameraDistance - minFarPos;

                camera.UpdateProjectionMatrix();
            }
            else
            {
                var update = false;

                if (camera.Near != _initialNear)
                {
                    camera.Near = _initialNear;
                    update = true;
                }

                if (camera.Far != _initialFar)
                {
                    camera.Far = _initialFar;
                    update = true;
                }

                if (update) camera.UpdateProjectionMatrix();
            }
        }
    }

    private float CalculateTbRadius(Camera camera)
    {
        var distance = camera.Position.DistanceTo(_gizmos.Position);

        if (camera is PerspectiveCamera)
        {
            var halfFovV = MathUtils.DEG2RAD * camera.Fov * 0.5f; //vertical fov/2 in radians
            var halfFovH = Math.Atan(camera.Aspect * Math.Tan(halfFovV)); //horizontal fov/2 in radians
            return (float)Math.Tan(Math.Min(halfFovV, halfFovH)) * distance * radiusFactor;
        }

        if (camera is OrthographicCamera) return Math.Min(camera.Top, camera.CameraRight) * radiusFactor;

        return 0.0f;
    }

    public void Update()
    {
        var EPS = 0.000001f;

        if (target.Equals(_currentTarget) == false)
        {
            _gizmos.Position.Copy(target); //for correct radius calculation
            _tbRadius = CalculateTbRadius(camera);
            MakeGizmos(target, _tbRadius);
            _currentTarget.Copy(target);
        }

        //check min/max parameters
        if (camera is OrthographicCamera)
        {
            //check zoom
            if (camera.Zoom > MaxZoom || camera.Zoom < MinZoom)
            {
                var newZoom = MathUtils.Clamp(camera.Zoom, MinZoom, MaxZoom);
                ApplyTransformMatrix(Scale(newZoom / camera.Zoom, _gizmos.Position));
            }
        }
        else if (camera is PerspectiveCamera)
        {
            //check distance
            var distance = camera.Position.DistanceTo(_gizmos.Position);

            if (distance > MaxDistance + EPS || distance < MinDistance - EPS)
            {
                var newDistance = MathUtils.Clamp(distance, MinDistance, MaxDistance);
                ApplyTransformMatrix(Scale(newDistance / distance, _gizmos.Position));
                UpdateMatrixState();
            }

            //check fov
            if (camera.Fov < minFov || camera.Fov > maxFov)
            {
                camera.Fov = MathUtils.Clamp(camera.Fov, minFov, maxFov);
                camera.UpdateProjectionMatrix();
            }

            var oldRadius = _tbRadius;
            _tbRadius = CalculateTbRadius(camera);

            if (oldRadius < _tbRadius - EPS || oldRadius > _tbRadius + EPS)
            {
                var scale = (_gizmos.Scale.X + _gizmos.Scale.Y + _gizmos.Scale.Z) / 3;
                var newRadius = _tbRadius / scale;
                var curve = new EllipseCurve(0, 0, newRadius, newRadius);
                var points = curve.GetPoints(_curvePts);
                var curveGeometry = new BufferGeometry().SetFromPoints(points);

                foreach (var gizmo in _gizmos.Children) gizmo.Geometry = curveGeometry;
                //this._gizmos.children[gizmo].geometry = curveGeometry;
            }
        }

        camera.LookAt(_gizmos.Position);
    }

    private void OnSinglePanStart(MouseEventArgs e, string operation)
    {
        if (Enabled == false) return;

        if (StartEvent != null) StartEvent();

        SetCenter(e.X, e.Y);

        switch (operation)
        {
            case "PAN":
                if (!EnablePan) return;
                if (_animationId != -1)
                {
                    _animationId = -1;
                    _timeStart = -1;

                    ActivateGizmos(false);

                    if (ChangeEvent != null) ChangeEvent();
                }

                UpdateTbState(STATE.PAN, true);
                _startCursorPosition.Copy(UnprojectOnTbPlane(camera, _center.x, _center.y));
                if (EnableGrid)
                {
                    DrawGrid();
                    if (ChangeEvent != null) ChangeEvent();
                }

                break;
            case "ROTATE":
                if (!EnableRotate) return;

                if (_animationId != -1)
                {
                    _animationId = -1;
                    _timeStart = -1;
                }

                UpdateTbState(STATE.ROTATE, true);
                _startCursorPosition.Copy(UnprojectOnTbSurface(camera, _center.x, _center.y, _tbRadius));
                ActivateGizmos(true);
                if (enableAnimations)
                {
                    _timePrev = _timeCurrent = stopWatch.ElapsedMilliseconds;
                    _angleCurrent = _anglePrev = 0;
                    _cursorPosPrev.Copy(_startCursorPosition);
                    _cursorPosCurr.Copy(_cursorPosPrev);
                    _wCurr = 0;
                    _wPrev = _wCurr;
                }

                if (ChangeEvent != null) ChangeEvent();

                break;
            case "FOV":
                if (camera is OrthographicCamera || !EnableZoom) return;

                if (_animationId != -1)
                {
                    //cancelAnimationFrame(this._animationId);
                    _animationId = -1;
                    _timeStart = -1;

                    ActivateGizmos(false);
                    if (ChangeEvent != null) ChangeEvent();
                }

                UpdateTbState(STATE.FOV, true);
                _startCursorPosition.SetY(GetCursorNDC(_center.x, _center.y).Y * 0.5f);
                _currentCursorPosition.Copy(_startCursorPosition);
                break;
            case "ZOOM":
                if (!EnableZoom) return;

                if (_animationId != -1)
                {
                    //cancelAnimationFrame(this._animationId);
                    _animationId = -1;
                    _timeStart = -1;

                    ActivateGizmos(false);
                    if (ChangeEvent != null) ChangeEvent();
                }

                UpdateTbState(STATE.SCALE, true);
                _startCursorPosition.SetY(GetCursorNDC(_center.x, _center.y).Y * 0.5f);
                _currentCursorPosition.Copy(_startCursorPosition);
                break;
        }
    }

    private void OnSinglePanMove(MouseEventArgs e, STATE opState)
    {
        if (Enabled)
        {
            var restart = opState != _state;
            SetCenter(e.X, e.Y);

            switch (opState)
            {
                case STATE.PAN:

                    if (EnablePan)
                    {
                        if (restart)
                        {
                            //switch to pan operation
                            if (EndEvent != null) EndEvent();
                            if (StartEvent != null) StartEvent();

                            UpdateTbState(opState, true);
                            _startCursorPosition.Copy(UnprojectOnTbPlane(camera, _center.x, _center.y));
                            if (EnableGrid) DrawGrid();

                            ActivateGizmos(false);
                        }
                        else
                        {
                            //continue with pan operation
                            _currentCursorPosition.Copy(UnprojectOnTbPlane(camera, _center.x, _center.y));
                            ApplyTransformMatrix(Pan(_startCursorPosition, _currentCursorPosition));
                        }
                    }

                    break;

                case STATE.ROTATE:

                    if (EnableRotate)
                    {
                        if (restart)
                        {
                            //switch to rotate operation

                            if (EndEvent != null) EndEvent();
                            if (StartEvent != null) StartEvent();

                            UpdateTbState(opState, true);
                            _startCursorPosition.Copy(UnprojectOnTbSurface(camera, _center.x, _center.y, _tbRadius));

                            if (EnableGrid) DisposeGrid();

                            ActivateGizmos(true);
                        }
                        else
                        {
                            //continue with rotate operation
                            _currentCursorPosition.Copy(UnprojectOnTbSurface(camera, _center.x, _center.y, _tbRadius));

                            var distance = _startCursorPosition.DistanceTo(_currentCursorPosition);
                            var angle = _startCursorPosition.AngleTo(_currentCursorPosition);
                            var amount = Math.Max(distance / _tbRadius, angle); //effective rotation angle

                            ApplyTransformMatrix(
                                Rotate(CalculateRotationAxis(_startCursorPosition, _currentCursorPosition), amount));

                            if (enableAnimations)
                            {
                                _timePrev = _timeCurrent;
                                _timeCurrent = stopWatch.ElapsedMilliseconds;
                                _anglePrev = _angleCurrent;
                                _angleCurrent = amount;
                                _cursorPosPrev.Copy(_cursorPosCurr);
                                _cursorPosCurr.Copy(_currentCursorPosition);
                                _wPrev = _wCurr;
                                _wCurr = CalculateAngularSpeed(_anglePrev, _angleCurrent, _timePrev, _timeCurrent);
                            }
                        }
                    }

                    break;

                case STATE.SCALE:

                    if (EnableZoom)
                    {
                        if (restart)
                        {
                            //switch to zoom operation

                            if (EndEvent != null) EndEvent();
                            if (StartEvent != null) StartEvent();

                            UpdateTbState(opState, true);
                            _startCursorPosition.SetY(GetCursorNDC(_center.x, _center.y).Y * 0.5f);
                            _currentCursorPosition.Copy(_startCursorPosition);

                            if (EnableGrid) DisposeGrid();

                            ActivateGizmos(false);
                        }
                        else
                        {
                            //continue with zoom operation
                            var screenNotches = 8; //how many wheel notches corresponds to a full screen pan
                            _currentCursorPosition.SetY(GetCursorNDC(_center.x, _center.y).Y * 0.5f);

                            var movement = _currentCursorPosition.Y - _startCursorPosition.Y;

                            float size = 1;

                            if (movement < 0)
                                size = 1 / (float)Math.Pow(ScaleFactor, -movement * screenNotches);
                            else if (movement > 0) size = (float)Math.Pow(ScaleFactor, movement * screenNotches);

                            _v3_1.SetFromMatrixPosition(_gizmoMatrixState);

                            ApplyTransformMatrix(Scale(size, _v3_1));
                        }
                    }

                    break;

                case STATE.FOV:

                    if (EnableZoom && camera is PerspectiveCamera)
                    {
                        if (restart)
                        {
                            //switch to fov operation

                            if (EndEvent != null) EndEvent();
                            if (StartEvent != null) StartEvent();

                            UpdateTbState(opState, true);
                            _startCursorPosition.SetY(GetCursorNDC(_center.x, _center.y).Y * 0.5f);
                            _currentCursorPosition.Copy(_startCursorPosition);

                            if (EnableGrid) DisposeGrid();

                            ActivateGizmos(false);
                        }
                        else
                        {
                            //continue with fov operation
                            var screenNotches = 8; //how many wheel notches corresponds to a full screen pan
                            _currentCursorPosition.SetY(GetCursorNDC(_center.x, _center.y).Y * 0.5f);

                            var movement = _currentCursorPosition.Y - _startCursorPosition.Y;

                            float size = 1;

                            if (movement < 0)
                                size = 1 / (float)Math.Pow(ScaleFactor, -movement * screenNotches);
                            else if (movement > 0) size = (float)Math.Pow(ScaleFactor, movement * screenNotches);

                            _v3_1.SetFromMatrixPosition(_cameraMatrixState);
                            var x = _v3_1.DistanceTo(_gizmos.Position);
                            var xNew = x /
                                       size; //distance between camera and gizmos if scale(size, scalepoint) would be performed

                            //check min and max distance
                            xNew = MathUtils.Clamp(xNew, MinDistance, MaxDistance);

                            var y = x * (float)Math.Tan(MathUtils.DEG2RAD * _fovState * 0.5f);

                            //calculate new fov
                            var newFov = MathUtils.RAD2DEG * ((float)Math.Atan(y / xNew) * 2);

                            //check min and max fov
                            newFov = MathUtils.Clamp(newFov, minFov, maxFov);

                            var newDistance = y / (float)Math.Tan(MathUtils.DEG2RAD * (newFov / 2));
                            size = x / newDistance;
                            _v3_2.SetFromMatrixPosition(_gizmoMatrixState);

                            SetFov(newFov);
                            ApplyTransformMatrix(Scale(size, _v3_2, false));

                            //adjusting distance
                            _offset.Copy(_gizmos.Position).Sub(camera.Position).Normalize()
                                .MultiplyScalar(newDistance / x);
                            _m4_1.MakeTranslation(_offset.X, _offset.Y, _offset.Z);
                        }
                    }

                    break;
            }

            if (ChangeEvent != null) ChangeEvent();
        }
    }

    private float CalculateAngularSpeed(float p0, float p1, long t0, long t1)
    {
        var s = p1 - p0;
        var t = (t1 - t0) / 1000;
        if (t == 0) return 0;

        return s / t;
    }

    /**
     * Calculate the rotation axis as the vector perpendicular between two vectors
     * @param {Vector3} vec1 The first vector
     * @param {Vector3} vec2 The second vector
     * @returns {Vector3} The normalized rotation axis
     */
    private Vector3 CalculateRotationAxis(Vector3 vec1, Vector3 vec2)
    {
        _rotationMatrix.ExtractRotation(_cameraMatrixState);
        _quat.SetFromRotationMatrix(_rotationMatrix);

        _rotationAxis.CrossVectors(vec1, vec2).ApplyQuaternion(_quat);
        return _rotationAxis.Normalize().Clone();
    }

    /**
     * Perform pan operation moving camera between two points
     * @param {Vector3} p0 Initial point
     * @param {Vector3} p1 Ending point
     * @param {Boolean} adjust If movement should be adjusted considering camera distance (Perspective only)
     */
    private Transformation Pan(Vector3 p0, Vector3 p1, bool adjust = false)
    {
        var movement = p0.Clone().Sub(p1);

        if (camera is OrthographicCamera)
        {
            //adjust movement amount
            movement.MultiplyScalar(1 / camera.Zoom);
        }
        else if (camera is PerspectiveCamera && adjust)
        {
            //adjust movement amount
            _v3_1.SetFromMatrixPosition(_cameraMatrixState0); //camera's initial position
            _v3_2.SetFromMatrixPosition(_gizmoMatrixState0); //gizmo's initial position
            var distanceFactor = _v3_1.DistanceTo(_v3_2) / camera.Position.DistanceTo(_gizmos.Position);
            movement.MultiplyScalar(1 / distanceFactor);
        }

        _v3_1.Set(movement.X, movement.Y, 0).ApplyQuaternion(camera.Quaternion);

        _m4_1.MakeTranslation(_v3_1.X, _v3_1.Y, _v3_1.Z);

        SetTransformationMatrices(_m4_1, _m4_1);
        return _transformation;
    }

    public void Reset()
    {
        camera.Zoom = _zoom0;

        if (camera is PerspectiveCamera) camera.Fov = _fov0;

        camera.Near = _nearPos;
        camera.Far = _farPos;
        _cameraMatrixState.Copy(_cameraMatrixState0);
        _cameraMatrixState.Decompose(camera.Position, camera.Quaternion, camera.Scale);
        camera.Up.Copy(_up0);

        camera.UpdateMatrix();
        camera.UpdateProjectionMatrix();

        _gizmoMatrixState.Copy(_gizmoMatrixState0);
        _gizmoMatrixState0.Decompose(_gizmos.Position, _gizmos.Quaternion, _gizmos.Scale);
        _gizmos.UpdateMatrix();

        _tbRadius = CalculateTbRadius(camera);

        MakeGizmos(_gizmos.Position, _tbRadius);

        camera.LookAt(_gizmos.Position);

        UpdateTbState(STATE.IDLE, false);

        if (ChangeEvent != null)
            ChangeEvent();
    }

    private void OnPointerMove(object sender, MouseEventArgs e)
    {
        if (_input != INPUT.CURSOR)
        {
            switch (_input)
            {
                case INPUT.ONE_FINGER:
                    //singleMove
                    UpdateTouchEvent(e);

                    OnSinglePanMove(e, STATE.ROTATE);
                    break;

                case INPUT.ONE_FINGER_SWITCHED:

                    var movement = CalculatePointersDistance(_touchCurrent[0], e) * _devPxRatio;

                    if (movement >= _switchSensibility)
                    {
                        //singleMove
                        _input = INPUT.ONE_FINGER;
                        UpdateTouchEvent(e);

                        OnSinglePanStart(e, "ROTATE");
                    }

                    break;
                case INPUT.TWO_FINGER:

                    //rotate/pan/pinchMove
                    UpdateTouchEvent(e);

                    OnRotateMove();
                    OnPinchMove();
                    OnDoublePanMove();

                    break;
                case INPUT.MULT_FINGER:

                    //multMove
                    UpdateTouchEvent(e);
                    onTriplePanMove(e);
                    break;
            }
        }
        else if (_input == INPUT.CURSOR)
        {
            string modifier = null;

            if (controlKey)
                modifier = "CTRL";
            else if (shiftKey) modifier = "SHIFT";

            var mouseOpState = GetOpStateFromAction(_button, modifier);

            if (mouseOpState != null) OnSinglePanMove(e, (STATE)mouseOpState);
        }

        //checkDistance
        if (_downValid)
        {
            float movement = 0;
            if (_downEvents.Count > 1)
                movement = CalculatePointersDistance(_downEvents[_downEvents.Count - 1], e) * _devPxRatio;

            if (movement > _movementThreshold) _downValid = false;
        }
    }

    private void OnRotateMove()
    {
        if (Enabled && EnableRotate)
        {
            SetCenter((_touchCurrent[0].X + _touchCurrent[1].X) / 2, (_touchCurrent[0].Y + _touchCurrent[1].Y) / 2);

            var rotationPoint = new Vector3();
            ;

            if (_state != STATE.ZROTATE)
            {
                UpdateTbState(STATE.ZROTATE, true);
                _startFingerRotation = _currentFingerRotation;
            }

            //this._currentFingerRotation = event.rotation;
            _currentFingerRotation =
                GetAngle(_touchCurrent[1], _touchCurrent[0]) + GetAngle(_touchStart[1], _touchStart[0]);

            if (!EnablePan)
            {
                rotationPoint = new Vector3().SetFromMatrixPosition(_gizmoMatrixState);
            }
            else
            {
                _v3_2.SetFromMatrixPosition(_gizmoMatrixState);
                rotationPoint = UnprojectOnTbPlane(camera, _center.x, _center.y).ApplyQuaternion(camera.Quaternion)
                    .MultiplyScalar(1 / camera.Zoom).Add(_v3_2);
            }

            var amount = MathUtils.DEG2RAD * (_startFingerRotation - _currentFingerRotation);

            ApplyTransformMatrix(ZRotate(rotationPoint, amount));
            if (ChangeEvent != null) ChangeEvent();
        }
    }

    /**
     * Calculate the angle between two pointers
     * @param {PointerEvent} p1
     * @param {PointerEvent} p2
     * @returns {Number} The angle between two pointers in degrees
     */
    private float GetAngle(MouseEventArgs p1, MouseEventArgs p2)
    {
        return (float)(Math.Atan2(p2.Y - p1.Y, p2.X - p1.X) * 180 / Math.PI);
    }

    private void OnPinchMove()
    {
        if (Enabled && EnableZoom)
        {
            SetCenter((_touchCurrent[0].X + _touchCurrent[1].X) / 2, (_touchCurrent[0].Y + _touchCurrent[1].Y) / 2);
            var minDistance = 12; //minimum distance between fingers (in css pixels)

            if (_state != STATE.SCALE)
            {
                _startFingerDistance = _currentFingerDistance;
                UpdateTbState(STATE.SCALE, true);
            }

            _currentFingerDistance = Math.Max(CalculatePointersDistance(_touchCurrent[0], _touchCurrent[1]),
                minDistance * _devPxRatio);
            var amount = _currentFingerDistance / _startFingerDistance;

            var scalePoint = new Vector3();
            ;

            if (!EnablePan)
            {
                scalePoint = _gizmos.Position;
            }
            else
            {
                if (camera is OrthographicCamera)
                    scalePoint = UnprojectOnTbPlane(camera, _center.x, _center.y)
                        .ApplyQuaternion(camera.Quaternion)
                        .MultiplyScalar(1 / camera.Zoom)
                        .Add(_gizmos.Position);
                else if (camera is PerspectiveCamera)
                    scalePoint = UnprojectOnTbPlane(camera, _center.x, _center.y)
                        .ApplyQuaternion(camera.Quaternion)
                        .Add(_gizmos.Position);
            }

            ApplyTransformMatrix(Scale(amount, scalePoint));

            if (ChangeEvent != null) ChangeEvent();
        }
    }

    private void OnDoublePanMove()
    {
        if (Enabled && EnablePan)
        {
            SetCenter((_touchCurrent[0].X + _touchCurrent[1].X) / 2, (_touchCurrent[0].Y + _touchCurrent[1].Y) / 2);

            if (_state != STATE.PAN)
            {
                UpdateTbState(STATE.PAN, true);
                _startCursorPosition.Copy(_currentCursorPosition);
            }

            _currentCursorPosition.Copy(UnprojectOnTbPlane(camera, _center.x, _center.y, true));
            ApplyTransformMatrix(Pan(_startCursorPosition, _currentCursorPosition, true));
            if (ChangeEvent != null) ChangeEvent();
        }
    }

    private void onTriplePanMove(MouseEventArgs e)
    {
        if (Enabled && EnableZoom)
        {
            //	  fov / 2
            //		|\
            //		| \
            //		|  \
            //	x	|	\
            //		| 	 \
            //		| 	  \
            //		| _ _ _\
            //			y

            //const center = event.center;
            var clientX = 0;
            var clientY = 0;
            var nFingers = _touchCurrent.Count;

            for (var i = 0; i < nFingers; i++)
            {
                clientX += _touchCurrent[i].X;
                clientY += _touchCurrent[i].Y;
            }

            SetCenter(clientX / nFingers, clientY / nFingers);

            var screenNotches = 8; //how many wheel notches corresponds to a full screen pan
            _currentCursorPosition.SetY(GetCursorNDC(_center.x, _center.y).Y * 0.5f);

            var movement = _currentCursorPosition.Y - _startCursorPosition.Y;

            float size = 1;

            if (movement < 0)
                size = 1 / (float)Math.Pow(ScaleFactor, -movement * screenNotches);
            else if (movement > 0) size = (float)Math.Pow(ScaleFactor, movement * screenNotches);

            _v3_1.SetFromMatrixPosition(_cameraMatrixState);
            var x = _v3_1.DistanceTo(_gizmos.Position);
            var xNew = x / size; //distance between camera and gizmos if scale(size, scalepoint) would be performed

            //check min and max distance
            xNew = MathUtils.Clamp(xNew, MinDistance, MaxDistance);

            var y = x * (float)Math.Tan(MathUtils.DEG2RAD * _fovState * 0.5f);

            //calculate new fov
            var newFov = MathUtils.RAD2DEG * ((float)Math.Atan(y / xNew) * 2);

            //check min and max fov
            newFov = MathUtils.Clamp(newFov, minFov, maxFov);

            var newDistance = y / (float)Math.Tan(MathUtils.DEG2RAD * (newFov / 2));
            size = x / newDistance;
            _v3_2.SetFromMatrixPosition(_gizmoMatrixState);

            SetFov(newFov);
            ApplyTransformMatrix(Scale(size, _v3_2, false));

            //adjusting distance
            _offset.Copy(_gizmos.Position).Sub(camera.Position).Normalize().MultiplyScalar(newDistance / x);
            _m4_1.MakeTranslation(_offset.X, _offset.Y, _offset.Z);

            if (ChangeEvent != null) ChangeEvent();
        }
    }

    private object GetOpStateFromAction(string mouse, string key)
    {
        MouseAction action;

        for (var i = 0; i < mouseActions.Count; i++)
        {
            action = mouseActions[i];
            if (action.mouse == mouse && action.key == key) return action.state;
        }

        if (key != null)
            for (var i = 0; i < mouseActions.Count; i++)
            {
                action = mouseActions[i];
                if (action.mouse == mouse && action.key == null) return action.state;
            }

        return null;
    }

    private void OnSinglePanEnd()
    {
    }

    private void OnPointerUp(object sender, MouseEventArgs e)
    {
        if (_input != INPUT.CURSOR)
        {
            var nTouch = _touchCurrent.Count;

            for (var i = 0; i < nTouch; i++)
                if (_touchCurrent[i].Equals(e))
                {
                    _touchCurrent.RemoveAt(i); //splice(i, 1);
                    _touchStart.RemoveAt(i); // splice(i, 1);
                    break;
                }

            switch (_input)
            {
                case INPUT.ONE_FINGER:
                case INPUT.ONE_FINGER_SWITCHED:
                    //singleEnd
                    glControl.MouseMove -= OnPointerMove;
                    glControl.MouseUp -= OnPointerUp;
                    _input = INPUT.NONE;
                    OnSinglePanEnd();
                    break;

                case INPUT.TWO_FINGER:

                    //doubleEnd
                    OnDoublePanEnd(e);
                    OnPinchEnd(e);
                    OnRotateEnd(e);

                    //switching to singleStart
                    _input = INPUT.ONE_FINGER_SWITCHED;

                    break;

                case INPUT.MULT_FINGER:

                    if (_touchCurrent.Count == 0)
                    {
                        glControl.MouseMove -= OnPointerMove;
                        glControl.MouseUp -= OnPointerUp;

                        //multCancel
                        _input = INPUT.NONE;
                        OnTriplePanEnd();
                    }

                    break;
            }
        }
        else if (_input == INPUT.CURSOR)
        {
            glControl.MouseMove -= OnPointerMove;
            glControl.MouseUp -= OnPointerUp;


            _input = INPUT.NONE;
            OnSinglePanEnd();
            _button = "NONE";
        }


        if (_downValid)
        {
            float downTime = 0;
            if (_downEventsTime.Count > 0)
                downTime = stopWatch.ElapsedMilliseconds - _downEventsTime[_downEventsTime.Count - 1];

            if (downTime <= _maxDownTime)
            {
                if (_nclicks == 0)
                {
                    //first valid click detected
                    _nclicks = 1;
                    _clickStart = stopWatch.ElapsedMilliseconds;
                }
                else
                {
                    var clickInterval = stopWatch.ElapsedMilliseconds - _clickStart;
                    float movement = 0;
                    if (_downEvents.Count > 2)
                        movement = CalculatePointersDistance(_downEvents[1], _downEvents[0]) * _devPxRatio;

                    if (clickInterval <= _maxInterval && movement <= _posThreshold)
                    {
                        //second valid click detected
                        //fire double tap and reset values
                        _nclicks = 0;
                        _downEvents.Clear(); // splice(0, this._downEvents.length);
                        //this.OnDoubleTap(e);
                    }
                    else
                    {
                        //new 'first click'
                        _nclicks = 1;
                        _downEvents.RemoveAt(0);
                        _clickStart = stopWatch.ElapsedMilliseconds;
                    }
                }
            }
            else
            {
                _downValid = false;
                _nclicks = 0;
                _downEvents.Clear(); // splice(0, this._downEvents.length);
            }
        }
        else
        {
            _nclicks = 0;
            _downEvents.Clear(); // splice(0, this._downEvents.length);
        }
    }

    private void OnDoubleTap(MouseEventArgs e)
    {
        if (Enabled && EnablePan && scene != null)
        {
            if (StartEvent != null) StartEvent();

            SetCenter(e.X, e.Y);
            var hitP = UnprojectOnObj(GetCursorNDC(_center.x, _center.y), camera);

            if (hitP != null && enableAnimations)
            {
                //const self = this;
                //if ( this._animationId != - 1 ) {

                //window.cancelAnimationFrame(this._animationId);

                //}

                //this._timeStart = - 1;
                //this._animationId = window.requestAnimationFrame(function (t ) {

                //	self.updateTbState(STATE.ANIMATION_FOCUS, true );
                //	self.onFocusAnim(t, hitP, self._cameraMatrixState, self._gizmoMatrixState);

                //} );
            }
            else if (hitP != null && !enableAnimations)
            {
                UpdateTbState(STATE.FOCUS, true);
                Focus(hitP, ScaleFactor);
                UpdateTbState(STATE.IDLE, false);
                if (ChangeEvent != null) ChangeEvent();
            }
        }

        if (EndEvent != null) EndEvent();
    }

    private void OnTriplePanEnd()
    {
        UpdateTbState(STATE.IDLE, false);
        if (EndEvent != null) EndEvent();
    }

    private void OnRotateEnd(MouseEventArgs e)
    {
        UpdateTbState(STATE.IDLE, false);
        ActivateGizmos(false);
        if (EndEvent != null) EndEvent();
    }

    private void OnPinchEnd(MouseEventArgs e)
    {
        UpdateTbState(STATE.IDLE, false);
        if (EndEvent != null) EndEvent();
    }

    private void OnDoublePanEnd(MouseEventArgs e)
    {
        UpdateTbState(STATE.IDLE, false);
        if (EndEvent != null) EndEvent();
    }

    private void OnPointerDown(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButton.Left)
        {
            _downValid = true;

            _downEvents.Add(e);

            _downStart = stopWatch.ElapsedMilliseconds;
        }
        else
        {
            _downValid = false;
        }

        if (_input != INPUT.CURSOR && e.Button != MouseButton.Right && e.Button != MouseButton.Middle)
        {
            _touchStart.Add(e);
            _touchCurrent.Add(e);

            switch (_input)
            {
                case INPUT.NONE:
                    // single start
                    _input = INPUT.ONE_FINGER;
                    OnSinglePanStart(e, "ROTATE");
                    glControl.MouseMove += OnPointerMove;
                    glControl.MouseUp += OnPointerUp;
                    break;
                case INPUT.ONE_FINGER:
                case INPUT.ONE_FINGER_SWITCHED:
                    //doubleStart
                    _input = INPUT.TWO_FINGER;
                    OnRotateStart();
                    OnPinchStart();
                    OnDoublePanStart();
                    break;

                case INPUT.TWO_FINGER:
                    //multipleStart
                    _input = INPUT.MULT_FINGER;
                    OnTriplePanStart();
                    break;
            }
        }
        else if (_input == INPUT.NONE)
        {
            string modifier = null;

            if (controlKey)
                modifier = "CTRL";
            else if (shiftKey) modifier = "SHIFT";

            _mouseOp = GetOpFromAction(GetMouseButtonToString(e), modifier);
            if (_mouseOp != null)
            {
                glControl.MouseMove += OnPointerMove;
                glControl.MouseUp += OnPointerUp;

                //singleStart
                _input = INPUT.CURSOR;
                _button = GetMouseButtonToString(e);
                OnSinglePanStart(e, _mouseOp);
            }
        }
    }

    private void OnRotateStart()
    {
        if (Enabled && EnableRotate)
        {
            if (StartEvent != null) StartEvent();

            UpdateTbState(STATE.ZROTATE, true);

            //this._startFingerRotation = event.rotation;

            _startFingerRotation =
                GetAngle(_touchCurrent[1], _touchCurrent[0]) + GetAngle(_touchStart[1], _touchStart[0]);
            _currentFingerRotation = _startFingerRotation;

            camera.GetWorldDirection(_rotationAxis); //rotation axis

            if (!EnablePan && !EnableZoom) ActivateGizmos(true);
        }
    }

    private void OnPinchStart()
    {
        if (Enabled && EnableZoom)
        {
            if (StartEvent != null) StartEvent();
            UpdateTbState(STATE.SCALE, true);

            _startFingerDistance = CalculatePointersDistance(_touchCurrent[0], _touchCurrent[1]);
            _currentFingerDistance = _startFingerDistance;

            ActivateGizmos(false);
        }
    }

    private void OnDoublePanStart()
    {
        if (Enabled && EnablePan)
        {
            if (StartEvent != null) StartEvent();

            UpdateTbState(STATE.PAN, true);

            SetCenter((_touchCurrent[0].X + _touchCurrent[1].X) / 2, (_touchCurrent[0].Y + _touchCurrent[1].Y) / 2);
            _startCursorPosition.Copy(UnprojectOnTbPlane(camera, _center.x, _center.y, true));
            _currentCursorPosition.Copy(_startCursorPosition);

            ActivateGizmos(false);
        }
    }

    private void OnTriplePanStart()
    {
        if (Enabled && EnableZoom)
        {
            if (StartEvent != null) StartEvent();

            UpdateTbState(STATE.SCALE, true);

            //const center = event.center;
            var clientX = 0;
            var clientY = 0;
            var nFingers = _touchCurrent.Count;

            for (var i = 0; i < nFingers; i++)
            {
                clientX += _touchCurrent[i].X;
                clientY += _touchCurrent[i].Y;
            }

            SetCenter(clientX / nFingers, clientY / nFingers);

            _startCursorPosition.SetY(GetCursorNDC(_center.x, _center.y).Y * 0.5f);
            _currentCursorPosition.Copy(_startCursorPosition);
        }
    }

    private string GetMouseButtonToString(MouseEventArgs e)
    {
        switch (e.Button)
        {
            case MouseButton.Left:
                return "LEFT";
            case MouseButton.Middle:
                return "MIDDLE";
            case MouseButton.Right:
                return "RIGHT";
            default:
                return "NONE";
        }
    }

    private string GetOpFromAction(string mouse, string key)
    {
        MouseAction action;

        for (var i = 0; i < mouseActions.Count; i++)
        {
            action = mouseActions[i];
            if (action.mouse == mouse && action.key == key) return action.operation;
        }

        if (key != null)
            for (var i = 0; i < mouseActions.Count; i++)
            {
                action = mouseActions[i];
                if (action.mouse == mouse && action.key == null) return action.operation;
            }

        return null;
    }

    private void OnWheel(object sender, MouseEventArgs e)
    {
        if (Enabled && EnableZoom)
        {
            string modifier = null;

            if (controlKey)
                modifier = "CTRL";
            else if (shiftKey) modifier = "SHIFT";

            var mouseOp = GetOpFromAction("WHEEL", modifier);

            if (mouseOp != null)
            {
                if (StartEvent != null) StartEvent();

                float notchDeltaY = 125; //distance of one notch of mouse wheel
                var sgn = e.Delta / notchDeltaY;

                float size = 1;

                if (sgn > 0)
                    size = 1 / ScaleFactor;
                else if (sgn < 0) size = ScaleFactor;

                switch (mouseOp)
                {
                    case "ZOOM":

                        UpdateTbState(STATE.SCALE, true);

                        if (sgn > 0)
                            size = 1 / (float)Math.Pow(ScaleFactor, sgn);
                        else if (sgn < 0) size = (float)Math.Pow(ScaleFactor, -sgn);

                        if (CursorZoom && EnablePan)
                        {
                            var scalePoint = new Vector3();

                            if (camera is OrthographicCamera)
                                scalePoint = UnprojectOnTbPlane(camera, e.X, e.Y).ApplyQuaternion(camera.Quaternion)
                                    .MultiplyScalar(1 / camera.Zoom).Add(_gizmos.Position);
                            else if (camera is PerspectiveCamera)
                                scalePoint = UnprojectOnTbPlane(camera, e.X, e.Y).ApplyQuaternion(camera.Quaternion)
                                    .Add(_gizmos.Position);

                            ApplyTransformMatrix(Scale(size, scalePoint));
                        }
                        else
                        {
                            ApplyTransformMatrix(Scale(size, _gizmos.Position));
                        }

                        if (_grid != null)
                        {
                            DisposeGrid();
                            DrawGrid();
                        }

                        UpdateTbState(STATE.IDLE, false);

                        if (ChangeEvent != null) ChangeEvent();
                        if (EndEvent != null) EndEvent();

                        break;

                    case "FOV":

                        if (camera is PerspectiveCamera)
                        {
                            UpdateTbState(STATE.FOV, true);


                            //Vertigo effect

                            //	  fov / 2
                            //		|\
                            //		| \
                            //		|  \
                            //	x	|	\
                            //		| 	 \
                            //		| 	  \
                            //		| _ _ _\
                            //			y

                            //check for iOs shift shortcut
                            if (e.Delta != 0)
                            {
                                sgn = e.Delta / notchDeltaY;

                                size = 1;

                                if (sgn > 0)
                                    size = 1 / (float)Math.Pow(ScaleFactor, sgn);
                                else if (sgn < 0) size = (float)Math.Pow(ScaleFactor, -sgn);
                            }

                            _v3_1.SetFromMatrixPosition(_cameraMatrixState);
                            var x = _v3_1.DistanceTo(_gizmos.Position);
                            var xNew = x /
                                       size; //distance between camera and gizmos if scale(size, scalepoint) would be performed

                            //check min and max distance
                            xNew = MathUtils.Clamp(xNew, MinDistance, MaxDistance);

                            var y = x * (float)Math.Tan(MathUtils.DEG2RAD * camera.Fov * 0.5f);

                            //calculate new fov
                            var newFov = MathUtils.RAD2DEG * (float)(Math.Atan(y / xNew) * 2);

                            //check min and max fov
                            if (newFov > maxFov)
                                newFov = maxFov;
                            else if (newFov < minFov) newFov = minFov;

                            var newDistance = y / (float)Math.Tan(MathUtils.DEG2RAD * (newFov / 2));
                            size = x / newDistance;

                            SetFov(newFov);
                            ApplyTransformMatrix(Scale(size, _gizmos.Position, false));
                        }

                        if (_grid != null)
                        {
                            DisposeGrid();
                            DrawGrid();
                        }

                        UpdateTbState(STATE.IDLE, false);

                        if (ChangeEvent != null) ChangeEvent();
                        if (EndEvent != null) EndEvent();

                        break;
                }
            }
        }
    }

    #region member

    public enum STATE
    {
        IDLE,
        ROTATE,
        PAN,
        SCALE,
        FOV,
        FOCUS,
        ZROTATE,
        TOUCH_MULTI,
        ANIMATION_FOCUS,
        ANIMATION_ROTATE,
        NONE
    }

    public enum INPUT
    {
        NONE,
        ONE_FINGER,
        ONE_FINGER_SWITCHED,
        TWO_FINGER,
        MULT_FINGER,
        CURSOR
    }

    public struct Center
    {
        public float x;
        public float y;
    }

    private Center _center;

    public class Transformation
    {
        public Matrix4 Camera = new();
        public Matrix4 gizmos = new();
    }

    private Transformation _transformation = new();

    private struct MouseAction
    {
        public string operation;
        public string mouse;
        public string key;
        public STATE state;
    }

    public Action ChangeEvent;
    public Action StartEvent;
    public Action EndEvent;

    private Raycaster _raycaster = new();
    private Vector3 _offset = new();

    private Matrix4 _gizmoMatrixStateTemp = new();
    private Matrix4 _cameraMatrixStateTemp = new();
    private Vector3 _scalePointTemp = new();

    private IControlsContainer glControl;
    private Camera camera;
    private Scene scene;

    private Vector3 target = new();
    private Vector3 _currentTarget = new();
    private float radiusFactor = 0.67f;

    public List<string> OperationInput = new() { "PAN", "ROTATE", "ZOOM", "FOV" };
    public List<string> MouseInput = new() { "LEFT", "MIDDLE", "RIGHT", "WHEEL" };
    public List<string> KeyInput = new() { "CTRL", "SHIFT", null };

    private List<MouseAction> mouseActions = new();

    private string _mouseOp;

    //global vectors and matrices that are used in some operations to avoid creating new objects every time (e.g. every time cursor moves)
    private Vector2 _v2_1 = new();
    private Vector3 _v3_1 = new();
    private Vector3 _v3_2 = new();

    private Matrix4 _m4_1 = new();
    private Matrix4 _m4_2 = new();

    private Quaternion _quat = new();

    //transformation matrices
    private Matrix4 _translationMatrix = new(); //matrix for translation operation
    private Matrix4 _rotationMatrix = new(); //matrix for rotation operation
    private Matrix4 _scaleMatrix = new(); //matrix for scaling operation

    private Vector3 _rotationAxis = new(); //axis for rotate operation


    //camera state
    private Matrix4 _cameraMatrixState = new();
    private Matrix4 _cameraProjectionState = new();

    private float _fovState = 1;
    private Vector3 _upState = new();
    private float _zoomState = 1;
    private float _nearPos;
    private float _farPos;

    private Matrix4 _gizmoMatrixState = new();

    //initial values
    private Vector3 _up0 = new();
    private float _zoom0 = 1;
    private float _fov0;
    private float _initialNear;
    private float _nearPos0;
    private float _initialFar;
    private float _farPos0;
    private Matrix4 _cameraMatrixState0 = new();
    private Matrix4 _gizmoMatrixState0 = new();

    //pointers array
    private string _button = "";
    private List<MouseEventArgs> _touchStart = new();
    private List<MouseEventArgs> _touchCurrent = new();
    private INPUT _input = INPUT.NONE;

    //two fingers touch interaction
    private float
        _switchSensibility =
            32; //minimum movement to be performed to fire single pan start after the second finger has been released

    private float _startFingerDistance; //distance between two fingers
    private float _currentFingerDistance;
    private float _startFingerRotation; //amount of rotation performed with two fingers
    private float _currentFingerRotation;

    //double tap
    private float _devPxRatio = 1;
    private bool _downValid = true;
    private float _nclicks;

    private List<MouseEventArgs> _downEvents = new();
    private List<long> _downEventsTime = new();
    private long _downStart; //pointerDown time
    private float _clickStart; //first click time
    private float _maxDownTime = 250;
    private float _maxInterval = 300;
    private float _posThreshold = 24;
    private float _movementThreshold = 24;

    //cursor positions
    private Vector3 _currentCursorPosition = new();
    private Vector3 _startCursorPosition = new();

    //grid
    private GridHelper _grid; //grid to be visualized during pan operation
    private Vector3 _gridPosition = new();

    //gizmos
    public Group _gizmos = new();
    private int _curvePts = 128;


    //animations
    private float _timeStart = -1; //initial time
    private int _animationId = -1;

    //focus animation
    private int focusAnimationTime = 500; //duration of focus animation in ms

    //rotate animation
    private long _timePrev; //time at which previous rotate operation has been detected
    private long _timeCurrent; //time at which current rotate operation has been detected
    private float _anglePrev; //angle of previous rotation
    private float _angleCurrent; //angle of current rotation
    private Vector3 _cursorPosPrev = new(); //cursor position when previous rotate operation has been detected
    private Vector3 _cursorPosCurr = new(); //cursor position when current rotate operation has been detected
    private float _wPrev; //angular velocity of the previous rotate operation
    private float _wCurr; //angular velocity of the current rotate operation


    //parameters
    public bool AdjustNearFar = false;
    public float ScaleFactor = 1.1f; //zoom/distance multiplier
    private float dampingFactor = 25;
    private float wMax = 20; //maximum angular velocity allowed
    private bool enableAnimations = true; //if animations should be performed
    public bool EnableGrid = false; //if grid should be showed during pan operation
    public bool CursorZoom = false; //if wheel zoom should be cursor centered
    private float minFov = 5;
    private float maxFov = 90;

    public bool Enabled = true;
    public bool EnablePan = true;
    public bool EnableRotate = true;
    public bool EnableZoom = true;
    public bool EnableGizmos = true;

    public float MinDistance = 0;
    public float MaxDistance = float.PositiveInfinity;
    public float MinZoom = 0;
    public float MaxZoom = float.PositiveInfinity;

    //trackball parameters
    private float _tbRadius = 1;

    protected readonly Stopwatch stopWatch = new();

    //FSA
    private STATE _state = STATE.IDLE;

    #endregion

    #region Dispose

    public event EventHandler<EventArgs> Disposed;

    public virtual void Dispose()
    {
        glControl.MouseDown -= OnPointerDown;
        glControl.MouseUp -= OnPointerCancel;
        glControl.MouseUp -= OnPointerUp;
        glControl.MouseMove -= OnPointerMove;
        glControl.SizeChanged -= Control_SizeChanged;
        glControl.MouseWheel -= OnWheel;
        DisposeGrid();
        stopWatch.Stop();
        Dispose(disposed);
    }

    protected virtual void RaiseDisposed()
    {
        var handler = Disposed;
        if (handler != null)
            handler(this, new EventArgs());
    }

    private bool disposed;


    protected virtual void Dispose(bool disposing)
    {
        if (disposed) return;
        try
        {
            RaiseDisposed();
            disposed = true;
        }
        finally
        {
            disposed = true;
        }
    }

    #endregion
}