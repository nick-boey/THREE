using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace THREE;

[Serializable]
public class Vector2 : IEquatable<Vector2>, INotifyPropertyChanged
{
    public float X;

    public float Y;

    public Vector2()
    {
        X = Y = 0;
    }

    public Vector2(float x, float y)
    {
        X = x;

        Y = y;
    }


    public int Width
    {
        get => (int)X;
        set => X = value;
    }

    public int Height
    {
        get => (int)Y;
        set => Y = value;
    }

    public bool Equals(Vector2 v)
    {
        return X == v.X && Y == v.Y;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public static Vector2 Zero()
    {
        return new Vector2(0, 0);
    }

    public Vector2 Set(float x, float y)
    {
        X = x;

        Y = y;

        return this;
    }

    public void SetScalar(float scalar)
    {
        X = scalar;

        Y = scalar;
    }

    public Vector2 SetX(float x)
    {
        X = x;

        return this;
    }

    public Vector2 SetY(float y)
    {
        Y = y;

        return this;
    }

    public Vector2 SetComponent(int index, float value)
    {
        switch (index)
        {
            case 0: X = value; break;
            case 1: Y = value; break;
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
            default: throw new IndexOutOfRangeException(string.Format("Index {0} is out of rangess", index));
        }
    }

    public Vector2 Clone()
    {
        return new Vector2(X, Y);
    }

    public Vector2 Copy(Vector2 v)
    {
        X = v.X;
        Y = v.Y;

        return this;
    }

    public Vector2 Add(Vector2 v)
    {
        X += v.X;
        Y += v.Y;

        return this;
    }

    public Vector2 AddVectors(Vector2 a, Vector2 b)
    {
        X = a.X + b.X;
        Y = a.Y + b.Y;

        return this;
    }

    public static Vector2 operator +(Vector2 v, Vector2 w)
    {
        var r = new Vector2();
        r.X = v.X + w.X;
        r.Y = v.Y + w.Y;

        return r;
    }

    public Vector2 AddScalar(float s)
    {
        X += s;
        Y += s;

        return this;
    }

    public Vector2 AddScaledVector(Vector2 v, float s)
    {
        X += v.X * s;
        Y += v.Y * s;

        return this;
    }

    public static Vector2 operator +(Vector2 v, float s)
    {
        var r = new Vector2();

        r.X = v.X + s;
        r.Y = v.Y + s;

        return r;
    }

    public Vector2 Sub(Vector2 v)
    {
        X -= v.X;
        Y -= v.Y;

        return this;
    }

    public Vector2 SubScalar(float s)
    {
        X -= s;
        Y -= s;

        return this;
    }

    public Vector2 SubVectors(Vector2 a, Vector2 b)
    {
        X = a.X - b.X;
        Y = a.Y - b.Y;

        return this;
    }

    public static Vector2 operator -(Vector2 a, Vector2 b)
    {
        var r = new Vector2();

        r.X = a.X - b.X;
        r.Y = a.Y - b.Y;

        return r;
    }

    public static Vector2 operator -(Vector2 a, float s)
    {
        var r = new Vector2();
        ;
        r.X = a.X - s;
        r.Y = a.Y - s;

        return r;
    }

    public Vector2 Multiply(Vector2 v)
    {
        X *= v.X;
        Y *= v.Y;

        return this;
    }

    public Vector2 MultiplyScalar(float s)
    {
        X *= s;
        Y *= s;

        return this;
    }

    public static Vector2 operator *(Vector2 a, Vector2 b)
    {
        var r = new Vector2();
        r.X = a.X * b.X;
        r.Y = a.Y * b.Y;

        return r;
    }

    public static Vector2 operator *(Vector2 a, float s)
    {
        var r = new Vector2();
        r.X = a.X * s;
        r.Y = a.Y * s;

        return r;
    }

    public Vector2 Divide(Vector2 v)
    {
        X /= v.X;
        Y /= v.Y;

        return this;
    }

    public Vector2 DivideScalar(float s)
    {
        return MultiplyScalar(1 / s);
    }

    public static Vector2 operator /(Vector2 a, Vector2 b)
    {
        var r = new Vector2();
        r.X = a.X / b.X;
        r.Y = a.Y / b.Y;

        return r;
    }

    public static Vector2 operator /(Vector2 a, float s)
    {
        var r = new Vector2();
        r = a * (1 / s);

        return r;
    }

    public Vector2 ApplyMatrix3(Matrix3 m)
    {
        var x = X;
        var y = Y;
        var e = m.Elements;

        X = e[0] * x + e[3] * y + e[6];
        Y = e[1] * x + e[4] * y + e[7];

        return this;
    }

    public static Vector2 operator *(Matrix3 m, Vector2 a)
    {
        var x = a.X;
        var y = a.Y;
        var e = m.Elements;

        var r = new Vector2();

        r.X = e[0] * x + e[3] * y + e[6];
        r.Y = e[1] * x + e[4] * y + e[7];

        return r;
    }

    public Vector2 Min(Vector2 v)
    {
        X = Math.Min(X, v.X);
        Y = Math.Min(Y, v.Y);

        return this;
    }

    public Vector2 Max(Vector2 v)
    {
        X = Math.Max(X, v.X);
        Y = Math.Min(Y, v.Y);

        return this;
    }

    public Vector2 Clamp(Vector2 min, Vector2 max)
    {
        X = Math.Max(min.X, Math.Min(max.X, X));
        Y = Math.Max(min.Y, Math.Min(max.Y, Y));

        return this;
    }

    public Vector2 ClampScalar(float minVal, float maxVal)
    {
        X = Math.Max(minVal, Math.Min(maxVal, X));
        Y = Math.Max(minVal, Math.Min(maxVal, Y));

        return this;
    }

    public Vector2 ClampLength(float min, float max)
    {
        var length = Length();

        return DivideScalar(length != 0 ? length : 1).MultiplyScalar(Math.Max(min, Math.Min(max, length)));
    }

    public Vector2 Floor()
    {
        X = (float)Math.Floor(X);
        Y = (float)Math.Floor(Y);

        return this;
    }

    public Vector2 Ceil()
    {
        X = (float)Math.Ceiling(X);
        Y = (float)Math.Ceiling(Y);

        return this;
    }

    public Vector2 Round()
    {
        X = (float)Math.Round(X);
        Y = (float)Math.Round(Y);

        return this;
    }

    public Vector2 RoundToZero()
    {
        X = X < 0 ? (float)Math.Ceiling(X) : (float)Math.Floor(X);
        Y = Y < 0 ? (float)Math.Ceiling(Y) : (float)Math.Floor(Y);

        return this;
    }

    public Vector2 Negate()
    {
        X = -X;
        Y = -Y;

        return this;
    }

    public float Dot(Vector2 v)
    {
        return X * v.X + Y * v.Y;
    }

    public float Cross(Vector2 v)
    {
        return X * v.Y - Y * v.X;
    }

    public float LengthSq()
    {
        return X * X + Y * Y;
    }

    public float Length()
    {
        return (float)Math.Sqrt(LengthSq());
    }

    public float ManhattanLength()
    {
        return Math.Abs(X) + Math.Abs(Y);
    }

    public Vector2 Normalize()
    {
        return DivideScalar(Length() != 0 ? Length() : 1);
    }

    public float Angle()
    {
        // computes the angle in radians with respect to the positive x-axis
        var angle = Math.Atan2(Y, X);
        if (angle < 0) angle += 2 * Math.PI;

        return (float)angle;
    }

    public float DistanceTo(Vector2 v)
    {
        return (float)Math.Sqrt(DistanceToSquared(v));
    }

    public float DistanceToSquared(Vector2 v)
    {
        var dx = X - v.X;
        var dy = Y - v.Y;

        return dx * dx + dy * dy;
    }

    public float ManhattanDistanceTo(Vector2 v)
    {
        return Math.Abs(X - v.X) + Math.Abs(Y - v.Y);
    }

    public Vector2 SetLength(float length)
    {
        return Normalize().MultiplyScalar(length);
    }

    public Vector2 Lerp(Vector2 v, float alpha)
    {
        X += (v.X - X) * alpha;
        Y += (v.Y - Y) * alpha;

        return this;
    }

    public Vector2 LerpVectors(Vector2 v1, Vector2 v2, float alpha)
    {
        return SubVectors(v2, v1).MultiplyScalar(alpha).Add(v1);
    }

    public Vector2 FromArray(float[] array, int? offset = null)
    {
        var index = 0;
        if (offset != null) index = offset.Value;

        X = array[index];
        Y = array[index + 1];

        return this;
    }

    public float[] ToArray(float[] array = null, int? offset = null)
    {
        var index = 0;
        if (array == null) array = new float[2];
        if (offset != null) index = offset.Value;

        array[index] = X;
        array[index + 1] = Y;

        return array;
    }

    public Vector2 FromBufferAttribute(BufferAttribute<float> attribute, int index)
    {
        X = attribute.GetX(index);
        Y = attribute.GetY(index);

        return this;
    }

    public Vector2 RotateAround(Vector2 center, float angle)
    {
        var c = (float)Math.Cos(angle);
        var s = (float)Math.Sin(angle);

        var x = X - center.X;
        var y = Y - center.Y;

        X = x * c - y * s + center.X;
        Y = x * s + y * c + center.Y;

        return this;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        var handler = PropertyChanged;
        if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }
}