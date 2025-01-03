namespace THREE;

[Serializable]
public class Box3 : ICloneable
{
    private Vector3 _center = new();
    private Vector3 _extents = new();

    // triangle edge vectors

    private Vector3 _f0 = new();
    private Vector3 _f1 = new();
    private Vector3 _f2 = new();

    private List<Vector3> _points = new()
    {
        new Vector3(),
        new Vector3(),
        new Vector3(),
        new Vector3(),
        new Vector3(),
        new Vector3(),
        new Vector3(),
        new Vector3()
    };

    private Vector3 _testAxis = new();
    private Vector3 _triangleNormal = new();

    // triangle centered vertices

    private Vector3 _v0 = new();
    private Vector3 _v1 = new();
    private Vector3 _v2 = new();

    private Vector3 _vector = Vector3.Zero();
    public Vector3 Max = new();
    public Vector3 Min = new();

    public Box3()
    {
        Min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        Max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
    }

    public Box3(Vector3 min, Vector3 max)
    {
        Min = min;
        Max = max;
    }

    protected Box3(Box3 source)
    {
        Min.Copy(source.Min);
        Max.Copy(source.Max);
    }

    public object Clone()
    {
        return new Box3(this);
    }

    public Box3 Copy(Box3 box)
    {
        Min.Copy(box.Min);
        Max.Copy(box.Max);

        return this;
    }

    public void SetFromArray(float[] array)
    {
        var minX = float.PositiveInfinity;
        var minY = float.PositiveInfinity;
        var minZ = float.PositiveInfinity;

        var maxX = float.NegativeInfinity;
        var maxY = float.NegativeInfinity;
        var maxZ = float.NegativeInfinity;

        for (var i = 0; i < array.Length; i += 3)
        {
            var x = array[i];
            var y = array[i + 1];
            var z = array[i + 2];

            if (x < minX) minX = x;
            if (y < minY) minY = y;
            if (z < minZ) minZ = z;

            if (x > maxX) maxX = x;
            if (y > maxY) maxY = y;
            if (z > maxZ) maxZ = z;
        }

        Min.X = minX;
        Min.Y = minY;
        Min.Z = minZ;

        Max.X = maxX;
        Max.Y = maxY;
        Max.Z = maxZ;
    }

    public void SetFromBufferAttribute(IBufferAttribute attribute)
    {
        var attr = attribute as BufferAttribute<float>;
        var minX = float.PositiveInfinity;
        var minY = float.PositiveInfinity;
        var minZ = float.PositiveInfinity;

        var maxX = float.NegativeInfinity;
        var maxY = float.NegativeInfinity;
        var maxZ = float.NegativeInfinity;

        for (var i = 0; i < attribute.count; i++)
        {
            var x = attr.GetX(i);
            var y = attr.GetY(i);
            var z = attr.GetZ(i);

            if (x < minX) minX = x;
            if (y < minY) minY = y;
            if (z < minZ) minZ = z;

            if (x > maxX) maxX = x;
            if (y > maxY) maxY = y;
            if (z > maxZ) maxZ = z;
        }

        Min.X = minX;
        Min.Y = minY;
        Min.Z = minZ;

        Max.X = maxX;
        Max.Y = maxY;
        Max.Z = maxZ;
    }

    public void SetFromBufferAttribute(InterleavedBufferAttribute<float> attribute)
    {
        var minX = float.PositiveInfinity;
        var minY = float.PositiveInfinity;
        var minZ = float.PositiveInfinity;

        var maxX = float.NegativeInfinity;
        var maxY = float.NegativeInfinity;
        var maxZ = float.NegativeInfinity;

        for (var i = 0; i < attribute.count; i++)
        {
            var x = attribute.GetX(i);
            var y = attribute.GetY(i);
            var z = attribute.GetZ(i);

            if (x < minX) minX = x;
            if (y < minY) minY = y;
            if (z < minZ) minZ = z;

            if (x > maxX) maxX = x;
            if (y > maxY) maxY = y;
            if (z > maxZ) maxZ = z;
        }

        Min.X = minX;
        Min.Y = minY;
        Min.Z = minZ;

        Max.X = maxX;
        Max.Y = maxY;
        Max.Z = maxZ;
    }

    public Box3 SetFromPoints(List<Vector4> points)
    {
        MakeEmpty();
        for (var i = 0; i < points.Count; i++) ExpandByPoint(points[i]);
        return this;
    }

    public Box3 SetFromPoints(List<Vector3> points)
    {
        MakeEmpty();

        for (var i = 0; i < points.Count; i++) ExpandByPoint(points[i]);

        return this;
    }

