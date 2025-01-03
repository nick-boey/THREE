using System.Collections;
using System.Diagnostics;
using OpenTK.Graphics.ES30;

namespace THREE;

[Serializable]
public class GLColorBuffer : IGLColorBuffer
{
    private Vector4 color = new();

    private Vector4 currentColorClear = new(0, 0, 0, 0);

    private bool? currentColorMask;
    private bool locked;

    public void SetMask(bool colorMask)
    {
        if (currentColorMask != colorMask && !locked)
        {
            GL.ColorMask(colorMask, colorMask, colorMask, colorMask);
            currentColorMask = colorMask;
        }
    }

    public void SetLocked(bool locked)
    {
        this.locked = locked;
    }

    public void SetClear(float r, float g, float b, float a, bool premultipliedAlpha = false)
    {
        if (premultipliedAlpha)
        {
            r *= a;
            g *= a;
            b *= a;
        }

        color.X = r;
        color.Y = g;
        color.Z = b;
        color.W = a;
        if (!currentColorClear.Equals(color))
        {
            GL.ClearColor(r, g, b, a);
            currentColorClear.Copy(color);
        }
    }

    public void Reset()
    {
        locked = false;
        currentColorMask = null;
        currentColorClear = new Vector4(-1, 0, 0, 0);
    }
}

public class GLDepthBuffer : IGLDepthBuffer
{
    private readonly GLState State;

    private float? currentDepthClear;

    private int? currentDepthFunc;

    private bool? currentDepthMask;
    private bool locked;

    public GLDepthBuffer(GLState state)
    {
        State = state;
    }

    public void SetTest(bool depthTest)
    {
        if (depthTest)
            State.Enable((int)EnableCap.DepthTest);
        else
            State.Disable((int)EnableCap.DepthTest);
    }

    public void SetMask(bool depthMask)
    {
        if (currentDepthMask != depthMask && !locked)
        {
            GL.DepthMask(depthMask);
            currentDepthMask = depthMask;
        }
    }

    public void SetFunc(int depthFunc)
    {
        //DepthFunction func = (DepthFunction)Enum.ToObject(typeof(DepthFunction), depthFunc);

        if (currentDepthFunc != depthFunc)
        {
            if (depthFunc > 0)
            {
                if (depthFunc == Constants.NeverDepth) GL.DepthFunc(DepthFunction.Never);

                else if (depthFunc == Constants.AlwaysDepth) GL.DepthFunc(DepthFunction.Always);

                else if (depthFunc == Constants.LessDepth) GL.DepthFunc(DepthFunction.Less);

                else if (depthFunc == Constants.LessEqualDepth) GL.DepthFunc(DepthFunction.Lequal);

                else if (depthFunc == Constants.EqualDepth) GL.DepthFunc(DepthFunction.Equal);

                else if (depthFunc == Constants.GreaterEqualDepth) GL.DepthFunc(DepthFunction.Gequal);

                else if (depthFunc == Constants.GreaterDepth) GL.DepthFunc(DepthFunction.Greater);

                else if (depthFunc == Constants.NotEqualDepth) GL.DepthFunc(DepthFunction.Notequal);

                else GL.DepthFunc(DepthFunction.Lequal);
            }
            else
            {
                GL.DepthFunc(DepthFunction.Lequal);
            }

            currentDepthFunc = depthFunc;
        }
    }

    public void SetLocked(bool locked)
    {
        this.locked = locked;
    }

    public void SetClear(float depth)
    {
        if (currentDepthClear != depth)
        {
            GL.ClearDepth(depth);
            currentDepthClear = depth;
        }
    }

    public void Reset()
    {
        locked = false;

        currentDepthMask = null;
        currentDepthFunc = null;
        currentDepthClear = null;
    }
}

public class GLStencilBuffer : IGLStencilBuffer
{
    private readonly GLState State;

    private int? currentStencilClear;

    private int? currentStencilFail;

    private int? currentStencilFunc;

    private int? currentStencilFuncMask;

    private int? currentStencilMask;

    private int? currentStencilRef;

