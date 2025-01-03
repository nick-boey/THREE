using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using static THREE.Constants;

namespace THREE;

[Serializable]
public class OrbitControls : IDisposable
{
    public enum STATE
    {
        NONE = -1,
        ROTATE = 0,
        DOLLY = 1,
        PAN = 2,
        TOUCH_ROTATE = 3,
        TOUCH_PAN = 4,
        TOUCH_DOLLY_PAN = 5,
        DOUCH_DOLLY_ROTATE = 6
    }

    public bool AutoRotate = false;
    public float AutoRotateSpeed = 2.0f;

    //public int mouseButtons;

    private Camera camera;

    private IControlsContainer control;

    public Dictionary<MouseButton, MOUSE> ControlMouseButtons = new()
    {
        { MouseButton.Left, MOUSE.ROTATE },
        { MouseButton.Middle, MOUSE.DOLLY },
        { MouseButton.Right, MOUSE.PAN }
    };

    public float DampingFactor = 0.05f;
    private Vector2 dollyDelta = Vector2.Zero();
    private Vector2 dollyEnd = Vector2.Zero();

    private Vector2 dollyStart = Vector2.Zero();

    public bool Enabled = true;

    public bool EnableDamping = false;

    public bool EnableKeys = true;

    public bool EnablePan = true;

    public bool EnableRotate = true;

    public bool EnableZoom = true;
    private float EPS = 0.000001f;
    public float KeyPanSpeed = 7.0f;
    public float MaxAzimuthAngle = float.PositiveInfinity;
    public float MaxDistance = float.PositiveInfinity;
    public float MaxPolarAngle = (float)Math.PI;
    public float MaxZoom = float.PositiveInfinity;

    public float MinAzimuthAngle = float.NegativeInfinity;


    public float MinDistance = 0;

    public float MinPolarAngle = 0;

    public float MinZoom = 0;
    private Vector2 panDelta = Vector2.Zero();
    private Vector2 panEnd = Vector2.Zero();
    private Vector3 panOffset = Vector3.Zero();
    public float PanSpeed = 1.0f;

    private Vector2 panStart = Vector2.Zero();
    private Vector3 position0;
    private Vector2 rotateDelta = Vector2.Zero();
    private Vector2 rotateEnd = Vector2.Zero();
    public float RotateSpeed = 1.0f;

    private Vector2 rotateStart = Vector2.Zero();
    private float scale = 1;

    public bool ScreenSpacePanning = true;
    private Spherical spherical = new();
    private Spherical sphericalDelta = new();

    private STATE state = STATE.NONE;

    public Vector3 target = new();

    private Vector3 target0;
    private float zoom0;
    private bool zoomChanged;
    public float ZoomSpeed = 1.0f;

    public OrbitControls(IControlsContainer control, Camera camera)
    {
        this.camera = camera;
        this.control = control;

        this.control.MouseDown += OnPointerDown;
        ;
        //this.control.MouseMove += Control_MouseMove;
        //this.control.MouseUp += Control_MouseUp;
        this.control.MouseWheel += Control_MouseWheel;
        this.control.KeyDown += Control_KeyDown;


        target0 = target.Clone();
        position0 = camera.Position.Clone();
        zoom0 = camera.Zoom;
    }

    public float GetAutoRotationAngle()
    {
        return 2 * (float)Math.PI / 60 / 60 * AutoRotateSpeed;
    }

    public float GetZoomScale()
    {
        return (float)Math.Pow(0.95f, ZoomSpeed);
    }

    public void RotateLeft(float angle)
    {
        sphericalDelta.Theta -= angle;
    }

    public void RotateUp(float angle)
    {
        sphericalDelta.Phi -= angle;
    }

    public void PanLeft(float distance, Matrix4 objectMatrix)
    {
        var v = Vector3.Zero();
        v.SetFromMatrixColumn(objectMatrix, 0); // get X column of objectMatrix
        v.MultiplyScalar(-distance);

        panOffset.Add(v);
    }

    public void PanUp(float distance, Matrix4 objectMatrix)
    {
        var v = Vector3.Zero();

        if (ScreenSpacePanning)
        {
            v.SetFromMatrixColumn(objectMatrix, 1);
        }
        else
        {
            v.SetFromMatrixColumn(objectMatrix, 0);
            v.CrossVectors(camera.Up, v);
        }

        v.MultiplyScalar(distance);

        panOffset.Add(v);
    }

