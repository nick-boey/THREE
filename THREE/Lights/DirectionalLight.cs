using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class DirectionalLight : Light, ICloneable
{
    public DirectionalLight(Color color, float? intensity = null) : base(color, intensity)
    {
        Position.Copy(DefaultUp);

        UpdateMatrix();

        Target = new Object3D();

        Shadow = new DirectionalLightShadow();

        type = "DirectionalLight";
    }

    public DirectionalLight() : this(new Color())
    {
    }

    public DirectionalLight(int color, float? intensity = null) : this(Color.Hex(color), intensity)
    {
    }

    protected DirectionalLight(DirectionalLight other) : base(other)
    {
        Target = other.Target;

        type = "DirectionalLight";

        Shadow = (LightShadow)other.Shadow.Clone();
    }

    public DirectionalLight(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}