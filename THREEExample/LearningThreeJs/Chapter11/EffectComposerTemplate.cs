using System;
using System.Collections;
using System.Collections.Generic;
using ImGuiNET;
using OpenTK.Windowing.Common;
using THREE;
using Color = THREE.Color;

namespace THREEExample.Learning.Chapter11;

public class EffectComposerTemplate : Example
{
    private float angle = 1.0f;
    private float blending = 1.0f;
    private int blendingMode = 1;
    public EffectComposer bloomComposer;


    public BloomPass bloomPass;
    public BokehPass bokehPass;

    //dotScreenPass Parameter
    private float centerX = 0.5f;
    private float centerY = 0.5f;

    public EffectComposer composer;

    public EffectComposer composer1;
    public EffectComposer composer2;
    public EffectComposer composer3;
    public EffectComposer composer4;
    public EffectComposer dotScreenComposer;
    public DotScreenPass dotScreenPass;

    //GiltchPass parameter
    private int dtSize = 64;
    public Mesh earth;
    private float edgeGlow;

    //OutlinePass parameter
    private float edgeStrength = 3.0f;
    private float edgeThickness = 1.0f;
    public FilmPass effectFilm;

    public EffectComposer effectFilmComposer;


    public GlitchPass glitchPass;

    //FilmPass parameter
    private bool grayScale;
    private bool greyScale;
    public HalftonePass halftonePass;

    public int halfWidth, halfHeight;
    private float height = 1.0f;
    private Color hiddenEdgeColor = new(0x190A05);
    private int kernelSize = 25;

    public Dictionary<string, Material> materialsLib = new();
    private float noiseIntensity = 0.8f;
    public OutlinePass outlinePass;

    public Scene pivot;
    private float pulsePeriod;
    private float radius = 4.0f;

    public TexturePass renderedScene;

    public RenderPass renderPass;
    private int resolution = 256;
    private float rotateB = (float)Math.PI / 12 * 2;
    private float rotateG = (float)Math.PI / 12 * 2;
    private float rotateR = (float)Math.PI / 12 * 1;
    private float scale = 1.0f;
    private float scanlinesCount = 256.0f;
    private float scanlinesIntensity = 0.325f;
    private float scatter;

    //HalftonePass paramter;
    private int shape;
    private float sigma = 5.0f;

    //BloomPass parameter
    private float strength = 3.0f;
    private float threshold = 0.1f;
    public UnrealBloomPass unrealBloomPass;
    private float unrealRadius = 0.1f;
    private int unrealResolution = 256;

    //unrealBloomPass parameter
    private float unrealStrength = 1.0f;
    private bool usePatternTexture = false;
    private Color visibleEdgeColor = new(0xffffff);
    private float width = 1.0f;

    public virtual void LoadGeometry()
    {
        (earth, pivot) = Util11.AddEarth(scene);
    }

    public override void InitRenderer()
    {
        base.InitRenderer();
        renderer.SetClearColor(new Color().SetHex(0x000000));
        renderer.ShadowMap.Enabled = true;
        renderer.ShadowMap.Type = Constants.PCFSoftShadowMap;
    }

    public override void InitCamera()
    {
        base.InitCamera();
        camera.Fov = 45.0f;
        camera.Aspect = glControl.AspectRatio;
        camera.Near = 0.1f;
        camera.Far = 1000.0f;
        camera.Position.Set(0, 20, 40);
        camera.LookAt(Vector3.Zero());
    }

    public override void Init()
    {
        base.Init();

        halfWidth = glControl.Width / 2;
        halfHeight = glControl.Height / 2;
    }


    public override void Render()
    {
        earth.Rotation.Y += 0.001f;
        pivot.Rotation.Y += -0.0003f;
        base.Render();
    }

    public override void OnResize(ResizeEventArgs clientSize)
    {
        base.OnResize(clientSize);
        halfWidth = glControl.Width / 2;
        halfHeight = glControl.Height / 2;
    }

