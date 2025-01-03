namespace THREE;

[Serializable]
public class InterleavedBuffer<T> : BufferAttribute<T>
{
    public InterleavedBuffer()
    {
        Usage = Constants.StaticDrawUsage;
    }


    public InterleavedBuffer(T[] array, int stride) : this()
    {
        Array = array;
        Stride = stride;
        UpdateRange = new UpdateRange { Offset = 0, Count = -1 };
        Type = typeof(T);
    }

    public int Stride { get; set; }


    public new int count => Array != null ? Array.Length / Stride : 0;


    public InterleavedBuffer<T> CopyAt(int index1, InterleavedBuffer<T> attribute, int index2)
    {
        index1 *= Stride;
        index2 *= attribute.Stride;

        for (int i = 0, l = Stride; i < l; i++) Array[index1 + i] = attribute.Array[index2 + i];

        return this;
    }

    public InterleavedBuffer<T> Set(List<T> value, int offset)
    {
        List<T> list = new();
        list = Array.ToList();
        for (var i = offset; i < value.Count; i++) list.Insert(i, value[i - offset]);
        Array = list.ToArray();
        return this;
    }
}