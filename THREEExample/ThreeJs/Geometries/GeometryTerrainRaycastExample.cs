﻿using System;
using THREE;

namespace THREEExample.Three.Geometries;

[Example("Terrain Raycast", ExampleCategory.ThreeJs, "geometry")]
public class GeometryTerrainRaycastExample : GeometryTerrainExample
{
    private readonly Vector2 mouse = new();

    private readonly Raycaster raycaster = new();

    private Mesh helper;
    private OrbitControls orbitControl;


    public override void InitCameraController()
    {
        orbitControl = new OrbitControls(this, camera);

        orbitControl.target.Y = Data[worldHalfWidth + worldHalfDepth * worldWidth] + 500;
        camera.Position.Y = orbitControl.target.Y + 2000;
        camera.Position.X = 2000;
        orbitControl.Update();
    }

    public override void Init()
    {
        base.Init();

        MouseMove += OnMouseMove;
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        mouse.X = 1.0f * e.X / glControl.Width * 2 - 1;
        mouse.Y = -(1.0f * e.Y / glControl.Height) * 2 + 1;
        raycaster.SetFromCamera(mouse, camera);

        // See if the ray from the camera into the world hits one of our meshes
        var intersects = raycaster.IntersectObject(mesh);

        // Toggle rotation bool for meshes that we clicked
        if (intersects.Count > 0)
        {
            helper.Position.Set(0, 0, 0);
            helper.LookAt(intersects[0].face.Normal);
            helper.Position.Copy(intersects[0].point);
        }
    }

    public override void BuildScene()
    {
        base.BuildScene();

        var coneGeometry = new ConeBufferGeometry(20, 100, 3);
        coneGeometry.Translate(0, 50, 0);
        coneGeometry.RotateX((float)Math.PI / 2);
        helper = new Mesh(coneGeometry, new MeshNormalMaterial());
        scene.Add(helper);
    }

    public override void Render()
    {
        renderer.Render(scene, camera);
    }
}