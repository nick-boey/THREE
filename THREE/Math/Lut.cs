using System.Collections;
using SkiaSharp;

namespace THREE;

[Serializable]
public class Lut
{
    public Hashtable ColorMapKeywords;
    private List<Color> lut = new();
    private List<float[]> map = new();
    private float minV, maxV;
    public int? n;

    public Lut(string colorMap = null, int? numberofcolors = null)
    {
        minV = 0;
        maxV = 1;

        ColorMapKeywords = new Hashtable
        {
            {
                "rainbow",
                new List<float[]>
                {
                    new[] { 0.0f, 0x0000FF }, new[] { 0.2f, 0x00FFFF }, new[] { 0.5f, 0x00FF00 },
                    new[] { 0.8f, 0xFFFF00 }, new[] { 1.0f, 0xFF0000 }
                }
            },
            {
                "cooltowarm",
                new List<float[]>
                {
                    new[] { 0.0f, 0x3C4EC2 }, new[] { 0.2f, 0x9BBCFF }, new[] { 0.5f, 0xDCDCDC },
                    new[] { 0.8f, 0xF6A385 }, new[] { 1.0f, 0xB40426 }
                }
            },
            {
                "blackbody",
                new List<float[]>
                {
                    new[] { 0.0f, 0x000000 }, new[] { 0.2f, 0x780000 }, new[] { 0.5f, 0xE63200 },
                    new[] { 0.8f, 0xFFFF00 }, new[] { 1.0f, 0xFFFFFF }
                }
            },
            {
                "grayscale",
                new List<float[]>
                {
                    new[] { 0.0f, 0x000000 }, new[] { 0.2f, 0x404040 }, new[] { 0.5f, 0x7F7F80 },
                    new[] { 0.8f, 0xBFBFBF }, new[] { 1.0f, 0xFFFFFF }
                }
            }
        };

        SetColorMap(colorMap, numberofcolors);
    }

    public Lut Set(Lut value)
    {
        if (value is Lut) Copy(value);

        return this;
    }

    public void SetMin(float min)
    {
        minV = min;
    }

    public void SetMax(float max)
    {
        maxV = max;
    }

    public Lut SetColorMap(string colormap, int? numberofcolors)
    {
        if (colormap != null && ColorMapKeywords.ContainsKey(colormap))
            map = (List<float[]>)ColorMapKeywords[colormap];
        else
            map = (List<float[]>)ColorMapKeywords["rainbow"];

        n = numberofcolors != null ? numberofcolors.Value : 32;

        var step = 1.0 / n.Value;

        lut.Clear();

        for (var i = 0.0; i <= 1.0; i += step)
        for (var j = 0; j < map.Count - 1; j++)
            if (i >= map[j][0] && i < map[j + 1][0])
            {
                var min = map[j][0];
                var max = map[j + 1][0];

                var minColor = new Color(map[j][1]);
                var maxColor = new Color(map[j + 1][1]);

                var color = minColor.Lerp(maxColor, (float)((i - min) / (max - min)));

                lut.Add(color);
            }

        return this;
    }

    public Lut Copy(Lut lut)
    {
        this.lut = lut.lut;
        map = lut.map;
        n = lut.n;
        minV = lut.minV;
        maxV = lut.maxV;
        this.lut = new List<Color>(lut.lut);

        return this;
    }

    public Color GetColor(float alpha)
    {
        if (alpha <= minV)
            alpha = minV;
        else if (alpha >= maxV) alpha = maxV;

        alpha = (alpha - minV) / (maxV - minV);

        var colorPosition = (int)Math.Round(alpha * n.Value);

        if (colorPosition == n.Value)
            colorPosition -= 1;

        return lut[colorPosition];
    }

    public void AddColorMap(string colormapName, List<float[]> arrayOfColors)
    {
        ColorMapKeywords[colormapName] = arrayOfColors;
    }

    public Texture CreateTexture()
    {
        var texture = new Texture();
        var bitmap = new SKBitmap(1, n.Value);
        texture.Image = bitmap;
        texture.ImageSize.Width = 1;
        texture.ImageSize.Height = n.Value;
        texture.Format = Constants.RGBAFormat;
        texture.NeedsUpdate = true;

        UpdateTexture(texture);

        return texture;
    }

    public void UpdateTexture(Texture texture)
    {
        var data = new byte[4 * n.Value];

        var k = 0;

        var step = 1.0 / n.Value;

        for (var i = 1.0; i >= 0.0; i -= step)
        for (var j = map.Count - 1; j >= 0; j--)
            if (i < map[j][0] && i >= map[j - 1][0])
            {
                var min = map[j - 1][0];
                var max = map[j][0];

                var minColor = new Color(map[j - 1][1]);
                var maxColor = new Color(map[j][1]);

                var color = minColor.Lerp(maxColor, (float)((i - min) / (max - min)));

                data[k * 4] = (byte)Math.Round(color.R * 255);
                data[k * 4 + 1] = (byte)Math.Round(color.G * 255);
                data[k * 4 + 2] = (byte)Math.Round(color.B * 255);
                data[k * 4 + 3] = 255;

                k += 1;
            }

        texture.Image = data.ToSKBitMap(1, n.Value);
    }
}