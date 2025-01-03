using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace THREE;

[Serializable]
public class Vector2d : IEquatable<Vector2d>, INotifyPropertyChanged
{
    public double X;

    public double Y;

    public Vector2d()
    {
        X = Y = 0;
    }

    public Vector2d(double x, double y)
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

    public bool Equals(Vector2d v)
    {
        return X == v.X && Y == v.Y;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public static Vector2d Zero()
    {
        return new Vector2d(0, 0);
    }

    public Vector2d Set(double x, double y)
    {
        X = x;

        Y = y;

        return this;
    }

    public void SetScalar(double scalar)
    {
        X = scalar;

        Y = scalar;
    }

    public Vector2d SetX(double x)
    {
        X = x;

        return this;
    }

    public Vector2d SetY(double y)
    {
        Y = y;

        return this;
    }

    public Vector2d SetComponent(int index, double value)
    {
        switch (index)
        {
            case 0: X = value; break;
            case 1: Y = value; break;
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
            default: throw new IndexOutOfRangeException(string.Format("Index {0} is out of rangess", index));
        }
    }

    public Vector2d Clone()
    {
        return new Vector2d(X, Y);
    }

    public Vector2d Copy(Vector2d v)
    {
        X = v.X;
        Y = v.Y;

        return this;
    }

    public Vector2d Add(Vector2d v)
    {
        X += v.X;
        Y += v.Y;

        return this;
    }

    public Vector2d AddVectors(Vector2d a, Vector2d b)
    {
        X = a.X + b.X;
        Y = a.Y + b.Y;

        return this;
    }

    public static Vector2d operator +(Vector2d v, Vector2d w)
    {
        var r = new Vector2d();
        r.X = v.X + w.X;
        r.Y = v.Y + w.Y;

        return r;
    }

    public Vector2d AddScalar(double s)
    {
        X += s;
        Y += s;

        return this;
    }

    public Vector2d AddScaledVector(Vector2d v, double s)
    {
        X += v.X * s;
        Y += v.Y * s;

        return this;
    }

    public static Vector2d operator +(Vector2d v, double s)
    {
        var r = new Vector2d();

        r.X = v.X + s;
        r.Y = v.Y + s;

        return r;
    }

    public Vector2d Sub(Vector2d v)
    {
        X -= v.X;
        Y -= v.Y;

        return this;
    }

    public Vector2d SubScalar(double s)
    {
        X -= s;
        Y -= s;

        return this;
    }

    public Vector2d SubVectors(Vector2d a, Vector2d b)
    {
        X = a.X - b.X;
        Y = a.Y - b.Y;

        return this;
    }

    public static Vector2d operator -(Vector2d a, Vector2d b)
    {
        var r = new Vector2d();

        r.X = a.X - b.X;
        r.Y = a.Y - b.Y;

        return r;
    }

    public static Vector2d operator -(Vector2d a, double s)
    {
        var r = new Vector2d();
        ;
        r.X = a.X - s;
        r.Y = a.Y - s;

        return r;
    }

    public Vector2d Multiply(Vector2d v)
    {
        X *= v.X;
        Y *= v.Y;

        return this;
    }

    public Vector2d MultiplyScalar(double s)
    {
        X *= s;
        Y *= s;

        return this;
    }

    public static Vector2d operator *(Vector2d a, Vector2d b)
    {
        var r = new Vector2d();
        r.X = a.X * b.X;
        r.Y = a.Y * b.Y;

        return r;
    }

    public static Vector2d operator *(Vector2d a, double s)
    {
        var r = new Vector2d();
        r.X = a.X * s;
        r.Y = a.Y * s;

        return r;
    }

    public Vector2d Divide(Vector2d v)
    {
        X /= v.X;
        Y /= v.Y;

        return this;
    }

    public Vector2d DivideScalar(double s)
    {
        return MultiplyScalar(1 / s);
    }

    public static Vector2d operator /(Vector2d a, Vector2d b)
    {
        var r = new Vector2d();
        r.X = a.X / b.X;
        r.Y = a.Y / b.Y;

        return r;
    }

    public static Vector2d operator /(Vector2d a, double s)
    {
        var r = new Vector2d();
        r = a * (1 / s);

        return r;
    }

    public Vector2d ApplyMatrix3(Matrix3 m)
    {
        var x = X;
        var y = Y;
        var e = m.Elements;

        X = e[0] * x + e[3] * y + e[6];
        Y = e[1] * x + e[4] * y + e[7];

        return this;
    }

    public static Vector2d operator *(Matrix3 m, Vector2d a)
    {
        var x = a.X;
        var y = a.Y;
        var e = m.Elements;

        var r = new Vector2d();

        r.X = e[0] * x + e[3] * y + e[6];
        r.Y = e[1] * x + e[4] * y + e[7];

        return r;
    }

    public Vector2d Min(Vector2d v)
    {
        X = Math.Min(X, v.X);
        Y = Math.Min(Y, v.Y);

        return this;
    }

    public Vector2d Max(Vector2d v)
    {
        X = Math.Max(X, v.X);
        Y = Math.Min(Y, v.Y);

        return this;
    }

    public Vector2d Clamp(Vector2d min, Vector2d max)
    {
        X = Math.Max(min.X, Math.Min(max.X, X));
        Y = Math.Max(min.Y, Math.Min(max.Y, Y));

        return this;
    }

    public Vector2d ClampScalar(double minVal, double maxVal)
    {
        X = Math.Max(minVal, Math.Min(maxVal, X));
        Y = Math.Max(minVal, Math.Min(maxVal, Y));

        return this;
    }

    public Vector2d ClampLength(double min, double max)
    {
        var length = Length();

        return DivideScalar(length != 0 ? length : 1).MultiplyScalar(Math.Max(min, Math.Min(max, length)));
    }

    public Vector2d Floor()
    {
        X = Math.Floor(X);
        Y = Math.Floor(Y);

        return this;
    }

    public Vector2d Ceil()
    {
        X = Math.Ceiling(X);
        Y = Math.Ceiling(Y);

        return this;
    }

    public Vector2d Round()
    {
        X = Math.Round(X);
        Y = Math.Round(Y);

        return this;
    }

    public Vector2d RoundToZero()
    {
        X = X < 0 ? Math.Ceiling(X) : Math.Floor(X);
        Y = Y < 0 ? Math.Ceiling(Y) : Math.Floor(Y);

        return this;
    }

    public Vector2d Negate()
    {
        X = -X;
        Y = -Y;

        return this;
    }

    public double Dot(Vector2d v)
    {
        return X * v.X + Y * v.Y;
    }

    public double Cross(Vector2d v)
    {
        return X * v.Y - Y * v.X;
    }

    public double LengthSq()
    {
        return X * X + Y * Y;
    }

    public double Length()
    {
        return Math.Sqrt(LengthSq());
    }

    public double ManhattanLength()
    {
        return Math.Abs(X) + Math.Abs(Y);
    }

    public Vector2d Normalize()
    {
        return DivideScalar(Length() != 0 ? Length() : 1);
    }

    public double Angle()
    {
        // computes the angle in radians with respect to the positive x-axis
        var angle = Math.Atan2(Y, X);
        if (angle < 0) angle += 2 * Math.PI;

        return angle;
    }

    public double DistanceTo(Vector2d v)
    {
        return Math.Sqrt(DistanceToSquared(v));
    }

    public double DistanceToSquared(Vector2d v)
    {
        var dx = X - v.X;
        var dy = Y - v.Y;

        return dx * dx + dy * dy;
    }

    public double ManhattanDistanceTo(Vector2d v)
    {
        return Math.Abs(X - v.X) + Math.Abs(Y - v.Y);
    }

    public Vector2d SetLength(double length)
    {
        return Normalize().MultiplyScalar(length);
    }

    public Vector2d Lerp(Vector2d v, double alpha)
    {
        X += (v.X - X) * alpha;
        Y += (v.Y - Y) * alpha;

        return this;
    }

    public Vector2d LerpVectors(Vector2d v1, Vector2d v2, double alpha)
    {
        return SubVectors(v2, v1).MultiplyScalar(alpha).Add(v1);
    }

    public Vector2d FromArray(double[] array, int? offset = null)
    {
        var index = 0;
        if (offset != null) index = offset.Value;

        X = array[index];
        Y = array[index + 1];

        return this;
    }

    public double[] ToArray(double[] array = null, int? offset = null)
    {
        var index = 0;
        if (array == null) array = new double[2];
        if (offset != null) index = offset.Value;

        array[index] = X;
        array[index + 1] = Y;

        return array;
    }

    public Vector2d FromBufferAttribute(BufferAttribute<double> attribute, int index)
    {
        X = attribute.GetX(index);
        Y = attribute.GetY(index);

        return this;
    }

    public Vector2d RotateAround(Vector2d center, double angle)
    {
        var c = Math.Cos(angle);
        var s = Math.Sin(angle);

        var x = X - center.X;
        var y = Y - center.Y;

        X = x * c - y * s + center.X;
        Y = x * s + y * c + center.Y;

        return this;
    }

    public Vector2 ToVector2()
    {
        return new Vector2((float)X, (float)Y);
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        var handler = PropertyChanged;
        if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }
}