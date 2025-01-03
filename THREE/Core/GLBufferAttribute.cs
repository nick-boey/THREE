using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class GLBufferAttribute : Dictionary<object, object>
{
    private bool isGLBufferAttribute = true;
    public int Version;


    public GLBufferAttribute(int buffer, int type, int itemSize, int elementSize, int count)
    {
        Buffer = buffer;
        Type = type;
        ItemSize = itemSize;
        ElementSize = elementSize;
        this.count = count;
    }

    public GLBufferAttribute()
    {
    }

    public GLBufferAttribute(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public int Buffer
    {
        get => (int)this["buffer"];
        set => this["buffer"] = value;
    }

    public int Type
    {
        get => (int)this["type"];
        set => this["type"] = value;
    }

    public int ItemSize
    {
        get => (int)this["itemSize"];
        set => this["itemSize"] = value;
    }

    public bool NeedsUpdate
    {
        get => (bool)this["needsUpdate"];
        set
        {
            Version++;
            this["needsUpdate"] = value;
        }
    }

    public int ElementSize
    {
        get => (int)this["elementSize"];
        set => this["elementSize"] = value;
    }

    public int count
    {
        get => (int)this["count"];
        set => this["count"] = value;
    }
}