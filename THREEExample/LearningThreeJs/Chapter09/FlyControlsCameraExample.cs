using System;
using THREE;
using THREEExample.Learning.Utils;
using Color = THREE.Color;

namespace THREEExample.Learning.Chapter09;

[Example("04-Fly-controls-camera", ExampleCategory.LearnThreeJS, "Chapter09")]
public class FlyControlsCameraExample : Example
{
    private new FlyControls controls;

    public override void InitRenderer()
    {
        base.InitRenderer();
        renderer.SetClearColor(Color.Hex(0x000000));
        renderer.ShadowMap.Enabled = true;
        renderer.ShadowMap.Type = Constants.PCFSoftShadowMap;
    }

    public override void InitCameraController()
    {
        controls = new FlyControls(this, camera);
        controls.MovementSpeed = 25.0f;
        controls.RollSpeed = (float)Math.PI / 24;
        controls.AutoForward = true;
        controls.DragToLook = false;
    }

    public override void InitLighting()
    {
        base.InitLighting();
        DemoUtils.InitDefaultLighting(scene);
    }

    public override void Init()
    {
        base.Init();
        var loader = new OBJLoader();

        var city = loader.Load(@"../../../../assets/models/city/city.obj");
        DemoUtils.SetRandomColors(city);
        scene.Add(city);
    }

    public override void Render()
    {
        var delta = stopWatch.ElapsedMilliseconds / 10000.0f;
        stopWatch.Reset();
        stopWatch.Start();
        controls.Update(delta);

        renderer.Render(scene, camera);
    }
}