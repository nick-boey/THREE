﻿using System.Collections;
using OpenTK.Graphics.ES30;
using THREE.OpenGL.Extensions;

namespace THREE;

[Serializable]
public class GLRenderer : DisposableObject, IGLRenderer
{
    private int _currentActiveMipmapLevel;

    private GLRenderList _currentRenderList;

    private GLRenderState _currentRenderState;
    private GLCapabilitiesParameters _parameters;

    private Stack<GLRenderList> _renderListStack = new();

    private Stack<GLRenderState> _renderStateStack = new();

    private GLAttributes attributes;

    private GLBackground background;

    public GLBindingStates bindingStates;

    private GLBufferRenderer bufferRenderer;

    public GLCapabilities Capabilities;

    // User defined clipping
    public List<Plane> ClippingPlanes = new();
    public IGraphicsContext Context;

    private GLCubeMap cubeMaps;

    public Hashtable debug = new();

    private Scene emptyScene;

    public GLExtensions Extensions;

    // Physically based shading
    public float GammaFactor = 2.0f; // for backwards compatibility

    private GLGeometries geometries;

    private GLIndexedBufferRenderer indexedBufferRenderer;
    public GLInfo Info;
    public bool LocalClippingEnabled = false;

    private GLMaterials materials;
    public int MaxMorphNormals = 4;

    // Morphs
    public int MaxMorphTargets = 8;

    private GLMorphtargets morphtargets;

    public GLMultiview Multiview;

    private GLObjects objects;

    public int OutputEncoding = Constants.LinearEncoding;
    public bool PhysicallyCorrectLights = false;

    private bool premultipliedAlpha = true;

    private GLPrograms programCache;

    public GLProperties Properties;

    private GLRenderLists renderLists;

    private GLRenderStates renderStates;

    public ShaderLib ShaderLib = Global.ShaderLib;

    public GLShadowMap ShadowMap;

    // Scene graph
    public bool SortObjects = true;
    public GLState State;

    private GLTextures textures;

    // Tone mapping
    public int ToneMapping = Constants.LinearToneMapping;
    public float ToneMappingExposure = 1.0f;
    public float ToneMappingWhitePoint = 1.0f;

    private GLUtils utils;

    public bool ShadowMapEnabled
    {
        get => ShadowMap.Enabled;
        set => ShadowMap.Enabled = value;
    }

    public int ShadowMapType
    {
        get => ShadowMap.ShadowMapType;
        set => ShadowMap.ShadowMapType = value;
    }

    public int ShadowMapCullFace
    {
        get => ShadowMap.Type;
        set => ShadowMap.Type = value;
    }

    public GLRenderer()
    {
    }

    public GLRenderer(IGraphicsContext context, int width, int height) : base()
    {
        Context = context;
        _viewport = new Vector4(0, 0, width, height);
        Width = width;
        Height = height;
        Init();
    }

    public bool IsGL2 { get; set; }

    public bool AutoClear { get; set; } = true;
    public bool AutoClearColor { get; set; } = true;
    public bool AutoClearDepth { get; set; } = true;
    public bool AutoClearStencil { get; set; } = true;

    public float GetPixelRatio()
    {
        return _pixelRatio;
    }

    public void ClearDepth()
    {
        Clear(false, true, false);
    }

    public Vector2 GetSize(Vector2 target = null)
    {
        if (target == null) target = new Vector2();

        target.Set(Width, Height);

        return target;
    }

    public void Clear(bool? color = null, bool? depth = null, bool? stencil = null)
    {
        var bits = 0;

        if (color == null || color == true) bits |= (int)ClearBufferMask.ColorBufferBit;
        if (depth == null || depth == true) bits |= (int)ClearBufferMask.DepthBufferBit;
        if (stencil == null || stencil == true) bits |= (int)ClearBufferMask.StencilBufferBit;


        var mask = (ClearBufferMask)Enum.ToObject(typeof(ClearBufferMask), bits);

        if (Context.IsCurrent)
            GL.Clear(mask);
    }

    public Color GetClearColor()
    {
        return background.ClearColor;
    }

    public float GetClearAlpha()
    {
        return background.ClearAlpha;
    }

    public void SetClearAlpha(float alpha)
    {
        background.SetClearAlpha(alpha);
    }

    public void SetClearColor(Color color, float alpha = 1)
    {
        background.SetClearColor(color, alpha);
    }

    public GLRenderTarget GetRenderTarget()
    {
        return _currentRenderTarget;
    }

