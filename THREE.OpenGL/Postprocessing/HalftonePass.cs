using System.Collections;

namespace THREE;

[Serializable]
public class HalftonePass : Pass
{
    public ShaderMaterial material;
    public GLUniforms uniforms;

    public HalftonePass(float? width = null, float? height = null, Hashtable parameter = null)
    {
        var halftoneShader = new HalftoneShader();
        uniforms = UniformsUtils.CloneUniforms(halftoneShader.Uniforms);
        material = new ShaderMaterial
        {
            Uniforms = uniforms,
            FragmentShader = halftoneShader.FragmentShader,
            VertexShader = halftoneShader.VertexShader
        };

        // set params

        (uniforms["width"] as GLUniform)["value"] = width;
        (uniforms["height"] as GLUniform)["value"] = height;

        if (parameter != null)
            foreach (DictionaryEntry key in parameter)
                if (key.Value != null && uniforms.ContainsKey((string)key.Key))
                    (uniforms[(string)key.Key] as GLUniform)["value"] = key.Value;

        fullScreenQuad = new FullScreenQuad(material);
    }

    public override void Render(GLRenderer renderer, GLRenderTarget writeBuffer, GLRenderTarget readBuffer,
        float? deltaTime = null, bool? maskActive = null)
    {
        (material.Uniforms["tDiffuse"] as GLUniform)["value"] = readBuffer.Texture;

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
        (uniforms["width"] as GLUniform)["value"] = width;
        (uniforms["height"] as GLUniform)["value"] = height;
    }
}