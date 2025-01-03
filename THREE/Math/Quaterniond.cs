using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace THREE;

[Serializable]
public class Quaterniond : IEquatable<Quaterniond>, ICloneable, INotifyPropertyChanged
{
    private double _w;
    private double _x;
    private double _y;
    private double _z;


    public Quaterniond()
    {
        X = Y = Z = 0;
        W = 1;
    }

    public Quaterniond(double x, double y, double z, double w)
    {
        _x = x;
        _y = y;
        _z = z;
        _w = w;
    }

    public double X
    {
        get => _x;
        set
        {
            _x = value;
            OnPropertyChanged();
        }
    }

    public double Y
    {
        get => _y;
        set
        {
            _y = value;
            OnPropertyChanged();
        }
    }

    public double Z
    {
        get => _z;
        set
        {
            _z = value;
            OnPropertyChanged();
        }
    }

    public double W
    {
        get => _w;
        set
        {
            _w = value;
            OnPropertyChanged();
        }
    }

    public object Clone()
    {
        return new Quaterniond(X, Y, Z, W);
    }

    public bool Equals(Quaterniond Quaterniond)
    {
        return Quaterniond.X == _x && Quaterniond.Y == _y && Quaterniond.Z == _z && Quaterniond.W == _w;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public static Quaterniond Identity()
    {
        return new Quaterniond();
    }

    public void Set(double x, double y, double z, double w)
    {
        _x = x;
        _y = y;
        _z = z;
        _w = w;

        OnPropertyChanged();
    }

    public Quaterniond Copy(Quaterniond Quaterniond)
    {
        _x = Quaterniond.X;
        _y = Quaterniond.Y;
        _z = Quaterniond.Z;
        _w = Quaterniond.W;

        OnPropertyChanged();

        return this;
    }

    public Quaterniond SetFromEuler(Eulerd euler, bool update = true)
    {
        double x = euler.X, y = euler.Y, z = euler.Z;
        var order = euler.Order;

        // http://www.mathworks.com/matlabcentral/fileexchange/
        // 	20696-function-to-convert-between-dcm-euler-angles-Quaternionds-and-euler-vectors/
        //	content/SpinCalc.m


        var c1 = Math.Cos(x / 2);
        var c2 = Math.Cos(y / 2);
        var c3 = Math.Cos(z / 2);

        var s1 = Math.Sin(x / 2);
        var s2 = Math.Sin(y / 2);
        var s3 = Math.Sin(z / 2);

        if (order == RotationOrder.XYZ)
        {
            _x = s1 * c2 * c3 + c1 * s2 * s3;
            _y = c1 * s2 * c3 - s1 * c2 * s3;
            _z = c1 * c2 * s3 + s1 * s2 * c3;
            _w = c1 * c2 * c3 - s1 * s2 * s3;
        }
        else if (order == RotationOrder.YXZ)
        {
            _x = s1 * c2 * c3 + c1 * s2 * s3;
            _y = c1 * s2 * c3 - s1 * c2 * s3;
            _z = c1 * c2 * s3 - s1 * s2 * c3;
            _w = c1 * c2 * c3 + s1 * s2 * s3;
        }
        else if (order == RotationOrder.ZXY)
        {
            _x = s1 * c2 * c3 - c1 * s2 * s3;
            _y = c1 * s2 * c3 + s1 * c2 * s3;
            _z = c1 * c2 * s3 + s1 * s2 * c3;
            _w = c1 * c2 * c3 - s1 * s2 * s3;
        }
        else if (order == RotationOrder.ZYX)
        {
            _x = s1 * c2 * c3 - c1 * s2 * s3;
            _y = c1 * s2 * c3 + s1 * c2 * s3;
            _z = c1 * c2 * s3 - s1 * s2 * c3;
            _w = c1 * c2 * c3 + s1 * s2 * s3;
        }
        else if (order == RotationOrder.YZX)
        {
            _x = s1 * c2 * c3 + c1 * s2 * s3;
            _y = c1 * s2 * c3 + s1 * c2 * s3;
            _z = c1 * c2 * s3 - s1 * s2 * c3;
            _w = c1 * c2 * c3 - s1 * s2 * s3;
        }
        else if (order == RotationOrder.XZY)
        {
            _x = s1 * c2 * c3 - c1 * s2 * s3;
            _y = c1 * s2 * c3 - s1 * c2 * s3;
            _z = c1 * c2 * s3 + s1 * s2 * c3;
            _w = c1 * c2 * c3 + s1 * s2 * s3;
        }

        if (update) OnPropertyChanged();

        return this;
    }

    public Quaterniond SetFromAxisAngle(Vector3d axis, double angle)
    {
        // http://www.euclideanspace.com/maths/geometry/rotations/conversions/angleToQuaterniond/index.htm

        // assumes axis is normalized

        var halfAngle = angle / 2;
        var s = Math.Sin(halfAngle);

        _x = axis.X * s;
        _y = axis.Y * s;
        _z = axis.Z * s;
        _w = Math.Cos(halfAngle);

        OnPropertyChanged();

        return this;
    }

    public Quaterniond SetFromRotationMatrix(Matrix4d m)
    {
        // http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaterniond/index.htm

        // assumes the upper 3x3 of m is a pure rotation matrix (i.e, unscaled)

        var te = m.Elements;

        double m11 = te[0],
            m12 = te[4],
            m13 = te[8],
            m21 = te[1],
            m22 = te[5],
            m23 = te[9],
            m31 = te[2],
            m32 = te[6],
            m33 = te[10],
            trace = m11 + m22 + m33,
            s;

        if (trace > 0)
        {
            s = 0.5f / Math.Sqrt(trace + 1.0);

            _w = 0.25f / s;
            _x = (m32 - m23) * s;
            _y = (m13 - m31) * s;
            _z = (m21 - m12) * s;
        }
        else if (m11 > m22 && m11 > m33)
        {
            s = 2.0f * Math.Sqrt(1.0 + m11 - m22 - m33);

            _w = (m32 - m23) / s;
            _x = 0.25f * s;
            _y = (m12 + m21) / s;
            _z = (m13 + m31) / s;
        }
        else if (m22 > m33)
        {
            s = 2.0f * Math.Sqrt(1.0 + m22 - m11 - m33);

            _w = (m13 - m31) / s;
            _x = (m12 + m21) / s;
            _y = 0.25f * s;
            _z = (m23 + m32) / s;
        }
        else
        {
            s = 2.0f * Math.Sqrt(1.0 + m33 - m11 - m22);

            _w = (m21 - m12) / s;
            _x = (m13 + m31) / s;
            _y = (m23 + m32) / s;
            _z = 0.25f * s;
        }

        OnPropertyChanged();

        return this;
    }

    public Quaterniond SetFromUnitVectors(Vector3d vFrom, Vector3d vTo)
    {
        // assumes direction vectors vFrom and vTo are normalized

        var EPS = 0.000001;

        var r = vFrom.Dot(vTo) + 1;

        if (r < EPS)
        {
            r = 0;

            if (Math.Abs(vFrom.X) > Math.Abs(vFrom.Z))
            {
                _x = -vFrom.Y;
                _y = vFrom.X;
                _z = 0;
                _w = r;
            }
            else
            {
                _x = 0;
                _y = -vFrom.Z;
                _z = vFrom.Y;
                _w = r;
            }
        }
        else
        {
            // crossVectors( vFrom, vTo ); // inlined to avoid cyclic dependency on Vector3d

            _x = vFrom.Y * vTo.Z - vFrom.Z * vTo.Y;
            _y = vFrom.Z * vTo.X - vFrom.X * vTo.Z;
            _z = vFrom.X * vTo.Y - vFrom.Y * vTo.X;
            _w = r;
        }

        return Normalize();
    }

    public double AngleTo(Quaterniond q)
    {
        return 2 * Math.Acos(Math.Abs(Dot(q).Clamp(-1, 1)));
    }

    public Quaterniond RotateTowards(Quaterniond q, double step)
    {
        var angle = AngleTo(q);

        if (angle == 0) return this;

        var t = Math.Min(1, step / angle);

        Slerp(q, t);

        return this;
    }

    public Quaterniond Inverse()
    {
        return Conjugate();
    }

    public Quaterniond Conjugate()
    {
        _x *= -1;
        _y *= -1;
        _z *= -1;

        OnPropertyChanged();

        return this;
    }

    public double Dot(Quaterniond v)
    {
        return _x * v.X + _y * v.Y + _z * v.Z + _w * v.W;
    }

    public double LengthSq()
    {
        return _x * _x + _y * _y + _z * _z + _w * _w;
    }

    public double Length()
    {
        return Math.Sqrt(_x * _x + _y * _y + _z * _z + _w * _w);
    }

    public Quaterniond Normalize()
    {
        var l = Length();

        if (l == 0)
        {
            _x = 0;
            _y = 0;
            _z = 0;
            _w = 1;
        }
        else
        {
            l = 1 / l;

            _x = _x * l;
            _y = _y * l;
            _z = _z * l;
            _w = _w * l;
        }

        OnPropertyChanged();

        return this;
    }

    public Quaterniond Multiply(Quaterniond q)
    {
        return MultiplyQuaternionds(this, q);
    }

    public Quaterniond PreMultiply(Quaterniond q)
    {
        return MultiplyQuaternionds(q, this);
    }

    public static Quaterniond operator *(Quaterniond a, Quaterniond b)
    {
        var r = new Quaterniond();

        double qax = a.X, qay = a.Y, qaz = a.Z, qaw = a.W;
        double qbx = b.X, qby = b.Y, qbz = b.Z, qbw = b.W;

        r._x = qax * qbw + qaw * qbx + qay * qbz - qaz * qby;
        r._y = qay * qbw + qaw * qby + qaz * qbx - qax * qbz;
        r._z = qaz * qbw + qaw * qbz + qax * qby - qay * qbx;
        r._w = qaw * qbw - qax * qbx - qay * qby - qaz * qbz;

        return r;
    }

    public Quaterniond MultiplyQuaternionds(Quaterniond a, Quaterniond b)
    {
        // from http://www.euclideanspace.com/maths/algebra/realNormedAlgebra/Quaternionds/code/index.htm

        double qax = a.X, qay = a.Y, qaz = a.Z, qaw = a.W;
        double qbx = b.X, qby = b.Y, qbz = b.Z, qbw = b.W;

        _x = qax * qbw + qaw * qbx + qay * qbz - qaz * qby;
        _y = qay * qbw + qaw * qby + qaz * qbx - qax * qbz;
        _z = qaz * qbw + qaw * qbz + qax * qby - qay * qbx;
        _w = qaw * qbw - qax * qbx - qay * qby - qaz * qbz;

        OnPropertyChanged();

        return this;
    }

    public Quaterniond Slerp(Quaterniond qa, Quaterniond qb, Quaterniond qm, double t)
    {
        return qm.Copy(qa).Slerp(qb, t);
    }

    public Quaterniond Slerp(Quaterniond qb, double t)
    {
        if (t == 0) return this;
        if (t == 1) return Copy(qb);

        double x = _x, y = _y, z = _z, w = _w;

        // http://www.euclideanspace.com/maths/algebra/realNormedAlgebra/Quaternionds/slerp/

        var cosHalfTheta = w * qb.W + x * qb.X + y * qb.Y + z * qb.Z;

        if (cosHalfTheta < 0)
        {
            _w = -qb.W;
            _x = -qb.X;
            _y = -qb.Y;
            _z = -qb.Z;

            cosHalfTheta = -cosHalfTheta;
        }
        else
        {
            Copy(qb);
        }

        if (cosHalfTheta >= 1.0)
        {
            _w = w;
            _x = x;
            _y = y;
            _z = z;

            return this;
        }

        var sqrSinHalfTheta = 1.0 - cosHalfTheta * cosHalfTheta;

        if (sqrSinHalfTheta <= double.Epsilon)
        {
            var s = 1 - t;
            _w = s * w + t * _w;
            _x = s * x + t * _x;
            _y = s * y + t * _y;
            _z = s * z + t * _z;

            Normalize();
            OnPropertyChanged();

            return this;
        }

        var sinHalfTheta = Math.Sqrt(sqrSinHalfTheta);
        var halfTheta = Math.Atan2(sinHalfTheta, cosHalfTheta);
        double ratioA = Math.Sin((1 - t) * halfTheta) / sinHalfTheta,
            ratioB = Math.Sin(t * halfTheta) / sinHalfTheta;

        _w = w * ratioA + _w * ratioB;
        _x = x * ratioA + _x * ratioB;
        _y = y * ratioA + _y * ratioB;
        _z = z * ratioA + _z * ratioB;

        OnPropertyChanged();

        return this;
    }

    public void SlerpFlat(double[] dst, int dstOffset, double[] src0, int srcOffset0, double[] src1, int srcOffset1,
        double t)
    {
        // fuzz-free, array-based Quaterniond SLERP operation

        double x0 = src0[srcOffset0 + 0],
            y0 = src0[srcOffset0 + 1],
            z0 = src0[srcOffset0 + 2],
            w0 = src0[srcOffset0 + 3],
            x1 = src1[srcOffset1 + 0],
            y1 = src1[srcOffset1 + 1],
            z1 = src1[srcOffset1 + 2],
            w1 = src1[srcOffset1 + 3];

        if (w0 != w1 || x0 != x1 || y0 != y1 || z0 != z1)
        {
            double s = 1 - t,
                cos = x0 * x1 + y0 * y1 + z0 * z1 + w0 * w1,
                dir = cos >= 0 ? 1 : -1,
                sqrSin = 1 - cos * cos;

            // Skip the Slerp for tiny steps to avoid numeric problems:
            if (sqrSin > double.Epsilon)
            {
                double sin = Math.Sqrt(sqrSin),
                    len = Math.Atan2(sin, cos * dir);

                s = Math.Sin(s * len) / sin;
                t = Math.Sin(t * len) / sin;
            }

            var tDir = t * dir;

            x0 = x0 * s + x1 * tDir;
            y0 = y0 * s + y1 * tDir;
            z0 = z0 * s + z1 * tDir;
            w0 = w0 * s + w1 * tDir;

            // Normalize in case we just did a lerp:
            if (s == 1 - t)
            {
                var f = 1 / Math.Sqrt(x0 * x0 + y0 * y0 + z0 * z0 + w0 * w0);

                x0 *= f;
                y0 *= f;
                z0 *= f;
                w0 *= f;
            }
        }

        dst[dstOffset] = x0;
        dst[dstOffset + 1] = y0;
        dst[dstOffset + 2] = z0;
        dst[dstOffset + 3] = w0;
    }

    public Quaterniond FromArray(double[] array, int? offset = null)
    {
        var index = 0;
        if (offset != null) index = (int)offset;

        _x = array[index];
        _y = array[index + 1];
        _z = array[index + 2];
        _w = array[index + 3];

        OnPropertyChanged();

        return this;
    }

    public double[] ToArray(double[] array = null, int? offset = null)
    {
        var index = 0;
        if (array == null) array = new double[4];
        if (offset != null) index = (int)offset;

        array[index] = _x;
        array[index + 1] = _y;
        array[index + 2] = _z;
        array[index + 3] = _w;

        return array;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        var handler = PropertyChanged;
        if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }
}