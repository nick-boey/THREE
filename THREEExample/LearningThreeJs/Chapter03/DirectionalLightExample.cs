using System;
using ImGuiNET;
using THREE;
using Vector3 = System.Numerics.Vector3;

namespace THREEExample.Learning.Chapter03;

[Example("04.Directional-Light", ExampleCategory.LearnThreeJS, "Chapter03")]
public class DirectionalLightExample : Example
{
    private readonly float bouncingSpeed = 0.03f;
    private readonly float rotationSpeed = 0.03f;
    private Vector3 ambientColor;
    private AmbientLight ambientLight;
    private bool debug;
    private DirectionalLight directionalLight;
    private Vector3 pointColor;
    private CameraHelper shadowCamera;
    private Mesh sphereLightMesh, plane, cube, sphere;
    private float step;
    private int targetIndex;

    public override void InitRenderer()
    {
        base.InitRenderer();
        renderer.SetClearColor(Color.Hex(0x000000));
    }

    public override void Init()
    {
        base.Init();
        CreateObject();

        AddGuiControlsAction = () =>
        {
            ambientColor = new Vector3(ambientLight.Color.R, ambientLight.Color.G, ambientLight.Color.B);
            pointColor = new Vector3(directionalLight.Color.R, directionalLight.Color.G, directionalLight.Color.B);
            if (ImGui.TreeNode("Light Colors"))
            {
                if (ImGui.ColorPicker3("ambientColor", ref ambientColor))
                    ambientLight.Color = new Color(ambientColor.X, ambientColor.Y, ambientColor.Z);
                if (ImGui.ColorPicker3("pointColor", ref pointColor))
                    directionalLight.Color = new Color(pointColor.X, pointColor.Y, pointColor.Z);
                ImGui.TreePop();
            }

            ImGui.SliderFloat("intensity", ref directionalLight.Intensity, 0.0f, 5.0f);
            if (ImGui.Checkbox("debug", ref debug))
            {
                if (debug) scene.Add(shadowCamera);
                else scene.Remove(shadowCamera);
            }

            ImGui.Checkbox("castShadow", ref directionalLight.CastShadow);
            if (ImGui.Combo("target", ref targetIndex, "Plane\0Sphere\0Cube\0"))
                switch (targetIndex)
                {
                    case 0: directionalLight.Target = plane; break;
                    case 1: directionalLight.Target = sphere; break;
                    case 2: directionalLight.Target = cube; break;
                }
        };
    }

    private void CreateObject()
    {
        var planeGeometry = new PlaneBufferGeometry(600, 200, 20, 20);
        var planeMaterial = new MeshLambertMaterial { Color = Color.Hex(0xffffff) };
        plane = new Mesh(planeGeometry, planeMaterial);
        plane.ReceiveShadow = true;

        plane.Rotation.X = (float)(-0.5 * Math.PI);
        plane.Position.X = 15;
        plane.Position.Y = -5;
        plane.Position.Z = 0;

        scene.Add(plane);

        var cubeMaterial = new MeshLambertMaterial { Color = Color.Hex(0xff3333) };
        var cubeGeometry = new BoxGeometry(4, 4, 4);
        cube = new Mesh(cubeGeometry, cubeMaterial);
        cube.CastShadow = true;
        cube.Position.X = -4;
        cube.Position.Y = 3;
        cube.Position.Z = 0;

        scene.Add(cube);

        var sphereGeometry = new SphereGeometry(4, 20, 20);
        var sphereMaterial = new MeshLambertMaterial { Color = Color.Hex(0x7777ff) };

        sphere = new Mesh(sphereGeometry, sphereMaterial);

        sphere.Position.Set(20, 0, 2);
        sphere.CastShadow = true;

        scene.Add(sphere);

        ambientLight = new AmbientLight(Color.Hex(0x1c1c1c));
        scene.Add(ambientLight);

        directionalLight = new DirectionalLight(Color.Hex(0xff5808));
        directionalLight.Target = plane;
        directionalLight.Position.Set(-40, 60, -10);
        directionalLight.CastShadow = true;

        directionalLight.Shadow.Camera.Near = 2;
        directionalLight.Shadow.Camera.Far = 80;
        ((OrthographicCamera)directionalLight.Shadow.Camera).Left = -30;
        ((OrthographicCamera)directionalLight.Shadow.Camera).CameraRight = 30;
        ((OrthographicCamera)directionalLight.Shadow.Camera).Top = 30;
        ((OrthographicCamera)directionalLight.Shadow.Camera).Bottom = -30;

        directionalLight.Intensity = 0.5f;
        directionalLight.Shadow.MapSize.Set(1024, 1024);

        scene.Add(directionalLight);

        shadowCamera = new CameraHelper(directionalLight.Shadow.Camera);

        var sphereLight = new SphereGeometry(0.2f);
        var sphereLightMaterial = new MeshBasicMaterial { Color = Color.Hex(0xac6c25) };

        sphereLightMesh = new Mesh(sphereLight, sphereLightMaterial);

        sphereLightMesh.Position.Set(3, 20, 3);

        scene.Add(sphereLightMesh);
    }

    public override void Render()
    {
        cube.Rotation.X += rotationSpeed;
        cube.Rotation.Y += rotationSpeed;
        cube.Rotation.Z += rotationSpeed;

        step += bouncingSpeed;

        sphere.Position.X = (float)(20 + 10 * Math.Cos(step));
        sphere.Position.Y = (float)(2 + 10 * Math.Abs(Math.Sin(step)));


        sphereLightMesh.Position.X = 10 + (float)(26 * Math.Cos(step / 3));
        sphereLightMesh.Position.Y = (float)(27 * Math.Cos(step / 3));
        sphereLightMesh.Position.Z = -8;

        directionalLight.Position.Copy(sphereLightMesh.Position);
        shadowCamera.Update();

        base.Render();
    }
}