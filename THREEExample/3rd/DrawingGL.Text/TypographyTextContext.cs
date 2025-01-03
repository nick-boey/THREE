//MIT, 2017, Zou Wei(github/zwcloud)

namespace DrawingGL.Text;

/// <summary>
///     Text context based on Typography
/// </summary>
internal class TypographyTextContext
{
    public string FontFamily
    {
        //TODO impl font file resolution
        get;
        set;
    }

    public float FontSize { get; set; }

    public FontStretch FontStretch { get; set; }

    public FontStyle FontStyle { get; set; }

    public FontWeight FontWeight { get; set; }

    public TextAlignment Alignment { get; set; }

    public int MaxWidth { get; set; }

    public int MaxHeight { get; set; }
}