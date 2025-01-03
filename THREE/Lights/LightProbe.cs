using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class LightProbe : Light, ICloneable
{
    public LightProbe() : base(Color.ColorName(ColorKeywords.white))
    {
        sh = new SphericalHarmonics3();
    }

    public LightProbe(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public LightProbe(SphericalHarmonics3 sh, int? intensity) : base(Color.ColorName(ColorKeywords.white), intensity)
    {
        if (sh != null) this.sh = sh;
        else sh = new SphericalHarmonics3();
    }

    protected LightProbe(LightProbe other)
    {
        sh = (SphericalHarmonics3)other.sh.Clone();
        Intensity = other.Intensity;
    }

    public LightProbe Copy(LightProbe source)
    {
        if (source.sh != null && source.sh.Coefficients.Count > 0) sh.Coefficients = source.sh.Coefficients.ToList();
        ;
        Intensity = source.Intensity;
        return this;
    }
}