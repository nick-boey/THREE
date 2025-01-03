using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace THREE;

[Serializable]
public class Vector3 : IEquatable<Vector3>, INotifyPropertyChanged
{
    public float X;

    public float Y;

    public float Z;

    public Vector3()
    {
        X = Y = Z = 0;
    }

    public Vector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public float this[char dirchar]
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

    public bool Equals(Vector3 v)
    {
        return X == v.X && Y == v.Y && Z == v.Z;
    }

    public event PropertyChangedEventHandler PropertyChanged;


    public static Vector3 Zero()
    {
        return new Vector3(0, 0, 0);
    }

    public static Vector3 One()
    {
        return new Vector3(1, 1, 1);
    }

    public Vector3 Set(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;

        return this;
    }

    public Vector3 SetScalar(float scalar)
    {
        X = scalar;
        Y = scalar;
        Z = scalar;

        return this;
    }

    public Vector3 SetX(float x)
    {
        X = x;
        return this;
    }

    public Vector3 SetY(float y)
    {
        Y = y;

        return this;
    }

    public Vector3 SetZ(float z)
    {
        Z = z;

        return this;
    }

    public Vector3 SetComponent(int index, float value)
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

    public float GetComponent(int index)
    {
        switch (index)
        {
            case 0: return X;
            case 1: return Y;
            case 2: return Z;
            default: throw new IndexOutOfRangeException(string.Format("Index {0} is out of rangess", index));
        }
    }

    public Vector3 Clone()
    {
        return new Vector3(X, Y, Z);
    }

    public Vector3 Copy(Vector3 v)
    {
        X = v.X;
        Y = v.Y;
        Z = v.Z;

        return this;
    }

    public Vector3 Add(Vector3 v)
    {
        X += v.X;
        Y += v.Y;
        Z += v.Z;

        return this;
    }

    public Vector3 AddScalar(float s)
    {
        X += s;
        Y += s;
        Z += s;

        return this;
    }

    public Vector3 AddVectors(Vector3 a, Vector3 b)
    {
        X = a.X + b.X;
        Y = a.Y + b.Y;
        Z = a.Z + b.Z;

        return this;
    }

    public Vector3 AddScaledVector(Vector3 v, float s)
    {
        X += v.X * s;
        Y += v.Y * s;
        Z += v.Z * s;

        return this;
    }

    public static Vector3 operator +(Vector3 v, Vector3 w)
    {
        var r = new Vector3();
        r.X = v.X + w.X;
        r.Y = v.Y + w.Y;
        r.Z = v.Z + w.Z;

        return r;
    }

    public static Vector3 operator +(Vector3 v, float s)
    {
        var r = new Vector3();

        r.X = v.X + s;
        r.Y = v.Y + s;
        r.Z = v.Z + s;

        return r;
    }

    public Vector3 Sub(Vector3 v)
    {
        X -= v.X;
        Y -= v.Y;
        Z -= v.Z;

        return this;
    }

    public Vector3 SubScalar(float s)
    {
        X -= s;
        Y -= s;
        Z -= s;
        return this;
    }

    public Vector3 SubVectors(Vector3 a, Vector3 b)
    {
        X = a.X - b.X;
        Y = a.Y - b.Y;
        Z = a.Z - b.Z;

        return this;
    }

    public static Vector3 operator -(Vector3 a, Vector3 b)
    {
        var r = new Vector3();

        r.X = a.X - b.X;
        r.Y = a.Y - b.Y;
        r.Z = a.Z - b.Z;
        return r;
    }

    public static Vector3 operator -(Vector3 a, float s)
    {
        var r = new Vector3();
        ;
        r.X = a.X - s;
        r.Y = a.Y - s;
        r.Z = a.Z - s;

        return r;
    }

    public Vector3 Multiply(Vector3 v)
    {
        X *= v.X;
        Y *= v.Y;
        Z *= v.Z;

        return this;
    }

    public Vector3 MultiplyScalar(float s)
    {
        X *= s;
        Y *= s;
        Z *= s;

        return this;
    }

