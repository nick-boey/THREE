namespace THREE;

[Serializable]
public class CompressedTexture : Texture
{
    private int Height;
    private int Width;

    public CompressedTexture(List<MipMap> mipmaps, int width, int height, int? mapping = null, int? wrapS = null,
        int? wrapT = null, int? magFilter = null, int? minFilter = null, int? format = null, int? type = null,
        int? anisotropy = null, int? encoding = null) :
        base(null, mapping, wrapS, wrapT, magFilter, minFilter, format, type, anisotropy, encoding)
    {
        Width = width;
        Height = height;
        Mipmaps = mipmaps;

        flipY = false;

        GenerateMipmaps = false;
    }
}