namespace THREE;

[Serializable]
public class Ray
{
    private Vector3 _diff = Vector3.Zero();

    private Vector3 _edge1 = Vector3.Zero();
    private Vector3 _edge2 = Vector3.Zero();
    private Vector3 _normal = Vector3.Zero();
    private Vector3 _segCenter = Vector3.Zero();
    private Vector3 _segDir = Vector3.Zero();
    private Vector3 _vector = Vector3.Zero();
    public Vector3 direction;
    public Vector3 origin;

    public Ray(Vector3 origin = null, Vector3 direction = null)
    {
        this.origin = origin != null ? origin : new Vector3();
        this.direction = direction != null ? direction : new Vector3(0, 0, -1);
    }

    public Ray(Ray source)
    {
        origin.Copy(source.origin);
        direction.Copy(source.direction);
    }

    public Ray Set(Vector3 origin, Vector3 direction)
    {
        this.origin.Copy(origin);
        this.direction.Copy(direction);
        return this;
    }

    public Ray Clone()
    {
        return new Ray(this);
    }

    public Ray copy(Ray source)
    {
        origin.Copy(source.origin);
        direction.Copy(source.direction);
        return this;
    }

    public Vector3 At(float t, Vector3 target = null)
    {
        Vector3 result;
        if (target == null)
        {
            result = new Vector3();
            return result.Copy(direction).MultiplyScalar(t).Add(origin);
        }

        return target.Copy(direction).MultiplyScalar(t).Add(origin);
    }

    public Vector3 At(float t, Vector4 target = null)
    {
        var result = new Vector3();
        if (target == null) return result.Copy(direction).MultiplyScalar(t).Add(origin);

        result.Copy(direction).MultiplyScalar(t).Add(origin);
        target.Set(result.X, result.Y, result.Z, 0);
        return result;
    }

    public Ray LookAt(Vector3 v)
    {
        direction.Copy(v).Sub(origin).Normalize();
        return this;
    }

    public Ray Recast(float t)
    {
        origin.Copy(At(t, _vector));
        return this;
    }

    public Vector3 ClosestPointToPoint(Vector3 point, Vector3 target = null)
    {
        Vector3 result;
        if (target == null)
            result = new Vector3();
        else
            result = target;

        result.SubVectors(point, origin);
        var directionDistance = target.Dot(direction);

        if (directionDistance < 0) return result.Copy(origin);
        return result.Copy(direction).MultiplyScalar(directionDistance).Add(origin);
    }

    public float DistanceToPoint(Vector3 point)
    {
        return (float)Math.Sqrt(DistanceSqToPoint(point));
    }

    public float DistanceSqToPoint(Vector3 point)
    {
        var directionDistance = _vector.SubVectors(point, origin).Dot(direction);

        if (directionDistance < 0) return origin.DistanceToSquared(point);
        _vector.Copy(direction).MultiplyScalar(directionDistance).Add(origin);

        return _vector.DistanceToSquared(point);
    }

