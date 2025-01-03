using OpenTK.Windowing.GraphicsLibraryFramework;
using static THREE.Constants;

namespace THREE;

[Serializable]
public class MapControls : OrbitControls
{
    public MapControls(IControlsContainer control, Camera camera) : base(control, camera)
    {
        ControlMouseButtons = new Dictionary<MouseButton, MOUSE>
        {
            { MouseButton.Left, MOUSE.PAN },
            { MouseButton.Middle, MOUSE.DOLLY },
            { MouseButton.Right, MOUSE.ROTATE }
        };

        ScreenSpacePanning = false;
    }
}