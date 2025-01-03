using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace THREE;

[Serializable]
public class Vector4 : IEquatable<Vector4>, ICloneable, INotifyPropertyChanged
{
    public float W;
    public float X;

    public float Y;

    public float Z;

    public Vector4()
    {
        X = Y = Z = 0;
        W = 1;
    }

    public Vector4(float x, float y, float z, float w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    public object Clone()
    {
        return new Vector4(X, Y, Z, W);
    }

    public bool Equals(Vector4 v)
    {
        return X == v.X && Y == v.Y && Z == v.Z && W == v.W;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public static Vector4 Zero()
    {
        return new Vector4(0, 0, 0, 0);
    }

    public static Vector4 One()
    {
        return new Vector4(1, 1, 1, 1);
    }

    public Vector4 Set(float x, float y, float z, float w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;

        return this;
    }

    public Vector4 SetScalar(float scalar)
    {
        X = scalar;
        Y = scalar;
        Z = scalar;
        W = scalar;

        return this;
    }

    public Vector4 SetX(float x)
    {
        X = x;
        return this;
    }

    public Vector4 SetY(float y)
    {
        Y = y;

        return this;
    }

    public Vector4 SetZ(float z)
    {
        Z = z;

        return this;
    }

    public Vector4 SetW(float w)
    {
        W = w;

        return this;
    }

    public Vector4 SetComponent(int index, float value)
    {
        switch (index)
        {
            case 0: X = value; break;
            case 1: Y = value; break;
            case 2: Z = value; break;
            case 3: W = value; break;
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
            case 3: return W;
            default: throw new IndexOutOfRangeException(string.Format("Index {0} is out of rangess", index));
        }
    }

    public Vector4 Copy(Vector4 v)
    {
        X = v.X;
        Y = v.Y;
        Z = v.Z;
        W = v.W;

        return this;
    }

    public Vector4 Add(Vector4 v)
    {
        X += v.X;
        Y += v.Y;
        Z += v.Z;
        W += v.W;
        return this;
    }

    public Vector4 AddScalar(float s)
    {
        X += s;
        Y += s;
        Z += s;
        W += s;
        return this;
    }

    public Vector4 AddVectors(Vector4 a, Vector4 b)
    {
        X = a.X + b.X;
        Y = a.Y + b.Y;
        Z = a.Z + b.Z;
        W = a.W + b.W;

        return this;
    }

    public Vector4 AddScaledVector(Vector4 v, float s)
    {
        X += v.X * s;
        Y += v.Y * s;
        Z += v.Z * s;
        W += v.W * s;

        return this;
    }

    public static Vector4 operator +(Vector4 v, Vector4 w)
    {
        var r = new Vector4();
        r.X = v.X + w.X;
        r.Y = v.Y + w.Y;
        r.Z = v.Z + w.Z;
        r.W = v.W + w.W;

        return r;
    }

    public static Vector4 operator +(Vector4 v, float s)
    {
        var r = new Vector4();

        r.X = v.X + s;
        r.Y = v.Y + s;
        r.Z = v.Z + s;
        r.W = v.W + s;

        return r;
    }

    public Vector4 Sub(Vector4 v)
    {
        X -= v.X;
        Y -= v.Y;
        Z -= v.Z;
        W -= v.W;

        return this;
    }

    public Vector4 SubScalar(float s)
    {
        X -= s;
        Y -= s;
        Z -= s;
        W -= s;

        return this;
    }

    public Vector4 SubVectors(Vector4 a, Vector4 b)
    {
        X = a.X - b.X;
        Y = a.Y - b.Y;
        Z = a.Z - b.Z;
        W = a.W - b.W;

        return this;
    }

    public static Vector4 operator -(Vector4 a, Vector4 b)
    {
        var r = new Vector4();

        r.X = a.X - b.X;
        r.Y = a.Y - b.Y;
        r.Z = a.Z - b.Z;
        r.W = a.W - b.W;

        return r;
    }

    public static Vector4 operator -(Vector4 a, float s)
    {
        var r = new Vector4();
        ;
        r.X = a.X - s;
        r.Y = a.Y - s;
        r.Z = a.Z - s;
        r.W = a.W - s;

        return r;
    }

    public Vector4 Multiply(Vector4 v)
    {
        X *= v.X;
        Y *= v.Y;
        Z *= v.Z;
        W *= v.W;

        return this;
    }

    public Vector4 MultiplyScalar(float s)
    {
        X *= s;
        Y *= s;
        Z *= s;
        W *= s;

        return this;
    }

    public Vector4 MultiplyVectors(Vector4 a, Vector4 b)
    {
        X = a.X * b.X;
        Y = a.Y * b.Y;
        Z = a.Z * b.Z;
        W = a.W * b.W;

        return this;
    }

    public static Vector4 operator *(Vector4 a, Vector4 b)
    {
        var r = new Vector4();
        r.X = a.X * b.X;
        r.Y = a.Y * b.Y;
        r.Z = a.Z * b.Z;
        r.W = a.W * b.W;

        return r;
    }

    public static Vector4 operator *(Vector4 a, float s)
    {
        var r = new Vector4();
        r.X = a.X * s;
        r.Y = a.Y * s;
        r.Z = a.Z * s;
        r.W = a.W * s;

        return r;
    }

    public Vector4 ApplyMatrix4(Matrix4 m)
    {
        float x = X, y = Y, z = Z, w = W;
        var e = m.Elements;

        X = e[0] * x + e[4] * y + e[8] * z + e[12] * w;
        Y = e[1] * x + e[5] * y + e[9] * z + e[13] * w;
        Z = e[2] * x + e[6] * y + e[10] * z + e[14] * w;
        W = e[3] * x + e[7] * y + e[11] * z + e[15] * w;

        return this;
    }

    public static Vector4 operator *(Matrix4 m, Vector4 a)
    {
        var x = a.X;
        var y = a.Y;
        var z = a.Z;
        var w = a.W;
        var e = m.Elements;

        var r = new Vector4();

        r.X = e[0] * x + e[4] * y + e[8] * z + e[12] * w;
        r.Y = e[1] * x + e[5] * y + e[9] * z + e[13] * w;
        r.Z = e[2] * x + e[6] * y + e[10] * z + e[14] * w;
        r.W = e[3] * x + e[7] * y + e[11] * z + e[15] * w;

        return r;
    }

    public Vector4 Divide(Vector4 v)
    {
        X /= v.X;
        Y /= v.Y;
        Z /= v.Z;
        W /= v.W;

        return this;
    }

    public Vector4 DivideScalar(float s)
    {
        return MultiplyScalar(1 / s);
    }

    public static Vector4 operator /(Vector4 a, Vector4 b)
    {
        var r = new Vector4();
        r.X = a.X / b.X;
        r.Y = a.Y / b.Y;
        r.Z = a.Z / b.Z;
        r.W = a.W / b.W;

        return r;
    }

    public static Vector4 operator /(Vector4 a, float s)
    {
        var r = new Vector4();
        r = a * (1 / s);

        return r;
    }

    public Vector4 SetAxisAngleFromQuaternion(Quaternion q)
    {
        // http://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToAngle/index.htm

        // q is assumed to be normalized

        W = 2 * (float)Math.Acos(q.W);

        var s = (float)Math.Sqrt(1 - q.W * q.W);

        if (s < 0.0001)
        {
            X = 1;
            Y = 0;
            Z = 0;
        }
        else
        {
            X = q.X / s;
            Y = q.Y / s;
            Z = q.Z / s;
        }

        return this;
    }

    public Vector4 SetAxisAngleFromRotationMatrix(Matrix4 m)
    {
        // http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToAngle/index.htm

        // assumes the upper 3x3 of m is a pure rotation matrix (i.e, unscaled)

        float angle,
            x,
            y,
            z, // variables for result
            epsilon = 0.01f, // margin to allow for rounding errors
            epsilon2 = 0.1f; // margin to distinguish between 0 and 180 degrees

        var te = m.Elements;

        float m11 = te[0],
            m12 = te[4],
            m13 = te[8],
            m21 = te[1],
            m22 = te[5],
            m23 = te[9],
            m31 = te[2],
            m32 = te[6],
            m33 = te[10];

        if (Math.Abs(m12 - m21) < epsilon &&
            Math.Abs(m13 - m31) < epsilon &&
            Math.Abs(m23 - m32) < epsilon)
        {
            // singularity found
            // first check for identity matrix which must have +1 for all terms
            // in leading diagonal and zero in other terms

            if (Math.Abs(m12 + m21) < epsilon2 &&
                Math.Abs(m13 + m31) < epsilon2 &&
                Math.Abs(m23 + m32) < epsilon2 &&
                Math.Abs(m11 + m22 + m33 - 3) < epsilon2)
            {
                // this singularity is identity matrix so angle = 0

                Set(1, 0, 0, 0);

                return this; // zero angle, arbitrary axis
            }

            // otherwise this singularity is angle = 180

            angle = (float)Math.PI;

            var xx = (m11 + 1) / 2;
            var yy = (m22 + 1) / 2;
            var zz = (m33 + 1) / 2;
            var xy = (m12 + m21) / 4;
            var xz = (m13 + m31) / 4;
            var yz = (m23 + m32) / 4;

            if (xx > yy && xx > zz)
            {
                // m11 is the largest diagonal term

                if (xx < epsilon)
                {
                    x = 0;
                    y = 0.707106781f;
                    z = 0.707106781f;
                }
                else
                {
                    x = (float)Math.Sqrt(xx);
                    y = xy / x;
                    z = xz / x;
                }
            }
            else if (yy > zz)
            {
                // m22 is the largest diagonal term

                if (yy < epsilon)
                {
                    x = 0.707106781f;
                    y = 0;
                    z = 0.707106781f;
                }
                else
                {
                    y = (float)Math.Sqrt(yy);
                    x = xy / y;
                    z = yz / y;
                }
            }
            else
            {
                // m33 is the largest diagonal term so base result on this

                if (zz < epsilon)
                {
                    x = 0.707106781f;
                    y = 0.707106781f;
                    z = 0;
                }
                else
                {
                    z = (float)Math.Sqrt(zz);
                    x = xz / z;
                    y = yz / z;
                }
            }

            Set(x, y, z, angle);

            return this; // return 180 deg rotation
        }

        // as we have reached here there are no singularities so we can handle normally

        var s = (float)Math.Sqrt((m32 - m23) * (m32 - m23) +
                                 (m13 - m31) * (m13 - m31) +
                                 (m21 - m12) * (m21 - m12)); // used to normalize

        if (Math.Abs(s) < 0.001) s = 1;

        // prevent divide by zero, should not happen if matrix is orthogonal and should be
        // caught by singularity test above, but I've left it in just in case

        X = (m32 - m23) / s;
        Y = (m13 - m31) / s;
        Z = (m21 - m12) / s;
        W = (float)Math.Acos((m11 + m22 + m33 - 1) / 2);

        return this;
    }

    public Vector4 Min(Vector4 v)
    {
        X = Math.Min(X, v.X);
        Y = Math.Min(Y, v.Y);
        Z = Math.Min(Z, v.Z);
        W = Math.Min(W, v.W);
        return this;
    }

    public Vector4 Max(Vector4 v)
    {
        X = Math.Max(X, v.X);
        Y = Math.Max(Y, v.Y);
        Z = Math.Max(Z, v.Z);
        W = Math.Max(W, v.W);

        return this;
    }

    public Vector4 Clamp(Vector4 min, Vector4 max)
    {
        X = Math.Max(min.X, Math.Min(max.X, X));
        Y = Math.Max(min.Y, Math.Min(max.Y, Y));
        Z = Math.Max(min.Z, Math.Min(max.Z, Z));
        W = Math.Max(min.W, Math.Min(max.W, W));

        return this;
    }

    public Vector4 ClampScalar(float minVal, float maxVal)
    {
        X = Math.Max(minVal, Math.Min(maxVal, X));
        Y = Math.Max(minVal, Math.Min(maxVal, Y));
        Z = Math.Max(minVal, Math.Min(maxVal, Z));
        W = Math.Max(minVal, Math.Min(maxVal, W));

        return this;
    }

    public Vector4 ClampLength(float min, float max)
    {
        var length = Length();

        return DivideScalar(length != 0 ? length : 1).MultiplyScalar(Math.Max(min, Math.Min(max, length)));
    }

    public Vector4 Floor()
    {
        X = (float)Math.Floor(X);
        Y = (float)Math.Floor(Y);
        Z = (float)Math.Floor(Z);
        W = (float)Math.Floor(W);

        return this;
    }

    public Vector4 Ceil()
    {
        X = (float)Math.Ceiling(X);
        Y = (float)Math.Ceiling(Y);
        Z = (float)Math.Ceiling(Z);
        W = (float)Math.Ceiling(W);

        return this;
    }

    public Vector4 Round()
    {
        X = (float)Math.Round(X);
        Y = (float)Math.Round(Y);
        Z = (float)Math.Round(Z);
        W = (float)Math.Round(W);

        return this;
    }

    public Vector4 RoundToZero()
    {
        X = X < 0 ? (float)Math.Ceiling(X) : (float)Math.Floor(X);
        Y = Y < 0 ? (float)Math.Ceiling(Y) : (float)Math.Floor(Y);
        Z = Z < 0 ? (float)Math.Ceiling(Z) : (float)Math.Floor(Z);
        W = W < 0 ? (float)Math.Ceiling(W) : (float)Math.Floor(W);

        return this;
    }

    public Vector4 Negate()
    {
        X = -X;
        Y = -Y;
        Z = -Z;
        W = -W;

        return this;
    }

    public float Dot(Vector4 v)
    {
        return X * v.X + Y * v.Y + Z * v.Z + W * v.W;
    }

    public float LengthSq()
    {
        return X * X + Y * Y + Z * Z + W * W;
    }

    public float Length()
    {
        return (float)Math.Sqrt(LengthSq());
    }

    public float ManhattanLength()
    {
        return Math.Abs(X) + Math.Abs(Y) + Math.Abs(Z) + Math.Abs(W);
    }

    public Vector4 Normalize()
    {
        return DivideScalar(Length() != 0 ? Length() : 1);
    }

    public Vector4 SetLength(float length)
    {
        return Normalize().MultiplyScalar(length);
    }

    public Vector4 Lerp(Vector4 v, float alpha)
    {
        X += (v.X - X) * alpha;
        Y += (v.Y - Y) * alpha;
        Z += (v.Z - Z) * alpha;
        W += (v.W - W) * alpha;

        return this;
    }

    public Vector4 LerpVectors(Vector4 v1, Vector4 v2, float alpha)
    {
        return SubVectors(v2, v1).MultiplyScalar(alpha).Add(v1);
    }

    public Vector4 FromArray(float[] array, int? offset = null)
    {
        var index = 0;
        if (offset != null) index = offset.Value;

        X = array[index];
        Y = array[index + 1];
        Z = array[index + 2];
        W = array[index + 3];

        return this;
    }

    public float[] ToArray(float[] array = null, int? offset = null)
    {
        var index = 0;
        if (array == null) array = new float[4];
        if (offset != null) index = offset.Value;

        array[index] = X;
        array[index + 1] = Y;
        array[index + 2] = Z;
        array[index + 3] = W;

        return array;
    }

    public Vector4 FromBufferAttribute(BufferAttribute<float> attribute, int index)
    {
        X = attribute.GetX(index);
        Y = attribute.GetY(index);
        Z = attribute.GetZ(index);
        W = attribute.GetW(index);

        return this;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        var handler = PropertyChanged;
        if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }
}