using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace THREE;

[Serializable]
public enum RotationOrder
{
    XYZ = 0,
    YZX = 1,
    ZXY = 2,
    XZY = 3,
    YXZ = 4,
    ZYX = 5
}

[Serializable]
public class Euler : INotifyPropertyChanged
{
    private RotationOrder _order = RotationOrder.XYZ;
    private float _x;

    private float _y;

    private float _z;

    //public Action<Euler> OnRotationChange;

    public Euler()
    {
        _x = _y = _z = 0;
        _order = RotationOrder.XYZ;
    }

    public Euler(float x, float y, float z, RotationOrder order = RotationOrder.XYZ)
    {
        _x = x;
        _y = y;
        _z = z;
        Order = order;
    }

    public float X
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

    public float Y
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

    public float Z
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

    public Euler Set(float a, float b, float c, RotationOrder o = RotationOrder.XYZ)
    {
        _x = a;
        _y = b;
        _z = c;
        Order = o;

        OnPropertyChanged();

        return this;
    }

    public Euler Copy(Euler other)
    {
        _x = other._x;
        _y = other._y;
        _z = other._z;
        Order = other.Order;

        OnPropertyChanged();

        return this;
    }

    public static float Clamp(float value, float min, float max)
    {
        return value.Clamp(min, max);
    }

    public Euler SetFromRotationMatrix(Matrix4 m)
    {
        return SetFromRotationMatrix(m, Order);
    }

    public Euler SetFromRotationMatrix(Matrix4 m, RotationOrder? rotationOrder = null, bool update = true)
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
            _y = (float)Math.Asin(m13.Clamp(-1, 1));

