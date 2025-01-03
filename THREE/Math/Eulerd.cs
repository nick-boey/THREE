using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace THREE;

[Serializable]
public class Eulerd : INotifyPropertyChanged
{
    private RotationOrder _order = RotationOrder.XYZ;
    private double _x;

    private double _y;

    private double _z;

    //public Action<Eulerd> OnRotationChange;

    public Eulerd()
    {
        _x = _y = _z = 0;
        _order = RotationOrder.XYZ;
    }

    public Eulerd(double x, double y, double z, RotationOrder order = RotationOrder.XYZ)
    {
        _x = x;
        _y = y;
        _z = z;
        Order = order;
    }

    public double X
    {
        get => _x;
        set
        {
            if (value != _x)
            {
                _x = value;
                //OnRotationChange(this);
                OnPropertyChanged();
            }
        }
    }

    public double Y
    {
        get => _y;
        set
        {
            if (value != _y)
            {
                _y = value;
                //OnRotationChange(this);
                OnPropertyChanged();
            }
        }
    }

    public double Z
    {
        get => _z;
        set
        {
            if (value != _z)
            {
                _z = value;
                //OnRotationChange(this);
                OnPropertyChanged();
            }
        }
    }

    public RotationOrder Order
    {
        get => _order;
        set
        {
            if (value != _order)
            {
                _order = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public Eulerd Set(double a, double b, double c, RotationOrder o)
    {
        _x = a;
        _y = b;
        _z = c;
        Order = o;

        OnPropertyChanged();

        return this;
    }

    public static double Clamp(double value, double min, double max)
    {
        return value.Clamp(min, max);
    }

    public Eulerd SetFromRotationMatrix(Matrix4d m)
    {
        return SetFromRotationMatrix(m, Order);
    }

    public Eulerd SetFromRotationMatrix(Matrix4d m, RotationOrder? rotationOrder = null, bool update = true)
    {
        var te = m.Elements;

        var m11 = te[0];
        var m12 = te[4];
        var m13 = te[8];
        var m21 = te[1];
        var m22 = te[5];
        var m23 = te[9];
        var m31 = te[2];
        var m32 = te[6];
        var m33 = te[10];

        var order = rotationOrder != null ? rotationOrder.Value : Order;

        if (order == RotationOrder.XYZ)
        {
            _y = Math.Asin(m13.Clamp(-1, 1));

            if (Math.Abs(m13) < 0.99999)
            {
                _x = Math.Atan2(-m23, m33);
                _z = Math.Atan2(-m12, m11);
            }
            else
            {
                _x = Math.Atan2(m32, m22);
                _z = 0;
            }
        }
        else if (order == RotationOrder.YXZ)
        {
            _x = Math.Asin(m23.Clamp(-1, 1));
            if (Math.Abs(m23) < 0.99999)
            {
                _y = Math.Atan2(m13, m33);
                _z = Math.Atan2(m21, m22);
            }
            else
            {
                _y = Math.Atan2(-m31, m11);
                _z = 0;
            }
        }
        else if (order == RotationOrder.ZXY)
        {
            _x = Math.Asin(m32.Clamp(-1, 1));
            if (Math.Abs(m32) < 0.99999)
            {
                _y = Math.Atan2(-m31, m33);
                _z = Math.Atan2(-m12, m22);
            }
            else
            {
                _y = 0;
                _z = Math.Atan2(m21, m11);
            }
        }
        else if (order == RotationOrder.ZYX)
        {
            _y = Math.Asin(m31.Clamp(-1, 1));
            if (Math.Abs(m31) < 0.99999)
            {
                _x = Math.Atan2(m32, m33);
                _z = Math.Atan2(m21, m11);
            }
            else
            {
                _x = 0;
                _z = Math.Atan2(-m12, m22);
            }
        }
        else if (order == RotationOrder.YZX)
        {
            _z = Math.Asin(m21.Clamp(-1, 1));
            if (Math.Abs(m21) < 0.99999)
            {
                _x = Math.Atan2(-m23, m22);
                _y = Math.Atan2(-m31, m11);
            }
            else
            {
                _x = 0;
                _y = Math.Atan2(m13, m33);
            }
        }
        else if (order == RotationOrder.XZY)
        {
            _z = Math.Asin(-m12.Clamp(-1, 1));
            if (Math.Abs(m12) < 0.99999)
            {
                _x = Math.Atan2(m32, m22);
                _y = Math.Atan2(m13, m11);
            }
            else
            {
                _x = Math.Atan2(-m23, m33);
                _y = 0;
            }
        }
        else
        {
            Trace.TraceInformation("THREE.Math.Eulerd: .setFromRotationMatrix() given unsupported order:" + order);
        }

        Order = order;

        if (update) OnPropertyChanged();
        return this;
    }

    public Eulerd SetFromQuaternion(Quaterniond q, RotationOrder? order, bool update = true)
    {
        var _m = Matrix4d.Identity().MakeRotationFromQuaternion(q);
        return SetFromRotationMatrix(_m, Order, update);
    }

    public Eulerd SetFromQuaternion(Quaterniond q, RotationOrder order)
    {
        var sqx = q.X * q.X;
        var sqy = q.Y * q.Y;
        var sqz = q.Z * q.Z;
        var sqw = q.W * q.W;

        Order = order;
        if (Order == RotationOrder.XYZ)
        {
            _x = Math.Atan2(2 * (q.X * q.W - q.Y * q.Z), sqw - sqx - sqy + sqz);
            _y = Math.Asin(Clamp(2 * (q.X * q.Z + q.Y * q.W), -1, 1));
            _z = Math.Atan2(2 * (q.Z * q.W - q.X * q.Y), sqw + sqx - sqy - sqz);
        }
        else if (Order == RotationOrder.YXZ)
        {
            _x = Math.Asin(Clamp(2 * (q.X * q.W - q.Y * q.Z), -1, 1));
            _y = Math.Atan2(2 * (q.X * q.Z + q.Y * q.W), sqw - sqx - sqy + sqz);
            _z = Math.Atan2(2 * (q.X * q.Y + q.Z * q.W), sqw - sqx + sqy - sqz);
        }
        else if (Order == RotationOrder.ZXY)
        {
            _x = Math.Asin(Clamp(2 * (q.X * q.W + q.Y * q.Z), -1, 1));
            _y = Math.Atan2(2 * (q.Y * q.W - q.Z * q.X), sqw - sqx - sqy + sqz);
            _z = Math.Atan2(2 * (q.Z * q.W - q.X * q.Y), sqw - sqx + sqy - sqz);
        }
        else if (Order == RotationOrder.ZYX)
        {
            _x = Math.Atan2(2 * (q.X * q.W + q.Z * q.Y), sqw - sqx - sqy + sqz);
            _y = Math.Asin(Clamp(2 * (q.Y * q.W - q.X * q.Z), -1, 1));
            _z = Math.Atan2(2 * (q.X * q.Y + q.Z * q.W), sqw + sqx - sqy - sqz);
        }
        else if (Order == RotationOrder.YZX)
        {
            _x = Math.Atan2(2 * (q.X * q.W - q.Z * q.Y), sqw - sqx + sqy - sqz);
            _y = Math.Atan2(2 * (q.Y * q.W - q.X * q.Z), sqw + sqx - sqy - sqz);
            _z = Math.Asin(Clamp(2 * (q.X * q.Y + q.Z * q.W), -1, 1));
        }
        else if (Order == RotationOrder.XZY)
        {
            _x = Math.Atan2(2 * (q.X * q.W + q.Y * q.Z), sqw - sqx + sqy - sqz);
            _y = Math.Atan2(2 * (q.X * q.Z + q.Y * q.W), sqw + sqx - sqy - sqz);
            _z = Math.Asin(Clamp(2 * (q.Z * q.W - q.X * q.Y), -1, 1));
        }

        return this;
    }

    public Eulerd SetFromVector3(Vector3 v, RotationOrder order)
    {
        return Set(v.X, v.Y, v.Z, order);
    }

    public void Reorder(RotationOrder newOrder)
    {
        var q = new Quaterniond().SetFromEuler(this); // FromEulerdAngles(new Vector3(this._x, this._y, this._z));
        Order = newOrder;
        SetFromQuaternion(q, newOrder);
    }

    public bool Equals(Eulerd o)
    {
        return o._x == _x && o._y == _y && o._z == _z && o._order == _order;
    }

    public Eulerd fromArray(double[] array)
    {
        _x = array[0];
        _y = array[1];
        _z = array[2];
        if (array.Length > 3) _order = (RotationOrder)array[3];

        OnPropertyChanged();

        return this;
    }

    public double[] ToArray(int offset = 0)
    {
        var array = new double[4];

        array[offset] = _x;
        array[offset + 1] = _y;
        array[offset + 2] = _z;
        array[offset + 3] = (int)_order;

        return array;
    }

    public Vector3d ToVector3(Vector3d optionalResult = null)
    {
        if (optionalResult != null) return optionalResult.Set(_x, _y, _z);

        return new Vector3d(_x, _y, _z);
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        var handler = PropertyChanged;
        if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }
}