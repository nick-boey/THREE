﻿using System;
using ImGuiNET;
using THREE;
using THREEExample.Learning.Chapter10;
using THREEExample.Learning.Utils;

namespace THREEExample.Learning.Chapter09;

[Example("09-normal-map", ExampleCategory.LearnThreeJS, "Chapter10")]
public class NormalMapExample : TemplateExample
{
    private MeshStandardMaterial cubeMaterialWithNormalMap;
    private int invert = 1;
    private float phase;
    private PointLight pointLight;
    private Mesh sphereLightMesh;

    public override void SetGeometryWithTexture()
    {
        var groundPlane = DemoUtils.AddLargeGroundPlane(scene);
        groundPlane.Position.Y = -10;

        scene.Add(new AmbientLight(new Color(0x444444)));

        pointLight = new PointLight(new Color(0xff5808));
        scene.Add(pointLight);

        var sphereLight = new SphereBufferGeometry(0.2f);
        var sphereLightMaterial = new MeshStandardMaterial { Color = new Color(0xff5808) };
        sphereLightMesh = new Mesh(sphereLight, sphereLightMaterial);

        scene.Add(sphereLightMesh);

        var cube = new BoxBufferGeometry(16, 16, 16);
        var cubeMaterial = new MeshStandardMaterial();
        cubeMaterial.Map = TextureLoader.Load("../../../../assets/textures/general/plaster.jpg");
        cubeMaterial.Metalness = 0.2f;
        cubeMaterial.Roughness = 0.07f;

        cubeMaterialWithNormalMap = (MeshStandardMaterial)cubeMaterial.Clone();
        cubeMaterialWithNormalMap.BumpMap = TextureLoader.Load("../../../../assets/textures/stone/stone-bump.jpg");

        var cube1 = AddGeometryWithMaterial(scene, cube, "cube-1", cubeMaterial);
        cube1.Position.X = -17;
        cube1.Rotation.Y = 1.0f / 3 * (float)Math.PI;

        var cube2 = AddGeometryWithMaterial(scene, cube, "cube-2", cubeMaterialWithNormalMap);
        cube2.Position.X = 17;
        cube2.Rotation.Y = -1.0f / 3 * (float)Math.PI;
    }

    public override void Init()
    {
        base.Init();

        AddGuiControlsAction = () =>
        {
            foreach (var item in materialsLib)
            {
                AddBasicMaterialSettings(item.Value, item.Key + "-THREE.Material");
                AddSpecificMaterialSettings(item.Value, item.Key + "-THREE.MeshStandardMaterial");
            }

            ImGui.SliderFloat("naormalScaleX", ref cubeMaterialWithNormalMap.NormalScale.X, -3.0f, 3.0f);
            ImGui.SliderFloat("naormalScaleY", ref cubeMaterialWithNormalMap.NormalScale.Y, -3.0f, 3.0f);
        };
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