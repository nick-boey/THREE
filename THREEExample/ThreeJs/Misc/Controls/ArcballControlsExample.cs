﻿using System;
using ImGuiNET;
using OpenTK.Windowing.Common;
using THREE;
using Color = THREE.Color;

namespace THREEExample.Three.Misc.Controls;

[Example("Controls-Arcball", ExampleCategory.Misc, "Controls")]
public class ArcballControlsExample : Example
{
    private readonly float orthographicDistance = 120;
    private readonly float perspectiveDistance = 2.5f;
    private ArcballControls arcControls;

    private bool gizmoVisible = true;


    public override void InitRenderer()
    {
        base.InitRenderer();
        renderer.SetClearColor(Color.Hex(0x000000));
        renderer.OutputEncoding = Constants.sRGBEncoding;
        renderer.ToneMapping = Constants.ReinhardToneMapping;
        renderer.ToneMappingExposure = 3;
    }

    private Camera makeOrthographicCamera()
    {
        var halfFovV = MathUtils.DEG2RAD * 45 * 0.5f;
        var halfFovH = (float)Math.Atan(glControl.Width / glControl.Height * Math.Tan(halfFovV));

        var halfW = perspectiveDistance * (float)Math.Tan(halfFovH);
        var halfH = perspectiveDistance * (float)Math.Tan(halfFovV);
        var near = 0.01f;
        var far = 2000;
        var newCamera = new OrthographicCamera(-halfW, halfW, halfH, -halfH, near, far);

        return newCamera;
    }

    private Camera makePerspectiveCamera()
    {
        var fov = 45;
        var aspect = glControl.AspectRatio;
        var near = 0.01f;
        var far = 2000;
        var newCamera = new PerspectiveCamera(fov, aspect, near, far);
        return newCamera;
    }

    public override void InitLighting()
    {
        base.InitLighting();
        scene.Add(new HemisphereLight(Color.Hex(0x443333), Color.Hex(0x222233), 4));
    }

    public override void InitCamera()
    {
        camera = makePerspectiveCamera();
        camera.Position.Set(0, 0, perspectiveDistance);
    }

    public override void InitCameraController()
    {
        arcControls = new ArcballControls(this, camera, scene);
    }

    public override void Init()
    {
        base.Init();

        var material = new MeshStandardMaterial();

        material.Roughness = 1;
        material.Metalness = 1;

        var loader = new OBJLoader();
        var group = loader.Load(@"../../../../assets/models/obj/cerberus/Cerberus.obj");

        var diffuseMap = TextureLoader.Load(@"../../../../assets/models/obj/cerberus/Cerberus_A.jpg");
        diffuseMap.Encoding = Constants.sRGBEncoding;
        material.Map = diffuseMap;

        material.MetalnessMap = material.RoughnessMap =
            TextureLoader.Load(@"../../../../assets/models/obj/cerberus/Cerberus_RM.jpg");

        material.NormalMap = TextureLoader.Load(@"../../../../assets/models/obj/cerberus/Cerberus_N.jpg");

        material.Map.WrapS = Constants.RepeatWrapping;
        material.RoughnessMap.WrapS = Constants.RepeatWrapping;
        material.MetalnessMap.WrapS = Constants.RepeatWrapping;
        material.NormalMap.WrapS = Constants.RepeatWrapping;

        group.Traverse(child =>
        {
            if (child is Mesh) child.Material = material;
        });

        group.Rotation.Y = (float)Math.PI / 2;
        group.Position.X += 0.25f;
        scene.Add(group);

        var curve = new EllipseCurve(0, 0, 0.001f, 0.001f);
        var points = curve.GetPoints(128);
        var curveGeometry = new BufferGeometry().SetFromPoints(points);
        var curveMaterialX = new LineBasicMaterial
            { Color = Color.Hex(0xff8080), Fog = true, Transparent = true, Opacity = 0.6f };
        var gizmoX = new Line(curveGeometry, curveMaterialX);
        var rotation = (float)Math.PI * 0.5f;
        gizmoX.Rotation.X = rotation;
        scene.Add(gizmoX);

        AddGuiControlsAction = AddGuiControls;
    }

    public override void Render()
    {
        if (!imGuiManager.ImWantMouse)
            arcControls.Enabled = true;
        else
            arcControls.Enabled = false;

        renderer.Render(scene, camera);
    }

    public override void OnResize(ResizeEventArgs clientSize)
    {
        base.OnResize(clientSize);
        if (camera is OrthographicCamera)
        {
            var halfFovV = MathUtils.DEG2RAD * 45 * 0.5f;
            var halfFovH = (float)Math.Atan(glControl.Width / glControl.Height * (float)Math.Tan(halfFovV));

            var halfW = perspectiveDistance * (float)Math.Tan(halfFovH);
            var halfH = perspectiveDistance * (float)Math.Tan(halfFovV);
            camera.Left = -halfW;
            camera.CameraRight = halfW;
            camera.Top = halfH;
            camera.Bottom = -halfH;
        }
        else if (camera is PerspectiveCamera)
        {
            camera.Aspect = glControl.AspectRatio;
        }

        camera.UpdateProjectionMatrix();
        //arcControls.Control_SizeChanged(this, clientSize);
    }

    private void SetCamera(int type)
    {
        if (type == 0)
        {
            camera = makeOrthographicCamera();
            camera.Position.Set(0, 0, orthographicDistance);
        }
        else
        {
            camera = makePerspectiveCamera();
            camera.Position.Set(0, 0, perspectiveDistance);
        }

        arcControls.SetCamera(camera);
    }

    private void AddGuiControls()
    {
        ImGui.Checkbox("Enable controls", ref arcControls.Enabled);
        ImGui.Checkbox("Enable Grid", ref arcControls.EnableGrid);
        ImGui.Checkbox("Enable rotate", ref arcControls.EnableRotate);
        ImGui.Checkbox("Enable pan", ref arcControls.EnablePan);
        ImGui.Checkbox("Enable zoom", ref arcControls.EnableZoom);
        ImGui.Checkbox("Cursor zoom", ref arcControls.CursorZoom);
        ImGui.Checkbox("adjust near/far", ref arcControls.AdjustNearFar);
        ImGui.SliderFloat("Scale factor", ref arcControls.ScaleFactor, 1.1f, 10);
        ImGui.SliderFloat("Min Distance", ref arcControls.MinDistance, 0, 50);
        ImGui.SliderFloat("Max Distance", ref arcControls.MaxDistance, 0, 50);
        ImGui.SliderFloat("Min Zoom", ref arcControls.MinZoom, 0, 50);
        ImGui.SliderFloat("Max Zoom", ref arcControls.MaxZoom, 0, 50);
        if (ImGui.Checkbox("Show gizmos", ref gizmoVisible)) arcControls.SetGizmosVisible(gizmoVisible);
        if (ImGui.Button("Reset")) arcControls.Reset();
    }
}