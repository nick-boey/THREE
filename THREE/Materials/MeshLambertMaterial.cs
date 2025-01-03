using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class MeshLambertMaterial : Material
{
    public MeshLambertMaterial()
    {
        type = "MeshLambertMaterial";

        Color = THREE.Color.ColorName(ColorKeywords.white);

        Opacity = 1;

        Emissive = THREE.Color.Hex(0x000000);

        Combine = Constants.MultiplyOperation;

        RefractionRatio = 0.98f;

        Transparent = false;

        WireframeLineWidth = 1;

        WireframeLineCap = "round";
        WireframeLineJoin = "round";
    }

    public MeshLambertMaterial(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    protected MeshLambertMaterial(MeshLambertMaterial source)
    {
        Copy(source);
    }

    public override object Clone()
    {
        var material = new MeshLambertMaterial();
        material.Copy(this);
        return material;
    }

    public object Copy(MeshLambertMaterial source)
    {
        base.Copy(source);
        type = source.type;

        Color = source.Color;

        Opacity = source.Opacity;

        Emissive = source.Emissive;

        Combine = source.Combine;

        RefractionRatio = source.RefractionRatio;

        Transparent = source.Transparent;

        WireframeLineWidth = source.WireframeLineWidth;

        WireframeLineCap = source.WireframeLineCap;
        WireframeLineJoin = source.WireframeLineJoin;

        return this;
    }
}