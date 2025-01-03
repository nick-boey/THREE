using ImGuiNET;
using THREE;

namespace THREEExample.Learning.Chapter05;

[Example("10.Basic-3D-Geometries-Torus-knot", ExampleCategory.LearnThreeJS, "Chapter05")]
public class Basic3D_Geometries_Torus_knot : Basic3D_Geometries_Cube
{
    private int p = 2;
    private int q = 3;
    private int radialSegments = 8;
    private float radius = 1;
    private float tube = 0.3f;
    private int tubularSegments = 64;

    public override BufferGeometry BuildGeometry()
    {
        return new TorusKnotBufferGeometry(radius, tube, tubularSegments, radialSegments, p, q);
    }

    public override bool AddGeometryParameter()
    {
        var rebuildGeometry = false;
        if (ImGui.SliderFloat("radius", ref radius, 0, 10)) rebuildGeometry = true;
        if (ImGui.SliderFloat("tube", ref tube, 0, 10)) rebuildGeometry = true;
        if (ImGui.SliderInt("radialSegments", ref radialSegments, 0, 400)) rebuildGeometry = true;
        if (ImGui.SliderInt("tubularSegments", ref tubularSegments, 1, 200)) rebuildGeometry = true;
        if (ImGui.SliderInt("p", ref p, 1, 10)) rebuildGeometry = true;
        if (ImGui.SliderInt("q", ref q, 1, 15)) rebuildGeometry = true;

        return rebuildGeometry;
    }
}