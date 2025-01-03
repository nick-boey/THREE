using System;
using ImGuiNET;
using THREE;
using THREEExample.Learning.Utils;
using Vector3 = System.Numerics.Vector3;

namespace THREEExample.Chapter03;

[Example("03.Point-Light", ExampleCategory.LearnThreeJS, "Chapter03")]
public class PointLightExample : Example
{
    private readonly float rotationSpeed = 0.01f;
    private AmbientLight ambientLight;
    private float bouncingSpeed = 0.03f;
    private float distance;
    private PointLightHelper helper;
    private float intensity = 1;
    private float invert = 1;
    private float phase;
    private PointLight pointLight;
    private CameraHelper shadowHelper;
    private Mesh sphereLightMesh;

    private float step = 0;

    public override void InitRenderer()
    {
        base.InitRenderer();
        renderer.SetClearColor(Color.Hex(0x000000));
    }

    public override void InitLighting()
    {
        base.InitLighting();
        ambientLight = new AmbientLight(Color.Hex(0x0c0c0c));
        scene.Add(ambientLight);

        pointLight = new PointLight(Color.Hex(0xccffcc));
        pointLight.Decay = 0.1f;
        pointLight.CastShadow = true;
        scene.Add(pointLight);
        distance = pointLight.Distance;
    }

    public override void Init()
    {
        base.Init();

        DemoUtils.AddHouseAndTree(scene);

        helper = new PointLightHelper(pointLight);
        //scene.Add(helper);
        shadowHelper = new CameraHelper(pointLight.Shadow.Camera);
        //scene.Add(shadowHelper);
        var sphereLight = new SphereGeometry(0.2f);
        var sphereLightMaterial = new MeshBasicMaterial { Color = Color.Hex(0xac6c25) };

        sphereLightMesh = new Mesh(sphereLight, sphereLightMaterial);
        sphereLightMesh.Position.Set(3, 0, 5);
        scene.Add(sphereLightMesh);

        AddGuiControlsAction = () =>
        {
            if (ImGui.TreeNode("Light Colors"))
            {
                var acolor = new Vector3(ambientLight.Color.R, ambientLight.Color.G, ambientLight.Color.B);
                if (ImGui.ColorPicker3("ambientColor", ref acolor))
                    ambientLight.Color = new Color(acolor.X, acolor.Y, acolor.Z);
                var pcolor = new Vector3(pointLight.Color.R, pointLight.Color.G, pointLight.Color.B);
                if (ImGui.ColorPicker3("pointLightColor", ref pcolor))
                    pointLight.Color = new Color(pcolor.X, pcolor.Y, pcolor.Z);
                ImGui.TreePop();
            }

            ImGui.SliderFloat("intensity", ref pointLight.Intensity, 0, 3);
            ImGui.SliderFloat("distance", ref pointLight.Distance, 0, 100);
        };
    }

    public override void Render()
    {
        helper.Update();
        shadowHelper.Update();
        pointLight.Position.Copy(sphereLightMesh.Position);
        controls.Update();

        if (phase > 2 * Math.PI)
        {
            invert = invert * -1;
            phase -= (float)(2 * Math.PI);
        }
        else
        {
            phase += rotationSpeed;
        }

        sphereLightMesh.Position.Z = (float)(25 * Math.Sin(phase));
        sphereLightMesh.Position.X = (float)(14 * Math.Cos(phase));
        sphereLightMesh.Position.Y = 15;

        if (invert < 0)
        {
            var pivot = 14;

            sphereLightMesh.Position.X = invert * (sphereLightMesh.Position.X - pivot) + pivot;
        }

        base.Render();
    }
}