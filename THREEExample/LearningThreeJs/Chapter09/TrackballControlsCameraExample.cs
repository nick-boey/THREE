using THREE;
using THREEExample.Learning.Utils;

namespace THREEExample.Learning.Chapter09;

[Example("03-Trackball-controls-camera", ExampleCategory.LearnThreeJS, "Chapter09")]
public class TrackballControlsCameraExample : Example
{
    private Mesh mesh;

    public override void InitRenderer()
    {
        base.InitRenderer();
        renderer.SetClearColor(Color.Hex(0x000000));
        renderer.ShadowMap.Enabled = true;
        renderer.ShadowMap.Type = Constants.PCFSoftShadowMap;
    }

    public override void Init()
    {
        base.Init();
        DemoUtils.InitDefaultLighting(scene);

        var loader = new OBJLoader();

        var city = loader.Load(@"../../../../assets/models/city/city.obj");
        DemoUtils.SetRandomColors(city);
        scene.Add(city);
    }
}