    private int? currentStencilZFail;

    private int? currentStencilZPass;
    private bool locked;

    public GLStencilBuffer(GLState state)
    {
        State = state;
    }

    public void SetTest(bool stencilTest)
    {
        if (!locked)
        {
            if (stencilTest)
                State.Enable((int)EnableCap.StencilTest);
            else
                State.Disable((int)EnableCap.StencilTest);
        }
    }

    public void SetMask(int stencilMask)
    {
        if (currentStencilMask != stencilMask && !locked)
        {
            GL.StencilMask(stencilMask);
            currentStencilMask = stencilMask;
        }
    }

    public void SetFunc(int stencilFunc, int stencilRef, int stencilMask)
    {
        if (currentStencilFunc != stencilFunc ||
            currentStencilRef != stencilRef ||
            currentStencilFuncMask != stencilMask)
        {
            //All localTarget = (All)Enum.ToObject(typeof(All), (int)target + i);
            var func = (StencilFunction)Enum.ToObject(typeof(StencilFunction), stencilFunc);
            GL.StencilFunc(func, stencilRef, stencilMask);

            currentStencilFunc = stencilFunc;
            currentStencilRef = stencilRef;
            currentStencilFuncMask = stencilMask;
        }
    }

    public void SetOp(int stencilFail, int stencilZFail, int stencilZPass)
    {
        if (currentStencilFail != stencilFail ||
            currentStencilZFail != stencilZFail ||
            currentStencilZPass != stencilZPass)
        {
            var fail = (StencilOp)Enum.ToObject(typeof(StencilOp), stencilFail);
            var zfail = (StencilOp)Enum.ToObject(typeof(StencilOp), stencilZFail);
            var zpass = (StencilOp)Enum.ToObject(typeof(StencilOp), stencilZPass);
            GL.StencilOp(fail, zfail, zpass);

            currentStencilFail = stencilFail;
            currentStencilZFail = stencilZFail;
            currentStencilZPass = stencilZPass;
        }
    }

    public void SetLocked(bool locked)
    {
        this.locked = locked;
    }

    public void SetClear(int stencil)
    {
        if (currentStencilClear != stencil)
        {
            GL.ClearStencil(stencil);
            currentStencilClear = stencil;
        }
    }

    public void Reset()
    {
        locked = false;

        currentStencilMask = null;
        currentStencilFunc = null;
        currentStencilRef = null;
        currentStencilFuncMask = null;
        currentStencilFail = null;
        currentStencilZFail = null;
        currentStencilZPass = null;
        currentStencilClear = null;
    }
}

[Serializable]
public struct BoundTexture
{
    public int type;

    public int? texture;
}

[Serializable]
public class GLStateBuffer : IGLStateBuffer
{
    public IGLColorBuffer color { get; set; }

    public IGLDepthBuffer depth { get; set; }

    public IGLStencilBuffer stencil { get; set; }
}

public class GLState : DisposableObject, IGLState
{
    private static readonly Hashtable enabledCapabilities = new();

    private readonly byte[] attributeDivisors;

    private readonly GLColorBuffer colorBuffer;

    private readonly Dictionary<int, BoundTexture> currentBoundTextures = new();

    private readonly Vector4 currentScissor = new();

    private readonly Vector4 currentViewport = new();

    private readonly GLDepthBuffer depthBuffer;

    private readonly Dictionary<int, int> emptyTextures = new();

    private readonly byte[] enabledAttributes;

    private readonly bool lineWidthAvailable;

    private readonly int maxTextures;

    private readonly int maxVertexAttributes = GL.GetInteger(GetPName.MaxVertexAttribs);

    private readonly byte[] newAttributes;

    private readonly GLStencilBuffer stencilBuffer;

    private readonly GLUtils Utils;

    private readonly float version;

    private GLCapabilities Capabilities;

    private List<int> compressedTextureFormats;

    private int? currentBlendDst;

    private int? currentBlendDstAlpha;

    private int? currentBlendEquation;

    private int? currentBlendEquationAlpha;