    public virtual void AddFilmPassControl(string rootName, FilmPass film)
    {
        if (ImGui.TreeNode(rootName))
        {
            if (ImGui.Checkbox("grayScale", ref grayScale))
                (film.uniforms["grayscale"] as GLUniform)["value"] = grayScale;
            if (ImGui.SliderFloat("noiseIntensity", ref noiseIntensity, 0.0f, 1.0f))
                (film.uniforms["nIntensity"] as GLUniform)["value"] = noiseIntensity;
            if (ImGui.SliderFloat("scanlinesIntensity", ref scanlinesIntensity, 0.0f, 1.0f))
                (film.uniforms["sIntensity"] as GLUniform)["value"] = scanlinesIntensity;
            if (ImGui.SliderFloat("scanlinesCount", ref scanlinesCount, 0.0f, 500.0f))
                (film.uniforms["sCount"] as GLUniform)["value"] = scanlinesCount;
            ImGui.TreePop();
        }
    }

    public virtual void AddDotScreenPassControl(string rootName, DotScreenPass dot)
    {
        if (ImGui.TreeNode(rootName))
        {
            if (ImGui.SliderFloat("centerX", ref centerX, 0.0f, 5.0f))
                (dot.uniforms["center"] as GLUniform)["value"] = new Vector2(centerX, centerY);
            if (ImGui.SliderFloat("centerY", ref centerY, 0.0f, 5.0f))
                (dot.uniforms["center"] as GLUniform)["value"] = new Vector2(centerX, centerY);
            if (ImGui.SliderFloat("angle", ref angle, 0.0f, 3.14f))
                (dot.uniforms["angle"] as GLUniform)["value"] = angle;
            if (ImGui.SliderFloat("scale", ref scale, 0.0f, 10.0f))
                (dot.uniforms["scale"] as GLUniform)["value"] = scale;
            ImGui.TreePop();
        }
    }

    public virtual void AddBloomPassControl(string rootName, BloomPass bloom)
    {
        var changed = false;
        if (ImGui.TreeNode(rootName))
        {
            if (ImGui.SliderFloat("strength", ref strength, 0.0f, 5.0f)) changed = true;
            if (ImGui.SliderInt("kernelSize", ref kernelSize, 10, 100)) changed = true;
            if (ImGui.SliderFloat("sigma", ref sigma, 1.0f, 8.0f)) changed = true;
            if (ImGui.SliderInt("resolution", ref resolution, 100, 156)) changed = true;
            if (changed)
            {
                var pass = new BloomPass(strength, kernelSize, sigma, resolution);
                bloomComposer.Passes[1] = pass;
            }

            ImGui.TreePop();
        }
    }

    public virtual void AddUnrealBloomPassControl(string rootName, BloomPass bloom)
    {
        var changed = false;
        if (ImGui.TreeNode(rootName))
        {
            if (ImGui.SliderInt("resolution", ref unrealResolution, 2, 1024)) changed = true;
            if (ImGui.SliderFloat("strength", ref unrealStrength, 0, 10)) changed = true;
            if (ImGui.SliderFloat("radius", ref unrealRadius, 0f, 10.0f)) changed = true;
            if (ImGui.SliderFloat("threshold", ref threshold, 0, 0.2f)) changed = true;
            if (changed)
            {
                var pass = new UnrealBloomPass(new Vector2(unrealResolution, unrealResolution), unrealStrength,
                    unrealRadius, threshold);
                bloomComposer.Passes[1] = pass;
            }

            ImGui.TreePop();
        }
    }

    public virtual void AddGiltchPassControl(string rootName, GlitchPass pass)
    {
        if (ImGui.TreeNode(rootName))
        {
            if (ImGui.SliderInt("dtsize", ref dtSize, 0, 1024)) composer1.Passes[1] = new GlitchPass(dtSize);

            ImGui.TreePop();
        }
    }

