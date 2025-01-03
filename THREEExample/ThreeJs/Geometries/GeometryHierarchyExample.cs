using System;
using OpenTK.Windowing.Common;
using THREE;
using Color = THREE.Color;

namespace THREEExample.Three.Geometries;

[Example("Hierachy", ExampleCategory.ThreeJs, "geometry")]
public class GeometryHierarchyExample : Example
{
    private Group group;
    public int mouseX;
    public int mouseY;
    public int windowHalfX, windowHalfY;

    public GeometryHierarchyExample()
    {
        scene.Background = Color.Hex(0xffffff);
        scene.Fog = new Fog(0xffffff, 1, 10000);
        stopWatch.Start();
    }

    public override void InitCamera()
    {
        camera = new PerspectiveCamera(60, glControl.AspectRatio, 1, 10000);
        camera.Position.Z = 500;
    }

    public override void OnResize(ResizeEventArgs clientSize)
    {
        base.OnResize(clientSize);
        windowHalfX = clientSize.Width / 2;
        windowHalfY = clientSize.Height / 2;
    }

    public override void Init()
    {
        base.Init();

        BuildScene();
    }

    public virtual void BuildScene()
    {
        var geometry = new BoxBufferGeometry(100, 100, 100);
        var material = new MeshNormalMaterial();

        group = new Group();

        for (var i = 0; i < 1000; i++)
        {
            var mesh = new Mesh(geometry, material);
            mesh.Position.X = MathUtils.NextFloat() * 2000 - 1000;
            mesh.Position.Y = MathUtils.NextFloat() * 2000 - 1000;
            mesh.Position.Z = MathUtils.NextFloat() * 2000 - 1000;

            mesh.Rotation.X = MathUtils.NextFloat() * 2 * (float)Math.PI;
            mesh.Rotation.Y = MathUtils.NextFloat() * 2 * (float)Math.PI;

            mesh.MatrixAutoUpdate = false;
            mesh.UpdateMatrix();

            group.Add(mesh);
        }

        scene.Add(group);

        MouseMove += OnMouseMove;
    }

    public void OnMouseMove(object sender, MouseEventArgs e)
    {
        mouseX = (e.X - windowHalfX) * 10;
        mouseY = (e.Y - windowHalfY) * 10;
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

        group.Rotation.X = rx;
        group.Rotation.Y = ry;
        group.Rotation.Z = rz;
        base.Render();
    }
}