using System.Collections;

namespace THREE;

[Serializable]
public class BloomPass : Pass
{
    public static Vector2 BlurX = new(0.001953125f, 0.0f);
    public static Vector2 BlurY = new(0.0f, 0.001953125f);
    private GLUniforms convolutionUniforms;
    private CopyShader copyShader;
    private int kernelSize;
    private ShaderMaterial materialConvolution;
    private ShaderMaterial materialCopy;

    private GLRenderTarget renderTargetX;
    private GLRenderTarget renderTargetY;
    private int resolution;
    private float sigma;
    private float strength;
    private GLUniforms uniforms;

    public BloomPass(float? strength = null, int? kernelSize = null, float? sigma = null, int? resolution = null)
    {
        this.strength = strength != null ? strength.Value : 1.0f;
        this.kernelSize = kernelSize != null ? kernelSize.Value : 25;
        this.sigma = sigma != null ? sigma.Value : 4.0f;
        this.resolution = resolution != null ? resolution.Value : 256;

        var pars = new Hashtable();
        pars.Add("minFilter", Constants.LinearFilter);
        pars.Add("magFilter", Constants.LinearFilter);
        pars.Add("format", Constants.RGBAFormat);

        renderTargetX = new GLRenderTarget(this.resolution, this.resolution, pars);
        renderTargetX.Texture.Name = "BloomPass.x";

        renderTargetY = new GLRenderTarget(this.resolution, this.resolution, pars);
        renderTargetY.Texture.Name = "BloomPass.y";


        copyShader = new CopyShader();

        uniforms = UniformsUtils.CloneUniforms(copyShader.Uniforms);

        (uniforms["opacity"] as GLUniform)["value"] = this.strength;

        materialCopy = new ShaderMaterial
        {
            Uniforms = uniforms,
            VertexShader = copyShader.VertexShader,
            FragmentShader = copyShader.FragmentShader,
            Blending = Constants.AdditiveBlending,
            Transparent = true
        };

        var convolutionShader = new ConvolutionShader();

        convolutionUniforms = UniformsUtils.CloneUniforms(convolutionShader.Uniforms);

        (convolutionUniforms["uImageIncrement"] as GLUniform)["value"] = BlurX;
        (convolutionUniforms["cKernel"] as GLUniform)["value"] = convolutionShader.BuildKernel(this.sigma);

        materialConvolution = new ShaderMaterial
        {
            Uniforms = convolutionUniforms,
            VertexShader = convolutionShader.VertexShader,
            FragmentShader = convolutionShader.FragmentShader
        };
        materialConvolution.Defines.Add("KERNEL_SIZE_FLOAT", this.kernelSize + ".0");
        materialConvolution.Defines.Add("KERNEL_SIZE_INT", this.kernelSize.ToString());

        NeedsSwap = false;

        fullScreenQuad = new FullScreenQuad();
    }

    public override void Render(GLRenderer renderer, GLRenderTarget writeBuffer, GLRenderTarget readBuffer,
        float? deltaTime = null, bool? maskActive = null)
    {
        if (maskActive != null && maskActive.Value) renderer.State.buffers.stencil.SetTest(false);

        // Render quad with blured scene into texture (convolution pass 1)

        fullScreenQuad.material = materialConvolution;

        (convolutionUniforms["tDiffuse"] as GLUniform)["value"] = readBuffer.Texture;
        (convolutionUniforms["uImageIncrement"] as GLUniform)["value"] = BlurX;

        renderer.SetRenderTarget(renderTargetX);
        renderer.Clear();
        fullScreenQuad.Render(renderer);


        // Render quad with blured scene into texture (convolution pass 2)

        (convolutionUniforms["tDiffuse"] as GLUniform)["value"] = renderTargetX.Texture;
        (convolutionUniforms["uImageIncrement"] as GLUniform)["value"] = BlurY;

        renderer.SetRenderTarget(renderTargetY);
        renderer.Clear();
        fullScreenQuad.Render(renderer);

        // Render original scene with superimposed blur to texture

        fullScreenQuad.material = materialCopy;

        (uniforms["tDiffuse"] as GLUniform)["value"] = renderTargetY.Texture;

        if (maskActive != null && maskActive.Value) renderer.State.buffers.stencil.SetTest(true);

        renderer.SetRenderTarget(readBuffer);
        if (Clear) renderer.Clear();
        fullScreenQuad.Render(renderer);
    }

    public override void SetSize(float width, float height)
    {
    }
}