    public void Pan(float deltaX, float deltaY)
    {
        var offset = Vector3.Zero();
        if (camera is PerspectiveCamera)
        {
            // perspective
            var position = camera.Position;
            offset.Copy(position).Sub(target);
            var targetDistance = offset.Length();

            // half of the fov is center to top of screen
            targetDistance *= (float)Math.Tan(camera.Fov / 2 * Math.PI / 180.0);

            // we use only clientHeight here so aspect ratio does not distort speed
            PanLeft(2 * deltaX * targetDistance / control.ClientRectangle.Height, camera.Matrix);
            PanUp(2 * deltaY * targetDistance / control.ClientRectangle.Height, camera.Matrix);
        }
        else if (camera is OrthographicCamera)
        {
            var ocamera = camera as OrthographicCamera;
            // orthographic
            PanLeft(deltaX * (ocamera.CameraRight - ocamera.Left) / camera.Zoom / control.ClientRectangle.Width,
                camera.Matrix);
            PanUp(deltaY * (ocamera.Top - ocamera.Bottom) / camera.Zoom / control.ClientRectangle.Height,
                camera.Matrix);
        }
        else
        {
            // camera neither orthographic nor perspective
            //console.warn('WARNING: OrbitControls.js encountered an unknown camera type - pan disabled.');
            EnablePan = false;
        }
    }

    public void DollyOut(float dollyScale)
    {
        if (camera is PerspectiveCamera)
        {
            scale /= dollyScale;
        }
        else if (camera is OrthographicCamera)
        {
            camera.Zoom = Math.Max(MinZoom, Math.Min(MaxZoom, camera.Zoom * dollyScale));
            camera.UpdateProjectionMatrix();
            zoomChanged = true;
        }
        else
        {
            //console.warn('WARNING: OrbitControls.js encountered an unknown camera type - dolly/zoom disabled.');
            EnableZoom = false;
        }
    }

    public void DollyIn(float dollyScale)
    {
        if (camera is PerspectiveCamera)
        {
            scale *= dollyScale;
        }
        else if (camera is OrthographicCamera)
        {
            camera.Zoom = Math.Max(MinZoom, Math.Min(MaxZoom, camera.Zoom / dollyScale));
            camera.UpdateProjectionMatrix();
            zoomChanged = true;
        }
        else
        {
            //console.warn('WARNING: OrbitControls.js encountered an unknown camera type - dolly/zoom disabled.');
            EnableZoom = false;
        }
    }

    private void handleMouseDownRotate(MouseEventArgs e)
    {
        rotateStart.Set(e.X, e.Y);
    }

    private void handleMouseDownDolly(MouseEventArgs e)
    {
        dollyStart.Set(e.X, e.Y);
    }

    private void handleMouseDownPan(MouseEventArgs e)
    {
        panStart.Set(e.X, e.Y);
    }

    private void handleMouseMoveRotate(MouseEventArgs e)
    {
        rotateEnd.Set(e.X, e.Y);
        rotateDelta.SubVectors(rotateEnd, rotateStart).MultiplyScalar(RotateSpeed);


        RotateLeft(2 * (float)Math.PI * rotateDelta.X / control.ClientRectangle.Height);
        RotateUp(2 * (float)Math.PI * rotateDelta.Y / control.ClientRectangle.Height);

        rotateStart.Copy(rotateEnd);

        Update();
    }

    private void handleMouseMoveDolly(MouseEventArgs e)
    {
        dollyEnd.Set(e.X, e.Y);
        dollyDelta.SubVectors(dollyEnd, dollyStart);

        if (dollyDelta.Y > 0)
            DollyOut(GetZoomScale());
        else if (dollyDelta.Y < 0) DollyIn(GetZoomScale());

        dollyStart.Copy(dollyEnd);

        Update();
    }

    private void handleMouseMovePan(MouseEventArgs e)
    {
        panEnd.Set(e.X, e.Y);
        panDelta.SubVectors(panEnd, panStart).MultiplyScalar(PanSpeed);

        Pan(panDelta.X, panDelta.Y);

        panStart.Copy(panEnd);

        Update();
    }

    private void handleMouseUp(MouseEventArgs e)
    {
        state = STATE.NONE;
    }

