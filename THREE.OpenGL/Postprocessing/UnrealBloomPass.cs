using System.Collections;

namespace THREE;

[Serializable]
public class UnrealBloomPass : Pass, IDisposable
{
    public static Vector2 BlurDirectionX = new(1.0f, 0.0f);
    public static Vector2 BlurDirectionY = new(0.0f, 1.0f);
    private MeshBasicMaterial basic;
    private List<Vector3> BloomTintColors;
    public Color ClearColor;
    private ShaderMaterial compositeMaterial;
    private GLUniforms copyUniforms;
    private bool disposed;
    private GLUniforms highPassUniforms;
    private ShaderMaterial materialCopy;
    private ShaderMaterial materialHighPassFilter;
    public int nMips;
    private float oldClearAlpha;
    private Color oldClearColor;
    public float Radius;
    public GLRenderTarget renderTargetBright;
    public List<GLRenderTarget> RenderTargetsHorizontal = new();
    public List<GLRenderTarget> RenderTargetsVertical = new();

    public Vector2 Resolution;
    private List<ShaderMaterial> separableBlurMaterials = new();
    public float Strength;
    public float Threshold;

    public UnrealBloomPass(Vector2 resolution = null, float? strength = null, float? radius = null,
        float? threshold = null)
    {
        Strength = strength != null ? strength.Value : 1;
        Radius = radius != null ? radius.Value : 0;
        Threshold = threshold != null ? threshold.Value : 0;

        Resolution = resolution != null ? new Vector2(resolution.X, resolution.Y) : new Vector2(256, 256);
        ClearColor = new Color(0, 0, 0);

        // render targets
        var pars = new Hashtable
        {
            { "minFilter", Constants.LinearFilter }, { "magFilter", Constants.LinearFilter },
            { "format", Constants.RGBAFormat }
        };

        nMips = 5;

        var resx = (int)Math.Round(Resolution.X / 2);
        var resy = (int)Math.Round(Resolution.Y / 2);

        renderTargetBright = new GLRenderTarget(resx, resy, pars);
        renderTargetBright.Texture.Name = "UnrealBloomPass.bright";
        renderTargetBright.Texture.GenerateMipmaps = false;

        for (var i = 0; i < nMips; i++)
        {
            var renderTargetHorizonal = new GLRenderTarget(resx, resy, pars);

            renderTargetHorizonal.Texture.Name = "UnrealBloomPass.h" + i;
            renderTargetHorizonal.Texture.GenerateMipmaps = false;

            RenderTargetsHorizontal.Add(renderTargetHorizonal);

            var renderTargetVertical = new GLRenderTarget(resx, resy, pars);

            renderTargetVertical.Texture.Name = "UnrealBloomPass.v" + i;
            renderTargetVertical.Texture.GenerateMipmaps = false;

            RenderTargetsVertical.Add(renderTargetVertical);

            resx = (int)Math.Round(resx / 2.0f);

            resy = (int)Math.Round(resy / 2.0f);
        }

        // luminosity high pass material


        var highPassShader = new LuminosityHighPassShader();
        highPassUniforms = UniformsUtils.CloneUniforms(highPassShader.Uniforms);

        (highPassUniforms["luminosityThreshold"] as GLUniform)["value"] = threshold;
        (highPassUniforms["smoothWidth"] as GLUniform)["value"] = 0.01f;

        materialHighPassFilter = new ShaderMaterial
        {
            Uniforms = highPassUniforms,
            VertexShader = highPassShader.VertexShader,
            FragmentShader = highPassShader.FragmentShader
        };

        // Gaussian Blur Materials
        var kernelSizeArray = new List<int> { 3, 5, 7, 9, 11 };
        resx = (int)Math.Round(Resolution.X / 2);
        resy = (int)Math.Round(Resolution.Y / 2);
        for (var i = 0; i < nMips; i++)
        {
            separableBlurMaterials.Add(GetSeperableBlurMaterial(kernelSizeArray[i]));

            (separableBlurMaterials[i].Uniforms["texSize"] as GLUniform)["value"] = new Vector2(resx, resy);

            resx = (int)Math.Round(resx / 2.0f);

            resy = (int)Math.Round(resy / 2.0f);
        }

        // Composite material
        compositeMaterial = GetCompositeMaterial(nMips);
        (compositeMaterial.Uniforms["blurTexture1"] as GLUniform)["value"] = RenderTargetsVertical[0].Texture;
        (compositeMaterial.Uniforms["blurTexture2"] as GLUniform)["value"] = RenderTargetsVertical[1].Texture;
        (compositeMaterial.Uniforms["blurTexture3"] as GLUniform)["value"] = RenderTargetsVertical[2].Texture;
        (compositeMaterial.Uniforms["blurTexture4"] as GLUniform)["value"] = RenderTargetsVertical[3].Texture;
        (compositeMaterial.Uniforms["blurTexture5"] as GLUniform)["value"] = RenderTargetsVertical[4].Texture;
        (compositeMaterial.Uniforms["bloomStrength"] as GLUniform)["value"] = strength;
        (compositeMaterial.Uniforms["bloomRadius"] as GLUniform)["value"] = 0.1f;
        compositeMaterial.NeedsUpdate = true;

        var bloomFactors = new List<float> { 1.0f, 0.8f, 0.6f, 0.4f, 0.2f };
        (compositeMaterial.Uniforms["bloomFactors"] as GLUniform)["value"] = bloomFactors;
        BloomTintColors = new List<Vector3>
        {
            new(1, 1, 1), new(1, 1, 1), new(1, 1, 1),
            new(1, 1, 1), new(1, 1, 1)
        };
        (compositeMaterial.Uniforms["bloomTintColors"] as GLUniform)["value"] = BloomTintColors;


        var copyShader = new CopyShader();

        copyUniforms = UniformsUtils.CloneUniforms(copyShader.Uniforms);
        (copyUniforms["opacity"] as GLUniform)["value"] = 1.0f;

        materialCopy = new ShaderMaterial
        {
            Uniforms = copyUniforms,
            VertexShader = copyShader.VertexShader,
            FragmentShader = copyShader.FragmentShader,
            Blending = Constants.AdditiveBlending,
            DepthTest = false,
            DepthWrite = false,
            Transparent = true
        };

        Enabled = true;
        NeedsSwap = false;

        oldClearColor = new Color();
        oldClearAlpha = 1;

        basic = new MeshBasicMaterial();

        fullScreenQuad = new FullScreenQuad();
    }