    private int? currentBlending;

    private bool currentBlendingEnabled;

    private int? currentBlendSrc;

    private int? currentBlendSrcAlpha;

    private int? currentCullFace;

    private bool? currentFlipSided;

    private float? currentLineWidth;

    private float? currentPolygonOffsetFactor;

    private float? currentPolygonOffsetUnits;

    private bool? currentPremultipliedAlpha;

    private int? currentTexturesSlot;


    private GLExtensions Extensions;

    public GLState(GLExtensions extensions, GLUtils utils, GLCapabilities capabilities)
    {
        Extensions = extensions;
        Utils = utils;
        Capabilities = capabilities;

        newAttributes = new byte[maxVertexAttributes];
        enabledAttributes = new byte[maxVertexAttributes];
        attributeDivisors = new byte[maxVertexAttributes];

        colorBuffer = new GLColorBuffer();

        depthBuffer = new GLDepthBuffer(this);

        stencilBuffer = new GLStencilBuffer(this);

        var glversion = GL.GetString(StringName.Version).Split(' ');

        if (glversion[1].IndexOf("OpenGL ES") > -1)
        {
            float.TryParse(glversion[0].Split('.')[0], out version);
            lineWidthAvailable = version >= 2.0;
        }
        else
        {
            float.TryParse(glversion[0].Split('.')[0], out version);
            lineWidthAvailable = version >= 1.0;
        }

        emptyTextures.Add((int)TextureTarget.Texture2D,
            CreateTexture(TextureTarget.Texture2D, TextureTarget.Texture2D, 1));
        emptyTextures.Add((int)TextureTarget.TextureCubeMap,
            CreateTexture(TextureTarget.TextureCubeMap, TextureTarget.TextureCubeMapPositiveX, 6));

        colorBuffer.SetClear(0, 0, 0, 1);
        depthBuffer.SetClear(1);
        stencilBuffer.SetClear(0);

        Enable((int)EnableCap.DepthTest);
        depthBuffer.SetFunc(Constants.LessEqualDepth);

        SetFlipSided(false);
        SetCullFace(Constants.CullFaceBack);
        Enable((int)EnableCap.CullFace);

        maxTextures = GL.GetInteger(GetPName.MaxCombinedTextureImageUnits);

        buffers.color = colorBuffer;
        buffers.depth = depthBuffer;
        buffers.stencil = stencilBuffer;
    }

    public IGLStateBuffer buffers { get; set; } = new GLStateBuffer();

    public int? currentProgram { get; set; }

    public void InitAttributes()
    {
        for (var i = 0; i < newAttributes.Length; i++) newAttributes[i] = 0;
    }

    public void EnableAttribute(int attribute)
    {
        EnableAttributeAndDivisor(attribute, 0);
    }

    public void EnableAttributeAndDivisor(int attribute, int meshPerAttribute)
    {
        newAttributes[attribute] = 1;

        if (enabledAttributes[attribute] == 0)
        {
            GL.EnableVertexAttribArray(attribute);
            enabledAttributes[attribute] = 1;
        }

        if (attributeDivisors[attribute] != meshPerAttribute) attributeDivisors[attribute] = (byte)meshPerAttribute;
    }

    public void DisableUnusedAttributes()
    {
        for (var i = 0; i < enabledAttributes.Length; i++)
            if (enabledAttributes[i] != newAttributes[i])
            {
                GL.DisableVertexAttribArray(i);
                enabledAttributes[i] = 0;
            }
    }

    public void Enable(int enableCap)
    {
        if (enabledCapabilities.Contains(enableCap))
        {
            var value = (bool)enabledCapabilities[enableCap];
            if (value != true)
            {
                GL.Enable((EnableCap)enableCap);
                enabledCapabilities[enableCap] = true;
            }
        }
        else
        {
            GL.Enable((EnableCap)enableCap);
            enabledCapabilities.Add(enableCap, true);
        }
    }

