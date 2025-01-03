using System;
using ImGuiNET;
using THREE;
using Vector3 = System.Numerics.Vector3;

namespace THREEExample.Learning.Chapter03;

[Example("05.Hemisphere-Light", ExampleCategory.LearnThreeJS, "Chapter03")]
public class HemisphereLightExample : Example
{
    private readonly float bouncingSpeed = 0.03f;
    private readonly float intensity = 0.6f;
    private readonly float rotationSpeed = 0.01f;
    private DirectionalLight directionalLight;
    private HemisphereLight hemiLight;
    private bool hemisphere = true;
    private Mesh plane, cube, sphere;

    private float step;
    private Texture textureGrass;

    public override void InitRenderer()
    {
        base.InitRenderer();
        renderer.SetClearColor(Color.Hex(0x000000));
    }

    public override void InitLighting()
    {
        base.InitLighting();
        var spotLight0 = new SpotLight(Color.Hex(0xcccccc));
        spotLight0.Position.Set(-40, 60, -10);
        //spotLight0.LookAt(plane.Position);
        scene.Add(spotLight0);


        hemiLight = new HemisphereLight(Color.Hex(0x0000ff), Color.Hex(0x00ff00), 0.6f);
        hemiLight.Position.Set(0, 500, 0);
        scene.Add(hemiLight);


        directionalLight = new DirectionalLight(Color.Hex(0xffffff));

        directionalLight.Position.Set(30, 10, -50);
        directionalLight.CastShadow = true;

        directionalLight.Shadow.Camera.Near = 0.1f;
        directionalLight.Shadow.Camera.Far = 200;
        ((OrthographicCamera)directionalLight.Shadow.Camera).Left = -50;
        ((OrthographicCamera)directionalLight.Shadow.Camera).CameraRight = 50;
        ((OrthographicCamera)directionalLight.Shadow.Camera).Top = 50;
        ((OrthographicCamera)directionalLight.Shadow.Camera).Bottom = -50;
        directionalLight.Shadow.MapSize.Set(2048, 2048);

        scene.Add(directionalLight);
    }

    public override void Init()
    {
        base.Init();
        textureGrass = TextureLoader.Load("../../../..//assets/textures/ground/grasslight-big.jpg");
        textureGrass.WrapS = Constants.RepeatWrapping;
        textureGrass.WrapT = Constants.RepeatWrapping;
        textureGrass.Repeat.Set(10, 10);

        var planeGeometry = new PlaneGeometry(1000, 1000, 20, 20);
        var planeMaterial = new MeshLambertMaterial { Map = textureGrass };
        plane = new Mesh(planeGeometry, planeMaterial);
        plane.ReceiveShadow = true;

        plane.Rotation.X = (float)(-0.5 * Math.PI);
        plane.Position.X = 15;
        plane.Position.Y = 0;
        plane.Position.Z = 0;
        directionalLight.Target = plane;

        scene.Add(plane);

        var cubeGeometry = new BoxGeometry(4, 4, 4);
        var cubeMaterial = new MeshLambertMaterial { Color = Color.Hex(0xff3333) };

        cube = new Mesh(cubeGeometry, cubeMaterial);
        cube.CastShadow = true;
        cube.Position.Set(-4, 3, 0);

        scene.Add(cube);

        var sphereGeometry = new SphereGeometry(4, 25, 25);
        var sphereMaterial = new MeshPhongMaterial { Color = Color.Hex(0x7777ff) };

        sphere = new Mesh(sphereGeometry, sphereMaterial);

        sphere.Position.Set(10, 5, 10);
        sphere.CastShadow = true;

        scene.Add(sphere);

        AddGuiControlsAction = () =>
        {
            if (ImGui.Checkbox("hemisphere", ref hemisphere))
            {
                if (!hemisphere)
                    hemiLight.Intensity = 0.0f;
                else
                    hemiLight.Intensity = intensity;
            }

            var groundColor = new Vector3(hemiLight.GroundColor.R, hemiLight.GroundColor.G, hemiLight.GroundColor.B);
            var color = new Vector3(hemiLight.Color.R, hemiLight.Color.G, hemiLight.Color.B);
            if (ImGui.TreeNode("Light Colors"))
            {
                if (ImGui.ColorPicker3("groundColor", ref groundColor))
                    hemiLight.GroundColor = new Color(groundColor.X, groundColor.Y, groundColor.Z);
                if (ImGui.ColorPicker3("color", ref color)) hemiLight.Color = new Color(color.X, color.Y, color.Z);
                ImGui.TreePop();
            }

            ImGui.SliderFloat("intensity", ref hemiLight.Intensity, 0.0f, 5.0f);
        };
    }


    public override void Render()
    {
        cube.Rotation.X += rotationSpeed;
        cube.Rotation.Y += rotationSpeed;
        cube.Rotation.Z += rotationSpeed;

        step += bouncingSpeed;

        sphere.Position.X = (float)(20 + 10 * Math.Cos(step));
        sphere.Position.Y = (float)(2 + 10 * Math.Abs(Math.Sin(step)));

        base.Render();
    }
}