using System;
using ImGuiNET;
using THREE;
using Vector3 = System.Numerics.Vector3;

namespace THREEExample.Learning.Chapter03;

[Example("07.Lensflares-Light", ExampleCategory.LearnThreeJS, "Chapter03")]
public class LensflaresExample : Example
{
    private readonly float bouncingSpeed = 0.03f;
    private readonly float rotationSpeed = 0.03f;
    private AmbientLight ambientLight;
    private float distance;
    private HemisphereLight hemiLight;
    private float intensity = 1;
    private float invert = 1;
    private float phase = 0;
    private CameraHelper shadowCamera;
    private Mesh sphereLightMesh, plane, cube, sphere;
    private DirectionalLight spotLight;

    private float step;
    private Texture textureGrass;

    public override void InitRenderer()
    {
        base.InitRenderer();
        renderer.SetClearColor(Color.Hex(0x000000));
    }

    public override void InitCamera()
    {
        base.InitCamera();
        camera.Position.X = -20;
        camera.Position.Y = 10;
        camera.Position.Z = 45;
    }

    public override void InitLighting()
    {
        base.InitLighting();
        ambientLight = new AmbientLight(Color.Hex(0x1c1c1c));
        scene.Add(ambientLight);


        var spotLight0 = new SpotLight(Color.Hex(0xcccccc));
        spotLight0.Position.Set(-40, 60, -10);
        //spotLight0.LookAt(plane.Position);
        scene.Add(spotLight0);


        spotLight = new DirectionalLight(Color.Hex(0xffffff));

        spotLight.Position.Set(30, 10, -50);
        spotLight.CastShadow = true;
        spotLight.Shadow.Camera.Near = 2;
        spotLight.Shadow.Camera.Far = 200;
        spotLight.Shadow.Camera.Fov = 50;

        spotLight.Distance = 0;
        ((OrthographicCamera)spotLight.Shadow.Camera).Left = -100;
        ((OrthographicCamera)spotLight.Shadow.Camera).CameraRight = 100;
        ((OrthographicCamera)spotLight.Shadow.Camera).Top = 100;
        ((OrthographicCamera)spotLight.Shadow.Camera).Bottom = -100;
        spotLight.Shadow.MapSize.Set(2048, 2048);
        scene.Add(spotLight);
    }

    public override void Init()
    {
        base.Init();
        textureGrass = TextureLoader.Load("../../../../assets/textures/ground/grasslight-big.jpg");
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
        spotLight.Target = plane;
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

        var textureFlare0 = TextureLoader.Load("../../../../assets/textures/flares/lensflare0.png");
        var textureFlare3 = TextureLoader.Load("../../../../assets/textures/flares/lensflare3.png");

        var flareColor = Color.Hex(0xffaacc);

        var lensFlare = new Lensflare();

        lensFlare.AddElement(new LensflareElement(textureFlare0, 350, 0.0f, flareColor));
        lensFlare.AddElement(new LensflareElement(textureFlare3, 60, 0.6f, flareColor));
        lensFlare.AddElement(new LensflareElement(textureFlare3, 70, 0.7f, flareColor));
        lensFlare.AddElement(new LensflareElement(textureFlare3, 120, 0.9f, flareColor));
        lensFlare.AddElement(new LensflareElement(textureFlare3, 70, 1.0f, flareColor));

        spotLight.Add(lensFlare);

        AddGuiControlsAction = () =>
        {
            var ambientColor = new Vector3(ambientLight.Color.R, ambientLight.Color.G, ambientLight.Color.B);
            var pointColor = new Vector3(spotLight.Color.R, spotLight.Color.G, spotLight.Color.B);
            if (ImGui.TreeNode("Light Colors"))
            {
                if (ImGui.ColorPicker3("ambientColor", ref ambientColor))
                    ambientLight.Color = new Color(ambientColor.X, ambientColor.Y, ambientColor.Z);
                if (ImGui.ColorPicker3("pointColor", ref pointColor))
                    spotLight.Color = new Color(pointColor.X, pointColor.Y, pointColor.Z);
                ImGui.TreePop();
            }

            ImGui.SliderFloat("intensity", ref spotLight.Intensity, 0, 5);
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