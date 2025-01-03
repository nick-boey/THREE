﻿using System;
using ImGuiNET;
using THREE;
using THREEExample.Learning.Utils;
using Color = THREE.Color;
using Vector3 = System.Numerics.Vector3;

namespace THREEExample.Learning.Chapter10;

[Example("15-emissive-map", ExampleCategory.LearnThreeJS, "Chapter10")]
public class EmissiveMapExample : TemplateExample
{
    private SpotLight spotLight;

    public override void SetGeometryWithTexture()
    {
        var groundPlane = DemoUtils.AddLargeGroundPlane(scene);
        groundPlane.Position.Y = -8;

        spotLight = (SpotLight)scene.GetObjectByName("spotLight");
        spotLight.Intensity = 0.1f;
        scene.Remove(scene.GetObjectByName("ambientLight"));

        var cubeMaterial = new MeshStandardMaterial
        {
            Emissive = new Color(0xffffff),
            EmissiveMap = TextureLoader.Load("../../../../assets/textures/emissive/lava.png"),
            NormalMap = TextureLoader.Load("../../../../assets/textures/emissive/lava-normals.png"),
            MetalnessMap = TextureLoader.Load("../../../../assets/textures/emissive/lava-smoothness.png"),
            Metalness = 1,
            Roughness = 0.4f,
            NormalScale = new Vector2(4, 4)
        };

        var cube = new BoxBufferGeometry(16, 16, 16);
        var cube1 = AddGeometryWithMaterial(scene, cube, "cube", cubeMaterial);
        cube1.Rotation.Y = 1 / 3 * (float)Math.PI;
        cube1.Position.X = -12;

        var sphere = new SphereBufferGeometry(9, 50, 50);
        var sphere1 = AddGeometryWithMaterial(scene, sphere, "sphere", (MeshStandardMaterial)cubeMaterial.Clone());
        sphere1.Rotation.Y = 1 / 6 * (float)Math.PI;
        sphere1.Position.X = 15;
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

    public override void AddSpecificMaterialSettings(Material material, string name)
    {
        var materialColor = material.Color.Value;
        var emissiveColor = material.Emissive.Value;
        var color = new Vector3(materialColor.R, materialColor.G, materialColor.B);
        var emissive = new Vector3(emissiveColor.R, emissiveColor.G, emissiveColor.B);
        if (ImGui.TreeNode(name))
        {
            switch (material.type)
            {
                case "MeshNormalMaterial":
                    ImGui.Checkbox("wireframe", ref material.Wireframe);
                    break;
                case "MeshPhongMaterial":
                    ImGui.SliderFloat("shininess", ref material.Shininess, 0, 100);
                    break;
                case "MeshStandardMaterial":
                    if (ImGui.ColorPicker3("color", ref color))
                    {
                        var mColor = new Color(color.X, color.Y, color.Z);
                        material.Color = mColor;
                    }

                    if (ImGui.ColorPicker3("emissive", ref emissive))
                    {
                        var eColor = new Color(emissive.X, emissive.Y, emissive.Z);
                        material.Emissive = eColor;
                    }

                    ImGui.SliderFloat("metalness", ref material.Metalness, 0, 1);
                    ImGui.SliderFloat("roughness", ref material.Roughness, 0, 1);
                    ImGui.Checkbox("wireframe", ref material.Wireframe);
                    break;
            }

            ImGui.SliderFloat("lightIntessity", ref spotLight.Intensity, 0.0f, 1.0f);
            ImGui.TreePop();
        }
    }
}