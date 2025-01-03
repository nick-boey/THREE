namespace THREE;

[Serializable]
public struct UpdateRange
{
    public int Offset;

    public int Count;
}

[Serializable]
public class BufferAttribute<T> : IBufferAttribute
{
    public bool needsUpdate;

    public UpdateRange UpdateRange;

    public int Version;

    public BufferAttribute()
    {
        Usage = Constants.StaticDrawUsage;
        ;

        UpdateRange = new UpdateRange { Offset = 0, Count = -1 };
    }

    public BufferAttribute(T[] array, int itemSize, bool? normalized = null)
        : this()
    {
        Name = "";
        Array = array;
        ItemSize = itemSize;
        Type = typeof(T);

        Normalized = normalized != null && normalized.Value ? true : false;
    }

    protected BufferAttribute(BufferAttribute<T> source)
    {
        Copy(source);
    }

    public int Usage { get; set; } // BufferUsageHint Usage;

    public T[] Array { get; set; }
    public string Name { get; set; }


    public Type Type { get; set; }

    public int ItemSize { get; set; } = -1;


    public int Buffer { get; set; } = -1;

    public bool NeedsUpdate
    {
        get => needsUpdate;
        set
        {
            Version++;
            needsUpdate = value;
        }
    }

    public int count => Array.Length / ItemSize;


    public bool Normalized { get; set; }

    public int Length => Array.Length;

    public object Getter(int k, int index)
    {
        switch (k)
        {
            case 0: return GetX(index);
            case 1: return GetY(index);
            case 2: return GetZ(index);
            case 3: return GetW(index);
            default: return 0;
        }
    }

    public void Setter(int k, int index, object value)
    {
        switch (k)
        {
            case 0: SetX(index, (T)value); break;
            case 1: SetY(index, (T)value); break;
            case 2: SetZ(index, (T)value); break;
            case 3: SetW(index, (T)value); break;
            default: return;
        }
    }

    public void SetUsage(int hint)
    {
        Usage = hint;
    }

    public BufferAttribute<T> Clone()
    {
        return new BufferAttribute<T>(this);
    }

    public BufferAttribute<T> Copy(BufferAttribute<T> source)
    {
        Name = source.Name;
        if (source.Array != null)
        {
            Array = new T[source.Array.Length];
            source.Array.CopyTo(Array, 0);
        }

        ItemSize = source.ItemSize;
        Normalized = source.Normalized;
        Usage = source.Usage;
        Type = typeof(T);
        return this;
    }

    public BufferAttribute<T> CopyAt(int index1, BufferAttribute<T> attribute, int index2)
    {
        index1 *= ItemSize;
        index2 *= attribute.ItemSize;

        for (var i = 0; i < ItemSize; i++) Array[index1 + i] = attribute.Array[index2 + i];

        return this;
    }

    public BufferAttribute<T> CopyArray(T[] array)
    {
        Array = array;

        return this;
    }

    public BufferAttribute<T> CopyVector2sArray(Vector2[] vectors)
    {
        var array = Array as float[];
        if (array is null)
        {
            array = new float[vectors.Length * 2];

            Array = array as T[];
        }

        var offset = 0;

        for (var i = 0; i < vectors.Length; i++)
        {
            var vector = vectors[i];

            array[offset++] = vector.X;
            array[offset++] = vector.Y;
        }

        return this;
    }

    public BufferAttribute<T> CopyColorsArray(Color[] colors)
    {
        var array = Array as float[];
        //color.R / 255.0f, color.G / 255.0f, color.B / 255.0f
        if (array is null)
        {
            array = new float[colors.Length * 3];

            Array = array as T[];
        }

        var offset = 0;

        for (var i = 0; i < colors.Length; i++)
        {
            var color = colors[i];

            array[offset++] = color.R;
            array[offset++] = color.G;
            array[offset++] = color.B;
        }

        return this;
    }

    public BufferAttribute<T> CopyVector3sArray(Vector3[] vectors)
    {
        var array = Array as float[];

        if (array is null)
        {
            array = new float[vectors.Length * 3];

            Array = array as T[];
        }

        var offset = 0;

        for (var i = 0; i < vectors.Length; i++)
        {
            var vector = vectors[i];

            array[offset++] = vector.X;
            array[offset++] = vector.Y;
            array[offset++] = vector.Z;
        }

        return this;
    }

    public BufferAttribute<T> CopyVector4sArray(Vector4[] vectors)
    {
        var array = Array as float[];
        if (array is null)
        {
            array = new float[vectors.Length * 4];

            Array = array as T[];
        }

        var offset = 0;

        for (var i = 0; i < vectors.Length; i++)
        {
            var vector = vectors[i];

            array[offset++] = vector.X;
            array[offset++] = vector.Y;
            array[offset++] = vector.Z;
            array[offset++] = vector.W;
        }

        return this;
    }

    public BufferAttribute<T> Set(float[] array, int offset = 0)
    {
        array.CopyTo(Array, offset);

        return this;
    }

    public T GetX(int index)
    {
        return Array[index * ItemSize];
    }

    public BufferAttribute<T> SetX(int index, T x)
    {
        Array[index * ItemSize] = x;
        return this;
    }

    public T GetY(int index)
    {
        return Array[index * ItemSize + 1];
    }

    public BufferAttribute<T> SetY(int index, T y)
    {
        Array[index * ItemSize + 1] = y;
        return this;
    }

    public T GetZ(int index)
    {
        return Array[index * ItemSize + 2];
    }

    public BufferAttribute<T> SetZ(int index, T z)
    {
        Array[index * ItemSize + 2] = z;
        return this;
    }

    public T GetW(int index)
    {
        return Array[index * ItemSize + 3];
    }

    public BufferAttribute<T> SetW(int index, T w)
    {
        Array[index * ItemSize + 3] = w;
        return this;
    }

    public BufferAttribute<T> SetXY(int index, T x, T y)
    {
        index *= ItemSize;
        Array[index + 0] = x;
        Array[index + 1] = y;
        return this;
    }

    public BufferAttribute<T> SetXYZ(int index, T x, T y, T z)
    {
        index *= ItemSize;
        Array[index + 0] = x;
        Array[index + 1] = y;
        Array[index + 2] = z;
        return this;
    }

    public BufferAttribute<T> SetXYZW(int index, T x, T y, T z, T w)
    {
        index *= ItemSize;
        Array[index + 0] = x;
        Array[index + 1] = y;
        Array[index + 2] = z;
        Array[index + 3] = w;
        return this;
    }
}