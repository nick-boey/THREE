﻿using THREE;
using THREEExample.Learning.Utils;

namespace THREEExample.Learning.Chapter10;

[Example("14-alpha-map", ExampleCategory.LearnThreeJS, "Chapter10")]
public class AlphaMapExample : TemplateExample
{
    public override void InitLighting()
    {
        base.InitLighting();
        scene.Add(new AmbientLight(new Color(0x444444)));
    }

    public override void SetGeometryWithTexture()
    {
        var groundPlane = DemoUtils.AddLargeGroundPlane(scene, true);
        groundPlane.Position.Y = -8;

        var sphere = new SphereBufferGeometry(8, 180, 180);
        var sphereMaterial = new MeshStandardMaterial
        {
            AlphaMap = TextureLoader.Load("../../../../assets/textures/alpha/partial-transparency.png"),
            Metalness = 0.02f,
            Roughness = 0.07f,
            Color = new Color(0xffffff),
            AlphaTest = 0.5f
        };

        sphereMaterial.AlphaMap.WrapS = Constants.RepeatWrapping;
        sphereMaterial.AlphaMap.WrapT = Constants.RepeatWrapping;
        sphereMaterial.AlphaMap.Repeat.Set(8, 8);

        var mesh = AddGeometryWithMaterial(scene, sphere, "sphere", sphereMaterial);
        mesh.CastShadow = false;
        mesh.ReceiveShadow = false;
    }

    public override void Render()
    {
        if (!imGuiManager.ImWantMouse)
            controls.Enabled = true;
        else
            controls.Enabled = false;

        controls.Update();
        renderer.Render(scene, camera);
    }
}