    public virtual void Dispose()
    {
        Dispose(disposed);
    }

    public event EventHandler<EventArgs> Disposed;

    private ShaderMaterial GetCompositeMaterial(int nMips)
    {
        return new ShaderMaterial
        {
            Defines = new Hashtable
            {
                { "NUM_MIPS", nMips.ToString() }
            },

            Uniforms = new GLUniforms
            {
                { "blurTexture1", new GLUniform { { "value", null } } },
                { "blurTexture2", new GLUniform { { "value", null } } },
                { "blurTexture3", new GLUniform { { "value", null } } },
                { "blurTexture4", new GLUniform { { "value", null } } },
                { "blurTexture5", new GLUniform { { "value", null } } },
                { "dirtTexture", new GLUniform { { "value", null } } },
                { "bloomStrength", new GLUniform { { "value", 1.0f } } },
                { "bloomFactors", new GLUniform { { "value", null } } },
                { "bloomTintColors", new GLUniform { { "value", null } } },
                { "bloomRadius", new GLUniform { { "value", 0.0f } } }
            },


            VertexShader = @"
			varying vec2 vUv;
			void main()
			{
					vUv = uv;
					gl_Position = projectionMatrix * modelViewMatrix * vec4(position, 1.0);
				}
			",

            FragmentShader = @"
			varying vec2 vUv;
			uniform sampler2D blurTexture1;
			uniform sampler2D blurTexture2;
			uniform sampler2D blurTexture3;
			uniform sampler2D blurTexture4;
			uniform sampler2D blurTexture5;
			uniform sampler2D dirtTexture;
			uniform float bloomStrength;
			uniform float bloomRadius;
			uniform float bloomFactors[NUM_MIPS];
			uniform vec3 bloomTintColors[NUM_MIPS];			
			float lerpBloomFactor(const in float factor) { 
				float mirrorFactor = 1.2 - factor;
				return mix(factor, mirrorFactor, bloomRadius);
			}
			
			void main()
			{
					gl_FragColor = bloomStrength * (lerpBloomFactor(bloomFactors[0]) * vec4(bloomTintColors[0], 1.0) * texture2D(blurTexture1, vUv) + 
													 lerpBloomFactor(bloomFactors[1]) * vec4(bloomTintColors[1], 1.0) * texture2D(blurTexture2, vUv) + 
													 lerpBloomFactor(bloomFactors[2]) * vec4(bloomTintColors[2], 1.0) * texture2D(blurTexture3, vUv) + 
													 lerpBloomFactor(bloomFactors[3]) * vec4(bloomTintColors[3], 1.0) * texture2D(blurTexture4, vUv) + 
													 lerpBloomFactor(bloomFactors[4]) * vec4(bloomTintColors[4], 1.0) * texture2D(blurTexture5, vUv) );
				}"
        };
    }

