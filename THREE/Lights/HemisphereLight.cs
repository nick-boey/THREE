using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class HemisphereLight : Light
{
    public HemisphereLight(Color skyColor, Color groundColor, float? itensity = null)
        : base(skyColor, itensity)
    {
        CastShadow = false;

        Position.Copy(DefaultUp);

        UpdateMatrix();

        GroundColor = groundColor;

        type = "HemisphereLight";
    }

    public HemisphereLight(int color, int gcolor, float? intensity) : this(Color.Hex(color), Color.Hex(gcolor),
        intensity)
    {
    }

    public HemisphereLight(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    protected HemisphereLight(HemisphereLight other) : base(other)
    {
        type = "HemisphereLight";
        GroundColor = other.GroundColor;
    }
}