    public Vector3 MultiplyVectors(Vector3 a, Vector3 b)
    {
        X = a.X * b.X;
        Y = a.Y * b.Y;
        Z = a.Z * b.Z;

        return this;
    }

    public static Vector3 operator *(Vector3 a, Vector3 b)
    {
        var r = new Vector3();
        r.X = a.X * b.X;
        r.Y = a.Y * b.Y;
        r.Z = a.Z * b.Z;
        return r;
    }

    public static Vector3 operator *(Vector3 a, float s)
    {
        var r = new Vector3();
        r.X = a.X * s;
        r.Y = a.Y * s;
        r.Z = a.Z * s;
        return r;
    }

    public Vector3 ApplyEuler(Euler euler)
    {
        var quaternion = new Quaternion();
        return ApplyQuaternion(quaternion.SetFromEuler(euler));
    }

    public Vector3 ApplyAxisAngle(Vector3 axis, float angle)
    {
        var quaternion = new Quaternion();
        return ApplyQuaternion(quaternion.SetFromAxisAngle(axis, angle));
    }

    public Vector3 ApplyMatrix3(Matrix3 m)
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

    public static Vector3 operator *(Matrix3 m, Vector3 a)
    {
        var x = a.X;
        var y = a.Y;
        var z = a.Z;
        var e = m.Elements;

        var r = new Vector3();

        r.X = e[0] * x + e[3] * y + e[6] * z;
        r.Y = e[1] * x + e[4] * y + e[7] * z;
        r.Z = e[2] * x + e[5] * y + e[8] * z;

        return r;
    }

    public Vector3 ApplyNormalMatrix(Matrix3 m)
    {
        return ApplyMatrix3(m).Normalize();
    }

    public Vector3 ApplyMatrix4(Matrix4 m)
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

    public static Vector3 operator *(Matrix4 m, Vector3 v)
    {
        var x = v.X;
        var y = v.Y;
        var z = v.Z;
        var e = m.Elements;

        var w = 1 / (e[3] * x + e[7] * y + e[11] * z + e[15]);

        var r = new Vector3();

        r.X = (e[0] * x + e[4] * y + e[8] * z + e[12]) * w;
        r.Y = (e[1] * x + e[5] * y + e[9] * z + e[13]) * w;
        r.Z = (e[2] * x + e[6] * y + e[10] * z + e[14]) * w;

        return r;
    }

    public Vector3 ApplyQuaternion(Quaternion q)
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

    public Vector3 Project(Camera camera)
    {
        return ApplyMatrix4(camera.MatrixWorldInverse).ApplyMatrix4(camera.ProjectionMatrix);
    }

    public Vector3 UnProject(Camera camera)
    {
        return ApplyMatrix4(camera.ProjectionMatrixInverse).ApplyMatrix4(camera.MatrixWorld);
    }

    public Vector3 TransformDirection(Matrix4 m)
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

    public Vector3 Divide(Vector3 v)
    {
        X /= v.X;
        Y /= v.Y;
        Z /= v.Z;

        return this;
    }


    public Vector3 DivideScalar(float s)
    {
        return MultiplyScalar(1 / s);
    }

    public static Vector3 operator /(Vector3 a, Vector3 b)
    {
        var r = new Vector3();
        r.X = a.X / b.X;
        r.Y = a.Y / b.Y;
        r.Z = a.Z / b.Z;
        return r;
    }

    public static Vector3 operator /(Vector3 a, float s)
    {
        var r = new Vector3();
        r = a * (1 / s);

        return r;
    }


    public Vector3 Min(Vector3 v)
    {
        X = Math.Min(X, v.X);
        Y = Math.Min(Y, v.Y);
        Z = Math.Min(Z, v.Z);

        return this;
    }

    public Vector3 Max(Vector3 v)
    {
        X = Math.Max(X, v.X);
        Y = Math.Max(Y, v.Y);
        Z = Math.Max(Z, v.Z);

        return this;
    }

    public Vector3 Clamp(Vector3 min, Vector3 max)
    {
        X = Math.Max(min.X, Math.Min(max.X, X));
        Y = Math.Max(min.Y, Math.Min(max.Y, Y));
        Z = Math.Max(min.Z, Math.Min(max.Z, Z));

        return this;
    }