    public virtual void AddHalftonePassControl(string rootName, HalftonePass pass)
    {
        if (ImGui.TreeNode(rootName))
        {
            if (ImGui.Combo("shape", ref shape, "dot\0ellipse\0line\0square\0"))
            {
                var param = new Hashtable { { "shape", shape } };


                var halfPass = new HalftonePass(width, height, param);

                composer2.Passes[1] = halfPass;
            }

            if (ImGui.SliderFloat("radius", ref radius, 0, 40.0f))
            {
                var param = new Hashtable { { "radius", radius } };
                var halfPass = new HalftonePass(width, height, param);
                composer2.Passes[1] = halfPass;
            }

            if (ImGui.SliderFloat("rotateR", ref rotateR, 0, (float)Math.PI * 2))
            {
                var param = new Hashtable { { "rotateR", rotateR } };

                var halfPass = new HalftonePass(width, height, param);
                composer2.Passes[1] = halfPass;
            }

            if (ImGui.SliderFloat("rotateG", ref rotateG, 0, (float)Math.PI * 2))
            {
                var param = new Hashtable { { "rotateG", rotateG } };

                var halfPass = new HalftonePass(width, height, param);
                composer2.Passes[1] = halfPass;
            }

            if (ImGui.SliderFloat("rotateB", ref rotateB, 0, (float)Math.PI * 2))
            {
                var param = new Hashtable { { "rotateB", rotateB } };

                var halfPass = new HalftonePass(width, height, param);
                composer2.Passes[1] = halfPass;
            }

            if (ImGui.SliderFloat("scatter", ref scatter, 0, 2.0f))
            {
                var param = new Hashtable { { "scatter", scatter } };

                var halfPass = new HalftonePass(width, height, param);
                composer2.Passes[1] = halfPass;
            }

            if (ImGui.SliderFloat("width", ref width, 0, 15.0f))
            {
                var param = new Hashtable { { "width", width } };

                var halfPass = new HalftonePass(width, height, param);
                composer2.Passes[1] = halfPass;
            }

            if (ImGui.SliderFloat("height", ref height, 0, 15.0f))
            {
                var param = new Hashtable { { "height", height } };

                var halfPass = new HalftonePass(width, height, param);
                composer2.Passes[1] = halfPass;
            }

            if (ImGui.SliderFloat("blending", ref blending, 0, 2.0f))
            {
                var param = new Hashtable { { "blending", blending } };

                var halfPass = new HalftonePass(width, height, param);
                composer2.Passes[1] = halfPass;
            }

            if (ImGui.Combo("blendingMode", ref blendingMode, "linear\0multiply\0add\0lighter\0darker\0"))
            {
                var param = new Hashtable { { "blendingMode", shape } };


                var halfPass = new HalftonePass(width, height, param);

                composer2.Passes[1] = halfPass;
            }

            if (ImGui.Checkbox("greyscale", ref greyScale))
            {
                var param = new Hashtable { { "greyscale", greyScale } };
                var halfPass = new HalftonePass(width, height, param);
                composer2.Passes[1] = halfPass;
            }

            ImGui.TreePop();
        }
    }

    public virtual void AddOutlinePassControls(string rootName, OutlinePass pass)
    {
        var color1 = new System.Numerics.Vector3(visibleEdgeColor.R, visibleEdgeColor.G, visibleEdgeColor.B);
        var color2 = new System.Numerics.Vector3(hiddenEdgeColor.R, hiddenEdgeColor.G, hiddenEdgeColor.B);

        if (ImGui.TreeNode(rootName))
        {
            if (ImGui.SliderFloat("edgeStrength", ref edgeStrength, 0.01f, 10.0f))
                outlinePass.edgeStrength = edgeStrength;
            if (ImGui.SliderFloat("edgeGlow", ref edgeGlow, 0.0f, 1.0f)) outlinePass.edgeGlow = edgeGlow;
            if (ImGui.SliderFloat("edgeThickness", ref edgeThickness, 1.0f, 4.0f))
                outlinePass.edgeThickness = edgeThickness;
            if (ImGui.SliderFloat("pulsePeriod", ref pulsePeriod, 0.0f, 5.0f)) outlinePass.pulsePeriod = pulsePeriod;
            if (ImGui.ColorPicker3("visibleEdgeColor", ref color1))
            {
                visibleEdgeColor = new Color(color1.X, color1.Y, color1.Z);
                outlinePass.visibleEdgeColor = visibleEdgeColor;
            }

            if (ImGui.ColorPicker3("hiddenEdgeColor", ref color2))
            {
                hiddenEdgeColor = new Color(color2.X, color2.Y, color2.Z);
                outlinePass.hiddenEdgeColor = hiddenEdgeColor;
            }

            ImGui.TreePop();
        }
    }

