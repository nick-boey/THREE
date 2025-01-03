﻿using THREE;

namespace THREEExample.Learning.Chapter04;

[Example("04.Mesh-Normal-Material", ExampleCategory.LearnThreeJS, "Chapter04")]
public class MeshNormalMaterialExample : BasicMeshMaterialExample
{
    public override void InitCamera()
    {
        base.InitCamera();
        camera.Position.X = -30;
        camera.Position.Y = 50;
        camera.Position.Z = 40;
        camera.LookAt(new Vector3(10, 0, 0));
    }

    public override void BuildMeshMaterial()
    {
        meshMaterial = new MeshNormalMaterial();
        meshMaterial.Name = "NormalMaterial";
    }

    public override void BuildGeometry()
    {
        base.BuildGeometry();

        for (var f = 0; f < plane.Geometry.Faces.Count; f++)
        {
            var face = plane.Geometry.Faces[f];
            var centroid = new Vector3(0, 0, 0);
            centroid.Add(plane.Geometry.Vertices[face.a]);
            centroid.Add(plane.Geometry.Vertices[face.b]);
            centroid.Add(plane.Geometry.Vertices[face.c]);
            centroid.DivideScalar(3);
            var arrow = new ArrowHelper(face.Normal, centroid, 2, Color.Hex(0x3333ff), 0.5f, 0.5f);
            plane.Add(arrow);
        }

        for (var f = 0; f < cube.Geometry.Faces.Count; f++)
        {
            var face = cube.Geometry.Faces[f];
            var centroid = new Vector3(0, 0, 0);
            centroid.Add(cube.Geometry.Vertices[face.a]);
            centroid.Add(cube.Geometry.Vertices[face.b]);
            centroid.Add(cube.Geometry.Vertices[face.c]);
            centroid.DivideScalar(3);
            var arrow = new ArrowHelper(face.Normal, centroid, 2, Color.Hex(0x3333ff), 0.5f, 0.5f);
            cube.Add(arrow);
        }

        for (var f = 0; f < sphere.Geometry.Faces.Count; f++)
        {
            var face = sphere.Geometry.Faces[f];
            var centroid = new Vector3(0, 0, 0);
            centroid.Add(sphere.Geometry.Vertices[face.a]);
            centroid.Add(sphere.Geometry.Vertices[face.b]);
            centroid.Add(sphere.Geometry.Vertices[face.c]);
            centroid.DivideScalar(3);
            var arrow = new ArrowHelper(face.Normal, centroid, 2, Color.Hex(0x3333ff), 0.5f, 0.5f);
            sphere.Add(arrow);
        }
    }
}