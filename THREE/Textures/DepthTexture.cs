namespace THREE;

[Serializable]
public class DepthTexture : Texture
{
    public DepthTexture(int width, int height, int? type, int? mapping = null, int? wrapS = null, int? wrapT = null,
        int? magFilter = null, int? minFilter = null, int? anisotropy = null, int? format = null)
        : base(null, mapping, wrapS, wrapT, magFilter, minFilter, format, anisotropy)
    {
        Format = format != null ? (int)format : Constants.DepthFormat;

        if (Format != Constants.DepthFormat && Format != Constants.DepthStencilFormat)
            throw new Exception(
                "DepthTexture format must be either Constants.DepthFormat or Constants.DepthStencilFormat");

        if (type == 0 && Format == Constants.DepthFormat) Type = Constants.UnsignedShortType;
        if (type == 0 && Format == Constants.DepthStencilFormat) Type = Constants.UnsignedInt248Type;

        ImageSize.Width = width;
        ImageSize.Height = height;

        flipY = false;
        GenerateMipmaps = false;
    }
}