    public virtual Mesh AddGeometryWithMaterial(Scene scene, BufferGeometry geometry, string name, Material material)
    {
        var mesh = new Mesh(geometry, material);
        mesh.CastShadow = true;

        scene.Add(mesh);

        materialsLib.Add(name, material);
        return mesh;
    }

    public virtual Mesh AddGeometry(Scene scene, Geometry geometry, string name, Texture texture)
    {
        var mat = new MeshStandardMaterial
        {
            Map = texture,
            Metalness = 0.2f,
            Roughness = 0.07f
        };

        var mesh = new Mesh(geometry, mat);

        mesh.CastShadow = true;

        scene.Add(mesh);

        materialsLib.Add(name, mat);

        return mesh;
    }

    public virtual void AddBasicMaterialSettings(Material material, string name)
    {
        var currentSide = material.Side;
        var shadowSide = material.ShadowSide == null ? 0 : material.ShadowSide.Value;
        if (ImGui.TreeNode(name))
        {
            ImGui.Text($"id={material.Id}");
            ImGui.Text($"uuid={material.Uuid}");
            ImGui.Text($"name={material.Name}");
            ImGui.SliderFloat("opacity", ref material.Opacity, 0.0f, 1.0f);
            ImGui.Checkbox("transparent", ref material.Transparent);
            ImGui.Checkbox("visible", ref material.Visible);
            if (ImGui.Combo("side", ref currentSide, "FrontSide\0BackSide\0BothSide\0")) material.Side = currentSide;
            ImGui.Checkbox("colorWrite", ref material.ColorWrite);
            if (ImGui.Checkbox("flatShading", ref material.FlatShading)) material.NeedsUpdate = true;
            ImGui.Checkbox("premultipliedAlpha", ref material.PremultipliedAlpha);
            ImGui.Checkbox("dithering", ref material.Dithering);
            if (ImGui.Combo("shadowSide", ref shadowSide, "FrontSide\0BackSide\0BothSide\0"))
                material.ShadowSide = shadowSide;
            ImGui.Checkbox("fog", ref material.Fog);
            ImGui.TreePop();
        }
    }

    public virtual void AddSpecificMaterialSettings(Material material, string name)
    {
        var materialColor = material.Color.Value;
        var emissiveColor = material.Emissive.Value;
        var color = new System.Numerics.Vector3(materialColor.R, materialColor.G, materialColor.B);
        var emissive = new System.Numerics.Vector3(emissiveColor.R, emissiveColor.G, emissiveColor.B);
        if (ImGui.TreeNode(name))
        {
            switch (material.type)
            {
                case "MeshNormalMaterial":
                    ImGui.Checkbox("wireframe", ref material.Wireframe);
                    break;
                case "MeshPhongMaterial":
                    ImGui.SliderFloat("shininess", ref material.Shininess, 0, 100);
                    break;
                case "MeshStandardMaterial":
                    if (ImGui.ColorPicker3("color", ref color))
                    {
                        var mColor = new Color(color.X, color.Y, color.Z);
                        material.Color = mColor;
                    }

                    if (ImGui.ColorPicker3("emissive", ref emissive))
                    {
                        var eColor = new Color(emissive.X, emissive.Y, emissive.Z);
                        material.Emissive = eColor;
                    }

                    ImGui.SliderFloat("metalness", ref material.Metalness, 0, 1);
                    ImGui.SliderFloat("roughness", ref material.Roughness, 0, 1);
                    ImGui.Checkbox("wireframe", ref material.Wireframe);
                    break;
            }

            ImGui.TreePop();
        }
    }
}