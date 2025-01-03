using System.Collections;
using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class RawShaderMaterial : ShaderMaterial
{
    public RawShaderMaterial(Hashtable parameters = null)
    {
        type = "RawShaderMaterial";

        SetValues(parameters);
    }

    public RawShaderMaterial(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}