﻿using System;
using System.Collections.Generic;
using THREE;
using Color = THREE.Color;

namespace THREEExample.Learning.Chapter10;

[Example("13-metal-roughness-map", ExampleCategory.LearnThreeJS, "Chapter10")]
public class MetalRoughnessMapExample : TemplateExample
{
    private MeshStandardMaterial cubeMaterialWithNormalMap;
    private int invert = 1;
    private float phase;
    private PointLight pointLight;
    private Mesh sphereLightMesh;

    public override void InitLighting()
    {
        scene.Add(new AmbientLight(new Color(0x888888)));

        pointLight = new PointLight(new Color(0xffffff));
        scene.Add(pointLight);
    }

    public override void SetGeometryWithTexture()
    {
        var sphereLight = new SphereBufferGeometry(0.2f);
        var sphereLightMaterial = new MeshStandardMaterial { Color = new Color(0xff5808) };
        sphereLightMesh = new Mesh(sphereLight, sphereLightMaterial);

        scene.Add(sphereLightMesh);

        var urls = new List<string>
        {
            "../../../../assets/textures/cubemap/car/right.png",
            "../../../../assets/textures/cubemap/car/left.png",
            "../../../../assets/textures/cubemap/car/top.png",
            "../../../../assets/textures/cubemap/car/bottom.png",
            "../../../../assets/textures/cubemap/car/front.png",
            "../../../../assets/textures/cubemap/car/back.png"
        };

        var cubeTexture = CubeTextureLoader.Load(urls);
        scene.Background = cubeTexture;

        var sphere = new SphereBufferGeometry(8, 50, 50);
        var cubeMaterial = new MeshStandardMaterial();
        cubeMaterial.EnvMap = scene.Background as CubeTexture;
        cubeMaterial.Color = new Color(0xffffff);
        cubeMaterial.Metalness = 1;
        cubeMaterial.Roughness = 0.5f;

        var cubeMaterialWithMetalMap = (MeshStandardMaterial)cubeMaterial.Clone();
        cubeMaterialWithMetalMap.MetalnessMap =
            TextureLoader.Load("../../../../assets/textures/engraved/roughness-map.jpg");

        var cubeMaterialWithRoughnessMap = (MeshStandardMaterial)cubeMaterial.Clone();
        cubeMaterialWithRoughnessMap.RoughnessMap =
            TextureLoader.Load("../../../../assets/textures/engraved/roughness-map.jpg");


        var cube1 = AddGeometryWithMaterial(scene, sphere, "metal", cubeMaterialWithMetalMap);
        cube1.Position.X = -10;
        cube1.Rotation.Y = 1.0f / 3 * (float)Math.PI;

        var cube2 = AddGeometryWithMaterial(scene, sphere, "rough", cubeMaterialWithRoughnessMap);
        cube2.Position.X = 17;
        cube2.Rotation.Y = -1.0f / 3 * (float)Math.PI;
    }


    public override void Render()
    {
        if (!imGuiManager.ImWantMouse)
            controls.Enabled = true;
        else
            controls.Enabled = false;

        controls.Update();
        renderer.Render(scene, camera);

        if (phase > 2 * Math.PI)
        {
            invert = invert * -1;
            phase -= 2 * (float)Math.PI;
        }
        else
        {
            phase += 0.02f;
        }

        sphereLightMesh.Position.Z = +(21 * (float)Math.Sin(phase));
        sphereLightMesh.Position.X = -14 + 14 * (float)Math.Cos(phase);
        sphereLightMesh.Position.Y = 5;

        if (invert < 0)
        {
            var pivot = 0;
            sphereLightMesh.Position.X = invert * (sphereLightMesh.Position.X - pivot) + pivot;
        }

        pointLight.Position.Copy(sphereLightMesh.Position);
    }
}