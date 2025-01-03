﻿using THREE;
using Color = THREE.Color;

namespace THREEExample.Learning.Chapter04;

[Example("08.Mesh-Toon-Material", ExampleCategory.LearnThreeJS, "Chapter04")]
public class MeshToonMaterialExample : MeshPhongMaterialExample
{
    public override void BuildMeshMaterial()
    {
        meshMaterial = new MeshToonMaterial();
        meshMaterial.Color = Color.Hex(0x7777ff);
        meshMaterial.Name = "MeshToonMaterial";
    }

    public override void AddSpotLight()
    {
        base.AddSpotLight();
        spotLight.Position.Set(0, 30, 60);
    }
}