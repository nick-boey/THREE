namespace THREE;

[Serializable]
public class Fog
{
    public Color Color;

    public float Far;
    public string Name;

    public float Near;

    public Fog()
    {
    }

    public Fog(Color color, float? near = null, float? far = null)
    {
        Name = "";

        Color = color;

        Near = near != null ? near.Value : 1;

        Far = far != null ? far.Value : 1000;
    }

    public Fog(int color, float? near = null, float? far = null) : this(Color.Hex(color), near, far)
    {
    }
}