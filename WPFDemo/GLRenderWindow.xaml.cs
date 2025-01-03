using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using THREEExample;
using MouseButton = OpenTK.Windowing.GraphicsLibraryFramework.MouseButton;
using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;
using Size = System.Drawing.Size;
using UserControl = System.Windows.Controls.UserControl;

namespace WPFDemo;

/// <summary>
///     UserExample.xaml에 대한 상호 작용 논리
/// </summary>
public partial class GLRenderWindow : UserControl, IDisposable
{
    public Example currentExample;


    private bool disposed;


    public GLRenderWindow()
    {
        InitializeComponent();
        //if (!IsInDesignerMode)
        //{
        //    this.renderer = new GLRenderControl();
        //    this.GLHost.Child = renderer;
        //    //this.example = new GLRenderer();
        //    //this.example.parentWindow = parentWindow;
        //    this.example.glControl = renderer;
        //    //this.example.renderer.ParentWindow = this as Control;

        //}
    }


    public GLRenderWindow(Example example)
    {
        InitializeComponent();

        GLHost.Child = example.glControl;
        currentExample = example;

        currentExample.glControl.MouseMove += glControl_MouseMove;
        currentExample.glControl.MouseDown += glControl_MouseDown;
        currentExample.glControl.MouseUp += glControl_MouseUp;
        currentExample.glControl.Resize += glControl_Resize;
        currentExample.glControl.MouseWheel += glControl_MouseWheel;
        currentExample.glControl.SizeChanged += glControl_SizeChanged;
        currentExample.glControl.KeyDown += glControl_KeyDown;
        currentExample.glControl.KeyUp += glControl_KeyUp;
    }

    public static bool IsInDesignerMode =>
        (bool)DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue;

    public virtual void Dispose()
    {
        currentExample.glControl.Dispose();
        currentExample.imGuiManager?.Dispose();
        currentExample?.Dispose();

        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void glControl_KeyDown(object sender, KeyEventArgs e)
    {
        if (currentExample != null)
        {
            var key = (Keys)e.KeyCode;
            switch (e.KeyCode)
            {
                case System.Windows.Forms.Keys.Right:
                    key = Keys.Right;
                    break;
                case System.Windows.Forms.Keys.Left:
                    key = Keys.Left;
                    break;
                case System.Windows.Forms.Keys.Down:
                    key = Keys.Down;
                    break;
                case System.Windows.Forms.Keys.Up:
                    key = Keys.Up;
                    break;
            }

            currentExample.OnKeyDown(key, e.KeyValue, (KeyModifiers)e.Modifiers);
        }
    }

    private void glControl_KeyUp(object sender, KeyEventArgs e)
    {
        if (currentExample != null)
        {
            var key = (Keys)e.KeyCode;
            switch (e.KeyCode)
            {
                case System.Windows.Forms.Keys.Right:
                    key = Keys.Right;
                    break;
                case System.Windows.Forms.Keys.Left:
                    key = Keys.Left;
                    break;
                case System.Windows.Forms.Keys.Down:
                    key = Keys.Down;
                    break;
                case System.Windows.Forms.Keys.Up:
                    key = Keys.Up;
                    break;
            }

            currentExample.OnKeyUp(key, e.KeyValue, (KeyModifiers)e.Modifiers);
        }
    }


    private void glControl_SizeChanged(object sender, EventArgs e)
    {
        var control = sender as GLControl;
        currentExample?.OnResize(new ResizeEventArgs(control.Width, control.Height));
    }


    private void glControl_Resize(object sender, EventArgs e)
    {
        var control = sender as GLControl;

        if (control.ClientSize.Height == 0)
            control.ClientSize = new Size(control.ClientSize.Width, 1);

        GL.Viewport(0, 0, control.ClientSize.Width, control.ClientSize.Height);

        if (currentExample != null)
            currentExample.OnResize(new ResizeEventArgs(control.ClientSize.Width, control.ClientSize.Height));
    }

    private MouseButton GetMouseButton(MouseEventArgs e)
    {
        var button = MouseButton.Left;
        switch (e.Button)
        {
            case MouseButtons.Middle:
                button = MouseButton.Middle;
                break;
            case MouseButtons.Right:
                button = MouseButton.Right;
                break;
            case MouseButtons.Left:
            case MouseButtons.None:
            default:
                break;
        }

        return button;
    }

    private void glControl_MouseDown(object sender, MouseEventArgs e)
    {
        if (currentExample == null) return;
        var button = MouseButton.Left;
        currentExample.OnMouseDown(GetMouseButton(e), e.X, e.Y);
    }

    private void glControl_MouseMove(object sender, MouseEventArgs e)
    {
        if (currentExample == null) return;
        currentExample.OnMouseMove(GetMouseButton(e), e.X, e.Y);
    }

    private void glControl_MouseUp(object sender, MouseEventArgs e)
    {
        if (currentExample == null) return;
        currentExample.OnMouseUp(GetMouseButton(e), e.X, e.Y);
    }

    private void glControl_MouseWheel(object sender, MouseEventArgs e)
    {
        if (currentExample == null) return;
        currentExample.OnMouseWheel(e.X, e.Y, e.Delta);
    }

    public void Render()
    {
        if (!IsInDesignerMode)
        {
            if (currentExample == null) return;
            currentExample.OnResize(
                new ResizeEventArgs(currentExample.glControl.Width, currentExample.glControl.Height));
            currentExample.Render();
            if (currentExample.AddGuiControlsAction != null)
            {
                ImGui.NewFrame();
                currentExample.ShowGUIControls();
                ImGui.Render();
                currentExample.imGuiManager.ImGui_ImplOpenGL3_RenderDrawData(ImGui.GetDrawData());
            }

            currentExample.glControl.SwapBuffers();
        }
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        Render();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed) return;
        if (disposing)
            if (currentExample != null)
                currentExample.Dispose();

        disposed = true;
    }
}