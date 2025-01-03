using SkiaSharp;

namespace THREE;

[Serializable]
public class VideoTexture : Texture
{
    public VideoTexture(SKBitmap video = null, int? mapping = null, int wrapS = 0, int wrapT = 0, int magFilter = 0,
        int minFilter = 0, int format = 0, int type = 0, int anisotropy = 1)
        : base(video, mapping, wrapS, wrapT, magFilter, minFilter, format, type, anisotropy)
    {
        GenerateMipmaps = false;
    }

    public void Update()
    {
        var video = Image;

        NeedsUpdate = true;
    }
}