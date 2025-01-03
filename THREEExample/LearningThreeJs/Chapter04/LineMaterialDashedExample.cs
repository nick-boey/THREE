﻿using THREE;

namespace THREEExample.Learning.Chapter04;

[Example("13.Line-Material-Dashed", ExampleCategory.LearnThreeJS, "Chapter04")]
public class LineMaterialDashedExample : LineMaterialExample
{
    public override void BuildMeshMaterial()
    {
        meshMaterial = new LineDashedMaterial
        {
            Opacity = 1.0f,
            LineWidth = 1,
            VertexColors = true
        };
    }

    public override void BuildMesh()
    {
        base.BuildMesh();
        line.ComputeLineDistances();
    }
}