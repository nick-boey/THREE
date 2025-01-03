using Pfim;
using SkiaSharp;
using StbImageSharp;

namespace THREE;

[Serializable]
public class TextureLoader
{
    public static Texture LoadTGA(string filePath)
    {
        var texture = new Texture();

        ImageResult image = null;

        using (var stream = File.OpenRead(filePath))
        {
            image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        }

        if (image != null)
        {
            var data = image.Data;
            for (var i = 0; i < image.Width * image.Height; ++i)
            {
                var r = data[i * 4];
                var g = data[i * 4 + 1];
                var b = data[i * 4 + 2];
                var a = data[i * 4 + 3];

                data[i * 4] = b;
                data[i * 4 + 1] = g;
                data[i * 4 + 2] = r;
                data[i * 4 + 3] = a;
            }

            //Bitmap bmp = new Bitmap(image.Width, image.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            //BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, image.Width, image.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, bmp.PixelFormat);

            //Marshal.Copy(data, 0, bmpData.Scan0, bmpData.Stride * bmp.Height);
            //bmp.UnlockBits(bmpData);
            // create an empty bitmap

            var bmp = data.ToSKBitMap(image.Width, image.Height);

            texture.Image = bmp;
            texture.Format = Constants.RGBFormat;
            texture.NeedsUpdate = true;
        }

        return texture;
    }

    public static Texture LoadDDS(string filePath)
    {
        var image = Pfimage.FromFile(filePath);

        //var data = Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0);
        //Bitmap bitmap = new Bitmap(image.Width, image.Height, image.Stride, System.Drawing.Imaging.PixelFormat.Format32bppArgb, data);
        //bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
        var texture = new Texture();
        texture.Image =
            image.Data.ToSKBitMap(image.Width,
                image.Height); // TOSKBitmap() is defined in THREE.Extensions.ImageExtension.cs
        texture.Format = Constants.RGBFormat;
        texture.NeedsUpdate = true;

        return texture;
    }

    public static Texture Load(string filePath)
    {
        var bitmap = SKBitmap.Decode(filePath);
        //bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
        var texture = new Texture();
        texture.Image = bitmap;
        texture.Format = Constants.RGBAFormat;
        texture.NeedsUpdate = true;

        return texture;
    }

    public static Texture LoadEmbedded(string EmbeddedPath)
    {
        var embeddedNameBase = "THREE.Resources.";
        //Bitmap bitmap = new Bitmap(typeof(THREE.Object3D).GetTypeInfo().Assembly.GetManifestResourceStream(embeddedNameBase + EmbeddedPath));

        //bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

        var texture = new Texture();
        texture.Image = SKBitmap.Decode(embeddedNameBase + EmbeddedPath);
        texture.Format = Constants.RGBFormat;
        texture.NeedsUpdate = true;

        return texture;
    }
}