    public float DistanceSqToSegment(Vector3 v0, Vector3 v1, Vector3 optionalPointOnRay = null,
        Vector3 optionalPointOnSegment = null)
    {
        // from http://www.geometrictools.com/GTEngine/Include/Mathematics/GteDistRaySegment.h
        // It returns the min distance between the ray and the segment
        // defined by v0 and v1
        // It can also set two optional targets :
        // - The closest point on the ray
        // - The closest point on the segment

        _segCenter.Copy(v0).Add(v1).MultiplyScalar(0.5f);
        _segDir.Copy(v1).Sub(v0).Normalize();
        _diff.Copy(origin).Sub(_segCenter);

        var segExtent = v0.DistanceTo(v1) * 0.5f;
        var a01 = -direction.Dot(_segDir);
        var b0 = _diff.Dot(direction);
        var b1 = -_diff.Dot(_segDir);
        var c = _diff.LengthSq();
        var det = Math.Abs(1 - a01 * a01);
        float s0, s1, sqrDist, extDet;

        if (det > 0)
        {
            // The ray and segment are not parallel.

            s0 = a01 * b1 - b0;
            s1 = a01 * b0 - b1;
            extDet = segExtent * det;

            if (s0 >= 0)
            {
                if (s1 >= -extDet)
                {
                    if (s1 <= extDet)
                    {
                        // region 0
                        // Minimum at interior points of ray and segment.

                        var invDet = 1 / det;
                        s0 *= invDet;
                        s1 *= invDet;
                        sqrDist = s0 * (s0 + a01 * s1 + 2 * b0) + s1 * (a01 * s0 + s1 + 2 * b1) + c;
                    }
                    else
                    {
                        // region 1

                        s1 = segExtent;
                        s0 = Math.Max(0, -(a01 * s1 + b0));
                        sqrDist = -s0 * s0 + s1 * (s1 + 2 * b1) + c;
                    }
                }
                else
                {
                    // region 5

                    s1 = -segExtent;
                    s0 = Math.Max(0, -(a01 * s1 + b0));
                    sqrDist = -s0 * s0 + s1 * (s1 + 2 * b1) + c;
                }
            }
            else
            {
                if (s1 <= -extDet)
                {
                    // region 4

                    s0 = Math.Max(0, -(-a01 * segExtent + b0));
                    s1 = s0 > 0 ? -segExtent : Math.Min(Math.Max(-segExtent, -b1), segExtent);
                    sqrDist = -s0 * s0 + s1 * (s1 + 2 * b1) + c;
                }
                else if (s1 <= extDet)
                {
                    // region 3

                    s0 = 0;
                    s1 = Math.Min(Math.Max(-segExtent, -b1), segExtent);
                    sqrDist = s1 * (s1 + 2 * b1) + c;
                }
                else
                {
                    // region 2

                    s0 = Math.Max(0, -(a01 * segExtent + b0));
                    s1 = s0 > 0 ? segExtent : Math.Min(Math.Max(-segExtent, -b1), segExtent);
                    sqrDist = -s0 * s0 + s1 * (s1 + 2 * b1) + c;
                }
            }
        }
        else
        {
            // Ray and segment are parallel.

            s1 = a01 > 0 ? -segExtent : segExtent;
            s0 = Math.Max(0, -(a01 * s1 + b0));
            sqrDist = -s0 * s0 + s1 * (s1 + 2 * b1) + c;
        }

        if (optionalPointOnRay != null) optionalPointOnRay.Copy(direction).MultiplyScalar(s0).Add(origin);

        if (optionalPointOnSegment != null) optionalPointOnSegment.Copy(_segDir).MultiplyScalar(s1).Add(_segCenter);

        return sqrDist;
    }

    public Vector3 IntersectSphere(Sphere sphere, Vector3 target)
    {
        _vector.SubVectors(sphere.Center, origin);
        var tca = _vector.Dot(direction);
        var d2 = _vector.Dot(_vector) - tca * tca;
        var radius2 = sphere.Radius * sphere.Radius;

        if (d2 > radius2) return null;

        var thc = (float)Math.Sqrt(radius2 - d2);

        // t0 = first intersect point - entrance on front of sphere
        var t0 = tca - thc;

        // t1 = second intersect point - exit point on back of sphere
        var t1 = tca + thc;

        // test to see if both t0 and t1 are behind the ray - if so, return null
        if (t0 < 0 && t1 < 0) return null;

        // test to see if t0 is behind the ray:
        // if it is, the ray is inside the sphere, so return the second exit point scaled by t1,
        // in order to always return an intersect point that is in front of the ray.
        if (t0 < 0) return At(t1, target);

        // else t0 is in front of the ray, so return the first collision point scaled by t0
        return At(t0, target);
    }

    public bool IntersectsSphere(Sphere sphere)
    {
        return DistanceSqToPoint(sphere.Center) <= sphere.Radius * sphere.Radius;
    }

    public float? DistanceToPlane(Plane plane)
    {
        var denominator = plane.Normal.Dot(direction);

        if (denominator == 0)
        {
            // line is coplanar, return origin
            if (plane.DistanceToPoint(origin) == 0) return 0;

            // Null is preferable to undefined since undefined means.... it is undefined

            return null;
        }

        var t = -(origin.Dot(plane.Normal) + plane.Constant) / denominator;

        // Return if the ray never intersects the plane

        return t >= 0 ? t : null;
    }

    public Vector3 IntersectPlane(Plane plane, Vector3 target)
    {
        var t = DistanceToPlane(plane);

        if (t == null) return null;

        return At(t.Value, target);
    }

    public bool IntersectsPlane(Plane plane)
    {
        // check if the ray lies on the plane first

        var distToPoint = plane.DistanceToPoint(origin);

        if (distToPoint == 0) return true;

        var denominator = plane.Normal.Dot(direction);

        if (denominator * distToPoint < 0) return true;

        // ray origin is behind the plane (and is pointing behind it)

        return false;
    }

