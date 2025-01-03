using System.Drawing;
using SkiaSharp;

namespace THREE;

[Serializable]
public class MipMap
{
    public byte[] Data;

    public int Height;

    public int Width;

    public MipMap()
    {
    }

    public MipMap(MipMap other)
    {
        if (other.Data.Length > 0)
            Data = other.Data.ToArray();
    }

    public MipMap Clone()
    {
        return new MipMap(this);
    }
}

[Serializable]
public class Texture : DisposableObject, ICloneable
{
    #region Static Fields

    protected static int TextureIdCount;

    #endregion

    public virtual object Clone()
    {
        return new Texture(this);
    }

    public void UpdateMatrix()
    {
        Matrix.SetUvTransform(Offset.X, Offset.Y, Repeat.X, Repeat.Y, Rotation, Center.X, Center.Y);
    }

    #region Fields

    public int Id = TextureIdCount++;

    public Guid Uuid = Guid.NewGuid();

    public string Name = "";

    public Size Resolution { get; protected set; } // this fields are not existed in three.js

    //public TextureTarget TextureTarget { get; private set; }// this fields are not existed in three.js

    public int TextureAddress { get; protected set; }


    public SKBitmap Image;

    public Texture[] Images = { null, null, null, null, null, null };

    public List<MipMap> Mipmaps = new();

    public Size ImageSize;

    public int Mapping = Constants.UVMapping;

    public int WrapS;
    public int WrapT;
    public int WrapR;

    public int MagFilter;
    public int MinFilter;
    public int MaxFilter;

    public float Anisotropy;

    public int Format = Constants.RGBAFormat;
    public int Type;

    public Vector2 Offset = new(0, 0);
    public Vector2 Repeat = new(1, 1);
    public Vector2 Center = new(0, 0);
    public float Rotation = 0;

    public bool MatrixAutoUpdate = true;
    public Matrix3 Matrix = new();

    public bool GenerateMipmaps = true;
    public bool PremultiplyAlpha = false;
    public bool flipY = true;
    public int UnpackAlignment = 4;

    private bool needsUpdate;

    public bool NeedsUpdate
    {
        get => needsUpdate;
        set
        {
            needsUpdate = value;
            if (value) version++;
        }
    }

    public string InternalFormat;

    public int Encoding = Constants.LinearEncoding;

    public int version;

    public string SourceFilePath;


    private readonly int defaultMapping = Constants.UVMapping;

    public bool NeedsFlipEnvMap = false;

    #endregion


    //public bool __glInit = false;

    //public int __glTexture { get; set; }

    //public int __version;

    #region Constructors and Destructors

    public Texture()
    {
        Anisotropy = 1;

        WrapS = Constants.ClampToEdgeWrapping;

        WrapT = Constants.ClampToEdgeWrapping;

        MagFilter = Constants.LinearFilter;

        MinFilter = Constants.LinearMipMapLinearFilter;

        Type = Constants.UnsignedByteType;
    }

    //public Texture(string bitmapPath, bool flipY = true)
    //{
    //    this.SourceFilePath = bitmapPath;

    //    using (var bitmap = Bitmap.FromFile(bitmapPath) as Bitmap)
    //    {
    //        HandleLoadingBitmapData(bitmap, flipY);
    //    }
    //}
    /// <summary>
    ///     Constructor
    /// </summary>
    public Texture(SKBitmap image = null, int? mapping = null, int? wrapS = null, int? wrapT = null,
        int? magFilter = null, int? minFilter = null, int? format = null, int? type = null, int? anisotropy = null,
        int? encoding = null)
        : this()
    {
        Image = image;

        Mapping = mapping != null ? (int)mapping : defaultMapping;

        WrapS = wrapS != null ? (int)wrapS : Constants.ClampToEdgeWrapping;
        WrapT = wrapT != null ? (int)wrapT : Constants.ClampToEdgeWrapping;

        MagFilter = magFilter != null ? (int)magFilter : Constants.LinearFilter;
        MinFilter = minFilter != null ? (int)minFilter : Constants.LinearMipmapLinearFilter;

        Anisotropy = anisotropy != null ? (int)anisotropy : 1;

        Format = format != null ? (int)format : Constants.RGBAFormat;
        InternalFormat = null;
        Type = type != null ? (int)type : Constants.UnsignedByteType;

        Encoding = encoding != null ? (int)encoding : Constants.LinearEncoding;
    }

    /// <summary>
    ///     Copy Constructor
    /// </summary>
    /// <param name="other"></param>
    protected Texture(Texture other) : this()
    {
        Image = other.Image;
        //this.Image = other.Image!=null ? (Bitmap)other.Image.Clone() : null;

        ImageSize = other.ImageSize;
        Images = other.Images;
        //if(other.Images.Length>0)
        //{
        //    for (int i = 0; i < other.Images.Length; i++)
        //        this.Images[i] = other.Images[i] != null ? (Texture)other.Images[i].Clone() : null;
        //}

        Mipmaps = other.Mipmaps;
        //this.Mipmaps = other.Mipmaps.Select(item => (MipMap)item.Clone()).ToList();

        Mapping = other.Mapping;

        WrapS = other.WrapS;
        WrapT = other.WrapT;

        MagFilter = other.MagFilter;
        MinFilter = other.MinFilter;

        Anisotropy = other.Anisotropy;

        Format = other.Format;
        InternalFormat = other.InternalFormat;
        Type = other.Type;

        Encoding = other.Encoding;

        version = other.version;
    }

    #endregion
}