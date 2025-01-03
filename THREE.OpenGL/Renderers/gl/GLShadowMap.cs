﻿using System.Collections;
using System.Diagnostics;

namespace THREE;

[Serializable]
public class GLShadowMap
{
    private Dictionary<int, Material> _depthMaterials = new();

    private Dictionary<int, Material> _distanceMaterial = new();

    private Frustum _frustum = new();

    private Hashtable _materialCache = new();

    private GLObjects _objects;

    private GLRenderer _renderer;

    private Vector2 _shadowMapSize = Vector2.Zero();

    private Vector4 _viewport = Vector4.Zero();

    private Vector2 _viewportSize = Vector2.Zero();

    public bool AutoUpdate = true;
    public bool Enabled = false;
    private Mesh fullScreenMesh;

    private GetMaterialVariant GetDepthMaterialVariant =
        delegate(int useMorphing, int useSkinning, int useInstancing, Dictionary<int, Material> depthMaterials)
        {
            var index = (useMorphing << 0) | (useSkinning << 1) | (useInstancing << 2);

            Material material = null;

            if (!depthMaterials.TryGetValue(index, out material))
            {
                material = new MeshDepthMaterial
                {
                    DepthPacking = Constants.RGBADepthPacking, MorphTargets = Convert.ToBoolean(useMorphing),
                    Skinning = Convert.ToBoolean(useSkinning)
                };
                depthMaterials[index] = material;
            }

            return material;
        };

    private GetMaterialVariant GetDistanceMaterialVariant =
        delegate(int useMorphing, int useSkinning, int useInstancing, Dictionary<int, Material> distanceMaterials)
        {
            var index = (useMorphing << 0) | (useSkinning << 1) | (useInstancing << 2);

            Material material = null;

            if (!distanceMaterials.TryGetValue(index, out material))
            {
                material = new MeshDistanceMaterial
                    { MorphTargets = Convert.ToBoolean(useMorphing), Skinning = Convert.ToBoolean(useSkinning) };
                distanceMaterials[index] = material;
            }

            return material;
        };

    private int maxTextureSize;

    public bool needsUpdate;

    public int ShadowMapType;

    private ShaderMaterial shadowMaterialHorizontal;

    private ShaderMaterial shadowMaterialVertical;

    private List<int> shadowSide = new() { Constants.BackSide, Constants.FrontSide, Constants.DoubleSide };

    public int Type = Constants.PCFShadowMap;

    private string vsm_frag =
        @"uniform sampler2D shadow_pass;
              uniform vec2 resolution;
              uniform float radius;

              #include <packing>

              void main() {
                float mean = 0.0;
                float squared_mean = 0.0;

              	// This seems totally useless but it's a crazy work around for a Adreno compiler bug
              	float depth = unpackRGBAToDepth( texture2D( shadow_pass, ( gl_FragCoord.xy  ) / resolution ) );

                for ( float i = -1.0; i < 1.0 ; i += SAMPLE_RATE) {

                  #ifdef HORIZONAL_PASS

                    vec2 distribution = unpack2HalfToRGBA ( texture2D( shadow_pass, ( gl_FragCoord.xy + vec2( i, 0.0 ) * radius ) / resolution ) );
                    mean += distribution.x;
                    squared_mean += distribution.y * distribution.y + distribution.x * distribution.x;

                  #else

                    float depth = unpackRGBAToDepth( texture2D( shadow_pass, ( gl_FragCoord.xy + vec2( 0.0,  i )  * radius ) / resolution ) );
                    mean += depth;
                    squared_mean += depth * depth;

                  #endif

                }

                mean = mean * HALF_SAMPLE_RATE;
                squared_mean = squared_mean * HALF_SAMPLE_RATE;

                float std_dev = sqrt( squared_mean - mean * mean );

                gl_FragColor = pack2HalfToRGBA( vec2( mean, std_dev ) );
              }";

    private string vsm_vert =
        @"void main() {

