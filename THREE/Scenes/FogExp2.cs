namespace THREE;

[Serializable]
public class FogExp2 : Fog
{
    public float Density;

    public FogExp2(Color color, float? density = null)
    {
        Name = "";
        Density = density != null ? density.Value : (float)0.00025;
    }
}