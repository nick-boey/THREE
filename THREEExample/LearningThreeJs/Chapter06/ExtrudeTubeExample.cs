﻿using System;
using System.Collections.Generic;
using ImGuiNET;
using THREE;
using THREEExample.Learning.Utils;
using Color = THREE.Color;

namespace THREEExample.Learning.Chapter06;

[Example("04.Extrude-Tube", ExampleCategory.LearnThreeJS, "Chapter06")]
public class ExtrudeTubeExample : ExtrudeGeometryExample
{
    private bool closed;
    private int numberOfPoints = 5;
    private int radius = 1;
    private int radiusSegments = 8;
    private int segments = 64;
    public Object3D spGroup;
    private float step;

    public override void Init()
    {
        base.Init();

        appliedNormalMaterial = appliedMesh.Material;

        groundPlane = DemoUtils.AddLargeGroundPlane(scene);

        groundPlane.Position.Y = -30;

        scene.Add(spGroup);
    }

    public override void Render()
    {
        if (!imGuiManager.ImWantMouse)
            controls.Enabled = true;
        else
            controls.Enabled = false;

        appliedMesh.Rotation.Y = step += 0.0001f;
        appliedMesh.Rotation.X = step;
        appliedMesh.Rotation.Z = step;

        if (spGroup != null)
        {
            spGroup.Rotation.Y = step;
            spGroup.Rotation.X = step;
            spGroup.Rotation.Z = step;
        }

        controls.Update();
        renderer.Render(scene, camera);
    }

    public override BufferGeometry BuildGeometry()
    {
        return GeneratePoints(NewPoints(numberOfPoints), segments, radius, radiusSegments, closed);
    }

    private List<Vector3> NewPoints(int numberOfPoints)
    {
        var points = new List<Vector3>();
        //{
        //    new Vector3(26,21,-18),
        //    new Vector3(11,-1,7),
        //    new Vector3(10,8,0),
        //    new Vector3(21,8,-4),
        //    new Vector3(28,8,-4)
        //};
        var random = new Random();
        for (var i = 0; i < numberOfPoints; i++)
        {
            var randomX = -20 + (float)Math.Round((double)random.Next(0, 50));
            var randomY = -15 + (float)Math.Round((double)random.Next(0, 40));
            var randomZ = -20 + (float)Math.Round((double)random.Next(0, 40));

            points.Add(new Vector3(randomX, randomY, randomZ));
        }

        return points;
    }

    private BufferGeometry GeneratePoints(List<Vector3> points, int segments, float radius, int radiusSegments,
        bool closed)
    {
        if (spGroup != null) scene.Remove(spGroup);

        spGroup = new Object3D();

        var material = new MeshBasicMaterial { Color = Color.Hex(0xff0000), Transparent = false };

        points.ForEach(delegate(Vector3 point)
            {
                var spGeom = new SphereBufferGeometry(0.2f);
                var spMesh = new Mesh(spGeom, material);
                spMesh.Position.Copy(point);
                spGroup.Add(spMesh);
            }
        );
        scene.Add(spGroup);
        return new TubeBufferGeometry(new CatmullRomCurve3(points), segments, radius, radiusSegments, closed);
    }

    public override void AddParameterSettings()
    {
        rebuildGeometry = false;
        if (ImGui.Button("newPoints")) rebuildGeometry = true;
        if (ImGui.SliderInt("numperOfPoints", ref numberOfPoints, 2, 15))
            rebuildGeometry = true;
        if (ImGui.SliderInt("segments", ref segments, 0, 200)) rebuildGeometry = true;
        if (ImGui.SliderInt("radius", ref radius, 0, 10)) rebuildGeometry = true;
        if (ImGui.SliderInt("radiusSegments", ref radiusSegments, 0, 100)) rebuildGeometry = true;
        if (ImGui.Checkbox("closed", ref closed)) rebuildGeometry = true;
    }

    public override void RebuildGeometry()
    {
        scene.Remove(appliedMesh);
        appliedMesh = new Mesh(BuildGeometry(), appliedNormalMaterial);
        scene.Add(appliedMesh);
    }
}