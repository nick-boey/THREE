using System.Collections;
using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class MeshPhongMaterial : Material
{
    public MeshPhongMaterial(Hashtable parameter = null)
    {
        type = "MeshPhongMaterial";
        Color = new Color().SetHex(0xffffff);
        ;
        Specular = new Color().SetHex(0x111111);
        ;
        Shininess = 30;

        LightMap = null;
        LightMapIntensity = 1.0f;

        AoMap = null;
        AoMapIntensity = 1.0f;

        Emissive = new Color().SetHex(0x000000);
        EmissiveIntensity = 1.0f;
        EmissiveMap = null;

        BumpMap = null;
        BumpScale = 1;

        NormalMap = null;
        NormalMapType = Constants.TangentSpaceNormalMap;
        NormalScale = new Vector2(1, 1);

        DisplacementMap = null;
        DisplacementScale = 1;
        DisplacementBias = 0;

        SpecularMap = null;

        AlphaMap = null;

        EnvMap = null;

        Combine = Constants.MultiplyOperation;

        Reflectivity = 1;

        RefractionRatio = 0.98f;

        Wireframe = false;
        WireframeLineWidth = 1;
        WireframeLineCap = "round";
        WireframeLineJoin = "round";

        Skinning = false;
        MorphTargets = false;
        MorphNormals = false;


        if (parameter != null)
            SetValues(parameter);
    }

    public MeshPhongMaterial(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}