using SkiaSharp;

namespace THREE;

[Serializable]
public class DataTexture : Texture
{
    public byte[] byteData;
    public float[] floatData;
    public int[] intData;

    public DataTexture()
    {
    }

    public DataTexture(SKBitmap image, int width, int height, int format, int type, int? mapping = null,
        int? wrapS = null, int? wrapT = null, int? magFilter = null, int? minFilter = null, int? anisotropy = null,
        int? encoding = null)
        : base(image, mapping, wrapS, wrapT, magFilter, minFilter, format, type, anisotropy, encoding)
    {
        MagFilter = magFilter != null ? (int)magFilter : Constants.NearestFilter;
        MinFilter = minFilter != null ? (int)minFilter : Constants.NearestFilter;

        GenerateMipmaps = false;
        flipY = false;
        UnpackAlignment = 1;

        ImageSize.Width = width;
        ImageSize.Height = height;
        NeedsUpdate = true;
    }
}