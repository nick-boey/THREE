using System.Collections;
using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class MeshStandardMaterial : Material
{
    public MeshStandardMaterial()
    {
        type = "MeshStandardMaterial";

        Defines.Add("STANDARD", "");

        Color = new Color().SetHex(0xffffff);
        Roughness = 1.0f;
        Metalness = 0.0f;

        Map = null;

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

        RoughnessMap = null;

        MetalnessMap = null;

        AlphaMap = null;

        EnvMap = null;
        EnvMapIntensity = 1.0f;

        RefractionRatio = 0.98f;

        Wireframe = false;
        WireframeLineWidth = 1;
        WireframeLineCap = "round";
        WireframeLineJoin = "round";

        Skinning = false;
        MorphTargets = false;
        MorphNormals = false;
    }

    public MeshStandardMaterial(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    protected MeshStandardMaterial(MeshStandardMaterial source) : base(source)
    {
        type = "MeshStandardMaterial";

        Defines = (Hashtable)source.Defines.Clone();

        Color = source.Color;

        Map = source.Map; //!=null ? (Texture)source.Map.Clone() : null;

        LightMap = source.LightMap; //!=null?(Texture)source.LightMap.Clone() : null;
        LightMapIntensity = source.LightMapIntensity;

        AoMap = source.AoMap; // != null ? (Texture)source.AoMap.Clone() : null;
        AoMapIntensity = source.AoMapIntensity;

        Emissive = source.Emissive;
        EmissiveIntensity = source.EmissiveIntensity;
        EmissiveMap = source.EmissiveMap; //!=null ? (Texture)source.EmissiveMap.Clone():null;

        BumpMap = source.BumpMap; // != null ? (Texture)source.BumpMap.Clone() : null;
        BumpScale = source.BumpScale;

        NormalMap = source.NormalMap; // != null ? (Texture)source.NormalMap.Clone() : null; 
        NormalMapType = source.NormalMapType;
        NormalScale = source.NormalScale;

        DisplacementMap = source.DisplacementMap; // != null ? (Texture)source.DisplacementMap.Clone() : null;
        DisplacementScale = source.DisplacementScale;
        DisplacementBias = source.DisplacementBias;

        RoughnessMap = source.RoughnessMap; // != null ? (Texture)source.RoughnessMap.Clone() : null;

        MetalnessMap = source.MetalnessMap; // != null ? (Texture)source.MetalnessMap.Clone() : null;

        AlphaMap = source.AlphaMap; // != null ? (Texture)source.AlphaMap.Clone() : null;

        EnvMap = source.EnvMap; // != null ? (Texture)source.EnvMap.Clone() : null;

        EnvMapIntensity = source.EnvMapIntensity;

        RefractionRatio = source.RefractionRatio;

        Wireframe = source.Wireframe;
        WireframeLineWidth = source.WireframeLineWidth;
        WireframeLineCap = source.WireframeLineCap;
        WireframeLineJoin = source.WireframeLineJoin;

        Skinning = source.Skinning;
        MorphTargets = source.MorphTargets;
        MorphNormals = source.MorphNormals;
    }

    public new object Clone()
    {
        return new MeshStandardMaterial(this);
    }
}