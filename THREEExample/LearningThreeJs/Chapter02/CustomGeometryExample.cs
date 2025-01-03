using System;
using System.Collections.Generic;
using ImGuiNET;
using THREE;
using Color = THREE.Color;

namespace THREEExample.Chapter02;

[Example("05-Custom Geometry", ExampleCategory.LearnThreeJS, "Chapter02")]
public class CustomGeometryExample : Example
{
    private List<Vector3> controlPoint;
    private Group mesh;
    private bool verticesChanged;

    public override void InitRenderer()
    {
        base.InitRenderer();
        renderer.SetClearColor(Color.Hex(0xEEEEEE));
        renderer.ShadowMapEnabled = true;
    }

    public override void InitLighting()
    {
        base.InitLighting();
        var ambientLight = new AmbientLight(Color.Hex(0x494949));
        scene.Add(ambientLight);

        var spotLight = new SpotLight(new Color().SetHex(0xffffff), 1, 180, (float)Math.PI / 4);
        spotLight.Shadow.MapSize.Height = 2048;
        spotLight.Shadow.MapSize.Width = 2048;
        spotLight.Position.Set(-40, 30, 30);
        spotLight.CastShadow = true;
        spotLight.LookAt(spotLight.Position);

        scene.Add(spotLight);
    }

    public override void Init()
    {
        base.Init();
        var planeGeometry = new PlaneBufferGeometry(60, 40, 1, 1);
        var planeMaterial = new MeshLambertMaterial { Color = Color.Hex(0xffffff) };

        var plane = new Mesh(planeGeometry, planeMaterial);
        plane.ReceiveShadow = true;

        plane.Rotation.X = (float)(-0.5 * Math.PI);
        plane.Position.X = 0;
        plane.Position.Y = 0;
        plane.Position.Z = 0;

        scene.Add(plane);


        var vertices = new List<Vector3>
        {
            new(1, 3, 1),
            new(1, 3, -1),
            new(1, -1, 1),
            new(1, -1, -1),
            new(-1, 3, -1),
            new(-1, 3, 1),
            new(-1, -1, -1),
            new(-1, -1, 1)
        };

        var faces = new List<Face3>
        {
            new(0, 2, 1),
            new(2, 3, 1),
            new(4, 6, 5),
            new(6, 7, 5),
            new(4, 5, 1),
            new(5, 0, 1),
            new(7, 6, 2),
            new(6, 3, 2),
            new(5, 7, 0),
            new(7, 2, 0),
            new(1, 3, 4),
            new(3, 6, 4)
        };

        controlPoint = new List<Vector3>(vertices);

        var geom = new Geometry();
        geom.Vertices = vertices;
        geom.Faces = faces;

        geom.ComputeFaceNormals();


        var materials = new List<Material>
        {
            new MeshBasicMaterial { Color = Color.ColorName(ColorKeywords.black), Wireframe = true },
            new MeshLambertMaterial { Opacity = 0.6f, Color = new Color().SetHex(0x44ff44), Transparent = true }
        };

        mesh = SceneUtils.CreateMultiMaterialObject(geom, materials);

        mesh.Traverse(o => { o.CastShadow = true; });
        scene.Add(mesh);


        AddGuiControlsAction = () =>
        {
            if (ImGui.Button("Clone")) GeometryClone();
            verticesChanged = false;
            for (var i = 0; i < vertices.Count; i++)
                AddControlPoint(i);
        };
    }

    public override void Render()
    {
        if (mesh != null)
            foreach (Mesh m in mesh.Children)
            {
                m.Geometry.Vertices = controlPoint;
                m.Geometry.ElementsNeedUpdate = true;
                m.Geometry.ComputeFaceNormals();
            }

        base.Render();
    }

    private void AddControlPoint(int index)
    {
        ImGui.Text("Vertices " + (index + 1));

        var point = controlPoint[index];

        if (ImGui.SliderFloat("x" + (index + 1), ref controlPoint[index].X, -10, 10)) verticesChanged = true;
        if (ImGui.SliderFloat("y" + (index + 1), ref controlPoint[index].Y, -10, 10)) verticesChanged = true;
        if (ImGui.SliderFloat("z" + (index + 1), ref controlPoint[index].Z, -10, 10)) verticesChanged = true;
    }


    internal void GeometryClone()
    {
        var clonedGeometry = (Geometry)mesh.Children[0].Geometry.Clone();
        var materials = new List<Material>
        {
            new MeshBasicMaterial { Color = new Color().SetHex(0x000000), Wireframe = true },
            new MeshLambertMaterial { Opacity = 0.6f, Color = new Color().SetHex(0xff44ff), Transparent = true }
        };
        var mesh2 = SceneUtils.CreateMultiMaterialObject(clonedGeometry, materials);

        mesh2.Traverse(e => { e.CastShadow = true; });

        mesh2.TranslateX(5);
        mesh2.TranslateZ(5);
        mesh2.Name = "clone";
        scene.Remove(scene.GetObjectByName("clone"));
        scene.Add(mesh2);
    }
}