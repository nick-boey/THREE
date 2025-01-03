using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class GLUniform : Dictionary<string, object>, ICloneable
{
    public List<object> Cache = new();

    public GLUniform()
    {
        UniformKind = "GLUniform";
    }

    public GLUniform(string id) : this()
    {
        Id = id;
    }

    public GLUniform(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public string Id { get; set; }

    public int Addr { get; set; }

    public string UniformKind { get; set; }

    public int UniformType { get; set; }

    public object Clone()
    {
        return this.DeepCopy();
    }

    public GLUniform Copy(GLUniform original)
    {
        return original.DeepCopy();
    }
}