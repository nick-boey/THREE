namespace THREE;

[Serializable]
public abstract class DisposableObject
{
    private bool disposed;
    public event EventHandler<EventArgs> Disposed;

    ~DisposableObject()
    {
        Dispose(false);
    }

    public bool IsDisposed()
    {
        return disposed;
    }

    public virtual void Dispose()
    {
        Dispose(disposed);
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
        RaiseDisposed();
        disposed = true;
        disposed = true;
    }
}