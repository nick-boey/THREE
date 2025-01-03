using System.Collections;

namespace THREE;

[Serializable]
public class SSAOPass : Pass, IDisposable
{
    public enum OUTPUT
    {
        Default = 0,
        SSAO = 1,
        Blur = 2,
        Beauty = 3,
        Depth = 4,
        Normal = 5
    }

    private Dictionary<Object3D, bool> _visibilityCache = new();
    private GLRenderTarget beautyRenderTarget;
    private ShaderMaterial blurMaterial;
    private GLRenderTarget blurRenderTarget;
    private Camera camera;
    private ShaderMaterial copyMaterial;
    private ShaderMaterial depthRenderMaterial;
    private bool disposed;
    private int height;
    private List<Vector3> kernel;
    private float kernelRadius;
    private int kernelSize;
    private float maxDistance;
    private float minDistance;
    private DataTexture noiseTexture;
    private MeshNormalMaterial normalMaterial;
    private GLRenderTarget normalRenderTarget;
    private Color originalClearColor;
    private int output;
    private Scene scene;
    private ShaderMaterial ssaoMaterial;
    private GLRenderTarget ssaoRenderTarget;
    private int width;

    public SSAOPass(Scene scene, Camera camera, int? width = null, int? height = null)
    {
        this.width = width != null ? width.Value : 512;
        this.height = height != null ? height.Value : 512;

        Clear = true;

        this.camera = camera;
        this.scene = scene;

        kernelRadius = 8;
        kernelSize = 32;
        kernel = new List<Vector3>();
        noiseTexture = null;
        output = 0;

        minDistance = 0.005f;
        maxDistance = 0.1f;

        GenerateSampleKernel();
        GenerateRandomKernelRotations();

        // beauty render target with depth buffer

        var depthTexture = new DepthTexture(0, 0, 0);
        depthTexture.Type = Constants.UnsignedShortType;
        depthTexture.MinFilter = Constants.NearestFilter;
        depthTexture.MaxFilter = Constants.NearestFilter;

        beautyRenderTarget = new GLRenderTarget(this.width, this.height, new Hashtable
        {
            { "minFilter", Constants.LinearFilter },
            { "magFilter", Constants.LinearFilter },
            { "format", Constants.RGBAFormat },
            { "depthTexture", depthTexture },
            { "depthBuffer", true }
        });

        // normal render target

        normalRenderTarget = new GLRenderTarget(this.width, this.height, new Hashtable
        {
            { "minFilter", Constants.NearestFilter },
            { "magFilter", Constants.NearestFilter },
            { "format", Constants.RGBAFormat }
        });

        // ssao render target

        ssaoRenderTarget = new GLRenderTarget(this.width, this.height, new Hashtable
        {
            { "minFilter", Constants.LinearFilter },
            { "magFilter", Constants.LinearFilter },
            { "format", Constants.RGBAFormat }
        });

        blurRenderTarget = (GLRenderTarget)ssaoRenderTarget.Clone();

        var ssaoShader = new SSAOShader();
        ssaoMaterial = new ShaderMaterial(new Hashtable
        {
            { "defines", ssaoShader.Defines },
            { "uniforms", UniformsUtils.CloneUniforms(ssaoShader.Uniforms) },
            { "vertexShader", ssaoShader.VertexShader },
            { "fragmentShader", ssaoShader.FragmentShader },
            { "blending", Constants.NoBlending }
        });

        (ssaoMaterial.Uniforms["tDiffuse"] as GLUniform)["value"] = beautyRenderTarget.Texture;
        (ssaoMaterial.Uniforms["tNormal"] as GLUniform)["value"] = normalRenderTarget.Texture;
        (ssaoMaterial.Uniforms["tDepth"] as GLUniform)["value"] = beautyRenderTarget.depthTexture;
        (ssaoMaterial.Uniforms["tNoise"] as GLUniform)["value"] = noiseTexture;
        (ssaoMaterial.Uniforms["kernel"] as GLUniform)["value"] = kernel;
        (ssaoMaterial.Uniforms["cameraNear"] as GLUniform)["value"] = this.camera.Near;
        (ssaoMaterial.Uniforms["cameraFar"] as GLUniform)["value"] = this.camera.Far;
        ((ssaoMaterial.Uniforms["resolution"] as GLUniform)["value"] as Vector2).Set(this.width, this.height);
        ((ssaoMaterial.Uniforms["cameraProjectionMatrix"] as GLUniform)["value"] as Matrix4).Copy(this.camera
            .ProjectionMatrix);
        ((ssaoMaterial.Uniforms["cameraInverseProjectionMatrix"] as GLUniform)["value"] as Matrix4).GetInverse(
            this.camera.ProjectionMatrixInverse);

        // normal material

        normalMaterial = new MeshNormalMaterial();
        normalMaterial.Blending = Constants.NoBlending;

        // blur material
        var ssaoBlurShader = new SSAOBlurShader();
        blurMaterial = new ShaderMaterial(new Hashtable
        {
            { "defines", ssaoBlurShader.Defines },
            { "uniforms", UniformsUtils.CloneUniforms(ssaoBlurShader.Uniforms) },
            { "vertexShader", ssaoBlurShader.VertexShader },
            { "fragmentShader", ssaoBlurShader.FragmentShader }
        });

        (blurMaterial.Uniforms["tDiffuse"] as GLUniform)["value"] = ssaoRenderTarget.Texture;
        ((blurMaterial.Uniforms["resolution"] as GLUniform)["value"] as Vector2).Set(this.width, this.height);

        // material for rendering the depth
        var ssaoDepthShader = new SSAODepthShader();
        depthRenderMaterial = new ShaderMaterial(new Hashtable
        {
            { "defines", ssaoDepthShader.Defines },
            { "uniforms", UniformsUtils.CloneUniforms(ssaoDepthShader.Uniforms) },
            { "vertexShader", ssaoDepthShader.VertexShader },
            { "fragmentShader", ssaoDepthShader.FragmentShader },
            { "blending", Constants.NoBlending }
        });
        (depthRenderMaterial.Uniforms["tDepth"] as GLUniform)["value"] = beautyRenderTarget.depthTexture;
        (depthRenderMaterial.Uniforms["cameraNear"] as GLUniform)["value"] = this.camera.Near;
        (depthRenderMaterial.Uniforms["cameraFar"] as GLUniform)["value"] = this.camera.Far;

        // material for rendering the content of a render target
        var copyShader = new CopyShader();
        copyMaterial = new ShaderMaterial(new Hashtable
        {
            { "uniforms", UniformsUtils.CloneUniforms(copyShader.Uniforms) },
            { "vertexShader", copyShader.VertexShader },
            { "fragmentShader", copyShader.FragmentShader },
            { "transparent", true },
            { "depthTest", false },
            { "depthWrite", false },
            { "blendSrc", Constants.DstColorFactor },
            { "blendDst", Constants.ZeroFactor },
            { "blendEquation", Constants.AddEquation },
            { "blendSrcAlpha", Constants.DstAlphaFactor },
            { "blendDstAlpha", Constants.ZeroFactor },
            { "blendEquationAlpha", Constants.AddEquation }
        });

        fullScreenQuad = new FullScreenQuad();

        originalClearColor = new Color();
    }

