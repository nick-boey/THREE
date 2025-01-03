using System;
using ImGuiNET;
using THREE;

namespace THREEExample.Learning.Chapter05;

[Example("09.Basic-3D-Geometries-Torus", ExampleCategory.LearnThreeJS, "Chapter05")]
public class Basic3D_Geometries_Torus : Basic3D_Geometries_Cube
{
    private float arc = (float)Math.PI * 2;
    private int radialSegments = 8;
    private float radius = 10;
    private float tube = 10;
    private int tubularSegments = 6;

    public override BufferGeometry BuildGeometry()
    {
        return new TorusBufferGeometry(radius, tube, radialSegments, tubularSegments, arc);
    }

    public override bool AddGeometryParameter()
    {
        var rebuildGeometry = false;
        if (ImGui.SliderFloat("radius", ref radius, 0, 40)) rebuildGeometry = true;
        if (ImGui.SliderFloat("tube", ref tube, 0, 40)) rebuildGeometry = true;
        if (ImGui.SliderInt("radialSegments", ref radialSegments, 0, 40)) rebuildGeometry = true;
        if (ImGui.SliderInt("tubularSegments", ref tubularSegments, 1, 20)) rebuildGeometry = true;
        if (ImGui.SliderFloat("arc", ref arc, 0, (float)Math.PI * 2)) rebuildGeometry = true;

        return rebuildGeometry;
    }
}