    public Vector3 IntersectBox(Box3 box, Vector3 target)
    {
        float tmin, tmax, tymin, tymax, tzmin, tzmax;

        var invdirx = 1 / direction.X;
        var invdiry = 1 / direction.Y;
        var invdirz = 1 / direction.Z;


        if (invdirx >= 0)
        {
            tmin = (box.Min.X - origin.X) * invdirx;
            tmax = (box.Max.X - origin.X) * invdirx;
        }
        else
        {
            tmin = (box.Max.X - origin.X) * invdirx;
            tmax = (box.Min.X - origin.X) * invdirx;
        }

        if (invdiry >= 0)
        {
            tymin = (box.Min.Y - origin.Y) * invdiry;
            tymax = (box.Max.Y - origin.Y) * invdiry;
        }
        else
        {
            tymin = (box.Max.Y - origin.Y) * invdiry;
            tymax = (box.Min.Y - origin.Y) * invdiry;
        }

        if (tmin > tymax || tymin > tmax) return null;

        // These lines also handle the case where tmin or tmax is NaN
        // (result of 0 * Infinity). x !== x returns true if x is NaN

        if (tymin > tmin) tmin = tymin;

        if (tymax < tmax) tmax = tymax;

        if (invdirz >= 0)
        {
            tzmin = (box.Min.Z - origin.Z) * invdirz;
            tzmax = (box.Max.Z - origin.Z) * invdirz;
        }
        else
        {
            tzmin = (box.Max.Z - origin.Z) * invdirz;
            tzmax = (box.Min.Z - origin.Z) * invdirz;
        }

        if (tmin > tzmax || tzmin > tmax) return null;

        if (tzmin > tmin || tmin != tmin) tmin = tzmin;

        if (tzmax < tmax || tmax != tmax) tmax = tzmax;

        //return point closest to the ray (positive side)

        if (tmax < 0) return null;

        return At(tmin >= 0 ? tmin : tmax, target);
    }

    public bool IntersectsBox(Box3 box)
    {
        return IntersectBox(box, _vector) != null;
    }

    public Vector3 IntersectTriangle(Vector3 a, Vector3 b, Vector3 c, bool backfaceCulling, Vector3 target = null)
    {
        // Compute the offset origin, edges, and normal.

        // from http://www.geometrictools.com/GTEngine/Include/Mathematics/GteIntrRay3Triangle3.h

        _edge1.SubVectors(b, a);
        _edge2.SubVectors(c, a);
        _normal.CrossVectors(_edge1, _edge2);

        // Solve Q + t*D = b1*E1 + b2*E2 (Q = kDiff, D = ray direction,
        // E1 = kEdge1, E2 = kEdge2, N = Cross(E1,E2)) by
        //   |Dot(D,N)|*b1 = sign(Dot(D,N))*Dot(D,Cross(Q,E2))
        //   |Dot(D,N)|*b2 = sign(Dot(D,N))*Dot(D,Cross(E1,Q))
        //   |Dot(D,N)|*t = -sign(Dot(D,N))*Dot(Q,N)
        var DdN = direction.Dot(_normal);
        float sign;

        if (DdN > 0)
        {
            if (backfaceCulling) return null;
            sign = 1;
        }
        else if (DdN < 0)
        {
            sign = -1;
            DdN = -DdN;
        }
        else
        {
            return null;
        }

        _diff.SubVectors(origin, a);
        var DdQxE2 = sign * direction.Dot(_edge2.CrossVectors(_diff, _edge2));

        // b1 < 0, no intersection
        if (DdQxE2 < 0) return null;

        var DdE1xQ = sign * direction.Dot(_edge1.Cross(_diff));

        // b2 < 0, no intersection
        if (DdE1xQ < 0) return null;

        // b1+b2 > 1, no intersection
        if (DdQxE2 + DdE1xQ > DdN) return null;

        // Line intersects triangle, check if ray does.
        var QdN = -sign * _diff.Dot(_normal);

        // t < 0, no intersection
        if (QdN < 0) return null;

        // Ray intersects triangle.
        return At(QdN / DdN, target);
    }

    public Ray ApplyMatrix4(Matrix4 matrix4)
    {
        origin.ApplyMatrix4(matrix4);
        direction.TransformDirection(matrix4);

        return this;
    }

    public bool Equals(Ray ray)
    {
        return ray.origin.Equals(origin) && ray.direction.Equals(direction);
    }
}