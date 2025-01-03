using OpenTK.Graphics.ES30;

namespace THREE;

[Serializable]
public class GLBufferRenderer
{
    private GLCapabilities capabilities;

    private GLExtensions extensions;

    public GLInfo info;

    public bool IsGL2;

    public PrimitiveType mode;
    private GLRenderer renderer;

    public GLBufferRenderer(GLRenderer renderer, GLExtensions extensions, GLInfo info, GLCapabilities capabilities)
    {
        this.renderer = renderer;

        this.extensions = extensions;

        this.info = info;

        this.capabilities = capabilities;
    }

    public void SetMode(PrimitiveType value)
    {
        mode = value;
    }

    public virtual void Render(int start, int count)
    {
        GL.DrawArrays(mode, start, count);

        info.Update(count, (int)mode);
    }

    public virtual void RenderInstances(Geometry geometry, int start, int count, int primcount)
    {
        if (primcount == 0) return;

        GL.DrawArraysInstanced(mode, start, count, primcount);

        info.Update(count, (int)mode, primcount);
    }
}