using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class MeshNormalMaterial : Material
{
    public MeshNormalMaterial()
    {
        type = "MeshNormalMaterial";

        BumpMap = null;
        BumpScale = 1;

        NormalMap = null;
        NormalMapType = Constants.TangentSpaceNormalMap;
        NormalScale = new Vector2(1, 1);

        DisplacementMap = null;
        DisplacementScale = 1;
        DisplacementBias = 0;

        Wireframe = false;
        WireframeLineWidth = 1;

        Fog = false;

        Skinning = false;
        MorphTargets = false;
        MorphNormals = false;
    }

    public MeshNormalMaterial(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}