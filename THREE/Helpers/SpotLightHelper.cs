using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class SpotLightHelper : Object3D
{
    private Vector3 _vector = Vector3.Zero();

    private Color? Color;

    private LineSegments Cone;

    private Light Light;

    public SpotLightHelper(Light light, Color? color = null)
    {
        type = "SpotLightHelper";
        Light = light;

        Light.UpdateMatrixWorld();

        Matrix = light.MatrixWorld;

        MatrixAutoUpdate = false;

        Color = color;

        var geometry = new BufferGeometry();

        var positions = new List<float>
        {
            0, 0, 0, 0, 0, 1,
            0, 0, 0, 1, 0, 1,
            0, 0, 0, -1, 0, 1,
            0, 0, 0, 0, 1, 1,
            0, 0, 0, 0, -1, 1
        };

        for (int i = 0, j = 1, l = 32; i < l; i++, j++)
        {
            var p1 = i / l * Math.PI * 2;
            var p2 = j / l * Math.PI * 2;

            positions.Add((float)Math.Cos(p1), (float)Math.Sin(p1), 1);
            positions.Add((float)Math.Cos(p2), (float)Math.Sin(p2), 1);
        }

        geometry.SetAttribute("position", new BufferAttribute<float>(positions.ToArray(), 3));

        var material = new LineBasicMaterial { Fog = false, ToneMapped = false };

        Cone = new LineSegments(geometry, material);

        Add(Cone);

        Update();
    }

    public SpotLightHelper(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public void Update()
    {
        Light.UpdateMatrixWorld();

        var coneLength = Light.Distance != 0 ? Light.Distance : 1000;
        var coneWidth = coneLength * (float)Math.Tan(Light.Angle);

        Cone.Scale.Set(coneWidth, coneWidth, coneLength);

        _vector.SetFromMatrixPosition(Light.Target.MatrixWorld);

        Cone.LookAt(_vector);

        if (Color != null)
            Cone.Material.Color = Color;
        else
            Cone.Material.Color = Light.Color;
    }

    public override void Dispose()
    {
        base.Dispose();

        Cone.Geometry.Dispose();
        Cone.Material.Dispose();
    }
}