	                gl_Position = vec4( position, 1.0 );

            }";

    public GLShadowMap(GLRenderer renderer, GLObjects objects, int maxTextureSize)
    {
        _renderer = renderer;

        this.maxTextureSize = maxTextureSize;

        _objects = objects;

        shadowMaterialVertical = new ShaderMaterial();
        shadowMaterialVertical.Defines.Add("SAMPLE_RATE", 2.0f / 8.0f);
        shadowMaterialVertical.Defines.Add("HALF_SAMPLE_RATE", 1.0f / 8.0f);

        shadowMaterialVertical.Uniforms.Add("shadow_pass", new GLUniform { { "value", null } });
        shadowMaterialVertical.Uniforms.Add("resolution", new GLUniform { { "value", Vector2.Zero() } });
        shadowMaterialVertical.Uniforms.Add("radius", new GLUniform { { "value", 4.0f } });

        shadowMaterialVertical.VertexShader = vsm_vert;
        shadowMaterialVertical.FragmentShader = vsm_vert;
        shadowMaterialHorizontal = (ShaderMaterial)shadowMaterialVertical.Clone();
        shadowMaterialHorizontal.Defines.Add("HORIZONAL_PASS", 1);
        Type = Constants.PCFShadowMap;

        var fullScreenTri = new BufferGeometry();
        var attribute = new BufferAttribute<float>(new[] { -1, -1, 0.5f, 3, -1, 0.5f, -1, 3, 0.5f }, 3);

        fullScreenTri.SetAttribute("position", attribute);

        fullScreenMesh = new Mesh(fullScreenTri, shadowMaterialVertical);
    }

    public void Render(List<Light> lights, Object3D scene, Camera camera)
    {
        if (Enabled == false) return;
        if (AutoUpdate == false && needsUpdate == false) return;

        if (lights.Count == 0) return;

        var currentRenderTarget = _renderer.GetRenderTarget();
        var activeCubeFace = _renderer.GetActiveCubeFace();
        var activeMipmapLevel = _renderer.GetActiveMipmapLevel();

        var _state = _renderer.State;

        // Set GL State for depth map
        _state.SetBlending(Constants.NoBlending);
        _state.buffers.color.SetClear(1, 1, 1, 1);
        _state.buffers.depth.SetTest(true);
        _state.SetScissorTest(false);

        // render depth map

        for (var i = 0; i < lights.Count; i++)
        {
            var light = lights[i];
            var shadow = light.Shadow;

            if (shadow == null)
            {
                Trace.TraceWarning("THREE.Renderers.gl.GLShadowMap:{0} has no shadow.", light.type);
                continue;
            }

            if (shadow.AutoUpdate == false && shadow.NeedsUpdate == false) continue;

            _shadowMapSize.Copy(shadow.MapSize);

            var shadowFrameExtents = shadow.GetFrameExtents();

            _shadowMapSize.Multiply(shadowFrameExtents);

            _viewportSize.Copy(shadow.MapSize);

            if (_shadowMapSize.X > maxTextureSize || _shadowMapSize.Y > maxTextureSize)
            {
                Trace.TraceWarning("THREE.Renderers.gl.GLShadowMap:{0} has shadow exceeding max texture size, reducing",
                    light.type);

                if (_shadowMapSize.X > maxTextureSize)
                {
                    _viewportSize.X = (float)Math.Floor(maxTextureSize / shadowFrameExtents.X);
                    _shadowMapSize.X = _viewportSize.X * shadowFrameExtents.X;
                    shadow.MapSize.X = _viewportSize.X;
                }

                if (_shadowMapSize.Y > maxTextureSize)
                {
                    _viewportSize.Y = (float)Math.Floor(maxTextureSize / shadowFrameExtents.Y);
                    _shadowMapSize.Y = _viewportSize.Y * shadowFrameExtents.Y;
                    shadow.MapSize.Y = _viewportSize.Y;
                }
            }

            if (shadow.Map == null && !(shadow is PointLightShadow) && Type == Constants.VSMShadowMap)
            {
                var pars = new Hashtable
                {
                    { "minFilter", Constants.LinearFilter }, { "magFilter", Constants.LinearFilter },
                    { "format", Constants.RGBAFormat }
                };

                shadow.Map = new GLRenderTarget((int)_shadowMapSize.X, (int)_shadowMapSize.Y, pars);

                shadow.Map.Texture.Name = light.Name + ".shadowMap";

                shadow.MapPass = new GLRenderTarget((int)_shadowMapSize.X, (int)_shadowMapSize.Y, pars);

                shadow.Camera.UpdateProjectionMatrix();
            }

            if (shadow.Map == null)
            {
                var pars = new Hashtable
                {
                    { "minFilter", Constants.NearestFilter }, { "magFilter", Constants.NearestFilter },
                    { "format", Constants.RGBAFormat }
                };

                shadow.Map = new GLRenderTarget((int)_shadowMapSize.X, (int)_shadowMapSize.Y, pars);
                shadow.Map.Texture.Name = light.Name + ".shadowMap";

                shadow.Camera.UpdateProjectionMatrix();
            }

            _renderer.SetRenderTarget(shadow.Map);
            _renderer.Clear();

            var viewportCount = shadow.GetViewportCount();

            for (var vp = 0; vp < viewportCount; vp++)
            {
                var viewport = shadow.GetViewport(vp);

                _viewport.Set(
                    _viewportSize.X * viewport.X,
                    _viewportSize.Y * viewport.Y,
                    _viewportSize.X * viewport.Z,
                    _viewportSize.Y * viewport.W);

                _state.Viewport(_viewport);

                if (shadow is PointLightShadow)
                    (shadow as PointLightShadow).UpdateMatrices(light, vp);
                else
                    shadow.UpdateMatrices(light);

                _frustum = shadow.GetFrustum();

                RenderObject(scene, camera, shadow.Camera, light, Type);
            }
            // do blur pass for VSM

            if (!(shadow is PointLightShadow) && Type == Constants.VSMShadowMap) VSMPass(shadow, camera);

            shadow.NeedsUpdate = false;
        }

        needsUpdate = false;
        _renderer.SetRenderTarget(currentRenderTarget, activeCubeFace, activeMipmapLevel);
    }

    private void AddUniformsValue(GLUniforms uniforms, string key, object value)
    {
        var uniform = new GLUniform { { "value", value } };
        if (!uniforms.ContainsKey(key))
            uniforms.Add(key, uniform);
        else
            uniforms[key] = uniform;
    }

    private void VSMPass(LightShadow shadow, Camera camera)
    {
        var geometry = _objects.Update(fullScreenMesh);

        // vertical pas
        AddUniformsValue(shadowMaterialVertical.Uniforms, "shadow_pass", shadow.Map.Texture);
        AddUniformsValue(shadowMaterialVertical.Uniforms, "resolution", shadow.MapSize);
        AddUniformsValue(shadowMaterialVertical.Uniforms, "radius", shadow.Radius);
        _renderer.SetRenderTarget(shadow.MapPass);
        _renderer.Clear();
        _renderer.RenderBufferDirect(camera, null, geometry, shadowMaterialVertical, fullScreenMesh, null);

        // horizontal pass
        AddUniformsValue(shadowMaterialHorizontal.Uniforms, "shadow_pass", shadow.MapPass.Texture);
        AddUniformsValue(shadowMaterialHorizontal.Uniforms, "resolution", shadow.MapSize);
        AddUniformsValue(shadowMaterialHorizontal.Uniforms, "radius", shadow.Radius);
        _renderer.SetRenderTarget(shadow.Map);
        _renderer.Clear();
        _renderer.RenderBufferDirect(camera, null, geometry, shadowMaterialHorizontal, fullScreenMesh, null);
    }

    public void RenderObject(Object3D object3D, Camera camera, Camera shadowCamera, Light light, int type)
    {
        if (object3D.Visible == false) return;

        var visible = object3D.Layers.Test(camera.Layers);

        if (visible && (object3D is Mesh || object3D is Line || object3D is Points))
            if ((object3D.CastShadow || (object3D.ReceiveShadow && type == Constants.VSMShadowMap)) &&
                (!object3D.FrustumCulled || _frustum.IntersectsObject(object3D)))
            {
                object3D.ModelViewMatrix = shadowCamera.MatrixWorldInverse * object3D.MatrixWorld;

                var geometry = _objects.Update(object3D);
                var material = object3D.Material;

                if (object3D.Materials.Count > 1)
                {
                    var groups = geometry.Groups;
                    for (var k = 0; k < groups.Count; k++)
                    {
                        var group = groups[k];
                        var groupMaterial = object3D.Materials[group.MaterialIndex];

                        if (groupMaterial != null && groupMaterial.Visible)
                        {
                            var depthMaterial = GetDepthMaterial(object3D, groupMaterial, light, shadowCamera.Near,
                                shadowCamera.Far, type);
                            _renderer.RenderBufferDirect(shadowCamera, null, geometry, depthMaterial, object3D, null);
                        }
                    }
                }
                else if (material.Visible)
                {
                    var depthMaterial = GetDepthMaterial(object3D, material, light, shadowCamera.Near, shadowCamera.Far,
                        type);
                    _renderer.RenderBufferDirect(shadowCamera, null, geometry, depthMaterial, object3D, null);
                }
            }

        var children = object3D.Children;

        for (var i = 0; i < children.Count; i++) RenderObject(children[i], camera, shadowCamera, light, type);
    }

    private Material GetDepthMaterial(Object3D object3D, Material material, Light light, float shadowCameraNear,
        float shadowCameraFar, int type)
    {
        var geometry = object3D.Geometry;

        Material result = null;

        var GetMaterialVariant = GetDepthMaterialVariant;
        var materials = _depthMaterials;
        var customMaterial = object3D.CustomDepthMaterial;

        if (light is PointLight)
        {
            GetMaterialVariant = GetDistanceMaterialVariant;
            customMaterial = object3D.CustomDistanceMaterial;
            materials = _distanceMaterial;
        }

        if (customMaterial == null)
        {
            var useMorphing = false;

            if (material.MorphTargets)
            {
                if (geometry is BufferGeometry)
                {
                    var attributes = (geometry as BufferGeometry).MorphAttributes;
                    useMorphing = attributes.Count > 0 && attributes.ContainsKey("position") &&
                                  (attributes["position"] as List<IBufferAttribute>).Count > 0;
                }
                else if (geometry is Geometry)
                {
                    useMorphing = geometry.MorphTargets != null && geometry.MorphTargets.Count > 0;
                }
            }

            var useSkinning = false;

            if (object3D is SkinnedMesh)
            {
                if (material.Skinning)
                    useSkinning = true;
                else
                    Trace.TraceWarning("THREE.WebGLShadowMap: THREE.SkinnedMesh with material.skinning set to false:",
                        object3D);
            }

            var useInstancing = object3D is InstancedMesh;

            result = GetMaterialVariant(useMorphing ? 1 : 0, useSkinning ? 1 : 0, useInstancing ? 1 : 0, materials);
        }
        else
        {
            result = customMaterial;
        }

        if (_renderer.LocalClippingEnabled && material.ClipShadows && material.ClippingPlanes.Count > 0)
        {
            // in this case we need a unique material instance reflecting the
            // appropriate state

            Guid keyA = result.Uuid, keyB = material.Uuid;

            var materialsForVariant = (Hashtable)_materialCache[keyA];

            if (materialsForVariant == null)
            {
                materialsForVariant = new Hashtable();
                _materialCache.Add(keyA, materialsForVariant);
            }

            var cachedMaterial = (Material)materialsForVariant[keyB];

            if (cachedMaterial == null)
            {
                cachedMaterial = (Material)result.Clone();
                materialsForVariant.Add(keyB, cachedMaterial);
            }

            result = cachedMaterial;
        }

        result.Visible = material.Visible;
        result.Wireframe = material.Wireframe;

        if (type == Constants.VSMShadowMap)
            result.Side = material.ShadowSide != null ? (int)material.ShadowSide : material.Side;
        else
            result.Side = material.ShadowSide != null ? (int)material.ShadowSide : shadowSide[material.Side];

        result.ClipShadows = material.ClipShadows;
        result.ClippingPlanes = material.ClippingPlanes;
        result.ClipIntersection = material.ClipIntersection;

        result.WireframeLineWidth = material.WireframeLineWidth;
        result.LineWidth = material.LineWidth;

        if (light is PointLight && result is MeshDistanceMaterial)
        {
            (result as MeshDistanceMaterial).ReferencePosition.SetFromMatrixPosition(light.MatrixWorld);
            (result as MeshDistanceMaterial).NearDistance = shadowCameraNear;
            (result as MeshDistanceMaterial).FarDistance = shadowCameraFar;
        }

        return result;
    }

    private delegate Material GetMaterialVariant(int useMorphing, int useSkinning, int useInstancing,
        Dictionary<int, Material> depthMaterials);
}