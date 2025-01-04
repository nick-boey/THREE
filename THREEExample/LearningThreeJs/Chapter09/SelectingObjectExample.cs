﻿using System;
using System.Collections.Generic;
using ImGuiNET;
using THREE;
using THREEExample.Learning.Utils;
using Color = THREE.Color;

namespace THREEExample.Learning.Chapter09;

[Example("02-SelectingObject", ExampleCategory.LearnThreeJS, "Chapter09")]
public class SelectingObjectExample : Example
{
    private float bouncingSpeed = 0.03f;
    public Mesh cube;
    public Mesh cylinder;
    public Mesh plane;

    private float rotationSpeed = 0.02f;
    private float scalingSpeed = 0.03f;
    private float scalingStep;
    public Mesh sphere;
    private float step;

    public override void InitRenderer()
    {
        renderer.SetClearColor(Color.Hex(0x000000));
        renderer.ShadowMap.Enabled = true;
        renderer.ShadowMap.Type = Constants.PCFSoftShadowMap;
    }

    public override void InitLighting()
    {
        base.InitLighting();
        DemoUtils.InitDefaultLighting(scene);
    }

    public override void Init()
    {
        base.Init();
        MouseMove += OnMouseMove;
        MouseDown += OnMouseDown;

        var groundPlane = DemoUtils.AddGroundPlane(scene);

        groundPlane.Position.Y = 0;

        var cubeGeometry = new BoxGeometry(4, 4, 4);
        var cubeMaterial = new MeshStandardMaterial { Color = Color.Hex(0xff0000) };
        cube = new Mesh(cubeGeometry, cubeMaterial);
        cube.CastShadow = true;
        cube.Position.Set(-10, 4, 0);
        scene.Add(cube);

        var sphereGeometry = new SphereGeometry(4, 20, 20);
        var sphereMaterial = new MeshStandardMaterial { Color = Color.Hex(0x7777ff) };
        sphere = new Mesh(sphereGeometry, sphereMaterial);
        sphere.Position.Set(20, 0, 2);
        sphere.CastShadow = true;
        scene.Add(sphere);

        var cylinderGeometry = new CylinderGeometry(2, 2, 20);
        var cylinderMaterial = new MeshStandardMaterial { Color = Color.Hex(0x77ff77) };
        cylinder = new Mesh(cylinderGeometry, cylinderMaterial);
        cylinder.CastShadow = true;
        cylinder.Position.Set(0, 0, 1);

        scene.Add(cylinder);

        // add subtle ambient lighting
        var ambienLight = new AmbientLight(Color.Hex(0x353535));
        scene.Add(ambienLight);

        AddGuiControlsAction = () =>
        {
            ImGui.Text("This is Rotaion Speed Control box");
            ImGui.SliderFloat("RotationSpeed", ref rotationSpeed, 0.0f, 0.5f);
            ImGui.SliderFloat("bouncingSpeed", ref bouncingSpeed, 0.0f, 0.5f);
            ImGui.SliderFloat("scalingSpeed", ref scalingSpeed, 0.0f, 0.5f);
        };
    }


    public override void Render()
    {
        cube.Rotation.X += rotationSpeed;
        cube.Rotation.Y += rotationSpeed;
        cube.Rotation.Z += rotationSpeed;

        step += bouncingSpeed;

        sphere.Position.X = 20 + 10 * (float)Math.Cos(step);
        sphere.Position.Y = 2 + 10 * (float)Math.Abs(Math.Sin(step));

        // scale the cylinder
        scalingStep += scalingSpeed;
        var scaleX = (float)Math.Abs(Math.Sin(scalingStep / 4));
        var scaleY = (float)Math.Abs(Math.Cos(scalingStep / 5));
        var scaleZ = (float)Math.Abs(Math.Sin(scalingStep / 7));
        cylinder.Scale.Set(scaleX, scaleY, scaleZ);


        renderer.Render(scene, camera);
    }

    public void OnMouseDown(object sender, MouseEventArgs e)
    {
        var vector = new Vector3((float)e.X / glControl.Width * 2 - 1, -((float)e.Y / glControl.Height) * 2 + 1, 0.5f);
        vector = vector.UnProject(camera);

        var raycaster = new Raycaster(camera.Position, vector.Sub(camera.Position).Normalize());
        var intersects = raycaster.IntersectObjects(new List<Object3D> { sphere, cylinder, cube });

        if (intersects.Count > 0)
        {
            intersects[0].Object3D.Material.Transparent = true;
            intersects[0].Object3D.Material.Opacity = 0.1f;
        }
    }

    public void OnMouseMove(object sender, MouseEventArgs e)
    {
    }
}