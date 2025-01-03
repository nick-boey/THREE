namespace THREE;

[Serializable]
public class CubeTexture : Texture
{
    public CubeTexture()
    {
        Mapping = Constants.CubeReflectionMapping;
        Format = Constants.RGBAFormat;

        flipY = false;
    }

    public CubeTexture(Texture[] images, int? mapping = null, int? wrapS = null, int? wrapT = null,
        int? magFilter = null, int? minFilter = null, int? format = null, int? type = null, int? anisotropy = null,
        int? encoding = null)
        : base(null, mapping, wrapS, wrapT, magFilter, minFilter, format, type, anisotropy, encoding)
    {
        if (images != null && images.Length > 0)
            Images = images;


        Mapping = mapping != null ? mapping.Value : Constants.CubeReflectionMapping;
        Format = format != null ? format.Value : Constants.RGBAFormat;
        NeedsFlipEnvMap = true;
        flipY = false;
    }
}