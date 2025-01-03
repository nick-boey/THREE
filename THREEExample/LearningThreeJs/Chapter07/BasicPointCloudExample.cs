using THREE;
using Color = THREE.Color;

namespace THREEExample.Learning.Chapter07;

[Example("03.Basic-Points-Cloud", ExampleCategory.LearnThreeJS, "Chapter07")]
public class BasicPointCloudExample : Example
{
    private Points cloud;
    private float step;

    public override void InitCamera()
    {
        base.InitCamera();
        camera.Position.Set(20, 0, 150);
    }

    public override void Init()
    {
        base.Init();
        CreateParticles();
    }

    public override void Render()
    {
        base.Render();

        step += 0.01f;
        cloud.Rotation.X = step;
        cloud.Rotation.Y = step;
    }

    private void CreateParticles()
    {
        var geom = new Geometry();

        var material = new PointsMaterial
        {
            Size = 4.0f, VertexColors = true, Opacity = 0.6f, Transparent = true, SizeAttenuation = true,
            Color = Color.Hex(0x00ff00)
        };

        var range = 500;
        for (var i = 0; i < 15000; i++)
        {
            var particle = new Vector3((float)random.NextDouble() * range - range / 2.0f,
                (float)random.NextDouble() * range - range / 2.0f, (float)random.NextDouble() * range - range / 2.0f);
            geom.Vertices.Add(particle);
            var color = Color.Hex(0x00ff00);
            var asHSL = color.GetHSL();
            color.SetHSL(asHSL.H, asHSL.S, asHSL.L * (float)random.NextDouble());
            geom.Colors.Add(color);
        }

        cloud = new Points(geom, material);
        scene.Add(cloud);
    }
}