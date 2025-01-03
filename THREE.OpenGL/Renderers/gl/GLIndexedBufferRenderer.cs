using OpenTK.Graphics.ES30;

namespace THREE;

[Serializable]
public class GLIndexedBufferRenderer : GLBufferRenderer
{
    private int bytesPerElement;
    private VertexAttribPointerType type;


    public GLIndexedBufferRenderer(GLRenderer renderer, GLExtensions extensions, GLInfo info,
        GLCapabilities capabilities) : base(renderer, extensions, info, capabilities)
    {
    }

    //public void SetMode(PrimitiveType value)
    //{
    //    this.mode = value;
    //}

    public void SetIndex(BufferType value)
    {
        var pointerType = (VertexAttribPointerType)Enum.ToObject(typeof(VertexAttribPointerType), value.Type);
        type = pointerType;

        bytesPerElement = value.BytesPerElement;
    }

    public override void Render(int start, int count)
    {
        var indices = start * bytesPerElement;
        var ptr = IntPtr.Add(IntPtr.Zero, indices);

        GL.DrawElements((All)mode, count, (All)type, ptr);

        info.Update(count, (int)mode);
    }

    public override void RenderInstances(Geometry geometry, int start, int count, int primcount)
    {
        if (primcount == 0) return;

        var indices = start * bytesPerElement;
        var ptr = IntPtr.Add(IntPtr.Zero, indices);

        GL.DrawElementsInstanced((All)mode, count, (All)type, ptr, primcount);

        info.Update(count, (int)mode, primcount);
    }
}