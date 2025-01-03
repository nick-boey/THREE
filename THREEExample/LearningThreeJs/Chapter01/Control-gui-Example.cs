using ImGuiNET;

namespace THREEExample.Chapter01;

[Example("05-Control-GUI", ExampleCategory.LearnThreeJS, "Chapter01")]
public class ControlGUIExample : MaterialsLightAnimationExample
{
    public override void Init()
    {
        base.Init();

        AddGuiControlsAction = () =>
        {
            ImGui.SliderFloat("RotationSpeed", ref rotationSpeed, 0.0f, 0.5f);
            ImGui.SliderFloat("BouncingSpeed", ref bouncingSpeed, 0.0f, 0.5f);
        };
    }
}