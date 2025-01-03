using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class PointLight : Light
{
    public PointLight()
    {
        Distance = 0;
        Decay = 1;
        Shadow = new PointLightShadow();
        type = "PointLight";
    }

    public PointLight(Color color, float? intensity = null, float? distance = null, float? decay = null)
        : base(color, intensity)
    {
        Distance = distance != null ? (float)distance : 0;
        Decay = decay != null ? (float)decay : 1;

        Shadow = new PointLightShadow();

        type = "PointLight";
    }

    public PointLight(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public PointLight(int color, float? intensity = null, float? distance = null, float? decay = null) : this(
        Color.Hex(color), intensity, distance, decay)
    {
    }

    public new float Power
    {
        get => (float)(Intensity * 4 * Math.PI);
        set => Intensity = (float)(value / (4 * Math.PI));
    }
}