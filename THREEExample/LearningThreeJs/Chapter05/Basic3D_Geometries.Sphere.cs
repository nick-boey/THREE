using System;
using ImGuiNET;
using THREE;

namespace THREEExample.Learning.Chapter05;

[Example("06.Basic-3D-Geometries-Sphere", ExampleCategory.LearnThreeJS, "Chapter05")]
public class Basic3D_Geometries_Sphere : Basic3D_Geometries_Cube
{
    private int heightSegments = 10;
    private float phiLength = (float)Math.PI * 2;
    private float phiStart;
    private float radius = 4;
    private float thetaLength = (float)Math.PI;
    private float thetaStart;
    private int widthSegments = 10;

    public override BufferGeometry BuildGeometry()
    {
        return new SphereBufferGeometry(radius, widthSegments, heightSegments, phiStart, phiLength, thetaStart,
            thetaLength);
    }

    public override bool AddGeometryParameter()
    {
        var rebuildGeometry = false;
        if (ImGui.SliderFloat("radius", ref radius, 0, 40)) rebuildGeometry = true;
        if (ImGui.SliderInt("widthSegments", ref widthSegments, 0, 20)) rebuildGeometry = true;
        if (ImGui.SliderInt("heightSegments", ref heightSegments, 0, 20)) rebuildGeometry = true;
        if (ImGui.SliderFloat("phiStart", ref phiStart, 0, (float)Math.PI * 2)) rebuildGeometry = true;
        if (ImGui.SliderFloat("phiLength", ref phiLength, 0, (float)Math.PI * 2)) rebuildGeometry = true;
        if (ImGui.SliderFloat("thetaStart", ref thetaStart, 0, (float)Math.PI * 2)) rebuildGeometry = true;
        if (ImGui.SliderFloat("thetaLength", ref thetaLength, 0, (float)Math.PI * 2)) rebuildGeometry = true;

        return rebuildGeometry;
    }
}