    private void handleMouseWheel(MouseEventArgs e)
    {
        if (e.Delta < 0)
            DollyIn(GetZoomScale());
        else if (e.Delta > 0) DollyOut(GetZoomScale());

        Update();
    }

    private void handleKeyDown(Keys keyCode)
    {
        var needsUpdate = false;

        switch (keyCode)
        {
            case Keys.Up:
                Pan(0, KeyPanSpeed);
                needsUpdate = true;
                break;
            case Keys.Down:
                Pan(0, -KeyPanSpeed);
                needsUpdate = true;
                break;
            case Keys.Left:
                Pan(KeyPanSpeed, 0);
                needsUpdate = true;
                break;
            case Keys.Right:
                Pan(-KeyPanSpeed, 0);
                needsUpdate = true;
                break;
        }

        if (needsUpdate) Update();
    }
    //private void handleTouchStartRotate()
    //private void handleTouchStartPan()
    //private void handleTouchStartDolly()
    //private void handleTouchStartDollyPen()
    //private void handleTouchStartDollyRotate()
    //private void handleTouchMoveRotate()
    //private void handleTouchMovePan();
    //private void handleTouchMoveDolly();
    //private void handleTouchMoveDollyPan();
    //private void handleTouchMoveDollyRotate();
    //private void handleTouchEnd();

    public float GetAzimuthalAngle()
    {
        return spherical.Theta;
    }

    public void SaveState()
    {
        target0.Copy(target);
        position0.Copy(camera.Position);
        zoom0 = camera.Zoom;
    }

    public void Reset()
    {
        target.Copy(target0);
        camera.Position.Copy(position0);
        camera.Zoom = zoom0;

        camera.UpdateProjectionMatrix();

        Update();

        state = STATE.NONE;
    }

    public bool Update()
    {
        var offset = new Vector3();

        var quat = new Quaternion().SetFromUnitVectors(camera.Up, new Vector3(0, 1, 0));
        var quatInverse = (quat.Clone() as Quaternion).Invert();

        var lastPosition = new Vector3();
        var lastQuaternion = new Quaternion();

        var twoPI = 2 * (float)Math.PI;

        var position = camera.Position;

        offset.Copy(position).Sub(target);

        // rotate offset to "y-axis-is-up" space
        offset.ApplyQuaternion(quat);

        // angle from z-axis around y-axis
        spherical.SetFromVector3(offset);

        if (AutoRotate && state == STATE.NONE) RotateLeft(GetAutoRotationAngle());

        if (EnableDamping)
        {
            spherical.Theta += sphericalDelta.Theta * DampingFactor;
            spherical.Phi += sphericalDelta.Phi * DampingFactor;
        }
        else
        {
            spherical.Theta += sphericalDelta.Theta;
            spherical.Phi += sphericalDelta.Phi;
        }

        // restrict theta to be between desired limits

        var min = MinAzimuthAngle;
        var max = MaxAzimuthAngle;

        if (!float.IsInfinity(min) && !float.IsInfinity(max))
        {
            if (min < -Math.PI) min += twoPI;
            else if (min > Math.PI) min -= twoPI;

            if (max < -Math.PI) max += twoPI;
            else if (max > Math.PI) max -= twoPI;

            if (min < max)
                spherical.Theta = Math.Max(min, Math.Min(max, spherical.Theta));
            else
                spherical.Theta = spherical.Theta > (min + max) / 2
                    ? Math.Max(min, spherical.Theta)
                    : Math.Min(max, spherical.Theta);
        }

        // restrict phi to be between desired limits
        spherical.Phi = Math.Max(MinPolarAngle, Math.Min(MaxPolarAngle, spherical.Phi));

        spherical.makeSafe();


        spherical.Radius *= scale;

        // restrict radius to be between desired limits
        spherical.Radius = Math.Max(MinDistance, Math.Min(MaxDistance, spherical.Radius));

        // move target to panned location

        if (EnableDamping)
            target.AddScaledVector(panOffset, DampingFactor);
        else
            target.Add(panOffset);

        offset.SetFromSpherical(spherical);

        // rotate offset back to "camera-up-vector-is-up" space
        offset.ApplyQuaternion(quatInverse);

        position.Copy(target).Add(offset);

        camera.LookAt(target);

        if (EnableDamping)
        {
            sphericalDelta.Theta *= 1 - DampingFactor;
            sphericalDelta.Phi *= 1 - DampingFactor;

            panOffset.MultiplyScalar(1 - DampingFactor);
        }
        else
        {
            sphericalDelta.Set(0, 0, 0);

            panOffset.Set(0, 0, 0);
        }

        scale = 1;

        // update condition is:
        // min(camera displacement, camera rotation in radians)^2 > EPS
        // using small-angle approximation cos(x/2) = 1 - x^2 / 8

        if (zoomChanged ||
            lastPosition.DistanceToSquared(camera.Position) > EPS ||
            8 * (1 - lastQuaternion.Dot(camera.Quaternion)) > EPS)
        {
            lastPosition.Copy(camera.Position);
            lastQuaternion.Copy(camera.Quaternion);
            zoomChanged = false;

            return true;
        }

        return false;
    }


