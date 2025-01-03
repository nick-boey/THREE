using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class SpriteMaterial : Material
{
    //public bool SizeAttenuation = true;

    public SpriteMaterial()
    {
        type = "SpriteMaterial";

        Color = new Color().SetHex(0x000000);

        Transparent = true;

        Map = null;

        AlphaMap = null;

        Rotation = 0;

        SizeAttenuation = true;

        Transparent = true;
    }

    public SpriteMaterial(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}