    private ShaderMaterial GetSeperableBlurMaterial(int kernelRadius)
    {
        return new ShaderMaterial
        {
            Defines = new Hashtable
            {
                { "KERNEL_RADIUS", kernelRadius.ToString() },
                { "SIGMA", kernelRadius.ToString() }
            },

            Uniforms = new GLUniforms
            {
                { "colorTexture", new GLUniform { { "value", null } } },
                { "texSize", new GLUniform { { "value", new Vector2(0.5f, 0.5f) } } },
                { "direction", new GLUniform { { "value", new Vector2(0.5f, 0.5f) } } }
            },

            VertexShader = @"
			
			varying vec2 vUv;
			void main()
			{
					vUv = uv;
					gl_Position = projectionMatrix * modelViewMatrix * vec4(position, 1.0);
				}
			",

            FragmentShader = @"
			#include <common>
			varying vec2 vUv;
			uniform sampler2D colorTexture;
			uniform vec2 texSize;
			uniform vec2 direction;
			
			float gaussianPdf(in float x, in float sigma)
			{
					return 0.39894 * exp(-0.5 * x * x / (sigma * sigma)) / sigma;
			}
			void main()
			{
				vec2 invSize = 1.0 / texSize;
				float fSigma = float(SIGMA);
				float weightSum = gaussianPdf(0.0, fSigma);
				vec3 diffuseSum = texture2D(colorTexture, vUv).rgb * weightSum;
				for (int i = 1; i < KERNEL_RADIUS; i++)
				{
					float x = float(i);
					float w = gaussianPdf(x, fSigma);
					vec2 uvOffset = direction * invSize * x;
					vec3 sample1 = texture2D(colorTexture, vUv + uvOffset).rgb;
					vec3 sample2 = texture2D(colorTexture, vUv - uvOffset).rgb;
					diffuseSum += (sample1 + sample2) * w;
					weightSum += 2.0 * w;
				}
					gl_FragColor = vec4(diffuseSum / weightSum, 1.0);
			}
			"
        };
    }

