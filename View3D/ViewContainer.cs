using System.Diagnostics;
using OpenTK.Windowing.Common;
using OpenTK.Wpf;
using THREE;
using Rectangle = THREE.Rectangle;

namespace View3D;

/// <summary>
/// ViewContainer is a base class for a viewer and scene.
/// It provides a camera, controls and a renderer, and outputs the rendered images to a GLWpfControl.
/// </summary>
[Serializable]
public abstract class ViewContainer : ControlsContainer
{
    private Raycaster _raycaster = new();
    private Vector2 _mouse = new(1, 1);

    /// <summary>
    /// GLControl that the View is currently bound to
    /// </summary>
    public GLWpfControl? GLControl;

    protected readonly Stopwatch Stopwatch = new();
    public Camera Camera = new PerspectiveCamera();
    public OrbitControls? CameraControls;

    public GLRenderer? Renderer;

    /// <summary>
    /// The Scene that the view is currently rendering.
    /// </summary>
    public Scene Scene = new();

    public List<Element> SceneElements = new();

    ~ViewContainer()
    {
        Dispose(false);
    }

    /// <summary>
    /// Adds an element to the Scene and SceneElements
    /// </summary>
    /// <param name="element">Element to be added</param>
    public void AddElement(Element element)
    {
        // Only allow unique elements to be added
        if (SceneElements.Contains(element)) return;

        SceneElements.Add(element);

        Scene.Add(element);
    }

    /// <summary>
    /// Ensures that all SceneElements are added to the Scene
    /// </summary>
    /// <param name="clearScene">true if all objects not in SceneElements are to be removed from the Scene</param>
    private void UpdateScene(bool clearScene = false)
    {
        if (clearScene)
        {
            // Remove objects that are not included in SceneElements
            var unusedObjects = Scene.Children.Where(object3D => !SceneElements.Contains(object3D)).ToList();
        }

        foreach (var element in SceneElements)
        {
            Scene.Add(element);
        }
    }

    public virtual void Load(GLWpfControl control)
    {
        GLControl = control;
        Renderer = new GLRenderer(control.Context, (int)control.RenderSize.Width,
            (int)control.RenderSize.Height);

        Initialize();

        Stopwatch.Start();
    }

    public override Rectangle GetClientRectangle()
    {
        if (Renderer == null) return new Rectangle(0, 0, 0, 0);
        return new Rectangle(0, 0, Renderer.Width, Renderer.Height);
    }

    public virtual void Initialize()
    {
        InitializeRenderer();
        InitializeCamera();
        InitializeControls();
        InitializeLighting();
        MouseMove += OnMouseMove;
    }

    public virtual void InitializeRenderer()
    {
        if (Renderer == null) return;

        Renderer.ShadowMap.Enabled = true;
        Renderer.ShadowMap.Type = Constants.PCFSoftShadowMap;
        Renderer?.SetClearColor(new Color().SetHex(0xEEEEEE));
    }

    public virtual void InitializeCamera()
    {
        if (GLControl == null) return;

        Camera.Aspect = (float)(GLControl.RenderSize.Width / GLControl.RenderSize.Height);
        Camera.Fov = 45.0f;
        Camera.Near = 0.1f;
        Camera.Far = 1000.0f;
        Camera.Position.X = -30;
        Camera.Position.Y = 40;
        Camera.Position.Z = 30;
        Camera.LookAt(Vector3.Zero());
    }

    public virtual void InitializeControls()
    {
        CameraControls = new OrbitControls(this, Camera);
        CameraControls.RotateSpeed = 1.5f;
        CameraControls.ZoomSpeed = 1.5f;
        CameraControls.PanSpeed = 1.5f;
        CameraControls.Update();
    }

    public virtual void InitializeLighting()
    {
    }

    public float GetDelta()
    {
        return Stopwatch.ElapsedMilliseconds / 1000.0f;
    }

    public override void OnResize(ResizeEventArgs clientSize)
    {
        if (Renderer == null || GLControl == null) return;

        Renderer.Resize(clientSize.Width, clientSize.Height);
        Camera.Aspect = (float)(GLControl.Width / GLControl.Height);
        Camera.UpdateProjectionMatrix();


        base.OnResize(clientSize);
    }

    private void OnMouseMove(object? sender, MouseEventArgs e)
    {
        _mouse.X = e.X * 1.0f / ClientRectangle.Width * 2 - 1.0f;
        _mouse.Y = -e.Y * 1.0f / ClientRectangle.Height * 2 + 1.0f;
    }

    /// <summary>
    /// Renders the Scene to the GLWpfControl
    /// </summary>
    public virtual void Render()
    {
        CameraControls?.Update();

        UpdateIntersections();

        Renderer?.Render(Scene, Camera);
    }

    /// <summary>
    /// Check whether the mouse is hovering over any objects in the Scene
    /// </summary>
    private void UpdateIntersections()
    {
        _raycaster.SetFromCamera(_mouse, Camera);

        var intersects = _raycaster.IntersectObjects(Scene.Children, true);

        foreach (var element in SceneElements.OfType<SelectableElement>())
        {
            element.Unhover();
        }

        if (intersects.Count <= 0) return;

        foreach (var intersect in intersects)
        {
            if (intersect.Object3D is SelectableElement element)
            {
                element.ActiveHover();
            }
        }
    }

    public virtual void Unload()
    {
        Renderer?.Dispose();
    }

    public override void Dispose()
    {
        OnDispose();
    }

    public virtual void OnDispose()
    {
        Unload();
    }
}