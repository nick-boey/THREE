﻿using System;
using System.Collections.Generic;
using ImGuiNET;
using THREE;

namespace THREEExample.Three.Buffergeometry;

[Example("buffergeometry_line", ExampleCategory.ThreeJs, "Buffergeometry")]
internal class BuffergeometryLineExample : Example
{
    private readonly int r = 800;

    private readonly int segments = 10000;

    private readonly DateTime time = DateTime.Now;
    private Line line;
    private bool morphTargets;
    private float t;

    public override void InitCamera()
    {
        camera = new PerspectiveCamera(27, glControl.AspectRatio, 1, 4000);
        camera.Position.Set(0, 0, 2750);
    }

    public override void Init()
    {
        base.Init();

        CreateObject();
    }

    public virtual void CreateObject()
    {
        var geometry = new BufferGeometry();
        var material = new LineBasicMaterial { VertexColors = true, MorphTargets = true };

        var positions = new List<float>();
        var colors = new List<float>();

        for (var i = 0; i < segments; i++)
        {
            var x = MathUtils.NextFloat() * r - r / 2;
            var y = MathUtils.NextFloat() * r - r / 2;
            var z = MathUtils.NextFloat() * r - r / 2;

            // positions

            positions.Add(x, y, z);

            // colors

            colors.Add(x / r + 0.5f);
            colors.Add(y / r + 0.5f);
            colors.Add(z / r + 0.5f);
        }

        geometry.SetAttribute("position", new BufferAttribute<float>(positions.ToArray(), 3));
        geometry.SetAttribute("color", new BufferAttribute<float>(colors.ToArray(), 3));
        GenerateMorphTargets(geometry);

        geometry.ComputeBoundingSphere();

        line = new Line(geometry, material);
        scene.Add(line);

        AddGuiControlsAction = showMorphTargets;
    }

    private void GenerateMorphTargets(BufferGeometry geometry)
    {
        var data = new List<float>();

        for (var i = 0; i < segments; i++)
        {
            var x = MathUtils.NextFloat() * r - r / 2;
            var y = MathUtils.NextFloat() * r - r / 2;
            var z = MathUtils.NextFloat() * r - r / 2;

            data.Add(x, y, z);
        }

        var morphTarget = new BufferAttribute<float>(data.ToArray(), 3);
        morphTarget.Name = "target1";

        geometry.MorphAttributes["position"] = new List<IBufferAttribute> { morphTarget };
    }

    public override void Render()
    {
        var delta = (DateTime.Now - time).Milliseconds;
        var elapsedTime = stopWatch.ElapsedMilliseconds;

        line.Rotation.X = elapsedTime * 0.0025f;
        line.Rotation.Y = elapsedTime * 0.005f;
        if (morphTargets)
        {
            t = t + delta * 0.005f;
            line.MorphTargetInfluences[0] = (float)Math.Abs(Math.Sin(t));
        }

        base.Render();
    }

    private void showMorphTargets()
    {
        ImGui.Checkbox("morphTargets", ref morphTargets);
    }
}