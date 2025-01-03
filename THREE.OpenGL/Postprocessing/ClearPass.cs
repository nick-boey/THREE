namespace THREE;

[Serializable]
public class ClearPass : Pass
{
    private float clearAlpha;
    private Color? clearColor;

    public ClearPass(Color? clearColor = null, float? clearAlpha = null)
    {
        NeedsSwap = false;

        this.clearColor = clearColor != null ? clearColor.Value : Color.Hex(0x000000);
        this.clearAlpha = clearAlpha != null ? clearAlpha.Value : 0.0f;
    }

    public override void Render(GLRenderer renderer, GLRenderTarget writeBuffer, GLRenderTarget readBuffer,
        float? deltaTime = null, bool? maskActive = null)
    {
        var oldClearColor = Color.Hex(0x000000);
        var oldClearAlpha = 0.0f;

        if (clearColor != null)
        {
            oldClearColor = renderer.GetClearColor();
            oldClearAlpha = renderer.GetClearAlpha();

            renderer.SetClearColor(clearColor.Value, clearAlpha);
        }

        renderer.SetRenderTarget(RenderToScreen ? null : readBuffer);
        renderer.Clear();

        if (clearColor != null) renderer.SetClearColor(oldClearColor, oldClearAlpha);
    }

    public override void SetSize(float width, float height)
    {
    }
}