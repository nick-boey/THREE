namespace THREE;

[Serializable]
public class DragControls : IDisposable
{
    private Object3D _hovered;
    private Vector3 _intersection = new();

    private List<Intersection> _intersections = new();
    private Matrix4 _inverseMatrix = new();
    private Vector3 _offset = new();
    private Plane _plane = new();
    private Raycaster _raycaster = new();

    private Object3D _selected;
    private Vector3 _worldPosition = new();
    private Camera camera;
    public Action<Object3D> Drag;
    public Action<Object3D> DragEnd;
    public Action<Object3D> DragStart;

    public bool Enabled = true;

    private IControlsContainer glControl;

    public Action<Object3D> HoverOff;
    public Action<Object3D> HoverOn;

    private Vector2 mouse = new();
    public List<Object3D> objects;
    public bool TransformGroup = false;

    public DragControls(IControlsContainer glControl, List<Object3D> objects, Camera camera)
    {
        this.glControl = glControl;
        this.objects = objects;
        this.camera = camera;

        Activate();
    }

    public event EventHandler<EventArgs> Disposed;

    private void Activate()
    {
        glControl.MouseMove += OnPointerMove;
        glControl.MouseDown += OnPointerDown;
        glControl.MouseUp += OnPointerCancel;
        glControl.MouseLeave += OnPointerLeave;
    }

    public List<Object3D> GetObjects()
    {
        return objects;
    }

    private void OnPointerLeave(object sender, EventArgs e)
    {
        if (!Enabled) return;

        if (_selected != null)
        {
            if (DragEnd != null) DragEnd(_selected);

            _selected = null;
        }
        // if your application support Mouse Cursor like System.Windows.Form.Cursor
        //Cursor.Current = _hovered != null ? Cursors.Arrow : Cursors.Default;
    }

    private void OnPointerCancel(object sender, MouseEventArgs e)
    {
        if (!Enabled) return;

        if (_selected != null)
        {
            if (DragEnd != null) DragEnd(_selected);

            _selected = null;
        }

        //Cursor.Current = _hovered != null ? Cursors.Arrow : Cursors.Default;
    }

    private void OnPointerDown(object sender, MouseEventArgs e)
    {
        mouse.X = e.X * 1.0f / glControl.ClientRectangle.Width * 2 - 1.0f;
        mouse.Y = -e.Y * 1.0f / glControl.ClientRectangle.Height * 2 + 1.0f;

        _intersections.Clear();
        _raycaster.SetFromCamera(mouse, camera);
        _raycaster.IntersectObjects(objects, true, _intersections);

        if (_intersections.Count > 0)
        {
            _selected = TransformGroup ? objects[0] : _intersections[0].Object3D;

            _plane.SetFromNormalAndCoplanarPoint(camera.GetWorldDirection(_plane.Normal),
                _worldPosition.SetFromMatrixPosition(_selected.MatrixWorld));

            if (_raycaster.ray.IntersectPlane(_plane, _intersection) != null)
            {
                _inverseMatrix.Copy(_selected.Parent.MatrixWorld).Invert();
                _offset.Copy(_intersection).Sub(_worldPosition.SetFromMatrixPosition(_selected.MatrixWorld));
            }

            if (DragStart != null) DragStart(_selected);
        }
    }

    private void OnPointerMove(object sender, MouseEventArgs e)
    {
        if (Enabled == false) return;

        mouse.X = e.X * 1.0f / glControl.ClientRectangle.Width * 2 - 1.0f;
        mouse.Y = -e.Y * 1.0f / glControl.ClientRectangle.Height * 2 + 1.0f;

        _raycaster.SetFromCamera(mouse, camera);

        if (_selected != null)
        {
            if (_raycaster.ray.IntersectPlane(_plane, _intersection) != null)
                _selected.Position.Copy(_intersection.Sub(_offset).ApplyMatrix4(_inverseMatrix));
            if (Drag != null)
                Drag(_selected);
            return;
        }

        _intersections.Clear();
        _raycaster.SetFromCamera(mouse, camera);
        _raycaster.IntersectObjects(objects, true, _intersections);

        if (_intersections.Count > 0)
        {
            var object3d = _intersections[0].Object3D;

            _plane.SetFromNormalAndCoplanarPoint(camera.GetWorldDirection(_plane.Normal),
                _worldPosition.SetFromMatrixPosition(object3d.MatrixWorld));

            if (_hovered != null && !object3d.Equals(_hovered))
            {
                if (HoverOff != null) HoverOff(_hovered);

                _hovered = null;
            }

            if (!object3d.Equals(_hovered))
            {
                if (HoverOn != null) HoverOn(object3d);

                _hovered = object3d;
            }
        }
        else
        {
            if (_hovered != null)
            {
                if (HoverOff != null) HoverOff(_hovered);
                _hovered = null;
            }
        }
    }

    private void Deactivate()
    {
        glControl.MouseMove -= OnPointerMove;
        glControl.MouseDown -= OnPointerDown;
        glControl.MouseUp -= OnPointerCancel;
        glControl.MouseLeave -= OnPointerLeave;
    }

    #region Dispose

    public virtual void Dispose()
    {
        Deactivate();
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