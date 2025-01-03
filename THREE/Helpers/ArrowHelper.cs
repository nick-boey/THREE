using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class ArrowHelper : Object3D
{
    private Vector3 _axis = Vector3.Zero();
    private CylinderBufferGeometry _coneGeometry;
    private BufferGeometry _lineGeometry;
    private Color Color = Color.Hex(0xffff00);
    private Mesh cone;
    private Vector3 Dir = new(0, 0, 1);
    private float HeadLength;
    private float HeadWidth;
    private float Length = 1;
    private Line line;
    private Vector3 Origin = Vector3.Zero();

    public ArrowHelper(Vector3 dir = null, Vector3 origin = null, float? length = null, Color? color = null,
        float? headLength = null, float? headWidth = null)
    {
        if (dir != null) Dir = dir;

        if (origin != null) Origin = origin;

        if (length != null) Length = length.Value;

        if (color != null) Color = color.Value;

        HeadLength = headLength != null ? headLength.Value : 0.2f * Length;

        HeadWidth = headWidth != null ? headWidth.Value : 0.2f * Length;

        if (_lineGeometry == null)
        {
            _lineGeometry = new BufferGeometry();
            _lineGeometry.SetAttribute("position", new BufferAttribute<float>(new float[] { 0, 0, 0, 0, 1, 0 }, 3));

            _coneGeometry = new CylinderBufferGeometry(0, 0.5f, 1, 5, 1);
            _coneGeometry.Translate(0, -0.5f, 0);
        }

        Position.Copy(Origin);

        line = new Line(_lineGeometry, new LineBasicMaterial { Color = Color, ToneMapped = false });
        line.MatrixAutoUpdate = false;
        Add(line);

        cone = new Mesh(_coneGeometry, new MeshBasicMaterial { Color = Color, ToneMapped = false });
        cone.MatrixAutoUpdate = false;
        Add(cone);

        SetDirection(Dir);
        SetLength(Length, HeadLength, HeadWidth);
    }

    protected ArrowHelper(ArrowHelper source)
    {
        line = (Line)source.line.Clone();
        cone = (Mesh)source.cone.Clone();
    }

    public ArrowHelper(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public new object Clone()
    {
        return new ArrowHelper(this);
    }

    private void SetDirection(Vector3 dir)
    {
        // dir is assumed to be normalized

        if (dir.Y > 0.99999)
        {
            Quaternion.Set(0, 0, 0, 1);
        }
        else if (dir.Y < -0.99999)
        {
            Quaternion.Set(1, 0, 0, 0);
        }
        else
        {
            _axis.Set(dir.Z, 0, -dir.X).Normalize();

            var radians = Math.Acos(dir.Y);

            Quaternion.SetFromAxisAngle(_axis, (float)radians);
        }
    }

    private void SetLength(float length, float headLength, float headWidth)
    {
        line.Scale.Set(1, Math.Max(0.0001f, length - headLength), 1); // see #17458

        line.UpdateMatrix();

        cone.Scale.Set(headWidth, headLength, headWidth);

        cone.Position.Y = length;

        cone.UpdateMatrix();
    }

    public void SetColor(Color color)
    {
        line.Material.Color = color;

        cone.Material.Color = color;
    }
}