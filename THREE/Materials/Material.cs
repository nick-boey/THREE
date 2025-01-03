using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class Material : IDisposable, ICloneable
{
    private static int materialIdCount;

    public Texture AlphaMap;

    public float AlphaTest;

    public Texture AoMap;

    public float AoMapIntensity = 1;

    public Color AttenuationColor = new(1, 1, 1);

    public float AttenuationDistance = 0.0f;

    public int BlendDst = Constants.OneMinusSrcAlphaFactor;

    public int? BlendDstAlpha;

    public int BlendEquation = Constants.AddEquation;

    public int? BlendEquationAlpha;

    public int Blending = Constants.NormalBlending;

    public int BlendSrc = Constants.SrcAlphaFactor;

    public int? BlendSrcAlpha;

    public Texture BumpMap;

    public float BumpScale;

    public float Clearcoat;

    public Texture ClearcoatMap;

    public Texture ClearcoatNormalMap;

    public Vector2 ClearcoatNormalScale;

    public float ClearcoatRoughness;

    public Texture ClearcoatRoughnessMap;

    public bool ClipIntersection;

    public bool Clipping;

    public List<Plane> ClippingPlanes = new();

    public bool ClipShadows;

    public Color? Color;

    public bool ColorWrite = true;

    public int Combine;
    public string customProgramCacheKey;

    public Hashtable Defines = new();

    public int DepthFunc = Constants.LessEqualDepth;

    public int DepthPacking = Constants.BasicDepthPacking;

    public bool DepthTest = true;

    public bool DepthWrite = true;

    public float DisplacementBias;

    public Texture DisplacementMap;

    public float DisplacementScale;

    private bool disposed;

    public bool Dithering;

    public Color? Emissive;

    public float EmissiveIntensity = 1;

    public Texture EmissiveMap;

    public Texture EnvMap;

    public float EnvMapIntensity = 1;

    public bool FlatShading;

    public bool Fog = true;

    public string glslVersion = "";

    public Texture GradientMap;

    public int Id = materialIdCount++;

    public string IndexOAttributeName;

    public Texture LightMap;

    public float LightMapIntensity = 1;

    public float LineWidth = 1;

    public Texture Map;

    public float Metalness = 0.5f;

    public Texture MetalnessMap;

    public bool MorphNormals;

    public bool MorphTargets;

    public string Name;

    private bool needsUpdate;

    public Texture NormalMap;

    public int NormalMapType = -1;

    public Vector2 NormalScale;

    public int numSupportedMorphNormals;

    public int numSupportedMorphTargets;

    public Action<Hashtable, IGLRenderer> OnBeforeCompile;
    public Action<Hashtable, IGLRenderer> OnBuild;

    public float Opacity = 1;

    public bool PolygonOffset;

    public float PolygonOffsetFactor;

    public float PolygonOffsetUnits;

    public string Precision;

    public bool PremultipliedAlpha;

    public IGLProgram Program;

    public float Reflectivity = 1;

    public float RefractionRatio;

    public float Rotation;

    public float Roughness = 0.5f;

    public Texture RoughnessMap;

    public int? ShadowSide;

    public Color? Sheen;

    public float Shininess;

    public int Side = Constants.FrontSide;

    public bool SizeAttenuation;

    public bool Skinning;

    public Color Specular;

    public Texture SpecularMap;

    public int StencilFail = Constants.KeepStencilOp;

    public int StencilFunc = Constants.AlwaysStencilFunc;

    public int StencilFuncMask = 0xff;

    public int StencilRef;

    public bool StencilWrite;

    public int StencilWriteMask = 0xff;

    public int StencilZFail = Constants.KeepStencilOp;

    public int StencilZPass = Constants.KeepStencilOp;

    public float Thickness = 0.01f;

    public Texture ThicknessMap = null;

    public bool ToneMapped = true;

    public float Transmission = 0.0f;

    public Texture TransmissionMap;

    public bool Transparent;

    public string type = "Material";

    public Dictionary<string, object> UserData = new();

    public Guid Uuid = Guid.NewGuid();

    public int Version;

    public bool VertexColors;

    public bool VertexTangents;

    public bool Visible = true;

    public bool Wireframe;

    public string WireframeLineCap;

    public string WireframeLineJoin;

    public float WireframeLineWidth = 1;

    public Material()
    {
    }

    public Material(SerializationInfo info, StreamingContext context)
    {
    }

    protected Material(Material source)
    {
        Copy(source);
    }


    public bool NeedsUpdate
    {
        get => needsUpdate;
        set
        {
            needsUpdate = value;
            if (needsUpdate) Version++;
        }
    }


    public virtual object Clone()
    {
        var material = new Material();
        material.Copy(this);

        return material;
    }

    public virtual void Dispose()
    {
        Dispose(disposed);
    }

    public event EventHandler<EventArgs> Disposed;

    public virtual object Copy(Material source)
    {
        type = source.type;

        Defines = source.Defines.Clone() as Hashtable;

        Name = source.Name;

        Fog = source.Fog;

        Blending = source.Blending;

        Side = source.Side;

        FlatShading = source.FlatShading;

        VertexTangents = source.VertexTangents;

        VertexColors = source.VertexColors;

        Opacity = source.Opacity;

        Transparent = source.Transparent;

        if (source.Color != null) Color = source.Color.Value;

        Specular = source.Specular;


        BlendSrc = source.BlendSrc;

        BlendDst = source.BlendDst;

        BlendEquation = source.BlendEquation;

        if (source.BlendSrcAlpha != null) BlendSrcAlpha = source.BlendSrcAlpha.Value;
        if (source.BlendDstAlpha != null) BlendDstAlpha = source.BlendDstAlpha.Value;
        if (source.BlendEquationAlpha != null) BlendEquationAlpha = source.BlendEquationAlpha.Value;


        DepthFunc = source.DepthFunc;

        DepthTest = source.DepthTest;

        DepthWrite = source.DepthWrite;

        StencilWriteMask = source.StencilWriteMask;

        StencilFunc = source.StencilFunc;

        StencilRef = source.StencilRef;

        StencilFuncMask = source.StencilRef;

        StencilFail = source.StencilFail;

        StencilZFail = source.StencilZFail;

        StencilZPass = source.StencilZPass;

        StencilWrite = source.StencilWrite;

        ClippingPlanes = source.ClippingPlanes.ToList();

        ClipIntersection = source.ClipIntersection;

        ClipShadows = source.ClipShadows;

        ShadowSide = source.ShadowSide;

        ColorWrite = source.ColorWrite;

        Precision = source.Precision;

        PolygonOffset = source.PolygonOffset;

        PolygonOffsetFactor = source.PolygonOffsetFactor;

        PolygonOffsetUnits = source.PolygonOffsetUnits;

        Dithering = source.Dithering;

        AlphaTest = source.AlphaTest;

        PremultipliedAlpha = source.PremultipliedAlpha;

        Visible = source.Visible;

        ToneMapped = source.ToneMapped;

        UserData = source.UserData.DeepCopy();

        NeedsUpdate = source.NeedsUpdate;

        glslVersion = source.glslVersion;

        IndexOAttributeName = source.IndexOAttributeName;

        MorphTargets = source.MorphTargets;

        MorphNormals = source.MorphNormals;

        if (source.Map != null)
            Map = (Texture)source.Map.Clone();

        if (source.AlphaMap != null)
            AlphaMap = (Texture)source.AlphaMap.Clone();

        if (source.SpecularMap != null)
            SpecularMap = (Texture)source.SpecularMap.Clone();

        if (source.EnvMap != null)
            EnvMap = (Texture)source.EnvMap.Clone();

        if (source.NormalMap != null)
            NormalMap = (Texture)source.NormalMap.Clone();

        NormalMapType = source.NormalMapType;

        if (source.NormalScale != null)
            NormalScale = source.NormalScale.Clone();

        if (source.BumpMap != null)
            BumpMap = (Texture)source.BumpMap.Clone();

        BumpScale = source.BumpScale;

        if (source.LightMap != null)
            LightMap = (Texture)source.LightMap.Clone();

        if (source.AoMap != null)
            AoMap = (Texture)source.AoMap.Clone();

        if (source.EmissiveMap != null)
            EmissiveMap = (Texture)source.EmissiveMap.Clone();

        if (source.DisplacementMap != null)
            DisplacementMap = (Texture)source.DisplacementMap.Clone();

        DisplacementScale = source.DisplacementScale;

        DisplacementBias = source.DisplacementBias;

        Clearcoat = source.Clearcoat;

        if (source.ClearcoatMap != null)
            ClearcoatMap = (Texture)source.ClearcoatMap.Clone();

        ClearcoatRoughness = source.ClearcoatRoughness;

        if (source.ClearcoatRoughnessMap != null)
            ClearcoatRoughnessMap = (Texture)source.ClearcoatRoughnessMap.Clone();

        if (source.ClearcoatNormalScale != null)
            ClearcoatNormalScale = source.ClearcoatNormalScale.Clone();

        if (source.ClearcoatNormalMap != null)
            ClearcoatNormalMap = (Texture)source.ClearcoatNormalMap.Clone();

        if (source.RoughnessMap != null)
            RoughnessMap = (Texture)source.RoughnessMap.Clone();

        if (source.MetalnessMap != null)
            MetalnessMap = (Texture)source.MetalnessMap.Clone();

        if (source.GradientMap != null)
            GradientMap = (Texture)source.GradientMap.Clone();

        if (source.TransmissionMap != null)
            TransmissionMap = (Texture)source.TransmissionMap.Clone();

        Sheen = source.Sheen;

        Emissive = source.Emissive;

        EmissiveIntensity = source.EmissiveIntensity;

        Combine = source.Combine;

        SizeAttenuation = source.SizeAttenuation;

        Skinning = source.Skinning;

        DepthPacking = source.DepthPacking;

        numSupportedMorphTargets = source.numSupportedMorphTargets;

        numSupportedMorphNormals = source.numSupportedMorphNormals;

        Clipping = source.Clipping;

        Rotation = source.Rotation;

        Reflectivity = source.Reflectivity;

        RefractionRatio = source.RefractionRatio;

        LightMapIntensity = source.LightMapIntensity;

        AoMapIntensity = source.AoMapIntensity;

        EnvMapIntensity = source.EnvMapIntensity;

        LineWidth = source.LineWidth;

        Wireframe = source.Wireframe;

        WireframeLineWidth = source.WireframeLineWidth;

        WireframeLineCap = source.WireframeLineCap;

        WireframeLineJoin = source.WireframeLineJoin;

        Shininess = source.Shininess;

        Version = source.Version;

        Roughness = source.Roughness;

        Metalness = source.Metalness;

        return this;
    }

    protected void SetValues(Hashtable values)
    {
        if (values == null)
            return;

        foreach (DictionaryEntry item in values)
        {
            var newValue = item.Value;
            var key = item.Key as string;

            var type = GetType();
            var propertyInfo = type.GetProperty(key, BindingFlags.Instance | BindingFlags.Public);
            if (propertyInfo != null)
            {
                propertyInfo.SetValue(this, newValue);
            }
            else
            {
                var fieldInfo = type.GetField(key,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (fieldInfo != null)
                    fieldInfo.SetValue(this, newValue);
                else
                    Trace.TraceWarning("attribute {0} not found", key);
            }
        }
    }

    ~Material()
    {
        Dispose(false);
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
        RaiseDisposed();
        disposed = true;
        disposed = true;
    }
}