    public virtual void Dispose()
    {
        Dispose(disposed);
    }


    private void GenerateRandomKernelRotations()
    {
        var width = 4;
        var height = 4;


        var simplex = new SimplexNoise();

        var size = width * height;
        var data = new float[size * 4];

        for (var i = 0; i < size; i++)
        {
            var stride = i * 4;

            var x = (float)(MathUtils.random.NextDouble() * 2) - 1;
            var y = (float)(MathUtils.random.NextDouble() * 2) - 1;
            var z = 0;

            var noise = simplex.Noise3d(x, y, z);

            data[stride] = noise;
            data[stride + 1] = noise;
            data[stride + 2] = noise;
            data[stride + 3] = 1;
        }
        //Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        //BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.WriteOnly, bitmap.PixelFormat);
        //IntPtr iptr = bitmapData.Scan0;

        //Marshal.Copy(iptr, data, 0, data.Length);

        //bitmap.UnlockBits(bitmapData);
        var bitmap = data.ToByteArray().ToSKBitMap(width, height);
        noiseTexture = new DataTexture(bitmap, width, height, Constants.RGBAFormat, Constants.FloatType);
        noiseTexture.WrapS = Constants.RepeatWrapping;
        noiseTexture.WrapT = Constants.RepeatWrapping;
    }

