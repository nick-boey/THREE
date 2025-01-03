using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class Light : Object3D
{
    public float Angle;
    public Color Color;

    public float Decay;

    public float Distance;

    public float Exponent;

    public Color GroundColor;

    public int Height;

    public float Intensity;

    public float Penumbra;

    public SphericalHarmonics3 sh;

    public LightShadow Shadow;

    public Object3D Target;

    //RectAreaLight

    public int Width;

    public Light()
    {
        IsLight = true;
    }

    public Light(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public Light(Color color, float? intensity = null)
    {
        type = "Light";

        Color = color;

        Intensity = intensity != null ? intensity.Value : 1;

        ReceiveShadow = false;

        IsLight = true;
    }

    public Light(int color, float? intensity = null) : this(Color.Hex(color), intensity)
    {
    }

    protected Light(Light other) : base(other)
    {
        type = "Light";

        Color = other.Color;

        Intensity = other.Intensity;

        IsLight = true;
    }

    public float Power { get; set; }

    public Light Copy(Light source)
    {
        return new Light(source);
    }
}