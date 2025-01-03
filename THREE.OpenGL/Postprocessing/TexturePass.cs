namespace THREE;

[Serializable]
public class TexturePass : Pass
{
    private Texture map;
    private ShaderMaterial material;
    private float opacity;
    private GLUniforms uniforms;

    public TexturePass(Texture map, float? opacity = null)
    {
        var shader = new CopyShader();

        this.map = map;
        this.opacity = opacity != null ? opacity.Value : 1.0f;
        uniforms = UniformsUtils.CloneUniforms(shader.Uniforms);

        material = new ShaderMaterial
        {
            Uniforms = uniforms,
            VertexShader = shader.VertexShader,
            FragmentShader = shader.FragmentShader,
            DepthTest = false,
            DepthWrite = false
        };

        NeedsSwap = false;

        fullScreenQuad = new FullScreenQuad();
    }

    public override void Render(GLRenderer renderer, GLRenderTarget writeBuffer, GLRenderTarget readBuffer,
        float? deltaTime = null, bool? maskActive = null)
    {
        var oldAutoClear = renderer.AutoClear;
        renderer.AutoClear = false;

        fullScreenQuad.material = material;

        (uniforms["opacity"] as GLUniform)["value"] = opacity;
        (uniforms["tDiffuse"] as GLUniform)["value"] = map;
        material.Transparent = opacity < 1.0;

        renderer.SetRenderTarget(RenderToScreen ? null : readBuffer);
        if (Clear) renderer.Clear();
        fullScreenQuad.Render(renderer);

        renderer.AutoClear = oldAutoClear;
    }

    public override void SetSize(float width, float height)
    {
    }
}