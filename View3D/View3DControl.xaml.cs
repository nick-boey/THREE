using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MouseButton = OpenTK.Windowing.GraphicsLibraryFramework.MouseButton;
using OpenTK.Windowing.Common;
using OpenTK.Wpf;
using GL = OpenTK.Graphics.OpenGL4.GL;
using OpenTK.Graphics.OpenGL4;

namespace View3D;

/// <summary>
/// Interaction logic for UserControl1.xaml
/// </summary>
public partial class View3DControl : UserControl, IDisposable
{
    public static bool IsInDesignerMode
    {
        get
        {
            return ((bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject))
                .DefaultValue));
        }
    }

    public View3DContainer? Container;

    public View3DControl()
    {
        InitializeComponent();

        GLWpfControlSettings settings = new GLWpfControlSettings
        {
            MajorVersion = 3,
            MinorVersion = 3,
            Profile = ContextProfile.Compatability
        };
        OpenTKControl.Start(settings);

        MouseMove += glControl_MouseMove;
        MouseDown += glControl_MouseDown;
        MouseUp += glControl_MouseUp;
        MouseWheel += glControl_MouseWheel;
        //KeyDown += glControl_KeyDown;
        //KeyUp += glControl_KeyUp;
    }

    public void Load(View3DContainer container)
    {
        Container = container;
        Container.Load(OpenTKControl);
        Render();
    }


    private void OpenTKControl_OnRender(TimeSpan delta)
    {
        Render();
    }

    /*
     TODO: Update key controls

     private void glControl_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (Container != null)
        {
            Keys key = (Keys)e.Key;
            switch (e.Key)
            {
                case Key.Right:
                    key = Keys.Right;
                    break;
                case Key.Left:
                    key = Keys.Left;
                    break;
                case Key.Down:
                    key = Keys.Down;
                    break;
                case Key.Up:
                    key = Keys.Up;
                    break;

            }
            Container.OnKeyDown(key, e.Key, (KeyModifiers)e);
        }
    }

    private void glControl_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (Container != null)
        {
            Keys key = (Keys)e.KeyCode;
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
            Container.OnKeyUp(key, e.KeyValue, (KeyModifiers)e.Modifiers);
        }
    }
    */

    private MouseButton GetMouseButton(System.Windows.Input.MouseEventArgs e)
    {
        if (e.RightButton == MouseButtonState.Pressed)
        {
            return MouseButton.Right;
        }

        if (e.MiddleButton == MouseButtonState.Pressed)
        {
            return MouseButton.Middle;
        }

        return MouseButton.Left;
    }

    private void glControl_MouseDown(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (Container == null) return;
        var position = e.GetPosition(this);
        Container.OnMouseDown(GetMouseButton(e), (int)position.X, (int)position.Y);
    }

    private void glControl_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (Container == null) return;
        var position = e.GetPosition(this);
        Container.OnMouseMove(GetMouseButton(e), (int)position.X, (int)position.Y);
    }

    private void glControl_MouseUp(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (Container == null) return;
        var position = e.GetPosition(this);
        Container.OnMouseUp(GetMouseButton(e), (int)position.X, (int)position.Y);
    }

    private void glControl_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
    {
        if (Container == null) return;
        var position = e.GetPosition(this);
        Container.OnMouseWheel((int)position.X, (int)position.Y, e.Delta);
    }

    public void Render()
    {
        if (!IsInDesignerMode)
        {
            if (Container == null) return;
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Viewport(0, 0, (int)Container.glControl.ActualWidth, (int)Container.glControl.ActualHeight);
            Container.OnResize(new ResizeEventArgs((int)Container.glControl.ActualWidth,
                (int)Container.glControl.ActualHeight));
            Container.Render();
            Container.Renderer.Context.SwapBuffers();
        }
    }


    private bool _disposed;

    public virtual void Dispose()
    {
        Container.glControl.Dispose();
        Container?.Dispose();

        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            Container?.Dispose();
        }

        _disposed = true;
    }
}