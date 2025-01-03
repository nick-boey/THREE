using System.Collections;

namespace THREE;

[Serializable]
public class BokehPass : Pass
{
    public Camera camera;
    private ShaderMaterial materialBokeh;
    private MeshDepthMaterial materialDepth;
    private Color oldClearColor;

    private GLRenderTarget renderTargetDepth;
    public Scene scene;
    public GLUniforms uniforms;

    public BokehPass(Scene scene, Camera camera, Hashtable parameter)
    {
        this.scene = scene;
        this.camera = camera;

        var focus = parameter.ContainsKey("focus") ? parameter["focus"] : 1.0f;
        var aspect = parameter.ContainsKey("aspect") ? parameter["aspect"] : camera.Aspect;
        var aperture = parameter.ContainsKey("aperture") ? parameter["aperture"] : 0.025f;
        var maxblur = parameter.ContainsKey("maxblur") ? parameter["maxblur"] : 1.0f;

        // render targets

        var width = parameter.ContainsKey("width") ? (int)parameter["width"] : 1;
        var height = parameter.ContainsKey("height") ? (int)parameter["height"] : 1;

        renderTargetDepth = new GLRenderTarget(width, height, new Hashtable
        {
            { "minFilter", Constants.NearestFilter },
            { "magFilter", Constants.NearestFilter }
        });

        renderTargetDepth.Texture.Name = "BokehPass.depth";

        // depth material

        materialDepth = new MeshDepthMaterial();
        materialDepth.DepthPacking = Constants.RGBADepthPacking;
        materialDepth.Blending = Constants.NoBlending;

        // bokeh material


        var bokehShader = new BokehShader();
        var bokehUniforms = UniformsUtils.CloneUniforms(bokehShader.Uniforms);

        (bokehUniforms["tDepth"] as GLUniform)["value"] = renderTargetDepth.Texture;

        (bokehUniforms["focus"] as GLUniform)["value"] = focus;
        (bokehUniforms["aspect"] as GLUniform)["value"] = aspect;
        (bokehUniforms["aperture"] as GLUniform)["value"] = aperture;
        (bokehUniforms["maxblur"] as GLUniform)["value"] = maxblur;
        (bokehUniforms["nearClip"] as GLUniform)["value"] = camera.Near;
        (bokehUniforms["farClip"] as GLUniform)["value"] = camera.Far;

        materialBokeh = new ShaderMaterial
        {
            Defines = bokehShader.Defines,
            Uniforms = bokehUniforms,
            VertexShader = bokehShader.VertexShader,
            FragmentShader = bokehShader.FragmentShader
        };

        uniforms = bokehUniforms;
        NeedsSwap = false;

        fullScreenQuad = new FullScreenQuad(materialBokeh);

        oldClearColor = new Color();
    }

    public override void Render(GLRenderer renderer, GLRenderTarget writeBuffer, GLRenderTarget readBuffer,
        float? deltaTime = null, bool? maskActive = null)
    {
        // Render depth into texture

        scene.OverrideMaterial = materialDepth;

        oldClearColor.Copy(renderer.GetClearColor());
        var oldClearAlpha = renderer.GetClearAlpha();
        var oldAutoClear = renderer.AutoClear;
        renderer.AutoClear = false;

        renderer.SetClearColor(Color.Hex(0xffffff));
        renderer.SetClearAlpha(1.0f);
        renderer.SetRenderTarget(renderTargetDepth);
        renderer.Clear();
        renderer.Render(scene, camera);

        // Render bokeh composite

        (uniforms["tColor"] as GLUniform)["value"] = readBuffer.Texture;
        (uniforms["nearClip"] as GLUniform)["value"] = camera.Near;
        (uniforms["farClip"] as GLUniform)["value"] = camera.Far;

        if (RenderToScreen)
        {
            renderer.SetRenderTarget(null);
            fullScreenQuad.Render(renderer);
        }
        else
        {
            renderer.SetRenderTarget(writeBuffer);
            renderer.Clear();
            fullScreenQuad.Render(renderer);
        }

        scene.OverrideMaterial = null;
        renderer.SetClearColor(oldClearColor);
        renderer.SetClearAlpha(oldClearAlpha);
        renderer.AutoClear = oldAutoClear;
    }

    public override void SetSize(float width, float height)
    {
    }
}