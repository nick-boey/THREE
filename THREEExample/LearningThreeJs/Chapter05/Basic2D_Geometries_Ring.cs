using System;
using ImGuiNET;
using THREE;

namespace THREEExample.Learning.Chapter05;

[Example("03.Basic-2D-Geometries-Ring", ExampleCategory.LearnThreeJS, "Chapter05")]
public class Basic2D_Geometries_Ring : Basic2D_Geometries_Plane
{
    private float innerRadius = 3;
    private float outerRadius = 10;
    private int phiSegments = 8;
    private float thetaLength = (float)Math.PI * 2;
    private int thetaSegments = 8;
    private float thetaStart;

    public override BufferGeometry BuildGeometry()
    {
        return new RingBufferGeometry(innerRadius, outerRadius, thetaSegments, phiSegments, thetaStart, thetaLength);
    }

    public override bool AddGeometryParameter()
    {
        var rebuildGeometry = false;
        if (ImGui.SliderFloat("innerRadius", ref innerRadius, 0, 40)) rebuildGeometry = true;
        if (ImGui.SliderFloat("outerRadius", ref outerRadius, 0, 100)) rebuildGeometry = true;
        if (ImGui.SliderInt("thetaSegments", ref thetaSegments, 1, 40)) rebuildGeometry = true;
        if (ImGui.SliderInt("phiSegments", ref phiSegments, 1, 40)) rebuildGeometry = true;
        if (ImGui.SliderFloat("thetaStart", ref thetaStart, 0, (float)Math.PI * 2)) rebuildGeometry = true;
        if (ImGui.SliderFloat("thetaLength", ref thetaLength, 0, (float)Math.PI * 2)) rebuildGeometry = true;

        return rebuildGeometry;
    }
}