    public override void Render(GLRenderer renderer, GLRenderTarget writeBuffer, GLRenderTarget readBuffer,
        float? deltaTime = null, bool? maskActive = null)
    {
        oldClearColor = renderer.GetClearColor();
        oldClearAlpha = renderer.GetClearAlpha();
        var oldAutoClear = renderer.AutoClear;
        renderer.AutoClear = false;

        renderer.SetClearColor(ClearColor, 0);

        if (maskActive != null && maskActive.Value) renderer.State.buffers.stencil.SetTest(false);

        // Render input to screen

        if (RenderToScreen)
        {
            fullScreenQuad.material = basic;
            basic.Map = readBuffer.Texture;

            renderer.SetRenderTarget(null);
            renderer.Clear();
            fullScreenQuad.Render(renderer);
        }

        // 1. Extract Bright Areas

        (highPassUniforms["tDiffuse"] as GLUniform)["value"] = readBuffer.Texture;
        (highPassUniforms["luminosityThreshold"] as GLUniform)["value"] = Threshold;
        fullScreenQuad.material = materialHighPassFilter;

        renderer.SetRenderTarget(renderTargetBright);
        renderer.Clear();
        fullScreenQuad.Render(renderer);

        // 2. Blur All the mips progressively

        var inputRenderTarget = renderTargetBright;

        for (var i = 0; i < nMips; i++)
        {
            fullScreenQuad.material = separableBlurMaterials[i];

            (separableBlurMaterials[i].Uniforms["colorTexture"] as GLUniform)["value"] = inputRenderTarget.Texture;
            (separableBlurMaterials[i].Uniforms["direction"] as GLUniform)["value"] = BlurDirectionX;
            renderer.SetRenderTarget(RenderTargetsHorizontal[i]);
            renderer.Clear();
            fullScreenQuad.Render(renderer);

            (separableBlurMaterials[i].Uniforms["colorTexture"] as GLUniform)["value"] =
                RenderTargetsHorizontal[i].Texture;
            (separableBlurMaterials[i].Uniforms["direction"] as GLUniform)["value"] = BlurDirectionY;
            renderer.SetRenderTarget(RenderTargetsVertical[i]);
            renderer.Clear();
            fullScreenQuad.Render(renderer);

            inputRenderTarget = RenderTargetsVertical[i];
        }

        // Composite All the mips

        fullScreenQuad.material = compositeMaterial;
        (compositeMaterial.Uniforms["bloomStrength"] as GLUniform)["value"] = Strength;
        (compositeMaterial.Uniforms["bloomRadius"] as GLUniform)["value"] = Radius;
        (compositeMaterial.Uniforms["bloomTintColors"] as GLUniform)["value"] = BloomTintColors;

        renderer.SetRenderTarget(RenderTargetsHorizontal[0]);
        renderer.Clear();
        fullScreenQuad.Render(renderer);

        // Blend it additively over the input texture

        fullScreenQuad.material = materialCopy;

        (copyUniforms["tDiffuse"] as GLUniform)["value"] = RenderTargetsHorizontal[0].Texture;
        if (maskActive != null && maskActive.Value) renderer.State.buffers.stencil.SetTest(true);

        if (RenderToScreen)
        {
            renderer.SetRenderTarget(null);
            fullScreenQuad.Render(renderer);
        }
        else
        {
            renderer.SetRenderTarget(readBuffer);
            fullScreenQuad.Render(renderer);
        }

        // Restore renderer settings

        renderer.SetClearColor(oldClearColor, oldClearAlpha);
        renderer.AutoClear = oldAutoClear;
    }

    public override void SetSize(float width, float height)
    {
        var resx = (int)Math.Round(width / 2);
        var resy = (int)Math.Round(height / 2);

        renderTargetBright.SetSize(resx, resy);

        for (var i = 0; i < nMips; i++)
        {
            RenderTargetsHorizontal[i].SetSize(resx, resy);
            RenderTargetsVertical[i].SetSize(resx, resy);

            (separableBlurMaterials[i].Uniforms["texSize"] as GLUniform)["value"] = new Vector2(resx, resy);

            resx = (int)Math.Round(resx / 2.0f);
            resy = (int)Math.Round(resy / 2.0f);
        }
    }

    protected virtual void RaiseDisposed()
    {
        var handler = Disposed;
        if (handler != null)
            handler(this, new EventArgs());
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed) return;
        for (var i = 0; i < RenderTargetsHorizontal.Count; i++) RenderTargetsHorizontal[i].Dispose();
        for (var i = 0; i < RenderTargetsVertical.Count; i++) RenderTargetsVertical[i].Dispose();

        renderTargetBright.Dispose();

        RaiseDisposed();
        disposed = true;
        disposed = true;
    }
}