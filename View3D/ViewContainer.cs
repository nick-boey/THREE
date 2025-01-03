﻿using System.Diagnostics;
using OpenTK.Windowing.Common;
using OpenTK.Wpf;
using THREE;
using Rectangle = THREE.Rectangle;

namespace View3D;

[Serializable]
public abstract class ViewContainer : ControlsContainer
{
    protected readonly Stopwatch Stopwatch = new();
    public Camera Camera = new PerspectiveCamera();

    public OrbitControls Controls;

    public GLWpfControl? GLControl;
    public GLRenderer? Renderer;
    public Scene Scene = new();

    ~ViewContainer()
    {
        Dispose(false);
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
        Controls = new OrbitControls(this, Camera);
        Controls.RotateSpeed = 1.5f;
        Controls.ZoomSpeed = 1.5f;
        Controls.PanSpeed = 1.5f;
        Controls.Update();
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

    public virtual void Render()
    {
        Controls?.Update();
        Renderer?.Render(Scene, Camera);
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