using System;
using System.Diagnostics;
using ImGuiNET;
using OpenTK.Windowing.Common;
using THREE;
using THREEExample.ThreeImGui;
using Rectangle = THREE.Rectangle;

namespace THREEExample;

[Serializable]
public abstract class Example : ControlsContainer
{
    public GLRenderer renderer;
    public Camera camera;
    public Scene scene;
    public TrackballControls controls;
    protected readonly Random random = new();

    public ImGuiManager imGuiManager { get; set; }

    protected readonly Stopwatch stopWatch = new();
#if WSL
        public IThreeWindow glControl;
#else
    public GLControl glControl;
#endif

    public Action AddGuiControlsAction;

    public Example()
    {
        camera = new PerspectiveCamera();
        scene = new Scene();
    }

    ~Example()
    {
        Dispose(false);
    }
#if WSL
        public virtual void Load(IThreeWindow control)
#else
    public virtual void Load(GLControl control)
#endif
    {
        Debug.Assert(null != control);

        glControl = control;
        renderer = new GLRenderer();

        renderer.Context = control.Context;
        renderer.Width = control.Width;
        renderer.Height = control.Height;

        renderer.Init();

        Init();

        stopWatch.Start();
    }

    public override Rectangle GetClientRectangle()
    {
        return new Rectangle(0, 0, renderer.Width, renderer.Height);
    }

    public virtual void InitCamera()
    {
        camera.Fov = 45.0f;
        camera.Aspect = glControl.AspectRatio;
        camera.Near = 0.1f;
        camera.Far = 1000.0f;
        camera.Position.X = -30;
        camera.Position.Y = 40;
        camera.Position.Z = 30;
        camera.LookAt(Vector3.Zero());
    }

    public virtual void InitRenderer()
    {
        renderer.SetClearColor(new Color().SetHex(0xEEEEEE));
        renderer.ShadowMap.Enabled = true;
        renderer.ShadowMap.Type = Constants.PCFSoftShadowMap;
    }

    public virtual void InitCameraController()
    {
        controls = new TrackballControls(this, camera);
        controls.StaticMoving = false;
        controls.RotateSpeed = 4.0f;
        controls.ZoomSpeed = 3;
        controls.PanSpeed = 3;
        controls.NoZoom = false;
        controls.NoPan = false;
        controls.NoRotate = false;
        controls.StaticMoving = true;
        controls.DynamicDampingFactor = 0.3f;
        controls.Update();
    }

    public virtual void InitLighting()
    {
    }

    public virtual void Init()
    {
        InitRenderer();

        InitCamera();

        InitCameraController();

        InitLighting();
    }

    public float GetDelta()
    {
        return stopWatch.ElapsedMilliseconds / 1000.0f;
    }

    public virtual void OnResize(ResizeEventArgs clientSize)
    {
        if (renderer != null)
        {
            renderer.Resize(clientSize.Width, clientSize.Height);
            camera.Aspect = glControl.AspectRatio;
            camera.UpdateProjectionMatrix();
        }

        base.OnResize(clientSize);
    }

    public virtual void Render()
    {
        if (!imGuiManager.ImWantMouse)
            controls.Enabled = true;
        else
            controls.Enabled = false;

        controls?.Update();
        renderer?.Render(scene, camera);
    }

    public virtual void ShowGUIControls()
    {
        if (AddGuiControlsAction != null)
        {
            ImGui.Begin("Controls");

            AddGuiControlsAction();

            ImGui.End();
        }
    }

    public virtual void Unload()
    {
        renderer.Dispose();
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