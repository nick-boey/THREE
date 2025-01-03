using SkiaSharp;

namespace THREE;

[Serializable]
public class CubeTextureLoader
{
    public static CubeTexture Load(List<string> filePath)
    {
        var texture = new CubeTexture();
        for (var i = 0; i < filePath.Count; i++)
        {
            var bitmap = SKBitmap.Decode(filePath[i]);
            //bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

            var image = new Texture();
            image.Image = bitmap;
            image.Format = Constants.RGBFormat;
            image.NeedsUpdate = true;

            texture.Images[i] = image;
        }

        texture.NeedsUpdate = true;
        return texture;
    }
}