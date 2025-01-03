using System;
using ImGuiNET;
using THREE;

namespace THREEExample.Learning.Chapter05;

[Example("02.Basic-2D-Geometries-Circle", ExampleCategory.LearnThreeJS, "Chapter05")]
public class Basic2D_Geometries_Circle : Basic2D_Geometries_Plane
{
    private float radius = 4;
    private int segments = 10;
    private float thetaLength = (float)(0.3f * Math.PI * 2);
    private float thetaStart = (float)(0.3f * Math.PI * 2);

    public override BufferGeometry BuildGeometry()
    {
        return new CircleBufferGeometry(radius, segments, thetaStart, thetaLength);
    }

    public override bool AddGeometryParameter()
    {
        var rebuildGeometry = false;
        if (ImGui.SliderFloat("radius", ref radius, 0, 40)) rebuildGeometry = true;
        if (ImGui.SliderInt("segments", ref segments, 0, 40)) rebuildGeometry = true;
        if (ImGui.SliderFloat("thetaStart", ref thetaStart, 0, (float)(2 * Math.PI))) rebuildGeometry = true;
        if (ImGui.SliderFloat("thetaLength", ref thetaLength, 0, (float)(2 * Math.PI))) rebuildGeometry = true;

        return rebuildGeometry;
    }
}