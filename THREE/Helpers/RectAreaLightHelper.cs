using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class RectAreaLightHelper : Line
{
    private Color? Color;
    private Light Light;

    public RectAreaLightHelper(Light light, Color? color = null)
    {
        type = "RectAreaLightHelper";
        Light = light;
        Light.UpdateMatrixWorld();

        //this.Matrix = this.Light.MatrixWorld;
        //this.MatrixAutoUpdate = false;

        Color = color;

        var positions = new List<float> { 1, 1, 0, -1, 1, 0, -1, -1, 0, 1, -1, 0, 1, 1, 0 };

        var geometry = new BufferGeometry();
        geometry.SetAttribute("position", new BufferAttribute<float>(positions.ToArray(), 3));
        geometry.ComputeBoundingSphere();

        InitGeometry(geometry, new List<Material> { new LineBasicMaterial { Fog = false, ToneMapped = false } });

        var positions2 = new List<float> { 1, 1, 0, -1, 1, 0, -1, -1, 0, 1, 1, 0, -1, -1, 0, 1, -1, 0 };

        var geometry2 = new BufferGeometry();
        geometry2.SetAttribute("position", new BufferAttribute<float>(positions2.ToArray(), 3));
        geometry2.ComputeBoundingSphere();

        Add(new Mesh(geometry2, new MeshBasicMaterial { Side = Constants.BackSide, Fog = false }));
        Update();
    }

    public RectAreaLightHelper(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public void Update()
    {
        Light.UpdateMatrixWorld();
        Scale.Set(0.5f * Light.Width, 0.5f * Light.Height, 1f);

        if (Color != null)
        {
            Material.Color = Color;
            Children[0].Material.Color = Color;
        }
        else
        {
            Material.Color = Light.Color.MultiplyScalar(Light.Intensity);

            // prevent hue shift
            var c = (Color)Material.Color;
            var max = Math.Max(c.R, Math.Max(c.G, c.B));
            if (max > 1) c.MultiplyScalar(1 / max);

            Children[0].Material.Color = Material.Color;
        }
    }

    public override void Dispose()
    {
        base.Dispose();

        Geometry.Dispose();
        Material.Dispose();
        Children[0].Geometry.Dispose();
        Children[0].Material.Dispose();
    }
}