using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class RectAreaLight : Light
{
    public RectAreaLight()
    {
        Width = 10;
        Height = 10;
        type = "RectAreaLight";
    }

    public RectAreaLight(Color color, float? itensity = null, int? width = null, int? height = null) : base(color,
        itensity)
    {
        Width = width != null ? (int)width : 10;
        Height = height != null ? (int)height : 10;
        type = "RectAreaLight";
    }

    public RectAreaLight(int color, float? itensity = null, int? width = null, int? height = null) : this(
        Color.Hex(color), itensity)
    {
    }

    public RectAreaLight(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    protected RectAreaLight(RectAreaLight other) : base(other)
    {
        Width = other.Width;
        Height = other.Height;
        type = "RectAreaLight";
    }
}