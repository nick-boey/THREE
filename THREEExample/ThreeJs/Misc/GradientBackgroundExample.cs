using THREE;

namespace THREEExample.ThreeJs.Loader;

[Example("GradientBackground", ExampleCategory.ThreeJs, "Misc")]
public class GradientBackgroundExample : LightHintExample
{
    private Camera backgroundCamera;
    private Scene backgroundScene;

    public override void Init()
    {
        base.Init();
        var texture = TextureLoader.Load("../../../../assets/textures/background.png");
        var backgroundMesh = new Mesh(
            new PlaneBufferGeometry(2, 2),
            new MeshBasicMaterial
            {
                Map = texture,
                DepthTest = false,
                DepthWrite = false
            });

        backgroundScene = new Scene();
        backgroundCamera = new Camera();
        backgroundScene.Add(backgroundCamera);
        backgroundScene.Add(backgroundMesh);
    }

    public override void Render()
    {
        renderer.AutoClear = false;
        renderer.Clear();
        renderer.Render(backgroundScene, backgroundCamera);
        base.Render();
    }
}