            if (Math.Abs(m13) < 0.99999)
            {
                _x = (float)Math.Atan2(-m23, m33);
                _z = (float)Math.Atan2(-m12, m11);
            }
            else
            {
                _x = (float)Math.Atan2(m32, m22);
                _z = 0;
            }
        }
        else if (order == RotationOrder.YXZ)
        {
            _x = (float)Math.Asin(m23.Clamp(-1, 1));
            if (Math.Abs(m23) < 0.99999)
            {
                _y = (float)Math.Atan2(m13, m33);
                _z = (float)Math.Atan2(m21, m22);
            }
            else
            {
                _y = (float)Math.Atan2(-m31, m11);
                _z = 0;
            }
        }
        else if (order == RotationOrder.ZXY)
        {
            _x = (float)Math.Asin(m32.Clamp(-1, 1));
            if (Math.Abs(m32) < 0.99999)
            {
                _y = (float)Math.Atan2(-m31, m33);
                _z = (float)Math.Atan2(-m12, m22);
            }
            else
            {
                _y = 0;
                _z = (float)Math.Atan2(m21, m11);
            }
        }
        else if (order == RotationOrder.ZYX)
        {
            _y = (float)Math.Asin(m31.Clamp(-1, 1));
            if (Math.Abs(m31) < 0.99999)
            {
                _x = (float)Math.Atan2(m32, m33);
                _z = (float)Math.Atan2(m21, m11);
            }
            else
            {
                _x = 0;
                _z = (float)Math.Atan2(-m12, m22);
            }
        }
        else if (order == RotationOrder.YZX)
        {
            _z = (float)Math.Asin(m21.Clamp(-1, 1));
            if (Math.Abs(m21) < 0.99999)
            {
                _x = (float)Math.Atan2(-m23, m22);
                _y = (float)Math.Atan2(-m31, m11);
            }
            else
            {
                _x = 0;
                _y = (float)Math.Atan2(m13, m33);
            }
        }
        else if (order == RotationOrder.XZY)
        {
            _z = (float)Math.Asin(-m12.Clamp(-1, 1));
            if (Math.Abs(m12) < 0.99999)
            {
                _x = (float)Math.Atan2(m32, m22);
                _y = (float)Math.Atan2(m13, m11);
            }
            else
            {
                _x = (float)Math.Atan2(-m23, m33);
                _y = 0;
            }
        }
        else
        {
            Trace.TraceInformation("THREE.Math.Euler: .setFromRotationMatrix() given unsupported order:" + order);
        }

        Order = order;

        if (update) OnPropertyChanged();
        return this;
    }

    public Euler SetFromQuaternion(Quaternion q, RotationOrder? order, bool update = true)
    {
        var _m = Matrix4.Identity().MakeRotationFromQuaternion(q);
        return SetFromRotationMatrix(_m, Order, update);
    }

    public Euler SetFromQuaternion(Quaternion q, RotationOrder order)
    {
        var sqx = q.X * q.X;
        var sqy = q.Y * q.Y;
        var sqz = q.Z * q.Z;
        var sqw = q.W * q.W;

        Order = order;
        if (Order == RotationOrder.XYZ)
        {
            _x = (float)Math.Atan2(2 * (q.X * q.W - q.Y * q.Z), sqw - sqx - sqy + sqz);
            _y = (float)Math.Asin(Clamp(2 * (q.X * q.Z + q.Y * q.W), -1, 1));
            _z = (float)Math.Atan2(2 * (q.Z * q.W - q.X * q.Y), sqw + sqx - sqy - sqz);
        }
        else if (Order == RotationOrder.YXZ)
        {
            _x = (float)Math.Asin(Clamp(2 * (q.X * q.W - q.Y * q.Z), -1, 1));
            _y = (float)Math.Atan2(2 * (q.X * q.Z + q.Y * q.W), sqw - sqx - sqy + sqz);
            _z = (float)Math.Atan2(2 * (q.X * q.Y + q.Z * q.W), sqw - sqx + sqy - sqz);
        }
        else if (Order == RotationOrder.ZXY)
        {
            _x = (float)Math.Asin(Clamp(2 * (q.X * q.W + q.Y * q.Z), -1, 1));
            _y = (float)Math.Atan2(2 * (q.Y * q.W - q.Z * q.X), sqw - sqx - sqy + sqz);
            _z = (float)Math.Atan2(2 * (q.Z * q.W - q.X * q.Y), sqw - sqx + sqy - sqz);
        }
        else if (Order == RotationOrder.ZYX)
        {
            _x = (float)Math.Atan2(2 * (q.X * q.W + q.Z * q.Y), sqw - sqx - sqy + sqz);
            _y = (float)Math.Asin(Clamp(2 * (q.Y * q.W - q.X * q.Z), -1, 1));
            _z = (float)Math.Atan2(2 * (q.X * q.Y + q.Z * q.W), sqw + sqx - sqy - sqz);
        }
        else if (Order == RotationOrder.YZX)
        {
            _x = (float)Math.Atan2(2 * (q.X * q.W - q.Z * q.Y), sqw - sqx + sqy - sqz);
            _y = (float)Math.Atan2(2 * (q.Y * q.W - q.X * q.Z), sqw + sqx - sqy - sqz);
            _z = (float)Math.Asin(Clamp(2 * (q.X * q.Y + q.Z * q.W), -1, 1));
        }
        else if (Order == RotationOrder.XZY)
        {
            _x = (float)Math.Atan2(2 * (q.X * q.W + q.Y * q.Z), sqw - sqx + sqy - sqz);
            _y = (float)Math.Atan2(2 * (q.X * q.Z + q.Y * q.W), sqw + sqx - sqy - sqz);
            _z = (float)Math.Asin(Clamp(2 * (q.Z * q.W - q.X * q.Y), -1, 1));
        }

        return this;
    }

    public Euler SetFromVector3(Vector3 v, RotationOrder order)
    {
        return Set(v.X, v.Y, v.Z, order);
    }

    public void Reorder(RotationOrder newOrder)
    {
        var q = new Quaternion().SetFromEuler(this); // FromEulerAngles(new Vector3(this._x, this._y, this._z));
        Order = newOrder;
        SetFromQuaternion(q, newOrder);
    }

    public bool Equals(Euler o)
    {
        return o._x == _x && o._y == _y && o._z == _z && o._order == _order;
    }

    public Euler fromArray(float[] array)
    {
        _x = array[0];
        _y = array[1];
        _z = array[2];
        if (array.Length > 3) _order = (RotationOrder)array[3];

        OnPropertyChanged();

        return this;
    }

    public float[] ToArray(int offset = 0)
    {
        var array = new float[4];

        array[offset] = _x;
        array[offset + 1] = _y;
        array[offset + 2] = _z;
        array[offset + 3] = (int)_order;

        return array;
    }

    public Vector3 ToVector3(Vector3 optionalResult = null)
    {
        if (optionalResult != null) return optionalResult.Set(_x, _y, _z);

        return new Vector3(_x, _y, _z);
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        var handler = PropertyChanged;
        if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }
}