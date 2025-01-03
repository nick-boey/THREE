using System.Collections;
using OpenTK.Graphics.ES30;

namespace THREE;

public class CinematicCamera : PerspectiveCamera
{
    private readonly BokehShader2 bokehShader = new();
    private readonly BokehDepthShader depthShader = new();

    private readonly IControlsContainer glControl;
    private readonly ShaderMaterial materialDepth;
    private readonly Hashtable ShaderSettings = new() { { "rings", 3 }, { "samples", 4 } };

    private float Aperture;

    private float Coc;
    private float DepthOfField;
    private float FarPoint;

    private float FNumber;

    private float HyperFocal;
    private float Ldistance;

    private float NearPoint;
    public Hashtable postprocessing = new();
    private float Sdistance;

    public CinematicCamera(IControlsContainer glControl, float fov, float aspect, float near, float far) : base(fov,
        aspect, near, far)
    {
        type = "CinematicCamera";
        this.glControl = glControl;
        postprocessing.Add("enabled", true);

        materialDepth = new ShaderMaterial
        {
            Uniforms = depthShader.Uniforms,
            VertexShader = depthShader.VertexShader,
            FragmentShader = depthShader.FragmentShader
        };

        materialDepth.Uniforms["mNear"] = new GLUniform { { "value", near } };
        materialDepth.Uniforms["mFar"] = new GLUniform { { "value", far } };

        SetLens();

        InitProcessing();
    }

    public void SetLens(float? focalLength = null, int? filmGauge = null, float? fNumber = null, float? coc = null)
    {
        // In case of cinematicCamera, having a default lens set is important
        if (focalLength == null) focalLength = 35;
        if (filmGauge != null) FilmGauge = filmGauge.Value;

        SetFocalLength(focalLength.Value);

        // if fnumber and coc are not provided, cinematicCamera tries to act as a basic PerspectiveCamera
        if (fNumber == null) fNumber = 8;
        if (coc == null) coc = 0.019f;

        FNumber = fNumber.Value;
        Coc = coc.Value;

        // fNumber is focalLength by aperture
        Aperture = focalLength.Value / FNumber;

        // hyperFocal is required to calculate depthOfField when a lens tries to focus at a distance with given fNumber and focalLength
        HyperFocal = focalLength.Value * focalLength.Value / (Aperture * Coc);
    }

    public float Linearize(float depth)
    {
        var zfar = Far;
        var znear = Near;

        return -zfar * znear / (depth * (zfar - znear) - zfar);
    }

    public float SmoothStep(float near, float far, float depth)
    {
        var x = Saturate((depth - near) / (far - near));

        return x * x * (3 - 2 * x);
    }

    public float Saturate(float x)
    {
        return Math.Max(0, Math.Min(1, x));
    }

    public void FocusAt(float? focusDistance)
    {
        if (focusDistance == null) focusDistance = 20;

        var focalLength = GetFocalLength();

        // distance from the camera (normal to frustrum) to focus on
        focus = focusDistance.Value;

        // the nearest point from the camera which is in focus (unused)
        NearPoint = HyperFocal * focus / (HyperFocal + (focus - focalLength));

        // the farthest point from the camera which is in focus (unused)
        FarPoint = HyperFocal * focus / (HyperFocal - (focus - focalLength));

        // the gap or width of the space in which is everything is in focus (unused)
        DepthOfField = FarPoint - NearPoint;

        // Considering minimum distance of focus for a standard lens (unused)
        if (DepthOfField < 0) DepthOfField = 0;

        Sdistance = SmoothStep(Near, Far, focus);

        Ldistance = Linearize(1 - Sdistance);

        (postprocessing["bokeh_uniforms"] as GLUniforms)["focalDepth"] = new GLUniform { { "value", Ldistance } };
    }