    public void Disable(int enableCap)
    {
        if (enabledCapabilities.Contains(enableCap))
        {
            var value = (bool)enabledCapabilities[enableCap];
            if (value)
            {
                GL.Disable((EnableCap)enableCap);
                enabledCapabilities[enableCap] = false;
            }
        }
        else
        {
            GL.Disable((EnableCap)enableCap);
            enabledCapabilities.Add(enableCap, false);
        }
    }

    public List<int> GetCompressedTextureFormats()
    {
        if (compressedTextureFormats == null)
        {
            compressedTextureFormats = new List<int>();

            var format = new int[100];
            GL.GetInteger(GetPName.CompressedTextureFormats, format);

            for (var i = 0; i < format.Length; i++)
                if (format[i] != 0)
                    compressedTextureFormats[i] = format[i];
        }

        return compressedTextureFormats;
    }

    public bool UseProgram(int program)
    {
        if (currentProgram != program)
        {
            GL.UseProgram(program);
            currentProgram = program;
            return true;
        }

        return false;
    }

    public void SetBlending(int blending, int? blendEquation = null, int? blendSrc = null, int? blendDst = null,
        int? blendEquationAlpha = null, int? blendSrcAlpha = null, int? blendDstAlpha = null,
        bool? premultipliedAlpha = null)
    {
        if (blending == Constants.NoBlending)
        {
            if (currentBlendingEnabled)
            {
                Disable((int)EnableCap.Blend);
                currentBlendingEnabled = false;
            }

            return;
        }

        if (!currentBlendingEnabled)
        {
            Enable((int)EnableCap.Blend);
            currentBlendingEnabled = true;
        }

        if (blending != Constants.CustomBlending)
        {
            if (blending != currentBlending || premultipliedAlpha != currentPremultipliedAlpha)
            {
                if (currentBlendEquation != Constants.AddEquation || currentBlendEquationAlpha != Constants.AddEquation)
                {
                    GL.BlendEquation(BlendEquationMode.FuncAdd);

                    currentBlendEquation = Constants.AddEquation;
                    currentBlendEquationAlpha = Constants.AddEquation;
                }

                if (premultipliedAlpha == true)
                {
                    if (blending == Constants.NormalBlending)
                        GL.BlendFuncSeparate(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha,
                            BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
                    else if (blending == Constants.AdditiveBlending)
                        GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);
                    else if (blending == Constants.SubtractiveBlending)
                        GL.BlendFuncSeparate(BlendingFactorSrc.Zero, BlendingFactorDest.Zero,
                            BlendingFactorSrc.OneMinusSrcColor, BlendingFactorDest.OneMinusSrcAlpha);
                    else if (blending == Constants.MultiplyBlending)
                        GL.BlendFuncSeparate(BlendingFactorSrc.Zero, BlendingFactorDest.SrcColor,
                            BlendingFactorSrc.Zero, BlendingFactorDest.SrcAlpha);
                    else
                        Trace.TraceError("THREE.GLState : Invalid blending:" + blending);
                }
                else
                {
                    if (blending == Constants.NormalBlending)
                        GL.BlendFuncSeparate(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha,
                            BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
                    else if (blending == Constants.AdditiveBlending)
                        GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
                    else if (blending == Constants.SubtractiveBlending)
                        GL.BlendFunc(BlendingFactorSrc.Zero, BlendingFactorDest.OneMinusSrcAlpha);
                    else if (blending == Constants.MultiplyBlending)
                        GL.BlendFunc(BlendingFactorSrc.Zero, BlendingFactorDest.SrcColor);
                    else
                        Trace.TraceError("THREE.GLState : Invalid blending:" + blending);
                }

                currentBlendSrc = null;
                currentBlendDst = null;
                currentBlendSrcAlpha = null;
                currentBlendDstAlpha = null;

                currentBlending = blending;
                currentPremultipliedAlpha = premultipliedAlpha;
            }

            return;
        }

        // custom blending

        blendEquationAlpha = blendEquationAlpha != null ? blendEquationAlpha : blendEquation;
        blendSrcAlpha = blendSrcAlpha != null ? blendSrcAlpha : blendSrc;
        blendDstAlpha = blendDstAlpha != null ? blendDstAlpha : blendDst;

        if (blendEquation != currentBlendEquation || blendEquationAlpha != currentBlendEquationAlpha)
        {
            GL.BlendEquationSeparate((BlendEquationMode)Utils.Convert(blendEquation.Value),
                (BlendEquationMode)Utils.Convert(blendEquationAlpha.Value));

            currentBlendEquation = blendEquation;
            currentBlendEquationAlpha = blendEquationAlpha;
        }

        if (blendSrc != currentBlendSrc || blendDst != currentBlendDst || blendSrcAlpha != currentBlendSrcAlpha ||
            blendDstAlpha != currentBlendDstAlpha)
        {
            GL.BlendFuncSeparate((BlendingFactorSrc)Utils.Convert(blendSrc.Value),
                (BlendingFactorDest)Utils.Convert(blendDst.Value),
                (BlendingFactorSrc)Utils.Convert(blendSrcAlpha.Value),
                (BlendingFactorDest)Utils.Convert(blendDstAlpha.Value));

            currentBlendSrc = blendSrc;
            currentBlendDst = blendDst;
            currentBlendSrcAlpha = blendSrcAlpha;
            currentBlendDstAlpha = blendDstAlpha;
        }

        currentBlending = blending;
        currentPremultipliedAlpha = null;
    }

    public void SetMaterial(Material material, bool? frontFaceCW = null)
    {
        if (material.Side == Constants.DoubleSide)
            //Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.CullFace);
        else
            //Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.CullFace);

        var flipSided = material.Side == Constants.BackSide;

        if (frontFaceCW != null && frontFaceCW.Value) flipSided = !flipSided;

        SetFlipSided(flipSided);

        if (material.Blending == Constants.NormalBlending && material.Transparent == false)
            SetBlending(Constants.NoBlending);
        else
            SetBlending(material.Blending, material.BlendEquation, material.BlendSrc, material.BlendDst,
                material.BlendEquationAlpha, material.BlendSrcAlpha, material.BlendDstAlpha,
                material.PremultipliedAlpha);

        depthBuffer.SetFunc(material.DepthFunc);
        depthBuffer.SetTest(material.DepthTest);
        depthBuffer.SetMask(material.DepthWrite);
        colorBuffer.SetMask(material.ColorWrite);


        //if (material.DepthTest)
        GL.Enable(EnableCap.DepthTest);
        //else
        //    GL.Disable(EnableCap.DepthTest);

        var stencilWrite = material.StencilWrite;

        stencilBuffer.SetTest(stencilWrite);
        //if (stencilWrite)
        //{
        //    GL.Enable(EnableCap.StencilTest);
        //}
        //else
        //{
        //    GL.Disable(EnableCap.StencilTest);
        //}

        if (stencilWrite)
        {
            stencilBuffer.SetMask(material.StencilWriteMask);
            stencilBuffer.SetFunc(material.StencilFunc, material.StencilRef, material.StencilFuncMask);
            stencilBuffer.SetOp(material.StencilFail, material.StencilZFail, material.StencilZPass);
        }

        SetPolygonOffset(material.PolygonOffset, material.PolygonOffsetFactor, material.PolygonOffsetUnits);
    }

    public void SetFlipSided(bool flipSided)
    {
        if (currentFlipSided != flipSided)
        {
            if (flipSided)
                GL.FrontFace(FrontFaceDirection.Cw);
            else
                GL.FrontFace(FrontFaceDirection.Ccw);

            currentFlipSided = flipSided;
        }
    }

    public void SetCullFace(int cullFace)
    {
        if (cullFace != Constants.CullFaceNone)
        {
            Enable((int)EnableCap.CullFace);
            if (cullFace != currentCullFace)
            {
                if (cullFace == Constants.CullFaceBack)
                    GL.CullFace(CullFaceMode.Back);
                else if (cullFace == Constants.CullFaceFront)
                    GL.CullFace(CullFaceMode.Front);
                else
                    GL.CullFace(CullFaceMode.FrontAndBack);
            }
        }
        else
        {
            Disable((int)EnableCap.CullFace);
        }

        currentCullFace = cullFace;
    }

    public void SetLineWidth(float width)
    {
        if (width != currentLineWidth)
        {
            if (lineWidthAvailable) GL.LineWidth(width);

            currentLineWidth = width;
        }
    }

    public void SetPolygonOffset(bool polygonoffset, float? factor = null, float? units = null)
    {
        if (polygonoffset)
        {
            Enable((int)EnableCap.PolygonOffsetFill);

            if ((factor != null && currentPolygonOffsetFactor != factor) ||
                (units != null && currentPolygonOffsetUnits != units))
            {
                GL.PolygonOffset(factor.Value, units.Value);

                currentPolygonOffsetFactor = factor;
                currentPolygonOffsetUnits = units;
            }
        }
        else
        {
            Disable((int)EnableCap.PolygonOffsetFill);
        }
    }

    public void SetScissorTest(bool scissorTest)
    {
        if (scissorTest)
            Enable((int)EnableCap.ScissorTest);
        else
            Disable((int)EnableCap.ScissorTest);
    }

    public void ActiveTexture(int? glSlot = null)
    {
        if (glSlot == null) glSlot = (int)TextureUnit.Texture0 + maxTextures - 1;

        if (currentTexturesSlot != glSlot)
        {
            var unit = (TextureUnit)Enum.ToObject(typeof(TextureUnit), glSlot);
            //if (glSlot > 34015)
            //{
            //    unit = TextureUnit.Texture31;
            //    glSlot = 34015;
            //}
            GL.ActiveTexture(unit);

            currentTexturesSlot = glSlot;
        }
    }

    public void BindTexture(int glType, int? glTexture)
    {
        if (currentTexturesSlot == null) ActiveTexture();

        var boundTexture = currentBoundTextures.ContainsKey(currentTexturesSlot.Value)
            ? (BoundTexture?)currentBoundTextures[currentTexturesSlot.Value]
            : null;

        if (boundTexture == null)
        {
            boundTexture = new BoundTexture();
            currentBoundTextures.Add(currentTexturesSlot.Value, boundTexture.Value);
        }

        if (boundTexture.Value.type != glType || boundTexture.Value.texture != glTexture)
        {
            var target = (TextureTarget)Enum.ToObject(typeof(TextureTarget), glType);
            GL.BindTexture(target, glTexture != null ? glTexture.Value : emptyTextures[glType]);

            var newBoundTexture = new BoundTexture();
            newBoundTexture.type = glType;
            newBoundTexture.texture = glTexture != null ? glTexture : null;
            currentBoundTextures[currentTexturesSlot.Value] = newBoundTexture;
        }
    }

    public void UnbindTexture()
    {
        BoundTexture? boundTexture = currentBoundTextures[currentTexturesSlot.Value];

        if (boundTexture != null && boundTexture.Value.type != -1)
        {
            var target = (TextureTarget)Enum.ToObject(typeof(TextureTarget), boundTexture.Value.type);
            GL.BindTexture(target, 0);

            var newBoundTexture = new BoundTexture();
            newBoundTexture.type = -1;
            newBoundTexture.texture = -1;
            currentBoundTextures[currentTexturesSlot.Value] = newBoundTexture;
        }
    }

    // Same interface as https://developer.mozilla.org/en-US/docs/Web/API/WebGLRenderingContext/compressedTexImage2D
    public void CompressedTexImage2D(int target, int level, int internalFormat, int width, int height, int border,
        byte[] data)
    {
        //GL.CompressedTexImage2D(target, level, internalFormat, width, height, border, data.Length, data);
        //CompressedTexImage2D(TextureTarget2d target, int level, CompressedInternalFormat internalformat, int width, int height, int border, int imageSize, IntPtr data);
        GL.CompressedTexImage2D((TextureTarget2d)target, level, (CompressedInternalFormat)internalFormat, width, height,
            border, data.Length, data);
    }

    // Same interface as https://developer.mozilla.org/en-US/docs/Web/API/WebGLRenderingContext/texImage2D
    public void TexImage2D(int target, int level, int internalFormat, int width, int height, int border, int format,
        int type, byte[] pixels)
    {
        //GL.TexImage2D(target, level, internalFormat, width, height, border, format, type,pixels);
        GL.TexImage2D((TextureTarget2d)target, level, (TextureComponentCount)internalFormat, width, height, border,
            (PixelFormat)format, (PixelType)type, pixels);
    }

    public void TexImage2D(int target, int level, int internalFormat, int width, int height, int border, int format,
        int type, IntPtr pixels)
    {
        //GL.TexImage2D(target, level, internalFormat, width, height, border, format, type, pixels);
        //TexImage2D(TextureTarget2d target, int level, TextureComponentCount internalformat, int width, int height, int border, PixelFormat format, PixelType type, IntPtr pixels);
        //GL.TexImage2D((TextureTarget2d)target, level, (TextureComponentCount)internalFormat, width, height, border, (PixelFormat)format, (PixelType)type, pixels);
        GL.TexImage2D((TextureTarget2d)target, level, (TextureComponentCount)internalFormat, width, height, border,
            (PixelFormat)format, (PixelType)type, pixels);
        //GL.TexImage2D(target, level, internalformat, width, height, int border, All format, All type, IntPtr pixels);
    }

    public void TexImage3D(int target, int level, int internalFormat, int width, int height, int depth, int border,
        int format, int type, byte[] pixels)
    {
        //GL.TexImage3D(target, level, internalFormat, width, height, depth, border, format, type, pixels);
        //TexImage3D<T9>(TextureTarget3d target, int level, TextureComponentCount internalformat, int width, int height, int depth, int border, PixelFormat format, PixelType type, T9[] pixels) where T9 : struct;
        GL.TexImage3D((TextureTarget3d)target, level, (TextureComponentCount)internalFormat, width, height, depth,
            border, (PixelFormat)format, (PixelType)type, pixels);
    }

    public void Scissor(Vector4 scissor)
    {
        if (!currentScissor.Equals(scissor))
        {
            GL.Scissor((int)scissor.X, (int)scissor.Y, (int)scissor.Z, (int)scissor.W);
            currentScissor.Copy(scissor);
        }
    }

    public void Viewport(Vector4 viewport)
    {
        if (!currentViewport.Equals(viewport))
        {
            GL.Viewport((int)viewport.X, (int)viewport.Y, (int)viewport.Z, (int)viewport.W);
            currentViewport.Copy(viewport);
        }
    }

    public void Reset()
    {
        for (var i = 0; i < enabledAttributes.Length; i++)
        {
            GL.DisableVertexAttribArray(i);
            enabledAttributes[i] = 0;
        }

        enabledCapabilities.Clear();

        compressedTextureFormats = null;

        currentTexturesSlot = null;

        currentBoundTextures.Clear();

        currentProgram = null;

        currentBlending = null;

        currentFlipSided = null;

        currentCullFace = null;

        colorBuffer.Reset();
        depthBuffer.Reset();
        stencilBuffer.Reset();
    }

    private int CreateTexture(TextureTarget type, TextureTarget target, int count)
    {
        var data = new byte[4] { 0, 0, 0, 0 };

        int texture;
        GL.GenTextures(1, out texture);
        GL.BindTexture(type, texture);
        GL.TexParameter(type, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(type, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        for (var i = 0; i < count; i++)
        {
            //IntPtr byteArrayPtr = IntPtr.Zero;
            //Marshal.Copy(byteArrayPtr, data, 0, data.Length);
            var localTarget = (TextureTarget2d)Enum.ToObject(typeof(TextureTarget2d), (int)target + i);

            GL.TexImage2D(localTarget, 0, TextureComponentCount.Rgba, 1, 1, 0, PixelFormat.Rgba, PixelType.UnsignedByte,
                data);
        }

        return texture;
    }

    public override void Dispose()
    {
        Reset();
        base.Dispose();
    }
}