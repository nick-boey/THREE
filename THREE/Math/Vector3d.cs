using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace THREE;

[Serializable]
public class Vector3d : IEquatable<Vector3d>, INotifyPropertyChanged
{
    public double X;

    public double Y;

    public double Z;

    public Vector3d()
    {
        X = Y = Z = 0;
    }

    public Vector3d(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public double this[char dirchar]
    {
        get
        {
            if (dirchar == 'x')
                return X;
            if (dirchar == 'y')
                return Y;
            if (dirchar == 'z')
                return Z;
            return 0;
        }
        set
        {
            if (dirchar == 'x')
                X = value;
            else if (dirchar == 'y')
                Y = value;
            else if (dirchar == 'z')
                Z = value;
            else
                return;
        }
    }

    public bool Equals(Vector3d v)
    {
        return X == v.X && Y == v.Y && Z == v.Z;
    }

    public event PropertyChangedEventHandler PropertyChanged;


    public static Vector3d Zero()
    {
        return new Vector3d(0, 0, 0);
    }

    public static Vector3d One()
    {
        return new Vector3d(1, 1, 1);
    }

    public Vector3d Set(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;

        return this;
    }

    public Vector3d SetScalar(double scalar)
    {
        X = scalar;
        Y = scalar;
        Z = scalar;

        return this;
    }

    public Vector3d SetX(double x)
    {
        X = x;
        return this;
    }

    public Vector3d SetY(double y)
    {
        Y = y;

        return this;
    }

    public Vector3d SetZ(double z)
    {
        Z = z;

        return this;
    }

    public Vector3d SetComponent(int index, double value)
    {
        switch (index)
        {
            case 0: X = value; break;
            case 1: Y = value; break;
            case 2: Z = value; break;
            default: throw new IndexOutOfRangeException(string.Format("Index {0} is out of rangess", index));
        }

        return this;
    }

    public double GetComponent(int index)
    {
        switch (index)
        {
            case 0: return X;
            case 1: return Y;
            case 2: return Z;
            default: throw new IndexOutOfRangeException(string.Format("Index {0} is out of rangess", index));
        }
    }

    public Vector3d Clone()
    {
        return new Vector3d(X, Y, Z);
    }

    public Vector3d Copy(Vector3d v)
    {
        X = v.X;
        Y = v.Y;
        Z = v.Z;

        return this;
    }

    public Vector3d Add(Vector3d v)
    {
        X += v.X;
        Y += v.Y;
        Z += v.Z;

        return this;
    }

    public Vector3d AddScalar(double s)
    {
        X += s;
        Y += s;
        Z += s;

        return this;
    }

    public Vector3d AddVectors(Vector3d a, Vector3d b)
    {
        X = a.X + b.X;
        Y = a.Y + b.Y;
        Z = a.Z + b.Z;

        return this;
    }

    public Vector3d AddScaledVector(Vector3d v, double s)
    {
        X += v.X * s;
        Y += v.Y * s;
        Z += v.Z * s;

        return this;
    }

    public static Vector3d operator +(Vector3d v, Vector3d w)
    {
        var r = new Vector3d();
        r.X = v.X + w.X;
        r.Y = v.Y + w.Y;
        r.Z = v.Z + w.Z;

        return r;
    }

    public static Vector3d operator +(Vector3d v, double s)
    {
        var r = new Vector3d();

        r.X = v.X + s;
        r.Y = v.Y + s;
        r.Z = v.Z + s;

        return r;
    }

    public Vector3d Sub(Vector3d v)
    {
        X -= v.X;
        Y -= v.Y;
        Z -= v.Z;

        return this;
    }

    public Vector3d SubScalar(double s)
    {
        X -= s;
        Y -= s;
        Z -= s;
        return this;
    }

    public Vector3d SubVectors(Vector3d a, Vector3d b)
    {
        X = a.X - b.X;
        Y = a.Y - b.Y;
        Z = a.Z - b.Z;

        return this;
    }

    public static Vector3d operator -(Vector3d a, Vector3d b)
    {
        var r = new Vector3d();

        r.X = a.X - b.X;
        r.Y = a.Y - b.Y;
        r.Z = a.Z - b.Z;
        return r;
    }

    public static Vector3d operator -(Vector3d a, double s)
    {
        var r = new Vector3d();
        ;
        r.X = a.X - s;
        r.Y = a.Y - s;
        r.Z = a.Z - s;

        return r;
    }

    public Vector3d Multiply(Vector3d v)
    {
        X *= v.X;
        Y *= v.Y;
        Z *= v.Z;

        return this;
    }

    public Vector3d MultiplyScalar(double s)
    {
        X *= s;
        Y *= s;
        Z *= s;

        return this;
    }

    public Vector3d MultiplyVectors(Vector3d a, Vector3d b)
    {
        X = a.X * b.X;
        Y = a.Y * b.Y;
        Z = a.Z * b.Z;

        return this;
    }

    public static Vector3d operator *(Vector3d a, Vector3d b)
    {
        var r = new Vector3d();
        r.X = a.X * b.X;
        r.Y = a.Y * b.Y;
        r.Z = a.Z * b.Z;
        return r;
    }

    public static Vector3d operator *(Vector3d a, double s)
    {
        var r = new Vector3d();
        r.X = a.X * s;
        r.Y = a.Y * s;
        r.Z = a.Z * s;
        return r;
    }

    public Vector3d ApplyEuler(Eulerd euler)
    {
        var quaternion = new Quaterniond();
        return ApplyQuaternion(quaternion.SetFromEuler(euler));
    }

    public Vector3d ApplyAxisAngle(Vector3d axis, double angle)
    {
        var quaternion = new Quaterniond();
        return ApplyQuaternion(quaternion.SetFromAxisAngle(axis, angle));
    }

    public Vector3d ApplyMatrix3(Matrix3d m)
    {
        var x = X;
        var y = Y;
        var z = Z;
        var e = m.Elements;

        X = e[0] * x + e[3] * y + e[6] * z;
        Y = e[1] * x + e[4] * y + e[7] * z;
        Z = e[2] * x + e[5] * y + e[8] * z;

        return this;
    }

    public static Vector3d operator *(Matrix3d m, Vector3d a)
    {
        var x = a.X;
        var y = a.Y;
        var z = a.Z;
        var e = m.Elements;

        var r = new Vector3d();

        r.X = e[0] * x + e[3] * y + e[6] * z;
        r.Y = e[1] * x + e[4] * y + e[7] * z;
        r.Z = e[2] * x + e[5] * y + e[8] * z;

        return r;
    }

    public Vector3d ApplyNormalMatrix(Matrix3d m)
    {
        return ApplyMatrix3(m).Normalize();
    }

    public Vector3d ApplyMatrix4(Matrix4d m)
    {
        var x = X;
        var y = Y;
        var z = Z;
        var e = m.Elements;

        var w = 1 / (e[3] * x + e[7] * y + e[11] * z + e[15]);

        X = (e[0] * x + e[4] * y + e[8] * z + e[12]) * w;
        Y = (e[1] * x + e[5] * y + e[9] * z + e[13]) * w;
        Z = (e[2] * x + e[6] * y + e[10] * z + e[14]) * w;

        return this;
    }

    public static Vector3d operator *(Matrix4 m, Vector3d v)
    {
        var x = v.X;
        var y = v.Y;
        var z = v.Z;
        var e = m.Elements;

        var w = 1 / (e[3] * x + e[7] * y + e[11] * z + e[15]);

        var r = new Vector3d();

        r.X = (e[0] * x + e[4] * y + e[8] * z + e[12]) * w;
        r.Y = (e[1] * x + e[5] * y + e[9] * z + e[13]) * w;
        r.Z = (e[2] * x + e[6] * y + e[10] * z + e[14]) * w;

        return r;
    }

    public Vector3d ApplyQuaternion(Quaterniond q)
    {
        var x = X;
        var y = Y;
        var z = Z;
        var qx = q.X;
        var qy = q.Y;
        var qz = q.Z;
        var qw = q.W;

        // calculate quat * vector

        var ix = qw * x + qy * z - qz * y;
        var iy = qw * y + qz * x - qx * z;
        var iz = qw * z + qx * y - qy * x;
        var iw = -qx * x - qy * y - qz * z;

        // calculate result * inverse quat

        X = ix * qw + iw * -qx + iy * -qz - iz * -qy;
        Y = iy * qw + iw * -qy + iz * -qx - ix * -qz;
        Z = iz * qw + iw * -qz + ix * -qy - iy * -qx;

        return this;
    }

    public Vector3d Project(Camera camera)
    {
        return ApplyMatrix4(camera.MatrixWorldInverse.ToMatrix4d()).ApplyMatrix4(camera.ProjectionMatrix.ToMatrix4d());
    }

    public Vector3d UnProject(Camera camera)
    {
        return ApplyMatrix4(camera.ProjectionMatrixInverse.ToMatrix4d()).ApplyMatrix4(camera.MatrixWorld.ToMatrix4d());
    }

    public Vector3d TransformDirection(Matrix4 m)
    {
        // input: THREE.Matrix4 affine matrix
        // vector interpreted as a direction

        var x = X;
        var y = Y;
        var z = Z;
        var e = m.Elements;

        X = e[0] * x + e[4] * y + e[8] * z;
        Y = e[1] * x + e[5] * y + e[9] * z;
        Z = e[2] * x + e[6] * y + e[10] * z;

        return Normalize();
    }

    public Vector3d Divide(Vector3d v)
    {
        X /= v.X;
        Y /= v.Y;
        Z /= v.Z;

        return this;
    }


    public Vector3d DivideScalar(double s)
    {
        return MultiplyScalar(1 / s);
    }

    public static Vector3d operator /(Vector3d a, Vector3d b)
    {
        var r = new Vector3d();
        r.X = a.X / b.X;
        r.Y = a.Y / b.Y;
        r.Z = a.Z / b.Z;
        return r;
    }

    public static Vector3d operator /(Vector3d a, double s)
    {
        var r = new Vector3d();
        r = a * (1 / s);

        return r;
    }


    public Vector3d Min(Vector3d v)
    {
        X = Math.Min(X, v.X);
        Y = Math.Min(Y, v.Y);
        Z = Math.Min(Z, v.Z);

        return this;
    }

    public Vector3d Max(Vector3d v)
    {
        X = Math.Max(X, v.X);
        Y = Math.Max(Y, v.Y);
        Z = Math.Max(Z, v.Z);

        return this;
    }

    public Vector3d Clamp(Vector3d min, Vector3d max)
    {
        X = Math.Max(min.X, Math.Min(max.X, X));
        Y = Math.Max(min.Y, Math.Min(max.Y, Y));
        Z = Math.Max(min.Z, Math.Min(max.Z, Z));

        return this;
    }

    public Vector3d ClampScalar(double minVal, double maxVal)
    {
        X = Math.Max(minVal, Math.Min(maxVal, X));
        Y = Math.Max(minVal, Math.Min(maxVal, Y));
        Z = Math.Max(minVal, Math.Min(maxVal, Z));

        return this;
    }

    public Vector3d ClampLength(double min, double max)
    {
        var length = Length();

        return DivideScalar(length != 0 ? length : 1).MultiplyScalar(Math.Max(min, Math.Min(max, length)));
    }

    public Vector3d Floor()
    {
        X = Math.Floor(X);
        Y = Math.Floor(Y);
        Z = Math.Floor(Z);

        return this;
    }

    public Vector3d Ceil()
    {
        X = Math.Ceiling(X);
        Y = Math.Ceiling(Y);
        Z = Math.Ceiling(Z);

        return this;
    }

    public Vector3d Round()
    {
        X = Math.Round(X);
        Y = Math.Round(Y);
        Z = Math.Round(Z);

        return this;
    }

    public Vector3d RoundToZero()
    {
        X = X < 0 ? Math.Ceiling(X) : Math.Floor(X);
        Y = Y < 0 ? Math.Ceiling(Y) : Math.Floor(Y);
        Z = Z < 0 ? Math.Ceiling(Z) : Math.Floor(Z);

        return this;
    }

    public Vector3d Negate()
    {
        X = -X;
        Y = -Y;
        Z = -Z;

        return this;
    }

    public static double Dot(Vector3d v1, Vector3d v2)
    {
        return v1.Dot(v2);
    }

    public double Dot(Vector3d v)
    {
        return X * v.X + Y * v.Y + Z * v.Z;
    }

    public double LengthSq()
    {
        return X * X + Y * Y + Z * Z;
    }

    public double Length()
    {
        return Math.Sqrt(LengthSq());
    }

    public double ManhattanLength()
    {
        return Math.Abs(X) + Math.Abs(Y) + Math.Abs(Z);
    }

    public Vector3d Normalize()
    {
        return DivideScalar(Length() != 0 ? Length() : 1);
    }

    public Vector3d SetLength(double length)
    {
        return Normalize().MultiplyScalar(length);
    }

    public Vector3d Lerp(Vector3d v, double alpha)
    {
        X += (v.X - X) * alpha;
        Y += (v.Y - Y) * alpha;
        Z += (v.Z - Z) * alpha;

        return this;
    }

    public Vector3d LerpVectors(Vector3d v1, Vector3d v2, double alpha)
    {
        return SubVectors(v2, v1).MultiplyScalar(alpha).Add(v1);
    }

    public Vector3d Cross(Vector3d v)
    {
        return CrossVectors(this, v);
    }

    public Vector3d CrossVectors(Vector3d a, Vector3d b)
    {
        double ax = a.X, ay = a.Y, az = a.Z;
        double bx = b.X, by = b.Y, bz = b.Z;

        X = ay * bz - az * by;
        Y = az * bx - ax * bz;
        Z = ax * by - ay * bx;

        return this;
    }

    public Vector3d ProjectOnVector(Vector3d v)
    {
        var scalar = v.Dot(this) / v.LengthSq();

        return Copy(v).MultiplyScalar(scalar);
        // return v*scalar;
    }

    public Vector3d ProjectOnPlane(Vector3d planeNormal)
    {
        var _vector = Zero();
        _vector.Copy(this).ProjectOnVector(planeNormal);

        return Sub(_vector);
    }

    public Vector3d Reflect(Vector3d normal)
    {
        // reflect incident vector off plane orthogonal to normal
        // normal is assumed to have unit length
        var _vector = Zero();
        return Sub(_vector.Copy(normal).MultiplyScalar(2 * Dot(normal)));
    }

    public double AngleTo(Vector3d v)
    {
        var denominator = Math.Sqrt(LengthSq() * v.LengthSq());

        if (denominator == 0) throw new Exception("THREE.Math.Vector3d: AngleTo() can\'t handle zero length vectors.");

        var theta = Dot(v) / denominator;

        // clamp, to handle numerical problems

        return Math.Acos(Clamp(theta, -1, 1));
    }

    public double Clamp(double value, double min, double max)
    {
        return Math.Max(min, Math.Min(max, value));
    }

    public double DistanceTo(Vector3d v)
    {
        return Math.Sqrt(DistanceToSquared(v));
    }

    public double DistanceToSquared(Vector3d v)
    {
        var dx = X - v.X;
        var dy = Y - v.Y;
        var dz = Z - v.Z;

        return dx * dx + dy * dy + dz * dz;
    }

    public double ManhattanDistanceTo(Vector3d v)
    {
        return Math.Abs(X - v.X) + Math.Abs(Y - v.Y) + Math.Abs(Z - v.Z);
    }

    public Vector3d SetFromSpherical(Spherical s)
    {
        return SetFromSphericalCoords(s.Radius, s.Phi, s.Theta);
    }

    public Vector3d SetFromSphericalCoords(double radius, double phi, double theta)
    {
        var sinPhiRadius = Math.Sin(phi) * radius;

        X = sinPhiRadius * Math.Sin(theta);
        Y = Math.Cos(phi) * radius;
        Z = sinPhiRadius * Math.Cos(theta);

        return this;
    }

    public Vector3d SetFromCylindrical(Cylindrical c)
    {
        return SetFromSphericalCoords(c.Radius, c.Theta, c.Y);
    }

    public Vector3d SetFromCylindricalCoords(double radius, double theta, double y)
    {
        X = radius * Math.Sin(theta);
        Y = y;
        Z = radius * Math.Cos(theta);

        return this;
    }

    public Vector3d SetFromMatrixPosition(Matrix4 m)
    {
        var e = m.Elements;

        X = e[12];
        Y = e[13];
        Z = e[14];

        return this;
    }

    public Vector3d SetFromMatrixScale(Matrix4d m)
    {
        var sx = SetFromMatrixColumn(m, 0).Length();
        var sy = SetFromMatrixColumn(m, 1).Length();
        var sz = SetFromMatrixColumn(m, 2).Length();

        X = sx;
        Y = sy;
        Z = sz;

        return this;
    }

    public Vector3d SetFromMatrixColumn(Matrix4d m, int index)
    {
        return FromArray(m.Elements, index * 4);
    }

    public Vector3d FromArray(double[] array, int? offset = null)
    {
        var index = 0;
        if (offset != null) index = offset.Value;
        var aLen = array.Length - 1;
        X = index <= aLen ? array[index] : double.NaN;
        Y = index <= aLen ? array[index + 1] : double.NaN;
        Z = index <= aLen ? array[index + 2] : double.NaN;

        return this;
    }

    public double[] ToArray(double[] array = null, int? offset = null)
    {
        var index = 0;
        if (array == null) array = new double[3];
        if (offset != null) index = offset.Value;

        array[index] = X;
        array[index + 1] = Y;
        array[index + 2] = Z;

        return array;
    }

    public Vector3d FromBufferAttribute(BufferAttribute<double> attribute, int index)
    {
        X = attribute.GetX(index);
        Y = attribute.GetY(index);
        Z = attribute.GetZ(index);

        return this;
    }

    public Vector2d ToVector2()
    {
        return new Vector2d(X, Y);
    }

    public Vector3 ToVector3()
    {
        return new Vector3((float)X, (float)Y, (float)Z);
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        var handler = PropertyChanged;
        if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }
}