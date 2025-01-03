using System.Collections;

namespace THREE;

[Serializable]
public class GLRenderTarget : Texture, ICloneable
{
    public bool depthBuffer;

    public DepthTexture depthTexture;

    public int Height;
    //protected static int RenderTargetIdCount;

    public bool IsGLMultiviewRenderTarget = false;

    public int NumViews = 0;

    //public int Id = RenderTargetIdCount++;

    //public Guid Uuid = Guid.NewGuid();

    public Hashtable Options;

    public Vector4 Scissor;

    public bool ScissorTest;

    public bool stencilBuffer;

    public Texture Texture;

    public Vector4 Viewport;

    public int Width;

    public GLRenderTarget(int width, int height, Hashtable options = null)
    {
        Width = width;

        Height = height;

        if (options != null)
            Options = (Hashtable)options;
        else
            Options = new Hashtable();

        Scissor = new Vector4(0, 0, width, height);
        ScissorTest = false;
        Viewport = new Vector4(0, 0, width, height);

        Texture = new Texture(null, null, (int?)Options["wrapS"], (int?)Options["wrapT"], (int?)Options["magFilter"],
            (int?)Options["minFilter"], (int?)Options["format"], (int?)Options["type"], (int?)Options["anisotropy"],
            (int?)Options["encoding"]);

        Texture.ImageSize.Width = width;
        Texture.ImageSize.Height = height;

        Texture.GenerateMipmaps = Options["generateMipmaps"] != null ? (bool)Options["generateMipmaps"] : false;
        Texture.MinFilter = Options["minFilter"] != null ? (int)Options["minFilter"] : Constants.LinearFilter;

        depthBuffer = Options["depthBuffer"] != null ? (bool)Options["depthBuffer"] : true;
        stencilBuffer = Options["stencilBuffer"] != null ? (bool)Options["stencilBuffer"] : true;
        depthTexture = Options["depthTexture"] != null ? (DepthTexture)Options["depthTexture"] : null;
    }

    protected GLRenderTarget(GLRenderTarget source)
    {
        Width = source.Width;
        Height = source.Height;

        Scissor = source.Scissor;
        ScissorTest = source.ScissorTest;
        Viewport = source.Viewport;


        Texture = (Texture)source.Texture.Clone();

        depthBuffer = source.depthBuffer;
        stencilBuffer = source.stencilBuffer;
        depthTexture = source.depthTexture;
    }

    public new object Clone()
    {
        return new GLRenderTarget(this);
    }

    public void SetSize(int width, int height)
    {
        if (Width != width || Height != height)
        {
            Width = width;
            Height = height;

            Texture.ImageSize.Width = width;
            Texture.ImageSize.Height = height;
        }
    }
}