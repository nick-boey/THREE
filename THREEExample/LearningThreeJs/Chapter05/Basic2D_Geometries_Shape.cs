using System.Collections.Generic;
using ImGuiNET;
using THREE;
using THREEExample.Learning.Utils;

namespace THREEExample.Learning.Chapter05;

[Example("04.Basic-2D-Geometries-Shape", ExampleCategory.LearnThreeJS, "Chapter05")]
public class Basic2D_Geometries_Shape : Basic2D_Geometries_Plane
{
    private int curveSegments = 12;

    public override BufferGeometry BuildGeometry()
    {
        return new ShapeBufferGeometry(new List<Shape> { DemoUtils.DrawShape() }, curveSegments);
    }

    public override bool AddGeometryParameter()
    {
        var rebuildGeometry = false;
        if (ImGui.SliderInt("curveSegments", ref curveSegments, 1, 100)) rebuildGeometry = true;

        return rebuildGeometry;
    }
}