using System;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL4;

namespace WPFDemo.Controls;

public class GLRenderControl : GLControl
{
    public EventHandler<EventArgs> OnInitGL;


    public GLRenderControl()
    {
        InitGLContext();
    }

    private void GLRenderControl_Paint(object sender, PaintEventArgs e)
    {
        throw new NotImplementedException();
    }

    /*public GLRenderControl(GraphicsMode mode) : base(mode)
    {
        InitGLContext();
    }*/

    private void InitGLContext()
    {
        MakeCurrent();
        //this.VSync = true;
        if (OnInitGL != null) OnInitGL(this, new EventArgs());
    }

    ///*public GLRenderControl(GraphicsMode mode, int major, int minor, GraphicsContextFlags flags) : base(mode, major, minor, flags)
    //{
    //    InitGLContext();
    //}*/
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        if (IsHandleCreated) SetViewport(0, 0, Size.Width, Size.Height);
        //if(scene!=null)
        //    scene.Resize(this.Size.Width,this.Size.Height);
    }

    public void SetViewport(int x, int y, int width, int height)
    {
        GL.Viewport(x, y, width, height);
    }


    //public void Clean()
    //{
    //    //GL.ClearColor(System.Drawing.Color.FromArgb(this.ClearAlpha, this.ClearColor));
    //    GL.ClearBuffer(ClearBuffer.Color, 0, new float[4] { 0, 0, 0, 1 });
    //}
}