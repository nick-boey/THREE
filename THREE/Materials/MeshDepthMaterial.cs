using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class MeshDepthMaterial : Material
{
    public MeshDepthMaterial()
    {
        type = "MeshDepthMaterial";

        Skinning = false;

        MorphTargets = false;

        AlphaMap = null;

        DisplacementMap = null;

        DisplacementScale = 1.0f;

        DisplacementBias = 0.0f;

        Wireframe = false;

        WireframeLineWidth = 1;

        Fog = false;
    }

    public MeshDepthMaterial(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}