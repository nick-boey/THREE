namespace THREE;

[Serializable]
public class Sphere : ICloneable
{
    public Vector3 Center = new();

    public float Radius;

    public Sphere()
    {
    }

    public Sphere(Vector3 center, float radius)
    {
        Center = center;
        Radius = radius;
    }

    protected Sphere(Sphere other)
    {
        Center = other.Center;
        Radius = other.Radius;
    }

    public object Clone()
    {
        return new Sphere(this);
    }

    public Sphere Copy(Sphere sphere)
    {
        Center.Copy(sphere.Center);
        Radius = sphere.Radius;

        return this;
    }

    public Sphere Set(Vector3 center, float radius)
    {
        Center.Copy(center);
        Radius = radius;
        return this;
    }

    public Sphere SetFromPoints(List<Vector3> points, Vector3 optionalCenter = null)
    {
        var center = Center;

        var box = new Box3();

        if (optionalCenter != null)
        {
            center.Copy(optionalCenter);
        }
        else
        {
            center = box.SetFromPoints(points).GetCenter(center);
            ;
        }

        float maxRadiusSq = 0;

        for (var i = 0; i < points.Count; i++) maxRadiusSq = Math.Max(maxRadiusSq, center.DistanceToSquared(points[i]));

        Radius = (float)Math.Sqrt(maxRadiusSq);

        return this;
    }

    public bool IsEmpty()
    {
        return Radius <= 0;
    }

    public bool ContainsPoint(Vector3 point)
    {
        return point.DistanceToSquared(Center) <= Radius * Radius;
    }

    public float DistanceToPoint(Vector3 point)
    {
        return point.DistanceTo(Center) - Radius;
    }

    public bool IntersectsSphere(Sphere sphere)
    {
        var radiusSum = Radius + sphere.Radius;

        return sphere.Center.DistanceToSquared(Center) <= radiusSum * radiusSum;
    }

    public bool IntersectBox(Box3 box)
    {
        return box.IntersectsSphere(this);
    }

    public bool IntersectsPlane(Plane plane)
    {
        return true;
        // return System.Math.Abs(plane.DistanceToPoint(this.Center))<=this.Radius;
    }

    public Vector3 ClampPoint(Vector3 point)
    {
        var deltaLengthSq = Center.DistanceToSquared(point);

        var target = point;

        if (deltaLengthSq > Radius * Radius)
        {
            target.Sub(Center).Normalize();
            target.MultiplyScalar(Radius).Add(Center);
        }

        return target;
    }

    public Box3 GetBoundingBox()
    {
        var target = new Box3(Center, Center);

        target.ExpandByScalar(Radius);

        return target;
    }

    public Sphere ApplyMatrix4(Matrix4 matrix)
    {
        Center = Center.ApplyMatrix4(matrix);
        Radius = Radius * matrix.GetMaxScaleOnAxis();

        return this;
    }

    public Sphere Translate(Vector3 offset)
    {
        Center = Center + offset;

        return this;
    }

    public override bool Equals(object obj)
    {
        var sphere = obj as Sphere;
        return sphere.Center.Equals(Center) && sphere.Radius == Radius;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}