    public Box3 SetFromCenterAndSize(Vector3 center, Vector3 size)
    {
        var HalfSize = _vector.Copy(size).MultiplyScalar(0.5f);

        Min.Copy(center).Sub(HalfSize);
        Max.Copy(center).Add(HalfSize);

        return this;
    }

    public Box3 SetFromObject(Object3D obj)
    {
        MakeEmpty();

        return ExpandByObject(obj);
    }

    public void MakeEmpty()
    {
        Min.X = Min.Y = Min.Z = float.PositiveInfinity;
        Max.X = Max.Y = Max.Z = float.NegativeInfinity;
    }

    public bool IsEmpty()
    {
        return Max.X < Min.X || Max.Y < Min.Y || Max.Z < Min.Z;
    }

    public Vector3 GetCenter(Vector3 target)
    {
        if (IsEmpty())
            target = new Vector3(0, 0, 0);
        else
            target.AddVectors(Min, Max).MultiplyScalar(0.5f);

        return target;
    }

    public Vector3 GetSize()
    {
        return IsEmpty() ? Vector3.Zero() : Max - Min;
    }

    public Box3 ExpandByPoint(Vector4 point)
    {
        _vector.Set(point.X, point.Y, point.Z);
        Min.Min(_vector);
        Max.Max(_vector);
        return this;
    }

    public Box3 ExpandByPoint(Vector3 point)
    {
        Min.Min(point);
        Max.Max(point);

        return this;
    }

    public Box3 ExpandByVector(Vector3 vector)
    {
        Min = Min - vector;
        Max = Max - vector;

        return this;
    }

    public Box3 ExpandByScalar(float scalar)
    {
        Min = Min.AddScalar(-scalar);
        Max = Min.AddScalar(scalar);

        return this;
    }

    public Box3 ExpandByObject(Object3D object3D)
    {
        object3D.UpdateWorldMatrix(false, false);

        var geometry = object3D.Geometry;

        if (geometry != null)
        {
            if (geometry.BoundingBox == null) geometry.ComputeBoundingBox();
            var _box = new Box3();
            _box.Copy(geometry.BoundingBox);
            _box.ApplyMatrix4(object3D.MatrixWorld);

            ExpandByPoint(_box.Min);
            ExpandByPoint(_box.Max);
        }

        var children = object3D.Children;

        for (var i = 0; i < children.Count; i++) ExpandByObject(children[i]);

        return this;
    }

    public bool ContainsPoint(Vector3 point)
    {
        return point.X < Min.X || point.X > Max.X ||
               point.Y < Min.Y || point.Y > Max.Y ||
               point.Z < Min.Z || point.Z > Max.Z
            ? false
            : true;
    }

    public bool ContainsBox(Box3 box)
    {
        return Min.X <= box.Min.X && box.Max.X <= Max.X &&
               Min.Y <= box.Min.Y && box.Max.Y <= Max.Y &&
               Min.Z <= box.Min.Z && box.Max.Z <= Max.Z;
    }

    public Vector3 GetParameter(Vector3 point)
    {
        var target = Vector3.Zero();

        target.X = (point.X - Min.X) / (Max.X - Min.X);
        target.Y = (point.Y - Min.Y) / (Max.Y - Min.Y);
        target.Z = (point.Z - Min.Z) / (Max.Z - Min.Z);

        return target;
    }

    public bool IntersectsBox(Box3 box)
    {
        // using 6 splitting planes to rule out intersections.
        return box.Max.X < Min.X || box.Min.X > Max.X ||
               box.Max.Y < Min.Y || box.Min.Y > Max.Y ||
               box.Max.Z < Min.Z || box.Min.Z > Max.Z
            ? false
            : true;
    }

    public bool IntersectsSphere(Sphere sphere)
    {
        ClampPoint(sphere.Center, _vector);

        return _vector.DistanceToSquared(sphere.Center) <= sphere.Radius * sphere.Radius;
    }

    public bool IntersectPlane(Plane plane)
    {
        return true;
    }