    public Vector3 ClampScalar(float minVal, float maxVal)
    {
        X = Math.Max(minVal, Math.Min(maxVal, X));
        Y = Math.Max(minVal, Math.Min(maxVal, Y));
        Z = Math.Max(minVal, Math.Min(maxVal, Z));

        return this;
    }

    public Vector3 ClampLength(float min, float max)
    {
        var length = Length();

        return DivideScalar(length != 0 ? length : 1).MultiplyScalar(Math.Max(min, Math.Min(max, length)));
    }

    public Vector3 Floor()
    {
        X = (float)Math.Floor(X);
        Y = (float)Math.Floor(Y);
        Z = (float)Math.Floor(Z);

        return this;
    }

    public Vector3 Ceil()
    {
        X = (float)Math.Ceiling(X);
        Y = (float)Math.Ceiling(Y);
        Z = (float)Math.Ceiling(Z);

        return this;
    }

    public Vector3 Round()
    {
        X = (float)Math.Round(X);
        Y = (float)Math.Round(Y);
        Z = (float)Math.Round(Z);

        return this;
    }

    public Vector3 RoundToZero()
    {
        X = X < 0 ? (float)Math.Ceiling(X) : (float)Math.Floor(X);
        Y = Y < 0 ? (float)Math.Ceiling(Y) : (float)Math.Floor(Y);
        Z = Z < 0 ? (float)Math.Ceiling(Z) : (float)Math.Floor(Z);

        return this;
    }

    public Vector3 Negate()
    {
        X = -X;
        Y = -Y;
        Z = -Z;

        return this;
    }

    public static float Dot(Vector3 v1, Vector3 v2)
    {
        return v1.Dot(v2);
    }

    public float Dot(Vector3 v)
    {
        return X * v.X + Y * v.Y + Z * v.Z;
    }

    public float LengthSq()
    {
        return X * X + Y * Y + Z * Z;
    }

    public float Length()
    {
        return (float)Math.Sqrt(LengthSq());
    }

    public float ManhattanLength()
    {
        return Math.Abs(X) + Math.Abs(Y) + Math.Abs(Z);
    }

    public Vector3 Normalize()
    {
        return DivideScalar(Length() != 0 ? Length() : 1);
    }

    public Vector3 SetLength(float length)
    {
        return Normalize().MultiplyScalar(length);
    }

    public Vector3 Lerp(Vector3 v, float alpha)
    {
        X += (v.X - X) * alpha;
        Y += (v.Y - Y) * alpha;
        Z += (v.Z - Z) * alpha;

        return this;
    }

    public Vector3 LerpVectors(Vector3 v1, Vector3 v2, float alpha)
    {
        return SubVectors(v2, v1).MultiplyScalar(alpha).Add(v1);
    }

    public Vector3 Cross(Vector3 v)
    {
        return CrossVectors(this, v);
    }

    public Vector3 CrossVectors(Vector3 a, Vector3 b)
    {
        float ax = a.X, ay = a.Y, az = a.Z;
        float bx = b.X, by = b.Y, bz = b.Z;

        X = ay * bz - az * by;
        Y = az * bx - ax * bz;
        Z = ax * by - ay * bx;

        return this;
    }

    public Vector3 ProjectOnVector(Vector3 v)
    {
        var scalar = v.Dot(this) / v.LengthSq();

        return Copy(v).MultiplyScalar(scalar);
        // return v*scalar;
    }

    public Vector3 ProjectOnPlane(Vector3 planeNormal)
    {
        var _vector = Zero();
        _vector.Copy(this).ProjectOnVector(planeNormal);

        return Sub(_vector);
    }

    public Vector3 Reflect(Vector3 normal)
    {
        // reflect incident vector off plane orthogonal to normal
        // normal is assumed to have unit length
        var _vector = Zero();
        return Sub(_vector.Copy(normal).MultiplyScalar(2 * Dot(normal)));
    }

