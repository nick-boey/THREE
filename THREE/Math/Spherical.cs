namespace THREE;

[Serializable]
public class Spherical : ICloneable
{
    public float Phi;
    public float Radius;
    public float Theta;

    public Spherical(float radius = 1, float phi = 0, float theta = 0)
    {
        Radius = radius;
        Phi = phi;
        Theta = theta;
    }

    public Spherical(Spherical source)
    {
        Radius = source.Radius;
        Phi = source.Phi;
        Theta = source.Theta;
    }

    public object Clone()
    {
        return new Spherical(this);
    }

    public Spherical Set(float radius, float phi, float theta)
    {
        Radius = radius;
        Phi = phi;
        Theta = theta;

        return this;
    }

    public Spherical Copy(Spherical source)
    {
        Radius = source.Radius;
        Phi = source.Phi;
        Theta = source.Theta;

        return this;
    }

    // restrict phi to be betwee EPS and PI-EPS
    public Spherical makeSafe()
    {
        var EPS = 0.000001f;
        Phi = (float)Math.Max(EPS, Math.Min(Math.PI - EPS, Phi));

        return this;
    }

    public Spherical SetFromVector3(Vector3 v)
    {
        return setFromCartesianCoords(v.X, v.Y, v.Z);
    }

    public Spherical setFromCartesianCoords(float x, float y, float z)
    {
        Radius = (float)Math.Sqrt(x * x + y * y + z * z);

        if (Radius == 0)
        {
            Theta = 0;
            Phi = 0;
        }
        else
        {
            Theta = (float)Math.Atan2(x, z);
            Phi = (float)Math.Acos((y / Radius).Clamp(-1, 1));
        }

        return this;
    }
}