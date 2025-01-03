namespace THREE;

[Serializable]
public class GLRenderState
{
    private GLLights lights;
    private List<Light> lightsArray = new();

    private List<Light> shadowsArray = new();

    public RenderState State;

    public GLRenderState(GLExtensions extensions, GLCapabilities capabilities)
    {
        lights = new GLLights(extensions, capabilities);
        State.LightsArray = lightsArray;
        State.ShadowsArray = shadowsArray;
        State.Lights = lights;
    }

    public void Init()
    {
        lightsArray.Clear();
        shadowsArray.Clear();
    }

    public void SetupLights()
    {
        lights.Setup(lightsArray);
    }

    public void SetupLightsView(Camera camera)
    {
        lights.SetupView(lightsArray, camera);
    }

    public void PushLight(Light light)
    {
        lightsArray.Add(light);
    }

    public void PushShadow(Light shadowLight)
    {
        shadowsArray.Add(shadowLight);
    }

    public struct RenderState
    {
        public List<Light> LightsArray;

        public List<Light> ShadowsArray;

        public GLLights Lights;
    }
}