    private void GenerateSampleKernel()
    {
        for (var i = 0; i < kernelSize; i++)
        {
            var sample = new Vector3();
            sample.X = (float)(MathUtils.random.NextDouble() * 2) - 1;
            sample.Y = (float)(MathUtils.random.NextDouble() * 2) - 1;
            sample.Z = (float)MathUtils.random.NextDouble();

            sample.Normalize();

            float scale = i / kernelSize;
            scale = MathUtils.Lerp(0.1f, 1, scale * scale);
            sample.MultiplyScalar(scale);

            kernel.Add(sample);
        }
    }

    public override void Render(GLRenderer renderer, GLRenderTarget writeBuffer, GLRenderTarget readBuffer,
        float? deltaTime = null, bool? maskActive = null)
    {
        // render beauty and depth

        renderer.SetRenderTarget(beautyRenderTarget);
        renderer.Clear();
        renderer.Render(scene, camera);

        // render normals
        OverrideVisibility();
        RenderOverride(renderer, normalMaterial, normalRenderTarget, Color.Hex(0x7777ff), 1.0f);
        RestoreVisibility();
        // render SSAO

        (ssaoMaterial.Uniforms["kernelRadius"] as GLUniform)["value"] = kernelRadius;
        (ssaoMaterial.Uniforms["minDistance"] as GLUniform)["value"] = minDistance;
        (ssaoMaterial.Uniforms["maxDistance"] as GLUniform)["value"] = maxDistance;
        RenderPass(renderer, ssaoMaterial, ssaoRenderTarget);

        // render blur

        RenderPass(renderer, blurMaterial, blurRenderTarget);

        // output result to screen

        switch (output)
        {
            case (int)OUTPUT.SSAO:

                (copyMaterial.Uniforms["tDiffuse"] as GLUniform)["value"] = ssaoRenderTarget.Texture;
                copyMaterial.Blending = Constants.NoBlending;
                RenderPass(renderer, copyMaterial, RenderToScreen ? null : writeBuffer);

                break;

            case (int)OUTPUT.Blur:

                (copyMaterial.Uniforms["tDiffuse"] as GLUniform)["value"] = blurRenderTarget.Texture;
                copyMaterial.Blending = Constants.NoBlending;
                RenderPass(renderer, copyMaterial, RenderToScreen ? null : writeBuffer);

                break;

            case (int)OUTPUT.Beauty:

                (copyMaterial.Uniforms["tDiffuse"] as GLUniform)["value"] = beautyRenderTarget.Texture;
                copyMaterial.Blending = Constants.NoBlending;
                RenderPass(renderer, copyMaterial, RenderToScreen ? null : writeBuffer);

                break;

            case (int)OUTPUT.Depth:

                RenderPass(renderer, depthRenderMaterial, RenderToScreen ? null : writeBuffer);

                break;

            case (int)OUTPUT.Normal:

                (copyMaterial.Uniforms["tDiffuse"] as GLUniform)["value"] = normalRenderTarget.Texture;
                copyMaterial.Blending = Constants.NoBlending;
                RenderPass(renderer, copyMaterial, RenderToScreen ? null : writeBuffer);

                break;

            case (int)OUTPUT.Default:

                (copyMaterial.Uniforms["tDiffuse"] as GLUniform)["value"] = beautyRenderTarget.Texture;
                copyMaterial.Blending = Constants.NoBlending;
                RenderPass(renderer, copyMaterial, RenderToScreen ? null : writeBuffer);

                (copyMaterial.Uniforms["tDiffuse"] as GLUniform)["value"] = blurRenderTarget.Texture;
                copyMaterial.Blending = Constants.CustomBlending;
                RenderPass(renderer, copyMaterial, RenderToScreen ? null : writeBuffer);

                break;
        }
    }

