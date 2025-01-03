﻿using System.Collections.Generic;
using ImGuiNET;
using THREE;

namespace THREEExample.Learning.Chapter05;

[Example("11.Basic-3D-Geometries-Polyhedron", ExampleCategory.LearnThreeJS, "Chapter05")]
public class Basic3D_Geometries_Polyhedron : Basic3D_Geometries_Cube
{
    private int detail;

    private float radius = 10;
    private int selectedIndex;

    public override BufferGeometry BuildGeometry()
    {
        switch (selectedIndex)
        {
            case 0:
                return new IcosahedronBufferGeometry(radius, detail);
            case 1: return new TetrahedronBufferGeometry(radius, detail);
            case 2: return new OctahedronBufferGeometry(radius, detail);
            case 3: return new DodecahedronBufferGeometry(radius, detail);
            case 4:
            default:
                var vertices = new List<float> { 1, 1, 1, -1, -1, 1, -1, 1, -1, 1, -1, -1 };
                var indices = new List<int> { 2, 1, 0, 0, 3, 2, 1, 3, 0, 2, 3, 1 };

                return new PolyhedronBufferGeometry(vertices, indices, radius, detail);
        }
    }

    public override bool AddGeometryParameter()
    {
        var rebuildGeometry = false;
        if (ImGui.SliderFloat("radius", ref radius, 0, 40)) rebuildGeometry = true;
        if (ImGui.SliderInt("detail", ref detail, 0, 3)) rebuildGeometry = true;
        if (ImGui.Combo("shadowSide", ref selectedIndex,
                "Icosahedron\0Tetrahedron\0Octahedron\0Dodecahedron\0Custom\0")) rebuildGeometry = true;

        return rebuildGeometry;
    }
}