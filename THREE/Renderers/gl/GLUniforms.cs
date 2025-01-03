using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class GLUniforms : StructuredUniform
{
    public GLUniforms()
    {
        UniformKind = "GLUniforms";
    }

    public GLUniforms(string id) : base(id)
    {
        UniformKind = "GLUniforms";
    }

    public GLUniforms(string id, string kind) : base(id)
    {
        UniformKind = kind;
    }

    public GLUniforms(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public int Program { get; set; } = 0;

    public new object Clone()
    {
        return this.DeepCopy();
    }

    public GLUniforms Copy(GLUniforms original)
    {
        return original.DeepCopy();
    }
}