    private void RestoreVisibility()
    {
        scene.Traverse(object3d => { object3d.Visible = _visibilityCache[object3d]; });
    }

    private void OverrideVisibility()
    {
        scene.Traverse(object3d =>
        {
            _visibilityCache[object3d] = object3d.Visible;

            if (object3d is Points || object3d is Line) object3d.Visible = false;
        });
    }

    public override void SetSize(float width, float height)
    {
        this.width = (int)width;
        this.height = (int)height;

        beautyRenderTarget.SetSize((int)width, (int)height);
        ssaoRenderTarget.SetSize((int)width, (int)height);
        normalRenderTarget.SetSize((int)width, (int)height);
        blurRenderTarget.SetSize((int)width, (int)height);

        ((ssaoMaterial.Uniforms["resolution"] as GLUniform)["value"] as Vector2).Set(width, height);
        ((ssaoMaterial.Uniforms["cameraProjectionMatrix"] as GLUniform)["value"] as Matrix4).Copy(
            camera.ProjectionMatrix);
        ((ssaoMaterial.Uniforms["cameraInverseProjectionMatrix"] as GLUniform)["value"] as Matrix4).GetInverse(
            camera.ProjectionMatrixInverse);

        ((blurMaterial.Uniforms["resolution"] as GLUniform)["value"] as Vector2).Set(width, height);
    }

    private void RenderPass(GLRenderer renderer, Material passMaterial, GLRenderTarget renderTarget,
        Color? clearColor = null, float? clearAlpha = 0.0f)
    {
        // save original state
        originalClearColor.Copy(renderer.GetClearColor());
        var originalClearAlpha = renderer.GetClearAlpha();
        var originalAutoClear = renderer.AutoClear;

        renderer.SetRenderTarget(renderTarget);

        // setup pass state
        renderer.AutoClear = false;
        if (clearColor != null && clearColor != null)
        {
            renderer.SetClearColor(clearColor.Value);
            renderer.SetClearAlpha(clearAlpha.Value);
            renderer.Clear();
        }

        fullScreenQuad.material = passMaterial;
        fullScreenQuad.Render(renderer);

        // restore original state
        renderer.AutoClear = originalAutoClear;
        renderer.SetClearColor(originalClearColor);
        renderer.SetClearAlpha(originalClearAlpha);
    }

    private void RenderOverride(GLRenderer renderer, Material overrideMaterial, GLRenderTarget renderTarget,
        Color? clearColor = null, float? clearAlpha = 0.0f)
    {
        originalClearColor.Copy(renderer.GetClearColor());
        var originalClearAlpha = renderer.GetClearAlpha();
        var originalAutoClear = renderer.AutoClear;

        if (clearAlpha == null) clearAlpha = 0.0f;

        renderer.SetRenderTarget(renderTarget);
        renderer.AutoClear = false;

        if (clearColor != null && clearAlpha != null)
        {
            renderer.SetClearColor(clearColor.Value);
            renderer.SetClearAlpha(clearAlpha.Value);
            renderer.Clear();
        }

        scene.OverrideMaterial = overrideMaterial;
        renderer.Render(scene, camera);
        scene.OverrideMaterial = null;

        // restore original state

        renderer.AutoClear = originalAutoClear;
        renderer.SetClearColor(originalClearColor);
        renderer.SetClearAlpha(originalClearAlpha);
    }

    public event EventHandler<EventArgs> Disposed;

    protected virtual void RaiseDisposed()
    {
        var handler = Disposed;
        if (handler != null)
            handler(this, new EventArgs());
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed) return;
        // dispose render targets

        beautyRenderTarget.Dispose();
        normalRenderTarget.Dispose();

        ssaoRenderTarget.Dispose();
        blurRenderTarget.Dispose();

        // dispose materials

        normalMaterial.Dispose();
        blurMaterial.Dispose();
        copyMaterial.Dispose();
        depthRenderMaterial.Dispose();

        // dipsose full screen quad

        fullScreenQuad.Dispose();

        RaiseDisposed();
        disposed = true;
        disposed = true;
    }
}