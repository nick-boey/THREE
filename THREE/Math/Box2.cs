namespace THREE;

[Serializable]
public class Box2 : ICloneable
{
    public Vector2 Max;

    public Vector2 Min;

    public Box2(Vector2 min = null, Vector2 max = null)
    {
        Min = min != null ? min : new Vector2(float.PositiveInfinity, float.PositiveInfinity);

        Max = max != null ? max : new Vector2(float.NegativeInfinity, float.NegativeInfinity);
    }

    protected Box2(Box2 source)
    {
        Min.Copy(source.Min);

        Max.Copy(source.Max);
    }

    public object Clone()
    {
        return new Box2(this);
    }

    public Box2 Set(Vector2 min, Vector2 max)
    {
        Min.Copy(min);

        Max.Copy(max);

        return this;
    }

    public Box2 SetFromPoints(Vector2[] points)
    {
        MakeEmpthy();

        for (var i = 0; i < points.Length; i++) ExpandByPoint(points[i]);

        return this;
    }

    public Box2 SetFromCenterAndSize(Vector2 center, Vector2 size)
    {
        var _vector = new Vector2();

        var halfSize = _vector.Copy(size).MultiplyScalar(0.5f);
        Min.Copy(center).Sub(halfSize);
        Max.Copy(center).Add(halfSize);

        return this;
    }

    public Box2 Copy(Box2 other)
    {
        Min.Copy(other.Min);
        Max.Copy(other.Max);

        return this;
    }

    public Box2 MakeEmpthy()
    {
        Min.X = Min.Y = float.PositiveInfinity;
        Max.X = Max.Y = float.NegativeInfinity;

        return this;
    }

    public bool IsEmpty()
    {
        return Max.X < Min.X || Max.Y < Min.Y;
    }

    public Vector2 GetCenter(Vector2 target)
    {
        if (IsEmpty())
            target.Set(0, 0);
        else
            target.AddVectors(Min, Max).MultiplyScalar(0.5f);

        return target;
    }

    public Vector2 GetSize(Vector2 target)
    {
        if (IsEmpty())
            target.Set(0, 0);
        else
            target.SubVectors(Max, Min);

        return target;
    }

    public Box2 ExpandByPoint(Vector2 point)
    {
        Min.Min(point);
        Max.Max(point);

        return this;
    }

    public Box2 ExpandByVector(Vector2 vector)
    {
        Min.Sub(vector);
        Max.Add(vector);

        return this;
    }

    public Box2 ExpandByScalar(float scalar)
    {
        Min.AddScalar(-scalar);
        Max.AddScalar(scalar);

        return this;
    }

    public bool ContainsPoint(Vector2 point)
    {
        return point.X < Min.X || point.X > Max.X ||
               point.Y < Min.Y || point.Y > Max.Y
            ? false
            : true;
    }

    public bool ContainBox(Box2 box)
    {
        return Min.X <= box.Min.X && box.Max.X <= Max.X &&
               Min.Y <= box.Min.Y && box.Max.Y <= Max.Y;
    }

    public Vector2 GetParameter(Vector2 point, Vector2 target)
    {
        return target.Set(
            (point.X - Min.X) / (Max.X - Min.X),
            (point.Y - Min.Y) / (Max.Y - Min.Y)
        );
    }

    public bool IntersectsBox(Box2 box)
    {
        return box.Max.X < Min.X || box.Min.X > Max.X ||
               box.Max.Y < Min.Y || box.Min.Y > Max.Y
            ? false
            : true;
    }

    public Vector2 ClampPoint(Vector2 point, Vector2 target)
    {
        return target.Copy(point).Clamp(Min, Max);
    }

    public float DistanceToPoint(Vector2 point)
    {
        var _vector = new Vector2();
        var clampedPoint = _vector.Copy(point).Clamp(Min, Max);
        return clampedPoint.Sub(point).Length();
    }

    public Box2 Intersect(Box2 box)
    {
        Min.Max(box.Min);
        Max.Min(box.Max);

        return this;
    }

    public Box2 Union(Box2 box)
    {
        Min.Min(box.Min);
        Max.Max(box.Max);

        return this;
    }

    public Box2 Translate(Vector2 offset)
    {
        Min.Add(offset);
        Max.Add(offset);

        return this;
    }

    public bool Equals(Box2 box)
    {
        return box.Min.Equals(Min) && box.Max.Equals(Max);
    }
}