using ImGuiNET;
using THREE;

namespace THREEExample.Learning.Chapter05;

[Example("05.Basic-3D-Geometries-Cube", ExampleCategory.LearnThreeJS, "Chapter05")]
public class Basic3D_Geometries_Cube : Basic2D_Geometries_Plane
{
    private float depth = 10;
    private int depthSegments = 4;
    private float height = 10;
    private int heightSegments = 4;
    private float width = 4;
    private int widthSegments = 4;

    public override BufferGeometry BuildGeometry()
    {
        return new BoxBufferGeometry(width, height, depth, widthSegments, heightSegments, depthSegments);
    }

    public override bool AddGeometryParameter()
    {
        var rebuildGeometry = false;
        if (ImGui.SliderFloat("width", ref width, 0, 40)) rebuildGeometry = true;
        if (ImGui.SliderFloat("height", ref height, 0, 40)) rebuildGeometry = true;
        if (ImGui.SliderFloat("depth", ref depth, 0, 40)) rebuildGeometry = true;
        if (ImGui.SliderInt("widthSegments", ref widthSegments, 0, 10)) rebuildGeometry = true;
        if (ImGui.SliderInt("heightSegments", ref heightSegments, 0, 10)) rebuildGeometry = true;
        if (ImGui.SliderInt("depthSegments", ref depthSegments, 0, 10)) rebuildGeometry = true;

        return rebuildGeometry;
    }
}