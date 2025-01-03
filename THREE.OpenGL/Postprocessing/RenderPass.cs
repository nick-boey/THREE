namespace THREE;

[Serializable]
public class RenderPass : Pass
{
    public Camera camera;

    public float ClearAlpha;

    public Color? ClearColor;

    public bool ClearDepth;

    public Material OverrideMaterial;
    public Scene scene;

    public RenderPass(Scene scene, Camera camera, Material overrideMaterial = null, Color? clearColor = null,
        float? clearAlpha = null)
    {
        this.scene = scene;
        this.camera = camera;

        OverrideMaterial = overrideMaterial;

        ClearColor = clearColor;
        if (clearAlpha == null)
            ClearAlpha = 1.0f;
        else
            ClearAlpha = clearAlpha.Value;

        Clear = true;
        ClearDepth = false;
        NeedsSwap = false;
    }

    public override void Render(GLRenderer renderer, GLRenderTarget writeBuffer, GLRenderTarget readBuffer,
        float? deltaTime = null, bool? maskActive = null)
    {
        var oldAutoClear = renderer.AutoClear;
        renderer.AutoClear = false;

        Color? oldClearColor = null;
        ;
        float oldClearAlpha = 1;
        Material oldOverrideMaterial;


        oldOverrideMaterial = scene.OverrideMaterial;

        scene.OverrideMaterial = OverrideMaterial;


        if (ClearColor != null)
        {
            oldClearColor = renderer.GetClearColor();
            oldClearAlpha = renderer.GetClearAlpha();

            renderer.SetClearColor(ClearColor.Value, ClearAlpha);
        }

        if (ClearDepth) renderer.ClearDepth();

        renderer.SetRenderTarget(RenderToScreen ? null : readBuffer);

        // TODO: Avoid using autoClear properties, see https://github.com/mrdoob/three.js/pull/15571#issuecomment-465669600
        if (Clear) renderer.Clear(renderer.AutoClearColor, renderer.AutoClearDepth, renderer.AutoClearStencil);
        renderer.Render(scene, camera);

        if (ClearColor != null) renderer.SetClearColor(oldClearColor.Value, oldClearAlpha);

        if (OverrideMaterial != null) scene.OverrideMaterial = oldOverrideMaterial;

        renderer.AutoClear = oldAutoClear;
    }

    public override void SetSize(float width, float height)
    {
    }
}