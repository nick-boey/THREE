namespace THREE;

[Serializable]
public class InterleavedBufferAttribute<T> : BufferAttribute<T>
{
    public InterleavedBufferAttribute()
    {
    }


    public InterleavedBufferAttribute(InterleavedBuffer<T> interleavedBuffer, int itemSize, int offset,
        bool normalized = false) : this()
    {
        Data = interleavedBuffer;
        ItemSize = itemSize;
        Offset = offset;
        Normalized = normalized;
    }

    public int Offset { get; set; }


    public int Stride { get; set; }

    public InterleavedBuffer<T> Data { get; set; }

    public new T[] Array => Data.Array;

    public new int count => Data.count;

    public new InterleavedBufferAttribute<T> SetY(int index, T y)
    {
        Data.Array[index * Data.Stride + Offset + 1] = y;
        return this;
    }

    public new InterleavedBufferAttribute<T> SetZ(int index, T z)
    {
        Data.Array[index * Data.Stride + Offset + 2] = z;

        return this;
    }

    public new InterleavedBufferAttribute<T> SetW(int index, T w)
    {
        Data.Array[index * Data.Stride + Offset + 3] = w;

        return this;
    }

    public new T GetX(int index)
    {
        return Data.Array[index * Data.Stride + Offset];
    }

    public new T GetY(int index)
    {
        return Data.Array[index * Data.Stride + Offset + 1];
    }

    public new T GetZ(int index)
    {
        return Data.Array[index * Data.Stride + Offset + 2];
    }

    public new T GetW(int index)
    {
        return Data.Array[index * Data.Stride + Offset + 3];
    }

    public new InterleavedBufferAttribute<T> SetXY(int index, T x, T y)
    {
        index = index * Data.Stride + Offset;
        Data.Array[index + 0] = x;
        Data.Array[index + 1] = y;

        return this;
    }

    public new InterleavedBufferAttribute<T> SetXYZ(int index, T x, T y, T z)
    {
        index = index * Data.Stride + Offset;
        Data.Array[index + 0] = x;
        Data.Array[index + 1] = y;
        Data.Array[index + 2] = z;

        return this;
    }

    public new InterleavedBufferAttribute<T> SetXYZW(int index, T x, T y, T z, T w)
    {
        index = index * Data.Stride + Offset;
        Data.Array[index + 0] = x;
        Data.Array[index + 1] = y;
        Data.Array[index + 2] = z;
        Data.Array[index + 3] = w;

        return this;
    }

    public InterleavedBufferAttribute<T> ApplyMatrix4(Matrix4 m)
    {
        var _vector = new Vector3();
        for (int i = 0, l = Data.count; i < l; i++)
        {
            _vector.X = (float)(object)GetX(i);
            _vector.Y = (float)(object)GetY(i);
            _vector.Z = (float)(object)GetZ(i);

            _vector.ApplyMatrix4(m);

            SetXYZ(i, (T)(object)_vector.X, (T)(object)_vector.Y, (T)(object)_vector.Z);
        }

        return this;
    }
}