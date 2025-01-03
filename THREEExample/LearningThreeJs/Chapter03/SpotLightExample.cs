using System;
using ImGuiNET;
using THREE;
using THREEExample.Learning.Utils;
using Vector3 = System.Numerics.Vector3;

namespace THREEExample.Learning.Chapter03;

[Example("02.Spot-Light", ExampleCategory.LearnThreeJS, "Chapter03")]
public class SpotLightExample : Example
{
    private readonly float bouncingSpeed = 0.03f;

    private readonly float rotationSpeed = 0.03f;
    private AmbientLight ambientLight;
    private CameraHelper debugCamera;
    private float invert = 1;
    private float phase;
    private Mesh plane, sphereLightMesh, cube, sphere;
    private int selectedTargetIndex;
    private bool shadowDebug;
    private SpotLight spotLight, spotLight0;
    private SpotLightHelper spotLightHelper;
    private float step;
    private bool stopMovingLight;

    public override void InitRenderer()
    {
        base.InitRenderer();
        renderer.SetClearColor(Color.Hex(0x000000));
    }

    public override void InitLighting()
    {
        base.InitLighting();
        ambientLight = new AmbientLight(Color.Hex(0x1c1c1c));
        scene.Add(ambientLight);

        spotLight0 = new SpotLight(Color.Hex(0xcccccc));
        spotLight0.Position.Set(-40, 30, -10);

        scene.Add(spotLight0);

        spotLight = new SpotLight(Color.Hex(0xffffff));
        spotLight.Position.Set(-40, 60, -10);
        spotLight.CastShadow = true;
        spotLight.Shadow.Camera.Near = 1;
        spotLight.Shadow.Camera.Far = 100;

        spotLight.Distance = 0;
        spotLight.Angle = 0.4f;
        spotLight.Shadow.Camera.Fov = 120;

        scene.Add(spotLight);
    }

    public override void Init()
    {
        base.Init();
        cube = DemoUtils.AddDefaultCube(scene);
        sphere = DemoUtils.AddDefaultSphere(scene);
        plane = DemoUtils.AddGroundPlane(scene);

        spotLight0.LookAt(plane.Position);
        spotLight.Target = plane;
        spotLightHelper = new SpotLightHelper(spotLight);
        scene.Add(spotLightHelper);

        debugCamera = new CameraHelper(spotLight.Shadow.Camera);

        var sphereLight = new SphereGeometry(0.2f);
        var sphereLightMaterial = new MeshBasicMaterial { Color = Color.Hex(0xac6c25) };

        sphereLightMesh = new Mesh(sphereLight, sphereLightMaterial);
        sphereLightMesh.CastShadow = true;
        sphereLightMesh.Position.Set(3, 20, 3);
        scene.Add(sphereLightMesh);

        AddGuiControlsAction = () =>
        {
            if (ImGui.TreeNode("Light Colors"))
            {
                var acolor = new Vector3(ambientLight.Color.R, ambientLight.Color.G, ambientLight.Color.B);
                if (ImGui.ColorPicker3("ambientColor", ref acolor))
                    ambientLight.Color = new Color(acolor.X, acolor.Y, acolor.Z);
                var pcolor = new Vector3(spotLight.Color.R, spotLight.Color.G, spotLight.Color.B);
                if (ImGui.ColorPicker3("spotLightColor", ref pcolor))
                    spotLight.Color = new Color(pcolor.X, pcolor.Y, pcolor.Z);
                ImGui.TreePop();
            }

            ImGui.SliderFloat("angle", ref spotLight.Angle, 0, (float)Math.PI * 2);
            ImGui.SliderFloat("intensity", ref spotLight.Intensity, 0, 5);
            ImGui.SliderFloat("penumbra", ref spotLight.Penumbra, 0, 1);
            ImGui.SliderFloat("distance", ref spotLight.Distance, 0, 200);

            if (ImGui.Checkbox("shadowDebug", ref shadowDebug))
            {
                if (shadowDebug)
                    scene.Add(debugCamera);
                else
                    scene.Remove(debugCamera);
            }

            ImGui.Checkbox("castShadow", ref spotLight.CastShadow);
            if (ImGui.Combo("target", ref selectedTargetIndex, "Plane\0Sphere\0Cube\0"))
                switch (selectedTargetIndex)
                {
                    case 0: spotLight.Target = plane; break;
                    case 1: spotLight.Target = sphere; break;
                    case 2: spotLight.Target = cube; break;
                }

            ImGui.Checkbox("stopMovingLight", ref stopMovingLight);
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

        if (stopMovingLight == false)
        {
            if (phase > 2 * Math.PI)
            {
                invert = invert * -1;
                phase -= (float)(2 * Math.PI);
            }
            else
            {
                phase += rotationSpeed;
            }

            sphereLightMesh.Position.Z = +(float)(7 * Math.Sin(phase));
            sphereLightMesh.Position.X = +(float)(14 * Math.Cos(phase));
            sphereLightMesh.Position.Y = 15;

            if (invert < 0)
            {
                var pivot = 14;

                sphereLightMesh.Position.X = invert * (sphereLightMesh.Position.X - pivot) + pivot;
            }

            spotLight.Position.Copy(sphereLightMesh.Position);
        }

        spotLightHelper.Update();

        base.Render();
    }
}