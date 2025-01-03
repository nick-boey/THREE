namespace THREE;

[Serializable]
public class InstancedInterleavedBuffer<T> : InterleavedBuffer<T>
{
    public InstancedInterleavedBuffer()
    {
    }

    public InstancedInterleavedBuffer(T[] array, int stride, int? meshPerAttribute = null) : this()
    {
        Array = array;
        Stride = stride;
        UpdateRange = new UpdateRange { Offset = 0, Count = -1 };
        MeshPerAttribute = meshPerAttribute != null ? (int)meshPerAttribute : 1;
    }

    public int MeshPerAttribute { get; set; } = 1;
}