﻿using THREE;
using THREEExample.Learning.Chapter10;
using THREEExample.Learning.Utils;

namespace THREEExample.Learning.Chapter09;

[Example("04-Basic-Texture-tga", ExampleCategory.LearnThreeJS, "Chapter10")]
public class BasicTextureTgaExample : TemplateExample
{
    public override void SetGeometryWithTexture()
    {
        var groundPlane = DemoUtils.AddLargeGroundPlane(scene);
        groundPlane.Position.Y = -10;

        scene.Add(new AmbientLight(new Color(0x444444)));

        var polyhedron = new IcosahedronBufferGeometry(8, 0);
        polyhedronMesh = AddGeometry(scene, polyhedron, "polyhedron",
            TextureLoader.LoadTGA("../../../../assets/textures/tga/dried_grass.tga"));
        polyhedronMesh.Position.X = 20;

        var sphere = new SphereBufferGeometry(5, 20, 20);
        sphereMesh = AddGeometry(scene, sphere, "sphere",
            TextureLoader.LoadTGA("../../../../assets/textures/tga/grass.tga"));

        var cube = new BoxBufferGeometry(10, 10, 10);
        cubeMesh = AddGeometry(scene, cube, "cube", TextureLoader.LoadTGA("../../../../assets/textures/tga/moss.tga"));
        cubeMesh.Position.X = -20;
    }
}