    public void SetRenderTarget(GLRenderTarget renderTarget, int? activeCubeFace = null,
        int? activeMipmapLevel = null)
    {
        _currentRenderTarget = renderTarget;

        if (activeCubeFace != null)
            _currentActiveCubeFace = activeCubeFace.Value;

        if (activeMipmapLevel != null)
            _currentActiveMipmapLevel = activeMipmapLevel.Value;

        if (renderTarget != null && Properties.Get(renderTarget)["glFramebuffer"] == null)
            textures.SetupRenderTarget(renderTarget);

        var framebuffer = _framebuffer;
        var isCube = false;

        if (renderTarget != null)
        {
            if (renderTarget is GLCubeRenderTarget)
            {
                var glFramebuffer = (int[])Properties.Get(renderTarget)["glFramebuffer"];

                framebuffer = glFramebuffer[activeCubeFace != null ? activeCubeFace.Value : 0];
                isCube = true;
            }
            else if (renderTarget is GLMultisampleRenderTarget)
            {
                var glFramebuffer = (int)Properties.Get(renderTarget)["glMultisampledFramebuffer"];
                framebuffer = glFramebuffer;
            }
            else
            {
                var glFramebuffer = (int)Properties.Get(renderTarget)["glFramebuffer"];
                framebuffer = glFramebuffer;
            }

            _currentViewport.Copy(renderTarget.Viewport);
            _currentScissor.Copy(renderTarget.Scissor);
            _currentScissorTest = renderTarget.ScissorTest;
        }
        else
        {
            _currentViewport.Copy(_viewport).MultiplyScalar(_pixelRatio).Floor();
            _currentScissor.Copy(_scissor).MultiplyScalar(_pixelRatio).Floor();
            _currentScissorTest = _scissorTest;
        }

        if (_currentFramebuffer != framebuffer)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer == null ? 0 : (int)framebuffer);
            _currentFramebuffer = framebuffer;
        }

        State.Viewport(_currentViewport);
        State.Scissor(_currentScissor);
        State.SetScissorTest(_currentScissorTest.Value);

        if (isCube)
        {
            var textureProperties = Properties.Get(renderTarget.Texture);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                TextureTarget2d.TextureCubeMapPositiveX + (activeCubeFace != null ? activeCubeFace.Value : 0),
                (int)textureProperties["glTexture"], activeMipmapLevel != null ? activeMipmapLevel.Value : 0);
        }
    }

    public void ReadRenderTargetPixels(GLRenderTarget renderTarget, float x, float y, int width, int height,
        byte[] buffer, int? activeCubeFaceIndex)
    {
        if (renderTarget == null)
            //console.error('THREE.WebGLRenderer.readRenderTargetPixels: renderTarget is null THREE.WebGLRenderTarget.');
            return;

        //var glFramebuffer = (int[])(properties.Get(renderTarget) as Hashtable)["glFramebuffer"];
        int framebuffer;
        if (renderTarget is GLCubeRenderTarget)
            framebuffer =
                ((int[])Properties.Get(renderTarget)["glFramebuffer"])[
                    activeCubeFaceIndex != null ? activeCubeFaceIndex.Value : 0];
        else if (renderTarget is GLMultisampleRenderTarget)
            framebuffer = (int)Properties.Get(renderTarget)["glMultisampledFramebuffer"];
        else
            framebuffer = (int)Properties.Get(renderTarget)["glFramebuffer"];

        if (framebuffer != 0)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);

            try
            {
                var texture = renderTarget.Texture;
                var textureFormat = texture.Format;
                var textureType = texture.Type;

                if (textureFormat != Constants.RGBAFormat)
                    //console.error('THREE.WebGLRenderer.readRenderTargetPixels: renderTarget is not in RGBA or implementation defined format.');
                    return;

                // the following if statement ensures valid read requests (no out-of-bounds pixels, see Three.js Issue #8604)
                if (x >= 0 && x <= renderTarget.Width - width &&
                    y >= 0 && y <= renderTarget.Height - height)
                    GL.ReadPixels((int)x, (int)y, width, height, utils.Convert(textureFormat),
                        utils.Convert(textureType), buffer);
            }
            finally
            {
                // restore framebuffer of current render target if necessary=
                if (_currentRenderTarget != null)
                {
                    if (renderTarget is GLCubeRenderTarget)
                        framebuffer =
                            ((int[])Properties.Get(_currentRenderTarget)["glFramebuffer"])[
                                activeCubeFaceIndex != null ? activeCubeFaceIndex.Value : 0];
                    else if (renderTarget is GLMultisampleRenderTarget)
                        framebuffer =
                            (int)Properties.Get(_currentRenderTarget)["glMultisampledFramebuffer"];
                    else
                        framebuffer = (int)Properties.Get(_currentRenderTarget)["glFramebuffer"];
                }

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
            }
        }
    }

    public void CopyFramebufferToTexture(Vector2 position, Texture texture, int? level = null)
    {
        if (level == null) level = 0;

        var levelScale = (float)Math.Pow(2, -level.Value);
        var width = (float)Math.Floor(texture.Image.Width * levelScale);
        var height = (float)Math.Floor(texture.Image.Height * levelScale);
        var glFormat = utils.Convert(texture.Format);

        textures.SetTexture2D(texture, 0);

        GL.CopyTexImage2D(TextureTarget2d.Texture2D, level.Value, (TextureCopyComponentCount)glFormat,
            (int)position.X, (int)position.Y, (int)width, (int)height, 0);
        //GL.CopyTexImage2D(All.Texture2D, level.Value, glFormat, (int)position.X, (int)position.Y, (int)width, (int)height, 0);
        State.UnbindTexture();
    }

    public void ClearStencil()
    {
        Clear(false, false, true);
    }

    public void SetSize(float width, float height)
    {
        Width = (int)Math.Floor(width * _pixelRatio);
        Height = (int)Math.Floor(height * _pixelRatio);

        SetViewport(Width, Height);
    }

    public void SetClearColor(int color, float alpha = 1)
    {
        SetClearColor(Color.Hex(color), alpha);
    }

    public void SetViewport(int width, int height)
    {
        _viewport.Set(0, 0, width, height);

        _currentViewport = (_viewport * _pixelRatio).Floor();
        State.Viewport(_currentViewport);
    }

    public void SetViewport(int x, int y, int width, int height)
    {
        _viewport.Set(x, y, width, height);
        _currentViewport = (_viewport * _pixelRatio).Floor();
        State.Viewport(_currentViewport);
    }

    public void SetScissorTest(bool value)
    {
        State.SetScissorTest(value);
    }

    public void SetScissor(int x, int y, int width, int height)
    {
        _scissor.Set(x, y, width, height);
        State.Scissor(_currentScissor.Copy(_scissor));
    }

    public Vector4 GetScissor(Vector4 target)
    {
        return target.Copy(_scissor);
    }

    private void InitGLContext()
    {
        Extensions = new GLExtensions();
        _parameters = new GLCapabilitiesParameters();
        Capabilities = new GLCapabilities(Extensions, ref _parameters);

        _viewport.Set(0, 0, Width, Height);
        _scissor.Set(0, 0, Width, Height);
        if (Capabilities.IsGL2 == false)
        {
            //extensions.get( 'WEBGL_depth_texture' );
            //extensions.get( 'OES_texture_float' );
            //extensions.get( 'OES_texture_half_float' );
            //extensions.get( 'OES_texture_half_float_linear' );
            //extensions.get( 'OES_standard_derivatives' );
            //extensions.get( 'OES_element_index_uint' );
            //extensions.get( 'ANGLE_instanced_arrays' );
        }

        //Extensions.Get("OES_texture_float_linear");
        utils = new GLUtils(Extensions, Capabilities);
        State = new GLState(Extensions, utils, Capabilities);
        _currentScissor = (_scissor * _pixelRatio).Floor();
        _currentViewport = (_viewport * _pixelRatio).Floor();

        State.Scissor(_currentScissor);
        State.Viewport(_currentViewport);

        Info = new GLInfo();

        Properties = new GLProperties();

        textures = new GLTextures(Context, Extensions, State, Properties, Capabilities, utils, Info);

        attributes = new GLAttributes();

        geometries = new GLGeometries(this, attributes, Info);

        objects = new GLObjects(geometries, attributes, Info);

        morphtargets = new GLMorphtargets();

        cubeMaps = new GLCubeMap(this);

        bindingStates = new GLBindingStates(Context, Extensions, attributes, Capabilities);

        programCache = new GLPrograms(this, cubeMaps, Extensions, Capabilities, bindingStates, _clipping);

        renderLists = new GLRenderLists(Properties);

        renderStates = new GLRenderStates(Extensions, Capabilities);

        background = new GLBackground(this, cubeMaps, State, objects, premultipliedAlpha);

        bufferRenderer = new GLBufferRenderer(this, Extensions, Info, Capabilities);

        indexedBufferRenderer = new GLIndexedBufferRenderer(this, Extensions, Info, Capabilities);


        Info.programs = programCache.Programs;

        emptyScene = new Scene();


        materials = new GLMaterials(Properties);
    }

    private int GetTargetPixelRatio()
    {
        return _currentRenderTarget == null ? _pixelRatio : 1;
    }

    private void OnMaterialDispose(object sender, EventArgs e)
    {
    }

    public GLRenderLists GetRenderLists()
    {
        return renderLists;
    }

    public GLRenderList GetRenderList()
    {
        return _currentRenderList;
    }

    public void SetRenderList(GLRenderList renderList)
    {
        _currentRenderList = renderList;
    }

    public GLRenderState GetRenderState()
    {
        return _currentRenderState;
    }

    public void SetRenderState(GLRenderState renderState)
    {
        _currentRenderState = renderState;
    }

    private GLProgram GetProgram(Material material, Object3D scene, Object3D object3D)
    {
        if (scene is not Scene) scene = emptyScene;

        var materialProperties = Properties.Get(material);

        var lights = _currentRenderState.State.Lights;
        var shadowsArray = _currentRenderState.State.ShadowsArray;

        var lightsStateVersion = (int)lights.state["version"];

        var parameters = programCache.GetParameters(material, lights, shadowsArray, scene, object3D);

        var programCacheKey = programCache.getProgramCacheKey(parameters).Replace("False", "false")
            .Replace("True", "true");

        var programs = (Hashtable)materialProperties["programs"];
        materialProperties["environment"] = material is MeshStandardMaterial
            ? scene is Scene ? (scene as Scene).Environment : null
            : null;
        materialProperties["fog"] = scene is Scene ? (scene as Scene).Fog : null;
        materialProperties["envMap"] = cubeMaps.Get(material.EnvMap != null
            ? material.EnvMap
            : materialProperties["environment"] as Texture);

        if (programs == null)
        {
            material.Disposed += (sender, e) => { DeallocateMaterial(material); };
            programs = new Hashtable();
            materialProperties["programs"] = programs;
        }

        var program = programs.ContainsKey(programCacheKey) ? (GLProgram)programs[programCacheKey] : null;
        if (program != null)
        {
            if (materialProperties.ContainsKey("currentProgram") &&
                (GLProgram)materialProperties["currentProgram"] == program &&
                materialProperties.ContainsKey("lightsStateVersion") &&
                (int)materialProperties["lightsStateVersion"] == lightsStateVersion)
            {
                UpdateCommonMaterialProperties(material, parameters);
                return program;
            }
        }
        else
        {
            parameters["uniforms"] = programCache.GetUniforms(material);
            if (material.OnBuild != null) material.OnBuild(parameters, this);
            if (material.OnBeforeCompile != null) material.OnBeforeCompile(parameters, this);
            program = programCache.AcquireProgram(parameters, programCacheKey);
            programs[programCacheKey] = program;
            materialProperties["uniforms"] = parameters["uniforms"];
        }

        var uniforms = materialProperties["uniforms"] as GLUniforms;

        if ((!(material is ShaderMaterial) && !(material is RawShaderMaterial)) || material.Clipping)
            uniforms["clippingPlanes"] = _clipping.uniform;

        UpdateCommonMaterialProperties(material, parameters);
        materialProperties["needsLights"] = MaterialNeedsLights(material);
        materialProperties["lightsStateVersion"] = lightsStateVersion;

        if ((bool)materialProperties["needsLights"])
        {
            (uniforms["ambientLightColor"] as GLUniform)["value"] = lights.state["ambient"];
            (uniforms["lightProbe"] as GLUniform)["value"] = lights.state["probe"];
            (uniforms["directionalLights"] as GLUniform)["value"] = lights.state["directional"];
            (uniforms["directionalLightShadows"] as GLUniform)["value"] = lights.state["directionalShadow"];
            (uniforms["spotLights"] as GLUniform)["value"] = lights.state["spot"];
            (uniforms["spotLightShadows"] as GLUniform)["value"] = lights.state["spotShadow"];
            (uniforms["rectAreaLights"] as GLUniform)["value"] = lights.state["rectArea"];
            (uniforms["ltc_1"] as GLUniform)["value"] = lights.state["rectAreaLTC1"];
            (uniforms["ltc_2"] as GLUniform)["value"] = lights.state["rectAreaLTC2"];
            (uniforms["pointLights"] as GLUniform)["value"] = lights.state["point"];
            (uniforms["pointLightShadows"] as GLUniform)["value"] = lights.state["pointShadow"];
            (uniforms["hemisphereLights"] as GLUniform)["value"] = lights.state["hemi"];

            (uniforms["directionalShadowMap"] as GLUniform)["value"] = lights.state["directionalShadowMap"];
            (uniforms["directionalShadowMatrix"] as GLUniform)["value"] = lights.state["directionalShadowMatrix"];
            (uniforms["spotShadowMap"] as GLUniform)["value"] = lights.state["spotShadowMap"];
            (uniforms["spotShadowMatrix"] as GLUniform)["value"] = lights.state["spotShadowMatrix"];
            (uniforms["pointShadowMap"] as GLUniform)["value"] = lights.state["pointShadowMap"];
            (uniforms["pointShadowMatrix"] as GLUniform)["value"] = lights.state["pointShadowMatrix"];
        }

        var progUniforms = program.GetUniforms();
        var uniformsList = GLUniformsLoader.SeqWithValue(progUniforms.Seq, uniforms);
        materialProperties["currentProgram"] = program;
        materialProperties["uniformsList"] = uniformsList;

        return program;
    }

    private void InitTestures()
    {
    }

    private void UpdateCommonMaterialProperties(Material material, Hashtable parameters)
    {
        var materialProperties = Properties.Get(material);
        materialProperties["outputEncoding"] = parameters["outputEncoding"];
        materialProperties["instancing"] = parameters["instancing"];
        materialProperties["skinning"] = parameters["skinning"];
        materialProperties["numClippingPlanes"] = parameters["numClippingPlanes"];
        materialProperties["numIntersection"] = parameters["numClipIntersection"];
        materialProperties["vertexAlphas"] = parameters["vertexAlphas"];
    }

    private GLProgram SetProgram(Camera camera, Object3D scene, Material material, Object3D object3D)
    {
        //if(scene.isScene!=true) scene = emptyScene;

        textures.ResetTextureUnits();

        var fog = scene is Scene ? (scene as Scene).Fog : null;

        var environment = material is MeshStandardMaterial
            ? scene is Scene ? (scene as Scene).Environment : null
            : null;
        var encoding = _currentRenderTarget == null ? OutputEncoding : _currentRenderTarget.Texture.Encoding;
        var envMap = cubeMaps.Get(material.EnvMap != null ? material.EnvMap : environment);
        var geometry = object3D.Geometry;
        var isBufferGeometry = geometry is BufferGeometry;
        var containsColor = isBufferGeometry && (geometry as BufferGeometry).Attributes.ContainsKey("color");
        var colorAttribute = containsColor ? (geometry as BufferGeometry).Attributes["color"] : null;
        var ItemSize = containsColor && colorAttribute is BufferAttribute<float>
            ? (colorAttribute as BufferAttribute<float>).ItemSize
            : containsColor && colorAttribute is BufferAttribute<byte>
                ? (colorAttribute as BufferAttribute<byte>).ItemSize
                : 0;
        var vertexAlphas = material.VertexColors && geometry != null && isBufferGeometry && containsColor &&
                           ItemSize == 4;

        //var vertexAlphas = material.VertexColors == true && object3D.Geometry!=null && object3D.Geometry is BufferGeometry && (object3D.Geometry as BufferGeometry).Attributes.ContainsKey("color")&&((object3D.Geometry as BufferGeometry).Attributes["color"] as BufferAttribute<float>).ItemSize == 4;

        var materialProperties = Properties.Get(material);


        //&& ((int)materialProperties["numClippingPlanes"] != _clipping.numPlanes) || (materialProperties.ContainsKey("numIntersection") && (int)materialProperties["numIntersection"] != _clipping.numIntersection))

        var lights = _currentRenderState.State.Lights;

        if (_clippingEnabled)
            if (_localClippingEnabled || !camera.Equals(_currentCamera))
            {
                var useCache = camera == _currentCamera && material.Id == _currentMaterialId;

                // we might wnat to call this function with some ClippingGroup
                // object instead of the material, once it becomes feasible
                _clipping.SetState(material.ClippingPlanes, material.ClipIntersection, material.ClipShadows, camera,
                    materialProperties, useCache);
            }

        var needsProgramChange = false;

        var version = materialProperties.ContainsKey("version") ? (int)materialProperties["version"] : -1;

        if (version == material.Version)
        {
            if (materialProperties.ContainsKey("needsLights") && (bool)materialProperties["needsLights"])
            {
                if (materialProperties.ContainsKey("lightsStateVersion") &&
                    (int)materialProperties["lightsStateVersion"] != (int)lights.state["version"])
                    needsProgramChange = true;
            }
            else if (materialProperties.ContainsKey("outputEncoding") &&
                     (int)materialProperties["outputEncoding"] != encoding)
            {
                needsProgramChange = true;
            }
            else if (object3D is InstancedMesh && (bool)materialProperties["instancing"] == false)
            {
                needsProgramChange = true;
            }
            else if (object3D is not InstancedMesh && (bool)materialProperties["instancing"])
            {
                needsProgramChange = true;
            }
            else if (object3D is SkinnedMesh && (bool)materialProperties["skinning"] == false)
            {
                needsProgramChange = true;
            }
            else if (object3D is not SkinnedMesh && (bool)materialProperties["skinning"])
            {
                needsProgramChange = true;
            }
            else if (materialProperties.ContainsKey("envMap") && (Texture)materialProperties["envMap"] != envMap)
            {
                needsProgramChange = true;
            }
            else if (material.Fog && (Fog)materialProperties["fog"] != fog)
            {
                needsProgramChange = true;
            }


            else if ((materialProperties.ContainsKey("numClippingPlanes") &&
                      (int)materialProperties["numClippingPlanes"] != _clipping.numPlanes) ||
                     (materialProperties.ContainsKey("numIntersection") &&
                      (int)materialProperties["numIntersection"] != _clipping.numIntersection))
            {
                needsProgramChange = true;
            }
            else if (materialProperties.ContainsKey("vertexAlphas") &&
                     (bool)materialProperties["vertexAlphas"] != vertexAlphas)
            {
                needsProgramChange = true;
            }
        }
        else
        {
            needsProgramChange = true;
            materialProperties["version"] = material.Version;
        }

        var program = (GLProgram)materialProperties["currentProgram"];
        if (needsProgramChange) program = GetProgram(material, scene, object3D);

        var p_uniforms = program.GetUniforms();
        var m_uniforms = materialProperties["uniforms"] as GLUniforms;


        if ((bool)materialProperties["needsLights"])
        {
            (m_uniforms["ambientLightColor"] as GLUniform)["value"] = lights.state["ambient"];
            (m_uniforms["lightProbe"] as GLUniform)["value"] = lights.state["probe"];
            (m_uniforms["directionalLights"] as GLUniform)["value"] = lights.state["directional"];
            (m_uniforms["directionalLightShadows"] as GLUniform)["value"] = lights.state["directionalShadow"];
            (m_uniforms["spotLights"] as GLUniform)["value"] = lights.state["spot"];
            (m_uniforms["spotLightShadows"] as GLUniform)["value"] = lights.state["spotShadow"];
            (m_uniforms["rectAreaLights"] as GLUniform)["value"] = lights.state["rectArea"];
            (m_uniforms["ltc_1"] as GLUniform)["value"] = lights.state["rectAreaLTC1"];
            (m_uniforms["ltc_2"] as GLUniform)["value"] = lights.state["rectAreaLTC2"];
            (m_uniforms["pointLights"] as GLUniform)["value"] = lights.state["point"];
            (m_uniforms["pointLightShadows"] as GLUniform)["value"] = lights.state["pointShadow"];
            (m_uniforms["hemisphereLights"] as GLUniform)["value"] = lights.state["hemi"];

            (m_uniforms["directionalShadowMap"] as GLUniform)["value"] = lights.state["directionalShadowMap"];
            (m_uniforms["directionalShadowMatrix"] as GLUniform)["value"] = lights.state["directionalShadowMatrix"];
            (m_uniforms["spotShadowMap"] as GLUniform)["value"] = lights.state["spotShadowMap"];
            (m_uniforms["spotShadowMatrix"] as GLUniform)["value"] = lights.state["spotShadowMatrix"];
            (m_uniforms["pointShadowMap"] as GLUniform)["value"] = lights.state["pointShadowMap"];
            (m_uniforms["pointShadowMatrix"] as GLUniform)["value"] = lights.state["pointShadowMatrix"];
        }

        var refreshProgram = false;
        var refreshMaterial = false;
        var refreshLights = false;

        if (State.UseProgram(program.program))
        {
            refreshProgram = true;
            refreshMaterial = true;
            refreshLights = true;
        }

        if (material.Id != _currentMaterialId)
        {
            _currentMaterialId = material.Id;
            refreshMaterial = true;
        }

        // When resizeing, it always need to apply camera ProjectionMatrix
        p_uniforms.SetProjectionMatrix(camera.ProjectionMatrix);

        if (refreshProgram || (_currentCamera != null && !_currentCamera.Equals(camera)))
        {
            if (Capabilities.logarithmicDepthBuffer)
                p_uniforms.SetValue("logDepthBufFC",
                    2.0f / (Math.Log(camera.Far + 1.0) / Math.Log(2)));

            if (_currentCamera == null || !_currentCamera.Equals(camera))
            {
                _currentCamera = camera;

                refreshMaterial = true;
                refreshLights = true;
            }

            if (material is ShaderMaterial ||
                material is MeshPhongMaterial ||
                material is MeshToonMaterial ||
                material is MeshStandardMaterial || material.EnvMap != null)
            {
                var uCamPos = p_uniforms.ContainsKey("cameraPosition")
                    ? p_uniforms["cameraPosition"] as SingleUniform
                    : null;

                if (uCamPos != null) uCamPos.SetValue(Vector3.Zero().SetFromMatrixPosition(camera.MatrixWorld));
            }

            if (material is MeshPhongMaterial ||
                material is MeshToonMaterial ||
                material is MeshLambertMaterial ||
                material is MeshBasicMaterial ||
                material is MeshStandardMaterial ||
                material is ShaderMaterial)
                p_uniforms.SetValue("isOrthographic", camera is OrthographicCamera);

            if (material is MeshPhongMaterial ||
                material is MeshToonMaterial ||
                material is MeshLambertMaterial ||
                material is MeshBasicMaterial ||
                material is MeshStandardMaterial ||
                material is ShaderMaterial ||
                material.Skinning)
                //if ( program.NumMultiviewViews > 0 ) {
                // Multiview.UpdateCameraViewMatricesUniform( camera, p_uniforms );
                //} else {
                p_uniforms.SetValue("viewMatrix", camera.MatrixWorldInverse);
            //}
        }

        //skinning uniforms must be set even if material didn't change auto-setting of texture unit for bone texture must go before other textures
        // not sure why, but otherwise weird things happen

        if (object3D is SkinnedMesh)
        {
            p_uniforms.SetOptional(object3D, "bindMatrix");
            p_uniforms.SetOptional(object3D, "bindMatrixInverse");

            var skeleton = (object3D as SkinnedMesh).Skeleton;

            if (skeleton != null)
            {
                var bones = skeleton.Bones;

                if (Capabilities.floatVertexTextures)
                {
                    if (skeleton.BoneTexture != null) skeleton.ComputeBoneTexture();

                    p_uniforms.SetValue("boneTexture", skeleton.BoneTexture, textures);
                    p_uniforms.SetValue("boneTextureSize", skeleton.BoneTextureSize);
                }
                else
                {
                    p_uniforms.SetOptional(skeleton, "boneMatrices");
                }
            }
        }

        if (refreshMaterial ||
            (materialProperties.ContainsKey("receiveShadow") && (bool)materialProperties["receiveShadow"]) !=
            object3D.ReceiveShadow)
        {
            materialProperties["receiveShadow"] = object3D.ReceiveShadow;
            p_uniforms.SetValue("receiveShadow", object3D.ReceiveShadow);
        }

        if (refreshMaterial)
        {
            p_uniforms.SetValue("toneMappingExposure", ToneMappingExposure);
            //p_uniforms.SetValue("toneMappingWhitePoint",ToneMappingWhitePoint );

            if (materialProperties.ContainsKey("needsLights") && (bool)materialProperties["needsLights"])
                // the current material requires lighting info
                // note: all lighting uniforms are always set correctly
                // they simply reference the renderer's state for their
                // values
                //
                // use the current material's .needsUpdate flags to set
                // the GL state when required
                MarkUniformsLightsNeedsUpdate(m_uniforms, refreshLights);

            // refresh uniforms common to several materials

            if (fog != null && material.Fog) materials.RefreshFogUniforms(m_uniforms, fog);

            materials.RefreshMaterialUniforms(m_uniforms, material, _pixelRatio, Height,
                _transmissionRenderTarget);

            if (ShaderLib.UniformsLib.ContainsKey("ltc_1"))
                (m_uniforms["ltc_1"] as GLUniform)["value"] = ShaderLib.UniformsLib["LTC_1"];
            if (ShaderLib.UniformsLib.ContainsKey("ltc_2"))
                (m_uniforms["ltc_2"] as GLUniform)["value"] = ShaderLib.UniformsLib["LTC_2"];

            //if (material is MeshLambertMaterial)
            //    Debug.WriteLine(material.type);
            GLUniformsLoader.Upload((List<GLUniform>)materialProperties["uniformsList"], m_uniforms, textures);
        }

        if (material is ShaderMaterial && (material as ShaderMaterial).UniformsNeedUpdate)
        {
            GLUniformsLoader.Upload((List<GLUniform>)materialProperties["uniformsList"], m_uniforms, textures);
            (material as ShaderMaterial).UniformsNeedUpdate = false;
        }

        if (material is SpriteMaterial) p_uniforms.SetValue("center", (object3D as Sprite).Center);


        p_uniforms.SetValue("modelViewMatrix", object3D.ModelViewMatrix);
        p_uniforms.SetValue("normalMatrix", object3D.NormalMatrix);
        p_uniforms.SetValue("modelMatrix", object3D.MatrixWorld);

        return program;
    }

    //TODO: Hashtable -> Dictionary
    private void MarkUniformsLightsNeedsUpdate(GLUniforms uniforms, object value)
    {
        (uniforms["ambientLightColor"] as GLUniform)["needsUpdate"] = value;
        (uniforms["lightProbe"] as GLUniform)["needsUpdate"] = value;

        (uniforms["directionalLights"] as GLUniform)["needsUpdate"] = value;
        (uniforms["directionalLightShadows"] as GLUniform)["needsUpdate"] = value;
        (uniforms["pointLights"] as GLUniform)["needsUpdate"] = value;
        (uniforms["pointLightShadows"] as GLUniform)["needsUpdate"] = value;
        (uniforms["spotLights"] as GLUniform)["needsUpdate"] = value;
        (uniforms["spotLightShadows"] as GLUniform)["needsUpdate"] = value;
        (uniforms["rectAreaLights"] as GLUniform)["needsUpdate"] = value;
        (uniforms["hemisphereLights"] as GLUniform)["needsUpdate"] = value;
    }

    private bool MaterialNeedsLights(Material material)
    {
        return material is MeshLambertMaterial || material is MeshToonMaterial || material is MeshPhongMaterial ||
               material is MeshStandardMaterial || material is ShadowMaterial
               || (material is ShaderMaterial && (material as ShaderMaterial).Lights);
    }


    public int GetActiveCubeFace()
    {
        return _currentActiveCubeFace;
    }

    public int GetActiveMipmapLevel()
    {
        return _currentActiveMipmapLevel;
    }

    public override void Dispose()
    {
        Properties.Dispose();
        State.Dispose();
        GL.Disable(EnableCap.Blend);
        GL.Disable(EnableCap.CullFace);
        GL.Disable(EnableCap.DepthTest);
        GL.Disable(EnableCap.PolygonOffsetFill);
        GL.Disable(EnableCap.ScissorTest);
        GL.Disable(EnableCap.StencilTest);
        GL.Disable(EnableCap.SampleAlphaToCoverage);

        GL.BlendEquation(BlendEquationMode.FuncAdd);
        GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.Zero);
        GL.BlendFuncSeparate(BlendingFactorSrc.One, BlendingFactorDest.Zero, BlendingFactorSrc.One,
            BlendingFactorDest.Zero);

        GL.ColorMask(true, true, true, true);
        GL.ClearColor(0, 0, 0, 0);

        GL.DepthMask(true);
        GL.DepthFunc(DepthFunction.Less);
        GL.ClearDepth(1);

        GL.StencilMask(0xffffffff);
        GL.StencilFunc(StencilFunction.Always, 0, 0xffffffff);
        GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
        GL.ClearStencil(0);

        GL.CullFace(CullFaceMode.Back);
        GL.FrontFace(FrontFaceDirection.Ccw);

        GL.PolygonOffset(0, 0);

        GL.ActiveTexture(TextureUnit.Texture0);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        if (Capabilities.IsGL2)
        {
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
        }

        GL.UseProgram(0);

        GL.LineWidth(1);

        GL.Scissor(0, 0, Width, Height);
        GL.Viewport(0, 0, Width, Height);


        base.Dispose();
    }

    #region internal properties

    private int? _framebuffer = null;

    private int _currentActiveCubeFace;

    private GLRenderTarget _currentRenderTarget;

    private int? _currentFramebuffer;

    private int _currentMaterialId = -1;

    private Camera _currentCamera;

    private Camera _currentArrayCamera;

    private Vector4 _currentViewport = Vector4.Zero();

    private Vector4 _currentScissor = Vector4.Zero();

    private bool? _currentScissorTest;


    private int _pixelRatio = 1;

    private Vector4 _viewport = Vector4.Zero();

    private Vector4 _scissor = Vector4.Zero();

    private bool _scissorTest = false;

    // frustum

    private Frustum _frustum = new();

    // clipping 

    private GLClipping _clipping = new();

    private bool _clippingEnabled;

    private bool _localClippingEnabled;

    private GLRenderTarget _transmissionRenderTarget;
    // camera matrices cache

    private Matrix4 _projScreenMatrix = Matrix4.Identity();

    private Vector3 _vector3 = Vector3.Zero();

    public int Width;
    public int Height;

    public float AspectRatio
    {
        get
        {
            if (Height == 0) return 1;
            return (float)Width / Height;
        }
    }

    #endregion

    #region constructor

    public GLRenderer(GLInfo info)
    {
        Info = info;
    }

    public GLRenderer(IGraphicsContext context, int width, int height, GLInfo info)
    {
        Context = context;
        _viewport = new Vector4(0, 0, width, height);
        Width = width;
        Height = height;
        Info = info;
        Init();
    }

    #endregion

    #region Private Renderring

    #region Buffer deallocation

    private void DeallocateMaterial(Material material)
    {
        //if (!this.Context.IsDisposed && this.Context.IsCurrent)
        //{

        ReleaseMaterialProgramReference(material);

        Properties.Remove(material);

        //}
    }

    private void ReleaseMaterialProgramReference(Material material)
    {
        var programInfo = Properties.Get(material)["program"];

        //material.Program = null;

        if (programInfo != null) programCache.ReleaseProgram((GLProgram)programInfo);
    }

    #endregion

    #region Buffer Rendering

    private void RenderObjectImmediate(Object3D object3D, GLProgram program)
    {
        RenderBufferImmediate(object3D, program);
    }

    private void RenderBufferImmediate(Object3D object3D, GLProgram program)
    {
        State.InitAttributes();
        /*
        var buffers = properties.Get(object3D);

        if (object3D.hasPositions && !buffers.position) buffers.position = _gl.createBuffer();
        if (object3D.hasNormals && !buffers.normal) buffers.normal = _gl.createBuffer();
        if (object3D.hasUvs && !buffers.uv) buffers.uv = _gl.createBuffer();
        if (object3D.hasColors && !buffers.color) buffers.color = _gl.createBuffer();

        var programAttributes = program.getAttributes();

        if (object3D.hasPositions)
        {

            _gl.bindBuffer(_gl.ARRAY_BUFFER, buffers.position);
            _gl.bufferData(_gl.ARRAY_BUFFER, object3D.positionArray, _gl.DYNAMIC_DRAW);

            state.enableAttribute(programAttributes.position);
            _gl.vertexAttribPointer(programAttributes.position, 3, _gl.FLOAT, false, 0, 0);

        }

        if (object3D.hasNormals)
        {

            _gl.bindBuffer(_gl.ARRAY_BUFFER, buffers.normal);
            _gl.bufferData(_gl.ARRAY_BUFFER, object3D.normalArray, _gl.DYNAMIC_DRAW);

            state.enableAttribute(programAttributes.normal);
            _gl.vertexAttribPointer(programAttributes.normal, 3, _gl.FLOAT, false, 0, 0);

        }

        if (object3D.hasUvs)
        {

            _gl.bindBuffer(_gl.ARRAY_BUFFER, buffers.uv);
            _gl.bufferData(_gl.ARRAY_BUFFER, object3D.uvArray, _gl.DYNAMIC_DRAW);

            state.enableAttribute(programAttributes.uv);
            _gl.vertexAttribPointer(programAttributes.uv, 2, _gl.FLOAT, false, 0, 0);

        }

        if (object3D.hasColors)
        {

            _gl.bindBuffer(_gl.ARRAY_BUFFER, buffers.color);
            _gl.bufferData(_gl.ARRAY_BUFFER, object3D.colorArray, _gl.DYNAMIC_DRAW);

            state.enableAttribute(programAttributes.color);
            _gl.vertexAttribPointer(programAttributes.color, 3, _gl.FLOAT, false, 0, 0);

        }

        state.disableUnusedAttributes();

        _gl.drawArrays(_gl.TRIANGLES, 0, object3D.count);

        object3D.count = 0;
        */
    }

    public void RenderBufferDirect(Camera camera, Object3D scene, Geometry geometry, Material material,
        Object3D object3D, DrawRange? group)
    {
        if (scene == null) scene = emptyScene;

        var frontFaceCW = object3D is Mesh && object3D.MatrixWorld.Determinant() < 0;

        var program = SetProgram(camera, scene, material, object3D);

        State.SetMaterial(material, frontFaceCW);

        var index = (geometry as BufferGeometry).Index;
        BufferAttribute<float> position = null;
        if ((geometry as BufferGeometry).Attributes.ContainsKey("position"))
        {
            var bufferGeom = geometry as BufferGeometry;
            position = (BufferAttribute<float>)bufferGeom.Attributes["position"];
        }

        //

        if (index != null && index.count == 0) return;
        //if (position != null || position.count === 0) return;
        if (position == null) return;

        //

        var rangeFactor = 1;

        if (material.Wireframe)
        {
            index = geometries.GetWireframeAttribute<int>(geometry);
            rangeFactor = 2;
        }

        if (material.MorphTargets || material.MorphNormals)
            morphtargets.Update(object3D, geometry as BufferGeometry, material, program);

        bindingStates.Setup(object3D, material, program, geometry, index);

        BufferType attribute = null;
        var renderer = bufferRenderer;

        if (index != null)
        {
            attribute = attributes.Get<int>(index);

            renderer = indexedBufferRenderer;
            (renderer as GLIndexedBufferRenderer).SetIndex(attribute);
        }
        //if (updateBuffers)
        //{

        //    SetupVertexAttributes(object3D, geometry, material, program);

        //    if (index != null)
        //    {

        //        GL.BindBuffer(BufferTarget.ElementArrayBuffer, attribute.buffer);

        //    }

        //}

        //

        var dataCount = index != null ? index.count :
            position is InterleavedBufferAttribute<float> ? (position as InterleavedBufferAttribute<float>).count :
            position.count;

        var rangeStart = (geometry as BufferGeometry).DrawRange.Start * rangeFactor;
        var rangeCount = (geometry as BufferGeometry).DrawRange.Count * rangeFactor;

        var groupStart = group != null ? group.Value.Start * rangeFactor : 0;
        var groupCount = group != null ? group.Value.Count * rangeFactor : float.PositiveInfinity;

        var drawStart = Math.Max(rangeStart, groupStart);
        var drawEnd = Math.Min(Math.Min(dataCount, rangeStart + rangeCount),
            groupStart + groupCount) - 1;

        var drawCount = Math.Max(0, drawEnd - drawStart + 1);

        if (drawCount == 0) return;

        //

        if (object3D is Mesh)
        {
            if (material.Wireframe)
            {
                State.SetLineWidth(material.WireframeLineWidth * GetTargetPixelRatio());
                renderer.SetMode(PrimitiveType.Lines);
            }
            else
            {
                renderer.SetMode(PrimitiveType.Triangles);
            }
        }
        else if (object3D is Line)
        {
            float lineWidth;
            if (material is LineBasicMaterial)
                lineWidth = (material as LineBasicMaterial).LineWidth;
            else
                lineWidth = 1f; // Not using Line*Material

            State.SetLineWidth(lineWidth * GetTargetPixelRatio());

            if (object3D is LineSegments)
                renderer.SetMode(PrimitiveType.Lines);
            else if (object3D is LineLoop)
                renderer.SetMode(PrimitiveType.LineLoop);
            else
                renderer.SetMode(PrimitiveType.LineStrip);
        }
        else if (object3D is Points)
        {
            OpenTK.Graphics.OpenGL.GL.Enable(OpenTK.Graphics.OpenGL.EnableCap.PointSprite);
            OpenTK.Graphics.OpenGL.GL.Enable(OpenTK.Graphics.OpenGL.EnableCap.ProgramPointSize);
            renderer.SetMode(PrimitiveType.Points);
        }
        else if (object3D is Sprite)
        {
            renderer.SetMode(PrimitiveType.Triangles);
        }

        if (object3D is InstancedMesh)
        {
            renderer.RenderInstances(geometry, (int)drawStart, (int)drawCount,
                (object3D as InstancedMesh).InstanceCount);
        }
        else if (geometry is InstancedBufferGeometry)
        {
            var instanceCount = Math.Min((geometry as InstancedBufferGeometry).InstanceCount,
                (geometry as InstancedBufferGeometry).MaxInstanceCount.Value);
            renderer.RenderInstances(geometry, (int)drawStart, (int)drawCount, instanceCount);
        }
        else
        {
            renderer.Render((int)drawStart, (int)drawCount);
        }
    }

    #endregion


    // this function is called by Render()
    //private void RenderSceneList(RenderInfo renderInfo)
    public void Render(Object3D scene, Camera camera)
    {
        //Scene scene = renderInfo.Scene;
        //Camera camera = renderInfo.Camera;

        if (scene == null || camera == null)
            throw new Exception("THREE.Renderers.RenderSceneList : scene or camera is null");

        if (!(camera is Camera))
            throw new Exception(
                "THREE.Renderers.RenderSceneList : camera is not an instance of THREE.Cameras.Camera");

        bindingStates.ResetDefaultState();
        _currentMaterialId = -1;
        _currentCamera = null;

        GLRenderTarget renderTarget = null;

        var forceClear = false;

        //update scene graph
        if (scene is Scene && (scene as Scene).AutoUpdate) scene.UpdateMatrixWorld();

        //update camera matrices and frustum

        if (camera.Parent == null) camera.UpdateMatrixWorld();

        //if (xr.enabled === true && xr.isPresenting === true)
        //{

        //    camera = xr.getCamera(camera);

        //}

        if (scene.OnBeforeRender != null)
            scene.OnBeforeRender(this, scene, camera, null, null, null,
                renderTarget != null ? renderTarget : _currentRenderTarget);

        _currentRenderState = renderStates.Get(scene, _renderStateStack.Count);
        _currentRenderState.Init();
        _renderStateStack.Push(_currentRenderState);


        _projScreenMatrix = camera.ProjectionMatrix * camera.MatrixWorldInverse;
        _frustum.SetFromProjectionMatrix(_projScreenMatrix);

        _localClippingEnabled = LocalClippingEnabled;
        _clippingEnabled = _clipping.Init(ClippingPlanes, _localClippingEnabled, camera);

        _currentRenderList = renderLists.Get(scene, _renderListStack.Count);
        _currentRenderList.Init();
        _renderListStack.Push(_currentRenderList);

        ProjectObject(scene, camera, 0, SortObjects);

        _currentRenderList.Finish();

        if (SortObjects) _currentRenderList.Sort();

        if (_clippingEnabled) _clipping.BeginShadows();

        var shadowsArray = _currentRenderState.State.ShadowsArray;

        ShadowMap.Render(shadowsArray, scene, camera);

        _currentRenderState.SetupLights();
        _currentRenderState.SetupLightsView(camera);

        if (_clippingEnabled) _clipping.EndShadows();

        //

        if (Info.AutoReset) Info.Reset();

        if (renderTarget != null) SetRenderTarget(renderTarget);

        //this.AutoClear = scene.ClearBeforeRender;

        background.Render(_currentRenderList, scene, camera, forceClear);

        // render scene

        var opaqueObjects = _currentRenderList.Opaque;
        var transmissionObjects = _currentRenderList.Transmissive;
        var transparentObjects = _currentRenderList.Transparent;

        if (opaqueObjects.Count > 0) RenderObjects(opaqueObjects, scene, camera);
        if (transmissionObjects.Count > 0)
            RenderTransmissiveObjects(opaqueObjects, transmissionObjects, scene, camera);
        if (transparentObjects.Count > 0) RenderObjects(transparentObjects, scene, camera);

        if (scene.OnAfterRender != null) scene.OnAfterRender(this, scene, camera);

        if (_currentRenderTarget != null)
        {
            // Generate mipmap if we're using any kind of mipmap filtering

            textures.UpdateRenderTargetMipmap(_currentRenderTarget);

            // resolve multisample renderbuffers to a single-sample texture if necessary

            textures.UpdateMultisampleRenderTarget(_currentRenderTarget);
        }

        // Ensure depth buffer writing is enabled so it can be cleared on next render

        State.buffers.depth.SetTest(true);
        State.buffers.depth.SetMask(true);
        State.buffers.color.SetMask(true);

        State.SetPolygonOffset(false);

        //bindingStates.ResetDefaultState();
        State.currentProgram = -1;
        bindingStates.Reset();

        _currentMaterialId = -1;
        _currentCamera = null;

        _renderStateStack.Pop();
        if (_renderStateStack.Count > 0)
            //CurrentRenderState = renderStateStack.ElementAt(renderStateStack.Count-1);
            _currentRenderState = _renderStateStack.Last();
        else
            _currentRenderState = null;

        _renderListStack.Pop();
        if (_renderListStack.Count > 0)
            //CurrentRenderList = renderListStack.ElementAt(renderListStack.Count - 1);
            _currentRenderList = _renderListStack.Last();
        else
            _currentRenderList = null;
    }

    private void ProjectObject(Object3D object3D, Camera camera, int groupOrder, bool sortObjects)
    {
        if (object3D.Visible == false) return;

        var visible = object3D.Layers.Test(camera.Layers);

        if (visible)
        {
            if (object3D.IsGroup)
            {
                groupOrder = object3D.RenderOrder;
            }
            else if (object3D is LOD)
            {
                if ((object3D as LOD).AutoUpdate)
                    (object3D as LOD).Update(camera);
            }
            else if (object3D is Light)
            {
                _currentRenderState.PushLight((Light)object3D);

                if (object3D.CastShadow) _currentRenderState.PushShadow((Light)object3D);
            }
            else if (object3D is Sprite)
            {
                if (!object3D.FrustumCulled || _frustum.IntersectsSprite(object3D as Sprite))
                {
                    if (sortObjects)
                        _vector3.SetFromMatrixPosition(object3D.MatrixWorld).ApplyMatrix4(_projScreenMatrix);

                    var geometry = objects.Update(object3D);
                    var material = object3D.Material;

                    if (material.Visible)
                        _currentRenderList.Push(object3D, geometry, material, groupOrder, _vector3.Z, null);
                }
            }
            else if (object3D is ImmediateRenderObject)
            {
                if (sortObjects) _vector3.SetFromMatrixPosition(object3D.MatrixWorld).ApplyMatrix4(_projScreenMatrix);

                _currentRenderList.Push(object3D, null, object3D.Material, groupOrder, _vector3.Z, null);
            }
            else if (object3D is Mesh || object3D is Line || object3D is Points)
            {
                if (object3D is SkinnedMesh)
                    //update skeleton only once in a frame
                    if ((object3D as SkinnedMesh).Skeleton.Frame != Info.render.Frame)
                    {
                        (object3D as SkinnedMesh).Skeleton.Update();
                        (object3D as SkinnedMesh).Skeleton.Frame = Info.render.Frame;
                    }

                if (!object3D.FrustumCulled || _frustum.IntersectsObject(object3D))
                {
                    if (sortObjects)
                        _vector3.SetFromMatrixPosition(object3D.MatrixWorld).ApplyMatrix4(_projScreenMatrix);

                    var geometry = objects.Update(object3D);
                    var material = object3D.Material;
                    if (object3D.Materials.Count > 1)
                    {
                        var materials = object3D.Materials;
                        var groups = geometry.Groups;

                        for (var i = 0; i < groups.Count; i++)
                        {
                            var group = groups[i];
                            var groupMaterial = materials[group.MaterialIndex];

                            if (groupMaterial != null && groupMaterial.Visible)
                                _currentRenderList.Push(object3D, geometry, groupMaterial,
                                    groupOrder, _vector3.Z, group);
                        }
                    }
                    else if (material.Visible)
                    {
                        _currentRenderList.Push(object3D, geometry, material, groupOrder,
                            _vector3.Z, null);
                    }
                }
            }
        }

        var children = object3D.Children;

        for (var i = 0; i < children.Count; i++) ProjectObject(children[i], camera, groupOrder, sortObjects);
    }

    private void RenderTransmissiveObjects(List<RenderItem> opaqueObjects, List<RenderItem> transmissiveObjects,
        Object3D scene, Camera camera)
    {
        if (_transmissionRenderTarget == null)
            _transmissionRenderTarget = new GLRenderTarget(1024, 1024, new Hashtable
            {
                { "generateMipmaps", true },
                { "minFilter", Constants.LinearMipmapLinearFilter },
                { "magFilter", Constants.NearestFilter },
                { "wrapS", Constants.ClampToEdgeWrapping },
                { "wrapT", Constants.ClampToEdgeWrapping }
            });

        var currentRenderTarget = GetRenderTarget();
        SetRenderTarget(_transmissionRenderTarget);
        Clear();

        RenderObjects(opaqueObjects, scene, camera);

        textures.UpdateRenderTargetMipmap(_transmissionRenderTarget);

        SetRenderTarget(currentRenderTarget);

        RenderObjects(transmissiveObjects, scene, camera);
    }

    private void RenderObjects(List<RenderItem> renderList, Object3D scene, Camera camera)
    {
        var overrideMaterial = scene is Scene ? (scene as Scene).OverrideMaterial : null;
        for (var i = 0; i < renderList.Count; i++)
        {
            var renderItem = renderList[i];

            var object3D = renderItem.Object3D;
            var geometry = renderItem.Geometry;
            var material = overrideMaterial == null ? renderItem.Material : overrideMaterial;
            var group = renderItem.Group;

            if (camera is ArrayCamera)
            {
                _currentArrayCamera = camera;

                // if(vr.)
                //{

                //}
                //else 
                //{
                var cameras = (camera as ArrayCamera).Cameras;
                for (var j = 0; j < cameras.Count; j++)
                {
                    var camera2 = cameras[j];

                    if (object3D.Layers.Test(camera2.Layers))
                    {
                        State.Viewport(_currentViewport.Copy(camera2.Viewport));

                        _currentRenderState.SetupLightsView(camera2);
                        RenderObject(object3D, scene, camera2, geometry, material, group);
                    }
                }

                // }
            }
            else
            {
                _currentArrayCamera = null;

                RenderObject(object3D, scene, camera, geometry, material, group);
            }
        }
    }

    private void RenderObject(Object3D object3D, Object3D scene, Camera camera, Geometry geometry,
        Material material, DrawRange? group)
    {
        //TODO:
        if (object3D.OnBeforeRender != null)
            object3D.OnBeforeRender(this, scene, camera, geometry, material, group, null);

        //CurrentRenderState = renderStates.Get(scene, _currentArrayCamera != null ? _currentArrayCamera : camera);

        object3D.ModelViewMatrix = camera.MatrixWorldInverse * object3D.MatrixWorld;
        object3D.NormalMatrix.GetNormalMatrix(object3D.ModelViewMatrix);

        if (object3D is ImmediateRenderObject)
        {
            var program = SetProgram(camera, scene, material, object3D);

            State.SetMaterial(material);

            RenderObjectImmediate(object3D, program);
        }
        else
        {
            RenderBufferDirect(camera, scene, geometry, material, object3D, group);
        }

        //TODO:
        //object3D.OnAfterRender()
        //CurrentRenderState = renderStates.Get(scene, _currentArrayCamera != null ? _currentArrayCamera : camera);
        if (object3D.OnAfterRender != null)
            object3D.OnAfterRender(this, scene, camera);
    }

    #endregion

    #region public Render function

    public virtual void Init()
    {
        debug.Add("checkShaderErrors", true);

        InitGLContext();

        //VR omitted

        Multiview = new GLMultiview(this);

        ShadowMap = new GLShadowMap(this, objects, Capabilities.maxTextureSize);
    }

    public Vector4 GetCurrentViewport(Vector4 target)
    {
        target.Copy(_currentViewport);

        return target;
    }

    public virtual void SetGraphicsContext(IGraphicsContext context, int width, int height)
    {
        Context = context;
        Resize(width, height);
    }

    public virtual void Resize(int width, int height)
    {
        //foreach (string key in sceneList.Keys)
        //{
        //    RenderInfo info = sceneList[key];
        //    info.Camera.MatrixWorldNeedsUpdate = true;
        //}
        Width = width;
        Height = height;

        _viewport.Set(0, 0, width, height);
        _currentViewport.Set(0, 0, width, height);
    }

    #endregion
}