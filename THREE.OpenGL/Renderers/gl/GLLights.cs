using System.Diagnostics;
using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class UniformsCache : GLUniforms
{
    public UniformsCache()
    {
        Add("DirectionalLight", new GLUniform
        {
            { "direction", Vector3.Zero() },
            { "color", new Color() }
        });

        Add("SpotLight", new GLUniform
        {
            { "position", Vector3.Zero() },
            { "direction", Vector3.Zero() },
            { "color", new Color() },
            { "distance", 0.0f },
            { "coneCos", 0.0f },
            { "penumbraCos", 0.0f },
            { "decay", 0.0f }
        });

        Add("PointLight", new GLUniform
        {
            { "position", Vector3.Zero() },
            { "color", new Color() },
            { "distance", 0 },
            { "decay", 0 }
        });

        Add("HemisphereLight", new GLUniform
        {
            { "direction", Vector3.Zero() },
            { "skyColor", new Color() },
            { "groundColor", new Color() }
        });

        Add("RectAreaLight", new GLUniform
        {
            { "color", new Color() },
            { "position", Vector3.Zero() },
            { "halfWidth", Vector3.Zero() },
            { "halfHeight", Vector3.Zero() }
            // TODO (abelnation): set RectAreaLight shadow uniforms
        });
    }

    public UniformsCache(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}

[Serializable]
public class ShadowUniformsCache : GLUniforms
{
    public ShadowUniformsCache()
    {
        Add("DirectionalLight", new GLUniform
        {
            { "shadowBias", 0.0f },
            { "shadowNormalBias", 0.0f },
            { "shadowRadius", 1.0f },
            { "shadowMapSize", Vector2.Zero() }
        });

        Add("SpotLight", new GLUniform
        {
            { "shadowBias", 0.0f },
            { "shadowNormalBias", 0.0f },
            { "shadowRadius", 1.0f },
            { "shadowMapSize", Vector2.Zero() }
        });

        Add("PointLight", new GLUniform
        {
            { "shadowBias", 0 },
            { "shadowNormalBias", 0.0f },
            { "shadowRadius", 1 },
            { "shadowMapSize", Vector2.Zero() },
            { "shadowCameraNear", 1 },
            { "shadowCameraFar", 1000 }
        });
    }

    public ShadowUniformsCache(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}

[Serializable]
public class GLLights
{
    // TODO: Hashtable --> Dictionary<string,object>
    private UniformsCache cache = new();
    private GLCapabilities capabilities;

    private GLExtensions extension;
    private Matrix4 matrix4 = Matrix4.Identity();
    private Matrix4 matrix42 = Matrix4.Identity();

    private int nextVersion;
    private Quaternion quaternion = Quaternion.Identity();
    private ShadowUniformsCache shadowCache = new();
    public GLUniform state;

    private Vector3 vector3 = Vector3.Zero();

    public GLLights(GLExtensions extension, GLCapabilities capabilities)
    {
        this.extension = extension;
        this.capabilities = capabilities;
        state = new GLUniform
        {
            { "version", 0 },

            {
                "hash", new GLUniform
                {
                    { "directionalLength", -1 },
                    { "pointLength", -1 },
                    { "spotLength", -1 },
                    { "rectAreaLength", -1 },
                    { "hemiLength", -1 },

                    { "numDirectionalShadows", -1 },
                    { "numPointShadows", -1 },
                    { "numSpotShadows", -1 }
                }
            },

            { "ambient", new float[] { 0, 0, 0 } },
            { "probe", null },
            { "directional", null },
            { "directionalShadow", null },
            { "directionalShadowMap", null },
            { "directionalShadowMatrix", null },
            { "spot", null },
            { "spotShadow", null },
            { "spotShadowMap", null },
            { "spotShadowMatrix", null },
            { "rectArea", null },
            { "rectAreaLTC1", null },
            { "rectAreaLTC2", null },
            { "point", null },
            { "pointShadow", null },
            { "pointShadowMap", null },
            { "pointShadowMatrix", null },
            { "hemi", null },

            { "numDirectionalShadows", -1 },
            { "numPointShadows", -1 },
            { "numSpotShadows", -1 }
        };

        var Zero = Vector3.Zero();
        state["probe"] = new[] { Zero, Zero, Zero, Zero, Zero, Zero, Zero, Zero, Zero };
    }

    public void Setup(List<Light> lights)
    {
        var ambientColor = Color.Hex(0x000000);
        var directionalLength = 0;

        var pointLength = 0;
        var spotLength = 0;
        var rectAreaLength = 0;
        var hemiLength = 0;

        var numDirectionalShadows = 0;
        var numPointShadows = 0;
        var numSpotShadows = 0;

        //var viewMatrix = camera.MatrixWorldInverse;

        lights.Sort(delegate(Light lightA, Light lightB)
        {
            return (lightB.CastShadow ? 1 : 0) - (lightA.CastShadow ? 1 : 0);
        });

        var directionalShadowList = new List<GLUniform>();

        var directionalShadowMapList = new List<Texture>();

        var directionalShadowMatrixList = new List<Matrix4>();

        var directionalList = new List<GLUniform>();

        var spotShadowList = new List<GLUniform>();

        var spotShadowMapList = new List<Texture>();

        var spotShadowMatrixList = new List<Matrix4>();

        var spotList = new List<GLUniform>();


        var rectAreaList = new List<GLUniform>();

        var pointShadowList = new List<GLUniform>();

        var pointShadowMapList = new List<Texture>();

        var pointShadowMatrixList = new List<Matrix4>();

        var pointList = new List<GLUniform>();

        var hemiList = new List<GLUniform>();

        for (var i = 0; i < lights.Count; i++)
        {
            var light = lights[i];

            var color = light.Color;
            var intensity = light.Intensity;
            var distance = light.Distance;

            var shadowMap = light.Shadow != null && light.Shadow.Map != null ? light.Shadow.Map.Texture : null;


            if (light is AmbientLight)
            {
                color.MultiplyScalar(intensity);
                ambientColor.Add(color);
            }
            else if (light is LightProbe)
            {
                var probe = (Vector3[])state["probe"];
                for (var j = 0; j < 9; j++) probe[j] = light.sh.Coefficients[j] * intensity;
            }
            else if (light is DirectionalLight)
            {
                var uniforms = (GLUniform)(cache[light.type] as GLUniform).Clone();
                var lightColor = light.Color;
                uniforms["color"] = lightColor.MultiplyScalar(light.Intensity);

                if (light.CastShadow)
                {
                    var shadow = light.Shadow;
                    var shadowUniforms = (GLUniform)(shadowCache[light.type] as GLUniform).Clone();
                    shadowUniforms["shadowBias"] = shadow.Bias;
                    shadowUniforms["shadowNormalBias"] = shadow.NormalBias;
                    shadowUniforms["shadowRadius"] = shadow.Radius;
                    shadowUniforms["shadowMapSize"] = shadow.MapSize;

                    directionalShadowList.Add(shadowUniforms);
                    directionalShadowMapList.Add(shadowMap);
                    directionalShadowMatrixList.Add(light.Shadow.Matrix);
                    numDirectionalShadows++;
                }

                directionalList.Add(uniforms);
                directionalLength++;
            }
            else if (light is SpotLight)
            {
                var uniforms = (GLUniform)(cache[light.type] as GLUniform).Clone();
                var position = Vector3.Zero().SetFromMatrixPosition(light.MatrixWorld);
                uniforms["position"] = position;

                var lightColor = light.Color;

                uniforms["color"] = lightColor.MultiplyScalar(light.Intensity);
                uniforms["distance"] = distance;

                uniforms["coneCos"] = (float)Math.Cos(light.Angle);
                uniforms["penumbraCos"] = (float)Math.Cos(light.Angle * (1 - light.Penumbra));
                uniforms["decay"] = light.Decay;

                if (light.CastShadow)
                {
                    var shadow = light.Shadow;

                    var shadowUniforms = (GLUniform)(shadowCache[light.type] as GLUniform).Clone();

                    shadowUniforms["shadowBias"] = shadow.Bias;
                    shadowUniforms["shadowNormalBias"] = shadow.NormalBias;
                    shadowUniforms["shadowRadius"] = shadow.Radius;
                    shadowUniforms["shadowMapSize"] = shadow.MapSize;

                    spotShadowList.Add(shadowUniforms);
                    spotShadowMapList.Add(shadowMap);
                    spotShadowMatrixList.Add(light.Shadow.Matrix);

                    numSpotShadows++;
                }

                spotList.Add(uniforms);
                spotLength++;
            }
            else if (light is RectAreaLight)
            {
                var uniforms = (GLUniform)(cache[light.type] as GLUniform).Clone();
                var lightColor = light.Color;
                uniforms["color"] = lightColor.MultiplyScalar(light.Intensity);

                var halfWidth = new Vector3(light.Width * 0.5f, 0.0f, 0.0f);
                var halfHeight = new Vector3(0.0f, light.Height * 0.5f, 0.0f);

                uniforms["halfWidth"] = halfWidth;
                uniforms["halfHeight"] = halfHeight;

                rectAreaList.Add(uniforms);

                rectAreaLength++;
            }
            else if (light is PointLight)
            {
                var uniforms = (GLUniform)(cache[light.type] as GLUniform).Clone();
                var lightColor = light.Color;
                uniforms["color"] = lightColor.MultiplyScalar(light.Intensity);
                uniforms["distance"] = distance;
                uniforms["decay"] = light.Decay;

                if (light.CastShadow)
                {
                    var shadow = light.Shadow;

                    var shadowUniforms = (GLUniform)(shadowCache[light.type] as GLUniform).Clone();

                    shadowUniforms["shadowBias"] = shadow.Bias;
                    shadowUniforms["shadowNormalBias"] = shadow.NormalBias;
                    shadowUniforms["shadowRadius"] = shadow.Radius;
                    shadowUniforms["shadowMapSize"] = shadow.MapSize;
                    shadowUniforms["shadowCameraNear"] = shadow.Camera.Near;
                    shadowUniforms["shadowCameraFar"] = shadow.Camera.Far;

                    pointShadowList.Add(shadowUniforms);
                    pointShadowMapList.Add(shadowMap);
                    pointShadowMatrixList.Add(light.Shadow.Matrix);

                    numPointShadows++;
                }

                pointList.Add(uniforms);

                pointLength++;
            }
            else if (light is HemisphereLight)
            {
                var uniforms = (GLUniform)(cache[light.type] as GLUniform).Clone();
                var lightColor = light.Color;
                uniforms["skyColor"] = lightColor.MultiplyScalar(light.Intensity);

                var groundColor = light.GroundColor;
                uniforms["groundColor"] = groundColor.MultiplyScalar(light.Intensity);

                hemiList.Add(uniforms);

                hemiLength++;
                /*
                Vector3 direction = Vector3.Zero().SetFromMatrixPosition(light.MatrixWorld);

                direction.TransformDirection(viewMatrix);

                uniforms["direction"] = direction;

                Color lightColor = light.Color;
                uniforms["skyColor"] = lightColor.MultiplyScalar(light.Intensity);

                Color groundColor = light.GroundColor;
                uniforms["groundColor"] = groundColor.MultiplyScalar(light.Intensity);
                hemiList.Add(uniforms);
                hemiLength++;
                */
            }
        }


        if (rectAreaLength > 0)
        {
            if (capabilities.IsGL2)
            {
                if (UniformsLib.LTC_FLOAT_1 != null) state["rectAreaLTC1"] = UniformsLib.LTC_FLOAT_1;
                if (UniformsLib.LTC_FLOAT_1 != null) state["rectAreaLTC2"] = UniformsLib.LTC_FLOAT_2;
                //if (state.Contains("rectAreaLTC1")) state["rectAreaLTC1"] = TextureLoader.LoadEmbedded("ltc_1.png");
                //if (state.Contains("rectAreaLTC2")) state["rectAreaLTC2"] = TextureLoader.LoadEmbedded("ltc_2.png");
            }
            else
            {
                //WebGL1
                if (extension.Get("OES_texture_float_linear") > -1)
                {
                    if (UniformsLib.LTC_FLOAT_1 != null) state["rectAreaLTC1"] = UniformsLib.LTC_FLOAT_1;
                    if (UniformsLib.LTC_FLOAT_1 != null) state["rectAreaLTC2"] = UniformsLib.LTC_FLOAT_2;
                }
                else if (extension.Get("OES_texture_half_float_linear") > -1)
                {
                    if (UniformsLib.LTC_HALF_1 != null) state["rectAreaLTC1"] = UniformsLib.LTC_HALF_1;
                    if (UniformsLib.LTC_HALF_2 != null) state["rectAreaLTC2"] = UniformsLib.LTC_HALF_2;
                }
                else
                {
                    Debug.WriteLine("THREE.GLRenderer : unable to use RectAreaLight : Missing GL Extension");
                }
            }
            //if (state.Contains("rectAreaLTC1")) state["rectAreaLTC1"] = TextureLoader.LoadEmbedded("ltc_1.png");
            //if (state.Contains("rectAreaLTC2")) state["rectAreaLTC2"] = TextureLoader.LoadEmbedded("ltc_2.png");
        }

        state["ambient"] = ambientColor;

        var hash = (GLUniform)state["hash"];

        if ((int)hash["directionalLength"] != directionalLength ||
            (int)hash["pointLength"] != pointLength ||
            (int)hash["spotLength"] != spotLength ||
            (int)hash["rectAreaLength"] != rectAreaLength ||
            (int)hash["hemiLength"] != hemiLength ||
            (int)hash["numDirectionalShadows"] != numDirectionalShadows ||
            (int)hash["numPointShadows"] != numPointShadows ||
            (int)hash["numSpotShadows"] != numSpotShadows)
        {
            hash["directionalLength"] = directionalLength;
            hash["pointLength"] = pointLength;
            hash["spotLength"] = spotLength;
            hash["rectAreaLength"] = rectAreaLength;
            hash["hemiLength"] = hemiLength;

            hash["numDirectionalShadows"] = numDirectionalShadows;
            hash["numPointShadows"] = numPointShadows;
            hash["numSpotShadows"] = numSpotShadows;

            state["version"] = nextVersion++;
        }

        state["directionalShadow"] = directionalShadowList.ToArray();
        state["directionalShadowMap"] = directionalShadowMapList.ToArray();
        state["directionalShadowMatrix"] = directionalShadowMatrixList.ToArray();
        state["directional"] = directionalList.ToArray();
        state["spotShadow"] = spotShadowList.ToArray();
        state["spotShadowMap"] = spotShadowMapList.ToArray();
        state["spotShadowMatrix"] = spotShadowMatrixList.ToArray();
        state["spot"] = spotList.ToArray();
        state["rectArea"] = rectAreaList.ToArray();
        state["pointShadow"] = pointShadowList.ToArray();
        state["pointShadowMap"] = pointShadowMapList.ToArray();
        state["pointShadowMatrix"] = pointShadowMatrixList.ToArray();
        state["point"] = pointList.ToArray();
        state["hemi"] = hemiList.ToArray();
    }


    public void SetupView(List<Light> lights, Camera camera)
    {
        var directionalLength = 0;
        var pointLength = 0;
        var spotLength = 0;
        var rectAreaLength = 0;
        var hemiLength = 0;

        var viewMatrix = camera.MatrixWorldInverse;

        for (var i = 0; i < lights.Count; i++)
        {
            var light = lights[i];

            if (light is DirectionalLight)
            {
                var uniforms = (state["directional"] as GLUniform[])[directionalLength];

                (uniforms["direction"] as Vector3).SetFromMatrixPosition(light.MatrixWorld);
                var vector3 = Vector3.Zero().SetFromMatrixPosition(light.Target.MatrixWorld);
                var direction = uniforms["direction"] as Vector3;
                direction.Sub(vector3);
                direction.TransformDirection(viewMatrix);

                directionalLength++;
            }
            else if (light is SpotLight)
            {
                var uniforms = (state["spot"] as GLUniform[])[spotLength];
                var position = uniforms["position"] as Vector3;

                position.SetFromMatrixPosition(light.MatrixWorld);
                position.ApplyMatrix4(viewMatrix);
                uniforms["position"] = position;
                var direction = uniforms["direction"] as Vector3;
                direction.SetFromMatrixPosition(light.MatrixWorld);
                var vector3 = Vector3.Zero().SetFromMatrixPosition(light.Target.MatrixWorld);
                direction.Sub(vector3);
                direction.TransformDirection(viewMatrix);
                uniforms["direction"] = direction;

                spotLength++;
            }
            else if (light is RectAreaLight)
            {
                var uniforms = (state["rectArea"] as GLUniform[])[rectAreaLength];
                var position = uniforms["position"] as Vector3;
                position.SetFromMatrixPosition(light.MatrixWorld);
                position.ApplyMatrix4(viewMatrix);

                // extract local rotation of light to derive width/height half vectors
                var matrix42 = Matrix4.Identity();
                var matrix4 = Matrix4.Identity();
                matrix4.Copy(light.MatrixWorld);
                matrix4.PreMultiply(viewMatrix);
                matrix42.ExtractRotation(matrix4);

                (uniforms["halfWidth"] as Vector3).Set((light as RectAreaLight).Width * 0.5f, 0.0f, 0.0f);
                (uniforms["halfHeight"] as Vector3).Set(0.0f, (light as RectAreaLight).Height * 0.5f, 0.0f);

                (uniforms["halfWidth"] as Vector3).ApplyMatrix4(matrix42);
                (uniforms["halfHeight"] as Vector3).ApplyMatrix4(matrix42);

                rectAreaLength++;
            }
            else if (light is PointLight)
            {
                var uniforms = (state["point"] as GLUniform[])[pointLength];
                var position = uniforms["position"] as Vector3;
                position.SetFromMatrixPosition(light.MatrixWorld);
                position.ApplyMatrix4(viewMatrix);

                pointLength++;
            }
            else if (light is HemisphereLight)
            {
                var uniforms = (state["hemi"] as GLUniform[])[hemiLength];
                var direction = uniforms["direction"] as Vector3;
                direction.SetFromMatrixPosition(light.MatrixWorld);
                direction.TransformDirection(viewMatrix);
                direction.Normalize();

                hemiLength++;
            }
        }
    }
}