    public bool IntersectsTriangle(Triangle triangle)
    {
        if (IsEmpty()) return false;

        // compute box center and extents
        GetCenter(_center);
        _extents.SubVectors(Max, _center);

        // translate triangle to aabb origin
        _v0.SubVectors(triangle.a, _center);
        _v1.SubVectors(triangle.b, _center);
        _v2.SubVectors(triangle.c, _center);

        // compute edge vectors for triangle
        _f0.SubVectors(_v1, _v0);
        _f1.SubVectors(_v2, _v1);
        _f2.SubVectors(_v0, _v2);

        // test against axes that are given by cross product combinations of the edges of the triangle and the edges of the aabb
        // make an axis testing of each of the 3 sides of the aabb against each of the 3 sides of the triangle = 9 axis of separation
        // axis_ij = u_i x f_j (u0, u1, u2 = face normals of aabb = x,y,z axes vectors since aabb is axis aligned)
        var axes = new[]
        {
            0, -_f0.Z, _f0.Y, 0, -_f1.Z, _f1.Y, 0, -_f2.Z, _f2.Y,
            _f0.Z, 0, -_f0.X, _f1.Z, 0, -_f1.X, _f2.Z, 0, -_f2.X,
            -_f0.Y, _f0.X, 0, -_f1.Y, _f1.X, 0, -_f2.Y, _f2.X, 0
        };

        if (!SatForAxes(axes, _v0, _v1, _v2, _extents)) return false;

        // test 3 face normals from the aabb
        axes = new float[] { 1, 0, 0, 0, 1, 0, 0, 0, 1 };
        if (!SatForAxes(axes, _v0, _v1, _v2, _extents)) return false;

        // finally testing the face normal of the triangle
        // use already existing triangle edge vectors here
        _triangleNormal.CrossVectors(_f0, _f1);
        axes = new[] { _triangleNormal.X, _triangleNormal.Y, _triangleNormal.Z };

        return SatForAxes(axes, _v0, _v1, _v2, _extents);
    }

    public Vector3 ClampPoint(Vector3 point, Vector3 target)
    {
        return target.Copy(point).Clamp(Min, Max);
    }

    public float DistanceToPoint(Vector3 point)
    {
        var clampedPoint = _vector.Copy(point).Clamp(Min, Max);

        return clampedPoint.Sub(point).Length();
    }

    public Sphere GetBoundingSphere(Sphere target)
    {
        target.Center = GetCenter(target.Center);
        target.Radius = GetSize().Length() * 0.5f;

        return target;
    }

    public Box3 Intersect(Box3 box)
    {
        Min.Max(box.Min);
        Max.Min(box.Max);

        if (IsEmpty()) MakeEmpty();

        return this;
    }

    public Box3 Union(Box3 box)
    {
        Min.Min(box.Min);
        Max.Max(box.Max);

        return this;
    }

    public Box3 ApplyMatrix4(Matrix4 matrix)
    {
        // transform of empty box is an empty box.
        if (IsEmpty()) return this;

        // NOTE: I am using a binary pattern to specify all 2^3 combinations below
        _points[0].Set(Min.X, Min.Y, Min.Z).ApplyMatrix4(matrix); // 000
        _points[1].Set(Min.X, Min.Y, Max.Z).ApplyMatrix4(matrix); // 001
        _points[2].Set(Min.X, Max.Y, Min.Z).ApplyMatrix4(matrix); // 010
        _points[3].Set(Min.X, Max.Y, Max.Z).ApplyMatrix4(matrix); // 011
        _points[4].Set(Max.X, Min.Y, Min.Z).ApplyMatrix4(matrix); // 100
        _points[5].Set(Max.X, Min.Y, Max.Z).ApplyMatrix4(matrix); // 101
        _points[6].Set(Max.X, Max.Y, Min.Z).ApplyMatrix4(matrix); // 110
        _points[7].Set(Max.X, Max.Y, Max.Z).ApplyMatrix4(matrix); // 111

        SetFromPoints(_points);

        return this;
    }

    public Box3 Translate(Vector3 offset)
    {
        Min = Min + offset;
        Max = Max + offset;

        return this;
    }

    public bool SatForAxes(float[] axes, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 extents)
    {
        int i, j;

        for (i = 0, j = axes.Length - 3; i <= j; i += 3)
        {
            _testAxis.FromArray(axes, i);
            // project the aabb onto the seperating axis
            var r = extents.X * Math.Abs(_testAxis.X) + extents.Y * Math.Abs(_testAxis.Y) +
                    extents.Z * Math.Abs(_testAxis.Z);
            // project all 3 vertices of the triangle onto the seperating axis
            var p0 = v0.Dot(_testAxis);
            var p1 = v1.Dot(_testAxis);
            var p2 = v2.Dot(_testAxis);
            // actual test, basically see if either of the most extreme of the triangle points intersects r
            if (Math.Max(-Math.Max(p0, Math.Max(p1, p2)), Math.Min(p0, Math.Min(p1, p2))) > r)
                // points of the projected triangle are outside the projected half-length of the aabb
                // the axis is seperating and we can exit
                return false;
        }

        return true;
    }
}