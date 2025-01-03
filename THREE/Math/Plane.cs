namespace THREE;

[Serializable]
public class Plane : ICloneable
{
    private Vector3 _vector1 = new();
    private Vector3 _vector2 = new();
    public float Constant;
    public Vector3 Normal = new(1, 0, 0);

    public Plane()
    {
    }

    protected Plane(Plane other)
    {
        Normal = other.Normal;
        Constant = other.Constant;
    }

    public Plane(Vector3 normal, float constant)
    {
        Normal = normal;
        Constant = constant;
    }

    public object Clone()
    {
        return new Plane(this);
    }

    public Plane Copy(Plane source)
    {
        Normal.Copy(source.Normal);
        Constant = source.Constant;
        return this;
    }

    public Plane Set(Vector3 normal, float constant)
    {
        Normal = normal;
        Constant = constant;

        return this;
    }

    public Plane SetComponents(float x, float y, float z, float w)
    {
        Normal = new Vector3(x, y, z);
        Constant = w;

        return this;
    }

    public Plane SetFromNormalAndCoplanarPoint(Vector3 normal, Vector3 point)
    {
        Normal = normal;
        Constant = -Vector3.Dot(point, Normal);

        return this;
    }

    public Plane SetFromCoplanarPoints(Vector3 a, Vector3 b, Vector3 c)
    {
        var normal = _vector1.SubVectors(c, b).Cross(_vector2.SubVectors(a, b)).Normalize();

        SetFromNormalAndCoplanarPoint(normal, a);

        return this;
    }

    public Plane Normalize()
    {
        var inverseNormalLength = 1.0f / Normal.Length();
        Normal.MultiplyScalar(inverseNormalLength);
        Constant *= inverseNormalLength;

        return this;
    }

    public Plane Negate()
    {
        Constant *= -1;
        Normal = Normal.Negate();

        return this;
    }

    public float DistanceToPoint(Vector3 point)
    {
        return Vector3.Dot(Normal, point) + Constant;
    }

    public float DistanceToSphere(Sphere sphere)
    {
        return DistanceToPoint(sphere.Center) - sphere.Radius;
    }

    public Vector3 ProjectPoint(Vector3 point)
    {
        return Normal * -DistanceToPoint(point) + point;
    }

    public bool IntersectLine(Line line)
    {
        throw new NotImplementedException();
    }

    public bool IntersectsLine(Line line)
    {
        throw new NotImplementedException();
    }

    public bool IntersectsBox(Box3 box)
    {
        return box.IntersectPlane(this);
    }

    public bool IntersectsSphere(Sphere sphere)
    {
        return sphere.IntersectsPlane(this);
    }

    public Vector3 CoplanarPoint()
    {
        return Normal * -Constant;
    }

    public Plane ApplyMatrix4(Matrix4 matrix, Matrix3 optionalNormalMatrix = null)
    {
        var normalMatrix = new Matrix3();

        if (optionalNormalMatrix != null)
            normalMatrix = (Matrix3)optionalNormalMatrix;
        else
            normalMatrix = normalMatrix.GetNormalMatrix(matrix);

        var referencePoint = CoplanarPoint().ApplyMatrix4(matrix);

        var normal = Normal.ApplyMatrix3(normalMatrix).Normalize();

        Constant = -Vector3.Dot(referencePoint, normal);

        return this;
    }

    public Plane translate(Vector3 offset)
    {
        Constant -= Vector3.Dot(offset, Normal);

        return this;
    }

    public override bool Equals(object obj)
    {
        var plane = obj as Plane;
        return plane.Normal.Equals(Normal) && plane.Constant == Constant;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}