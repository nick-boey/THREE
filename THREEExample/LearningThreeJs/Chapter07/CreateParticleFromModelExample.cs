using OpenTK.Graphics.OpenGL;
using THREE;
using Color = THREE.Color;

namespace THREEExample.Learning.Chapter07;

[Example("11-Create-Particle-from-model", ExampleCategory.LearnThreeJS, "Chapter07")]
public class CreateParticleFromModelExample : Example
{
    private readonly bool AsParticle = true;

    private int sprite = 0;

    private float step = 0;

    public override void InitRenderer()
    {
        base.InitRenderer();
        renderer.SetClearColor(0x000000);
    }

    public override void InitCamera()
    {
        base.InitCamera();
        camera.Position.Set(-30, 40, 50);
    }

    public override void Init()
    {
        base.Init();

        var texture = TextureLoader.Load(@"../../../../assets/textures/particles/model.png");

        var geom = new TorusKnotGeometry(13, 1.7f, 156, 12, 5, 4);

        Mesh knot = null;

        if (AsParticle)
        {
            var point = CreatePoints(geom, texture);
            scene.Add(point);
        }
        else
        {
            knot = new Mesh(geom, new MeshNormalMaterial());
            scene.Add(knot);
        }
    }

    public override void Render()
    {
        base.Render();
        GL.Enable(EnableCap.PointSprite);
    }

    private Points CreatePoints(Geometry geom, Texture texture)
    {
        var material = new PointsMaterial
        {
            Color = Color.Hex(0xffffff),
            Size = 3,
            Transparent = true,
            Blending = Constants.AdditiveBlending,
            Map = texture,
            DepthWrite = false
        };

        var cloud = new Points(geom, material);

        return cloud;
    }
}