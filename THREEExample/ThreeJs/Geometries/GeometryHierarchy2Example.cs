using System;
using THREE;

namespace THREEExample.Three.Geometries;

[Example("Hierachy2", ExampleCategory.ThreeJs, "geometry")]
public class GeometryHierarchy2Example : GeometryHierarchyExample
{
    private Mesh root;

    public override void BuildScene()
    {
        var geometry = new BoxBufferGeometry(100, 100, 100);
        var material = new MeshNormalMaterial();

        root = new Mesh(geometry, material);
        root.Position.X = 1000;
        scene.Add(root);
        var amount = 200;
        Mesh object3d;
        var parent = root;

        for (var i = 0; i < amount; i++)
        {
            object3d = new Mesh(geometry, material);
            object3d.Position.X = 100;

            parent.Add(object3d);
            parent = object3d;
        }

        parent = root;

        for (var i = 0; i < amount; i++)
        {
            object3d = new Mesh(geometry, material);
            object3d.Position.X = -100;

            parent.Add(object3d);
            parent = object3d;
        }

        parent = root;

        for (var i = 0; i < amount; i++)
        {
            object3d = new Mesh(geometry, material);
            object3d.Position.Y = -100;

            parent.Add(object3d);
            parent = object3d;
        }

        parent = root;

        for (var i = 0; i < amount; i++)
        {
            object3d = new Mesh(geometry, material);
            object3d.Position.Y = 100;

            parent.Add(object3d);
            parent = object3d;
        }

        parent = root;

        for (var i = 0; i < amount; i++)
        {
            object3d = new Mesh(geometry, material);
            object3d.Position.Z = -100;

            parent.Add(object3d);
            parent = object3d;
        }

        parent = root;

        for (var i = 0; i < amount; i++)
        {
            object3d = new Mesh(geometry, material);
            object3d.Position.Z = 100;

            parent.Add(object3d);
            parent = object3d;
        }

        MouseMove += OnMouseMove;
    }

    public override void Render()
    {
        var time = stopWatch.ElapsedMilliseconds * 0.001f;

        var rx = (float)Math.Sin(time * 0.7) * 0.5f;
        var ry = (float)Math.Sin(time * 0.3) * 0.5f;
        var rz = (float)Math.Sin(time * 0.2) * 0.5f;

        camera.Position.X += (mouseX - camera.Position.X) * 0.05f;
        camera.Position.Y += (-mouseY - camera.Position.Y) * 0.05f;

        camera.LookAt(scene.Position);

        root.Traverse(o =>
        {
            o.Rotation.X = rx;
            o.Rotation.Y = ry;
            o.Rotation.Z = rz;
        });

        renderer.Render(scene, camera);
    }
}