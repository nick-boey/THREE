namespace THREE;

[Serializable]
public class DotScreenPass : Pass
{
    private ShaderMaterial material;
    public GLUniforms uniforms;

    public DotScreenPass(Vector2 center = null, float? angle = null, float? scale = null)
    {
        var shader = new DotScreenShader();

        uniforms = UniformsUtils.CloneUniforms(shader.Uniforms);

        if (center != null) (uniforms["center"] as GLUniform)["value"] = center;
        if (angle != null) (uniforms["angle"] as GLUniform)["value"] = angle;
        if (scale != null) (uniforms["scale"] as GLUniform)["value"] = scale;

        material = new ShaderMaterial
        {
            Uniforms = uniforms,
            VertexShader = shader.VertexShader,
            FragmentShader = shader.FragmentShader
        };

        fullScreenQuad = new FullScreenQuad(material);
    }

    public override void Render(GLRenderer renderer, GLRenderTarget writeBuffer, GLRenderTarget readBuffer,
        float? deltaTime = null, bool? maskActive = null)
    {
        (uniforms["tDiffuse"] as GLUniform)["value"] = readBuffer.Texture;
        ((uniforms["tSize"] as GLUniform)["value"] as Vector2).Set(readBuffer.Width, readBuffer.Height);

        if (RenderToScreen)
        {
            renderer.SetRenderTarget(null);
            fullScreenQuad.Render(renderer);
        }
        else
        {
            renderer.SetRenderTarget(writeBuffer);
            if (Clear) renderer.Clear();
            fullScreenQuad.Render(renderer);
        }
    }

    public override void SetSize(float width, float height)
    {
    }
}