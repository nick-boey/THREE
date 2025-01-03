namespace THREE;

[Serializable]
public class ShaderPass : Pass
{
    private ShaderMaterial material;
    private string textureId;
    public GLUniforms uniforms;

    public ShaderPass(Material shader, string textureId = null)
    {
        this.textureId = textureId != null ? textureId : "tDiffuse";
        if (shader != null && shader is ShaderMaterial)
        {
            uniforms = (shader as ShaderMaterial).Uniforms;
            if (textureId != null && !uniforms.ContainsKey(textureId))
                uniforms[textureId] = new GLUniform { { "value", null } };
            material = shader as ShaderMaterial;
        }

        fullScreenQuad = new FullScreenQuad(material);
    }

    public override void Render(GLRenderer renderer, GLRenderTarget writeBuffer, GLRenderTarget readBuffer,
        float? deltaTime = null, bool? maskActive = null)
    {
        if (uniforms.ContainsKey(textureId)) (uniforms[textureId] as GLUniform)["value"] = readBuffer.Texture;

        fullScreenQuad.material = material;
        if (RenderToScreen)
        {
            renderer.SetRenderTarget(null);
            fullScreenQuad.Render(renderer);
        }
        else
        {
            renderer.SetRenderTarget(writeBuffer);
            // TODO: Avoid using autoClear properties, see https://github.com/mrdoob/three.js/pull/15571#issuecomment-465669600
            if (Clear) renderer.Clear(renderer.AutoClearColor, renderer.AutoClearDepth, renderer.AutoClearStencil);
            fullScreenQuad.Render(renderer);
        }
    }

    public override void SetSize(float width, float height)
    {
    }
}