    public float AngleTo(Vector3 v)
    {
        var denominator = Math.Sqrt(LengthSq() * v.LengthSq());

        if (denominator == 0) throw new Exception("THREE.Math.Vector3: AngleTo() can\'t handle zero length vectors.");

        var theta = (float)(Dot(v) / denominator);

        // clamp, to handle numerical problems

        return (float)Math.Acos(Clamp(theta, -1, 1));
    }

    public float Clamp(float value, float min, float max)
    {
        return Math.Max(min, Math.Min(max, value));
    }

    public float DistanceTo(Vector3 v)
    {
        return (float)Math.Sqrt(DistanceToSquared(v));
    }

    public float DistanceToSquared(Vector3 v)
    {
        var dx = X - v.X;
        var dy = Y - v.Y;
        var dz = Z - v.Z;

        return dx * dx + dy * dy + dz * dz;
    }

    public float ManhattanDistanceTo(Vector3 v)
    {
        return Math.Abs(X - v.X) + Math.Abs(Y - v.Y) + Math.Abs(Z - v.Z);
    }

    public Vector3 SetFromSpherical(Spherical s)
    {
        return SetFromSphericalCoords(s.Radius, s.Phi, s.Theta);
    }

    public Vector3 SetFromSphericalCoords(float radius, float phi, float theta)
    {
        var sinPhiRadius = Math.Sin(phi) * radius;

        X = (float)(sinPhiRadius * Math.Sin(theta));
        Y = (float)(Math.Cos(phi) * radius);
        Z = (float)(sinPhiRadius * Math.Cos(theta));

        return this;
    }

    public Vector3 SetFromCylindrical(Cylindrical c)
    {
        return SetFromSphericalCoords(c.Radius, c.Theta, c.Y);
    }

    public Vector3 SetFromCylindricalCoords(float radius, float theta, float y)
    {
        X = (float)(radius * Math.Sin(theta));
        Y = y;
        Z = (float)(radius * Math.Cos(theta));

        return this;
    }

    public Vector3 SetFromMatrixPosition(Matrix4 m)
    {
        var e = m.Elements;

        X = e[12];
        Y = e[13];
        Z = e[14];

        return this;
    }

    public Vector3 SetFromMatrixScale(Matrix4 m)
    {
        var sx = SetFromMatrixColumn(m, 0).Length();
        var sy = SetFromMatrixColumn(m, 1).Length();
        var sz = SetFromMatrixColumn(m, 2).Length();

        X = sx;
        Y = sy;
        Z = sz;

        return this;
    }

    public Vector3 SetFromMatrixColumn(Matrix4 m, int index)
    {
        return FromArray(m.Elements, index * 4);
    }

    public Vector3 FromArray(float[] array, int? offset = null)
    {
        var index = 0;
        if (offset != null) index = offset.Value;
        var aLen = array.Length - 1;
        X = index <= aLen ? array[index] : float.NaN;
        Y = index <= aLen ? array[index + 1] : float.NaN;
        Z = index <= aLen ? array[index + 2] : float.NaN;

        return this;
    }

    public float[] ToArray(float[] array = null, int? offset = null)
    {
        var index = 0;
        if (array == null) array = new float[3];
        if (offset != null) index = offset.Value;

        array[index] = X;
        array[index + 1] = Y;
        array[index + 2] = Z;

        return array;
    }

    public Vector3 FromBufferAttribute(IBufferAttribute attribute, int index)
    {
        var attr = attribute as BufferAttribute<float>;
        X = attr.GetX(index);
        Y = attr.GetY(index);
        Z = attr.GetZ(index);

        return this;
    }

    public Vector3 FromBufferAttribute(InterleavedBufferAttribute<float> attribute, int index)
    {
        X = attribute.GetX(index);
        Y = attribute.GetY(index);
        Z = attribute.GetZ(index);

        return this;
    }

    public Vector2 ToVector2()
    {
        return new Vector2(X, Y);
    }

    public Vector3d ToVector3d()
    {
        return new Vector3d(X, Y, Z);
    }

    public Vector4 ToVector4()
    {
        return new Vector4(X, Y, Z, 1);
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        var handler = PropertyChanged;
        if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }
}