    public void InitProcessing()
    {
        if ((bool)postprocessing["enabled"])
        {
            postprocessing["scene"] = new Scene();

            postprocessing["camera"] = new OrthographicCamera(glControl.ClientRectangle.Width / -2,
                glControl.ClientRectangle.Width / 2, glControl.ClientRectangle.Height / 2,
                glControl.ClientRectangle.Height / -2, -10000, 10000);

            (postprocessing["scene"] as Scene).Add(postprocessing["camera"] as OrthographicCamera);

            var pars = new Hashtable
            {
                { "minFilter", Constants.LinearFilter }, { "magFilter", Constants.LinearFilter },
                { "format", Constants.RGBFormat }
            };
            postprocessing["rtTextureDepth"] =
                new GLRenderTarget(glControl.ClientRectangle.Width, glControl.ClientRectangle.Height, pars);
            postprocessing["rtTextureColor"] =
                new GLRenderTarget(glControl.ClientRectangle.Width, glControl.ClientRectangle.Height, pars);

            var bokeh_shader = bokehShader;

            postprocessing["bokeh_uniforms"] = UniformsUtils.CloneUniforms(bokeh_shader.Uniforms);

            (postprocessing["bokeh_uniforms"] as GLUniforms)["tColor"] = new GLUniform
                { { "value", (postprocessing["rtTextureColor"] as GLRenderTarget).Texture } };
            (postprocessing["bokeh_uniforms"] as GLUniforms)["tDepth"] = new GLUniform
                { { "value", (postprocessing["rtTextureDepth"] as GLRenderTarget).Texture } };

            (postprocessing["bokeh_uniforms"] as GLUniforms)["manualdof"] = new GLUniform { { "value", 0 } };
            (postprocessing["bokeh_uniforms"] as GLUniforms)["shaderFocus"] = new GLUniform { { "value", 0 } };

            (postprocessing["bokeh_uniforms"] as GLUniforms)["fstop"] = new GLUniform { { "value", 2.8f } };

            (postprocessing["bokeh_uniforms"] as GLUniforms)["showFocus"] = new GLUniform { { "value", 1 } };

            (postprocessing["bokeh_uniforms"] as GLUniforms)["focalDepth"] = new GLUniform { { "value", 0.1f } };

            //console.log( this.postprocessing["bokeh_uniforms"] as Uniforms)[ "focalDepth" ] = new Uniform{{"value", );

            (postprocessing["bokeh_uniforms"] as GLUniforms)["znear"] = new GLUniform { { "value", Near } };
            (postprocessing["bokeh_uniforms"] as GLUniforms)["zfar"] = new GLUniform { { "value", Near } };


            (postprocessing["bokeh_uniforms"] as GLUniforms)["textureWidth"] =
                new GLUniform { { "value", glControl.ClientRectangle.Width } };

            (postprocessing["bokeh_uniforms"] as GLUniforms)["textureHeight"] = new GLUniform
                { { "value", glControl.ClientRectangle.Height } };

            postprocessing["materialBokeh"] = new ShaderMaterial
            {
                Uniforms = postprocessing["bokeh_uniforms"] as GLUniforms,
                VertexShader = bokeh_shader.VertexShader,
                FragmentShader = bokeh_shader.FragmentShader,
                Defines = new Hashtable
                {
                    { "RINGS", ShaderSettings["rings"] },

                    { "SAMPLES", ShaderSettings["samples"] },

                    { "DEPTH_PACKING", 1 }
                }
            };

            postprocessing["quad"] =
                new Mesh(new PlaneBufferGeometry(glControl.ClientRectangle.Width, glControl.ClientRectangle.Height),
                    postprocessing["materialBokeh"] as ShaderMaterial);
            (postprocessing["quad"] as Mesh).Position.Z = -500;
            (postprocessing["scene"] as Scene).Add(postprocessing["quad"] as Mesh);
        }
    }

    public void RenderCinematic(Scene scene, GLRenderer renderer)
    {
        if ((bool)postprocessing["enabled"])
        {
            var currentRenderTarget = renderer.GetRenderTarget();

            renderer.Clear();

            // Render scene into texture

            scene.OverrideMaterial = null;
            renderer.SetRenderTarget(postprocessing["rtTextureColor"] as GLRenderTarget);
            renderer.Clear();
            renderer.Render(scene, this);

            // Render depth into texture

            scene.OverrideMaterial = materialDepth;
            renderer.SetRenderTarget(postprocessing["rtTextureDepth"] as GLRenderTarget);
            renderer.Clear();
            renderer.Render(scene, this);

            // Render bokeh composite

            renderer.SetRenderTarget(null);
            renderer.Render(postprocessing["scene"] as Scene, postprocessing["camera"] as Camera);

            renderer.SetRenderTarget(currentRenderTarget);

            GL.ActiveTexture(TextureUnit.Texture0);
        }
    }
}