using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class PointsMaterial : Material
{
    public float Size = 1;

    public PointsMaterial()
    {
        type = "PointsMaterial";

        Color = new Color().SetHex(0xffffff);

        Map = null;

        AlphaMap = null;

        Size = 1;

        SizeAttenuation = true;

        MorphTargets = false;
    }

    public PointsMaterial(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}