using System.Collections;
using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public abstract class BasicObject : Hashtable, IDisposable
{
    private bool disposed;

    public BasicObject()
    {
    }

    public BasicObject(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public virtual void Dispose()
    {
        Dispose(disposed);
    }

    public event EventHandler<EventArgs> Disposed;

    ~BasicObject()
    {
        Dispose(false);
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