﻿using System.Collections;
using ImGuiNET;
using THREE;
using THREEExample.Learning.Utils;
using Matrix4 = THREE.Matrix4;

namespace THREEExample.Learning.Chapter06;

[Example("07-Text-Geometry", ExampleCategory.LearnThreeJS, "Chapter06")]
public class TextGeometryExample : MaterialExampleTemplate
{
    public Mesh appliedMesh;
    private bool bevelEnabled = true;
    private int bevelSegments = 3;
    private float bevelSize = 0.5f;
    private int bevelThickness = 2;
    private int curveSegments = 12;
    private Font font;

    private int fontIndex;

    public Mesh groundPlane;
    private int height = 90;

    private Hashtable options;
    private int size = 90;

    private float step;
    private int steps = 1;

    public override void Init()
    {
        base.Init();

        BuildMesh();

        AddGuiControlsAction = ShowControls;
    }

    public override void InitLighting()
    {
        base.InitLighting();
        DemoUtils.InitDefaultLighting(scene);
    }

    private TextGeometry createTextGeometry(Hashtable parameter)
    {
        return new TextGeometry("Learning Three.js", parameter);
    }

    private void BuildMesh()
    {
        groundPlane = DemoUtils.AddLargeGroundPlane(scene);

        groundPlane.Position.Y = -30;

        font = FontLoader.Load(@"../../../../assets/fonts/bitstream_vera_sans_mono_roman.typeface.json");

        options = new Hashtable
        {
            { "size", size },
            { "height", height },
            { "font", font },
            { "bevelThickness", bevelThickness },
            { "bevelSize", bevelSize },
            { "bevelSegments", bevelSegments },
            { "bevelEnabled", bevelEnabled },
            { "curveSegments", curveSegments },
            { "steps", steps }
        };


        var geom = createTextGeometry(options);
        geom.ApplyMatrix4(new Matrix4().MakeScale(0.05f, 0.05f, 0.05f));
        geom.Center();

        //var geometry = GeneratePoints(NewPoints(5),64,1,8,false);
        Material material = null;
        appliedMesh = DemoUtils.AppliyMeshNormalMaterial(geom, ref material);
        material.Name = "MeshNormalMaterial";
        materialsLib.Add(material.Name, material);

        appliedMesh.CastShadow = true;

        scene.Add(appliedMesh);
    }

    public override void Render()
    {
        appliedMesh.Rotation.Y = step += 0.005f;
        appliedMesh.Rotation.X = step;
        appliedMesh.Rotation.Z = step;
        base.Render();
    }

    private void ShowControls()
    {
        var redraw = false;

        if (ImGui.SliderInt("size", ref size, 0, 200))
            redraw = true;
        if (ImGui.SliderInt("height", ref height, 0, 200))
            redraw = true;
        if (ImGui.Combo("fontName", ref fontIndex, "bitstream vera sans mono\0helvetiker\0helvetiker bold\0"))
        {
            redraw = true;
            switch (fontIndex)
            {
                case 0:
                    font = FontLoader.Load(@"../../../../assets/fonts/bitstream_vera_sans_mono_roman.typeface.json");
                    break;
                case 1: font = FontLoader.Load(@"../../../../assets/fonts/helvetiker_regular.typeface.json"); break;
                case 2: font = FontLoader.Load(@"../../../../assets/fonts/helvetiker_bold.typeface.json"); break;
            }
        }

        if (ImGui.SliderInt("bevelThickness", ref bevelThickness, 0, 10))
            redraw = true;

        if (ImGui.SliderFloat("bevelSize", ref bevelSize, 0, 100))
            redraw = true;
        if (ImGui.SliderInt("bevelSegments", ref bevelSegments, 0, 30))
            redraw = true;
        if (ImGui.Checkbox("bevelEnabled", ref bevelEnabled))
            redraw = true;
        if (ImGui.SliderInt("curveSegments", ref curveSegments, 1, 30))
            redraw = true;
        if (ImGui.SliderInt("steps", ref steps, 1, 5))
            redraw = true;

        if (redraw)
        {
            options["size"] = size;
            options["font"] = font;
            options["height"] = height;
            options["bevelThickness"] = bevelThickness;
            options["bevelSize"] = bevelSize;
            options["bevelSegments"] = bevelSegments;
            options["bevelEnabled"] = bevelEnabled;
            options["steps"] = steps;

            appliedMesh.Geometry = createTextGeometry(options);
            appliedMesh.Geometry.ApplyMatrix4(new Matrix4().MakeScale(0.05f, 0.05f, 0.05f));
            appliedMesh.Geometry.Center();
        }

        foreach (var item in materialsLib)
        {
            AddBasicMaterialSettings(item.Value, item.Key + "-THREE.Material");
            AddSpecificMaterialSettings(item.Value, item.Key + "-THREE.MeshStandardMaterial");
        }
    }

    public override void AddWireframeProperty(Material material)
    {
        ImGui.Checkbox("wireframe", ref material.Wireframe);
    }

    public override void AddSpecificMaterialSettings(Material material, string name)
    {
        if (ImGui.TreeNode(name))
        {
            AddWireframeProperty(material);
            ImGui.TreePop();
        }
    }
}