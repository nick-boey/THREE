using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class SpotLight : Light
{
    public SpotLight()
    {
        Position.Copy(DefaultUp);
        UpdateMatrix();

        Target = new Object3D();

        Distance = 0;

        Angle = (float)(Math.PI / 3);

        Penumbra = 0;

        Decay = 1;

        Shadow = new SpotLightShadow();

        type = "SpotLight";
    }

    public SpotLight(Color color, float? intensity = null, float? distance = null, float? angle = null,
        float? penumbra = null, float? decay = null)
        : base(color, intensity)
    {
        Position.Copy(DefaultUp);
        UpdateMatrix();

        Target = new Object3D();

        Distance = distance != null ? (float)distance : 0;

        Angle = angle != null ? (float)angle : (float)(Math.PI / 3);

        Penumbra = penumbra != null ? (float)penumbra : 0;

        Decay = decay != null ? (float)decay : 1;

        Shadow = new SpotLightShadow();

        type = "SpotLight";
    }

    public SpotLight(int color, float? intensity = null, float? distance = null, float? angle = null,
        float? penumbra = null, float? decay = null) :
        this(Color.Hex(color), intensity, distance, angle, penumbra, decay)
    {
    }

    public SpotLight(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    protected SpotLight(SpotLight other) : base(other)
    {
        Distance = other.Distance;

        Angle = other.Angle;

        Penumbra = other.Penumbra;

        Decay = other.Decay;

        Target = other.Target;

        Shadow = (SpotLightShadow)other.Shadow.Clone();

        type = "SpotLight";
    }

    public new float Power
    {
        get => (float)(Intensity * Math.PI);
        set => Intensity = (float)(value / Math.PI);
    }
}