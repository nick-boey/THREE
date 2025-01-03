namespace THREE;

[Serializable]
// Helper for passes that need to fill the viewport with a single quad.
public class FullScreenQuad : IDisposable
{
    private Mesh _mesh;
    private OrthographicCamera camera = new(-1, 1, 1, -1, 0, 1);
    private bool disposed;
    private BufferGeometry geometry = new();

    public FullScreenQuad(Material material = null)
    {
        geometry.SetAttribute("position", new BufferAttribute<float>(new float[] { -1, 3, 0, -1, -1, 0, 3, -1, 0 }, 3));
        geometry.SetAttribute("uv", new BufferAttribute<float>(new float[] { 0, 2, 0, 0, 2, 0 }, 2));
        _mesh = new Mesh(geometry, material);
    }

    public Material material
    {
        get => _mesh.Material;
        set => _mesh.Material = value;
    }

    public virtual void Dispose()
    {
        Dispose(disposed);
    }

    public event EventHandler<EventArgs> Disposed;

    ~FullScreenQuad()
    {
        Dispose(false);
    }

    public void Render(GLRenderer renderer)
    {
        renderer.Render(_mesh, camera);
    }

    protected virtual void RaiseDisposed()
    {
        var handler = Disposed;
        if (handler != null)
            handler(this, new EventArgs());
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed) return;
        if (_mesh != null)
            _mesh.Geometry.Dispose();
        RaiseDisposed();
        disposed = true;
        disposed = true;
    }
}

[Serializable]
public abstract class Pass
{
    public bool Clear = false;
    public bool Enabled = true;
    public FullScreenQuad fullScreenQuad;
    public bool NeedsSwap = true;
    public bool RenderToScreen = false;

    public abstract void SetSize(float width, float height);


    public abstract void Render(GLRenderer renderer, GLRenderTarget writeBuffer, GLRenderTarget readBuffer = null,
        float? deltaTime = null, bool? maskActive = null);
}