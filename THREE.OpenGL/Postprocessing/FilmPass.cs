namespace THREE;

[Serializable]
public class FilmPass : Pass
{
    public ShaderMaterial material;
    public GLUniforms uniforms;

    public FilmPass(float? noiseIntensity, float? scanlinesIntensity, float? scanlinesCount, bool? grayscale)
    {
        var shader = new FilmShader();

        uniforms = UniformsUtils.CloneUniforms(shader.Uniforms);

        material = new ShaderMaterial();
        material.Uniforms = uniforms;
        material.VertexShader = shader.VertexShader;
        material.FragmentShader = shader.FragmentShader;


        if (grayscale != null) (uniforms["grayscale"] as GLUniform)["value"] = grayscale.Value;
        if (noiseIntensity != null) (uniforms["nIntensity"] as GLUniform)["value"] = noiseIntensity.Value;
        if (scanlinesIntensity != null) (uniforms["sIntensity"] as GLUniform)["value"] = scanlinesIntensity.Value;
        if (scanlinesCount != null) (uniforms["sCount"] as GLUniform)["value"] = scanlinesCount.Value;

        fullScreenQuad = new FullScreenQuad(material);
    }

    public override void Render(GLRenderer renderer, GLRenderTarget writeBuffer, GLRenderTarget readBuffer,
        float? deltaTime = null, bool? maskActive = null)
    {
        (uniforms["tDiffuse"] as GLUniform)["value"] = readBuffer.Texture;
        var currentDeltaTime = (float)(uniforms["time"] as GLUniform)["value"] + deltaTime.Value;
        (uniforms["time"] as GLUniform)["value"] = currentDeltaTime;

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