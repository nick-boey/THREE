using System;
using ImGuiNET;
using THREE;
using THREEExample.Learning.Utils;
using Vector3 = System.Numerics.Vector3;

namespace THREEExample.Learning.Chapter03;

[Example("01.Ambient-Light", ExampleCategory.LearnThreeJS, "Chapter03")]
public class AmbientLightExample : Example
{
    private AmbientLight ambientLight;
    private SpotLight spotLight;

    public override void InitRenderer()
    {
        base.InitRenderer();
        renderer.SetClearColor(Color.Hex(0x000000));
    }

    public override void InitLighting()
    {
        base.InitLighting();
        ambientLight = new AmbientLight(Color.Hex(0x606008), 1);
        scene.Add(ambientLight);

        spotLight = new SpotLight(Color.Hex(0xffffff), 1, 180, (float)Math.PI / 4);
        spotLight.Shadow.MapSize.Set(2048, 2048);
        spotLight.Position.Set(-30, 40, -10);
        spotLight.CastShadow = true;
        scene.Add(spotLight);
    }

    public override void Init()
    {
        base.Init();
        DemoUtils.AddHouseAndTree(scene);

        AddGuiControlsAction = () =>
        {
            ImGui.SliderFloat("intensity", ref ambientLight.Intensity, 0, 3);
            var color = new Vector3(ambientLight.Color.R, ambientLight.Color.G, ambientLight.Color.B);
            if (ImGui.TreeNode("Light Colors"))
                if (ImGui.ColorPicker3("ambientColor", ref color))
                    ambientLight.Color = new Color(color.X, color.Y, color.Z);

            ImGui.Checkbox("disableSpotlight", ref spotLight.Visible);
        };
    }
}