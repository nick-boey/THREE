using OpenTK.Graphics.ES30;

namespace THREE;

[Serializable]
public class BufferType
{
    public int buffer;

    public int BytesPerElement;

    public int Type;

    public int Version;
}

[Serializable]
public class GLAttributes : Dictionary<object, object>
{
    // buffers = this

    public BufferType CreateBuffer<T>(IBufferAttribute attribute, BufferTarget bufferType)
    {
        var attr = attribute as BufferAttribute<T>;
        var array = attr.Array;
        var usage = (BufferUsageHint)attr.Usage;

        int buffer;

        var type = (int)VertexAttribPointerType.Float;

        var bytePerElement = 4;

        GL.GenBuffers(1, out buffer);
        GL.BindBuffer(bufferType, buffer);


        if (typeof(T) == typeof(float))
        {
            GL.BufferData(bufferType, array.Length * sizeof(float), array as float[], usage);
            type = (int)VertexAttribPointerType.Float;
            bytePerElement = sizeof(float);
        }
        else if (typeof(T) == typeof(int))
        {
            GL.BufferData(bufferType, array.Length * sizeof(int), array as int[], usage);
            type = (int)VertexAttribPointerType.UnsignedInt;
            bytePerElement = sizeof(int);
        }
        else if (typeof(T) == typeof(uint))
        {
            GL.BufferData(bufferType, array.Length * sizeof(uint), array as uint[], usage);
            type = (int)VertexAttribPointerType.UnsignedInt;
            bytePerElement = sizeof(uint);
        }
        else if (typeof(T) == typeof(byte))
        {
            GL.BufferData(bufferType, array.Length * sizeof(byte), array as byte[], usage);
            type = (int)VertexAttribPointerType.UnsignedByte;
            bytePerElement = sizeof(byte);
        }
        else
        {
            GL.BufferData(bufferType, array.Length * 2, array as short[], usage);
            type = (int)VertexAttribPointerType.UnsignedShort;
            bytePerElement = sizeof(short);
        }


        return new BufferType
            { buffer = buffer, Type = type, BytesPerElement = bytePerElement, Version = attr.Version };
    }

    public void UpdateBuffer<T>(int buffer, IBufferAttribute attribute, BufferTarget bufferType)
    {
        var attr = attribute as BufferAttribute<T>;
        var array = attr.Array;
        var updateRange = attr.UpdateRange;

        GL.BindBuffer(bufferType, buffer);

        if (updateRange.Count == -1)
        {
            if (null != array as float[])
                GL.BufferSubData(bufferType, IntPtr.Zero, attribute.Length * sizeof(float), array as float[]);
            else if (null != array as ushort[])
                GL.BufferSubData(bufferType, IntPtr.Zero, attribute.Length * sizeof(ushort), array as ushort[]);
            else if (null != array as uint[])
                GL.BufferSubData(bufferType, IntPtr.Zero, attribute.Length * sizeof(uint), array as uint[]);
            else if (null != array as byte[])
                GL.BufferSubData(bufferType, IntPtr.Zero, attribute.Length * sizeof(byte), array as byte[]);
        }
        else
        {
            var length = updateRange.Offset + updateRange.Count;
            var startIndex = updateRange.Offset;

            var subarray = new T[length];

            Array.Copy(array, startIndex, subarray, 0, length);

            if (null != array as float[])
                GL.BufferSubData(bufferType, new IntPtr(updateRange.Offset * sizeof(float)), length * sizeof(float),
                    subarray as float[]);
            else if (null != array as ushort[])
                GL.BufferSubData(bufferType, new IntPtr(updateRange.Offset * sizeof(ushort)), length * sizeof(float),
                    subarray as float[]);
            else if (null != array as uint[])
                GL.BufferSubData(bufferType, new IntPtr(updateRange.Offset * sizeof(uint)), length * sizeof(uint),
                    subarray as uint[]);
            else if (null != array as byte[])
                GL.BufferSubData(bufferType, new IntPtr(updateRange.Offset * sizeof(byte)), length * sizeof(byte),
                    subarray as byte[]);

            (attribute as BufferAttribute<T>).UpdateRange.Count = -1;
        }
    }

    public BufferType Get<T>(object attribute)
    {
        //if(!this.ContainsKey(attribute))
        //{
        //    this.Add(attribute,new BufferType());
        //}
        if (attribute is InterleavedBufferAttribute<T>) attribute = (attribute as InterleavedBufferAttribute<T>).Data;


        return ContainsKey(attribute) ? (BufferType)this[attribute] : null;
    }

    //public void Remove(string attribute)
    //{
    //    this.Remove(attribute);
    //}
    public void UpdateBufferAttribute(GLBufferAttribute attribute, BufferTarget bufferType)
    {
        var cached = this[attribute] as BufferType;
        if (cached != null || cached.Version < attribute.Version)
            Add(attribute,
                new BufferType
                {
                    buffer = attribute.Buffer, Type = attribute.Type, BytesPerElement = attribute.ElementSize,
                    Version = attribute.Version
                });
    }

    public void Update<T>(IBufferAttribute attribute, BufferTarget bufferType)
    {
        if (attribute is InterleavedBufferAttribute<T>)
            attribute = (attribute as InterleavedBufferAttribute<T>).Data;


        var data = Get<T>(attribute);

        //if (!this.ContainsKey(attribute))
        //{
        //    this.Add(attribute,CreateBuffer(attribute,bufferType));
        //}
        if (data == null)
            Add(attribute, CreateBuffer<T>(attribute, bufferType));
        else if (data.Version < (attribute as BufferAttribute<T>).Version)
            UpdateBuffer<T>(data.buffer, attribute, bufferType);
        //BufferType data = (BufferType)this[attribute];
        //if (data.Version < attribute.Version)
        //{
        //    UpdateBuffer<T>(data.buffer, attribute, bufferType);
        //    data.Version = attribute.Version;
        //    this[attribute] = data;
        //}
    }
}