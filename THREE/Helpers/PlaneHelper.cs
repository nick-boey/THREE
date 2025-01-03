using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class PlaneHelper : Line
{
    public PlaneHelper(Plane plane, float? size, int? hex)
    {
        var color = hex != null ? hex.Value : 0xffff00;

        var positions = new float[]
            { 1, -1, 1, -1, 1, 1, -1, -1, 1, 1, 1, 1, -1, 1, 1, -1, -1, 1, 1, -1, 1, 1, 1, 1, 0, 0, 1, 0, 0, 0 };

        var geometry = new BufferGeometry();
        geometry.SetAttribute("position", new BufferAttribute<float>(positions, 3));
        geometry.ComputeBoundingSphere();

        InitGeometry(geometry, new LineBasicMaterial { Color = Color.Hex(color), ToneMapped = false });

        type = "PlaneHelper";

        Plane = plane;

        Size = size == null ? 1 : size.Value;

        var positions2 = new float[] { 1, 1, 1, -1, 1, 1, -1, -1, 1, 1, 1, 1, -1, -1, 1, 1, -1, 1 };

        var geometry2 = new BufferGeometry();
        geometry2.SetAttribute("position", new BufferAttribute<float>(positions2, 3));
        geometry2.ComputeBoundingSphere();

        Add(new Mesh(geometry2,
            new MeshBasicMaterial
            {
                Color = Color.Hex(color), Opacity = 0.2f, Transparent = true, DepthWrite = false, ToneMapped = false
            }));
    }

    public PlaneHelper(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public Plane Plane { get; set; }
    public float Size { get; set; }


    public override void UpdateMatrixWorld(bool force = false)
    {
        var scale = -Plane.Constant;

        if (Math.Abs(scale) < 1e-8) scale = 1e-8f; // sign does not matter

        Scale.Set(0.5f * Size, 0.5f * Size, scale);

        Children[0].Material.Side =
            scale < 0
                ? Constants.BackSide
                : Constants.FrontSide; // renderer flips side when determinant < 0; flipping not wanted here

        LookAt(Plane.Normal);

        base.UpdateMatrixWorld(force);
    }
}