    private void Control_KeyDown(object sender, KeyboardKeyEventArgs e)
    {
    }

    private void Control_MouseWheel(object sender, MouseEventArgs e)
    {
        if (Enabled == false || EnableZoom == false || (state != STATE.NONE && state != STATE.ROTATE)) return;

        handleMouseWheel(e);
    }

    private void Control_MouseUp(object sender, MouseEventArgs e)
    {
        if (Enabled == false) return;

        handleMouseUp(e);

        control.MouseMove -= onPointerMove;
        control.MouseUp -= onPointerUp;
    }

    private void Control_MouseMove(object sender, MouseEventArgs e)
    {
        if (Enabled == false) return;

        switch (state)
        {
            case STATE.ROTATE:
                if (EnableRotate == false) return;

                handleMouseMoveRotate(e);

                break;
            case STATE.DOLLY:
                if (EnableZoom == false) return;
                handleMouseMoveDolly(e);
                break;
            case STATE.PAN:
                if (EnablePan == false) return;
                handleMouseMovePan(e);
                break;
        }
    }

    private void Control_MouseDown(object sender, MouseEventArgs e)
    {
        if (Enabled == false) return;
        int mouseAction;

        switch (e.Button)
        {
            case MouseButton.Left:
                mouseAction = (int)ControlMouseButtons[MouseButton.Left];
                break;
            case MouseButton.Middle:
                mouseAction = (int)ControlMouseButtons[MouseButton.Middle];
                break;
            case MouseButton.Right:
                mouseAction = (int)ControlMouseButtons[MouseButton.Right];
                break;
            default:
                mouseAction = -1;
                break;
        }

        var action = (MOUSE)Enum.ToObject(typeof(MOUSE), mouseAction);
        switch (action)
        {
            case MOUSE.DOLLY:
                if (EnableZoom == false) return;

                handleMouseDownDolly(e);

                state = STATE.DOLLY;

                break;

            case MOUSE.ROTATE:
                //if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) || Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                //{
                //    if (EnablePan == false) return;

                //    handleMouseDownPan(e);

                //    state = STATE.PAN;
                //}
                //else
                //{
                if (EnableRotate == false) return;

                handleMouseDownRotate(e);

                state = STATE.ROTATE;
                //}
                break;

            case MOUSE.PAN:
                //if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) || Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                //{
                //    if (EnableRotate == false) return;

                //    handleMouseDownRotate(e);

                //    state = STATE.ROTATE;
                //}
                //else
                //{
                if (EnablePan == false) return;

                handleMouseDownPan(e);

                state = STATE.PAN;
                //}
                break;
            default:
                state = STATE.NONE;
                break;
        }

        if (state != STATE.NONE)
        {
            control.MouseMove += onPointerMove;
            control.MouseUp += onPointerUp;
        }
    }

    private void onPointerMove(object sender, MouseEventArgs e)
    {
        Control_MouseMove(sender, e);
    }

    private void onPointerUp(object sender, MouseEventArgs e)
    {
        Control_MouseUp(sender, e);
    }

    private void OnPointerDown(object sender, MouseEventArgs e)
    {
        Control_MouseDown(sender, e);
    }

    #region Dispose

    public event EventHandler<EventArgs> Disposed;

    public virtual void Dispose()
    {
        control.MouseDown -= OnPointerDown;
        //this.control.MouseMove -= Control_MouseMove;
        //this.control.MouseUp -= Control_MouseUp;
        control.MouseWheel -= Control_MouseWheel;
        control.KeyDown -= Control_KeyDown;

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