//MIT, 2016-present, WinterDev

using PixelFarm.Drawing;
using Typography.OpenFont;
using Typography.TextLayout;

namespace Typography.Contours;

/// <summary>
///     base TextPrinter class
/// </summary>
public abstract class TextPrinterBase
{
    private float _fontSizeInPoints;

    public TextPrinterBase()
    {
        FontSizeInPoints = 14; //
        ScriptLang = new ScriptLang("latn");
    }


    public abstract GlyphLayout GlyphLayoutMan { get; }
    public abstract Typeface Typeface { get; set; }
    public bool FillBackground { get; set; }
    public bool DrawOutline { get; set; }
    public float FontAscendingPx { get; set; }
    public float FontDescedingPx { get; set; }
    public float FontLineGapPx { get; set; }
    public float FontLineSpacingPx { get; set; }
    public TextBaseline TextBaseline { get; set; }

    public bool SimulateSlant { get; set; }

    public HintTechnique HintTechnique { get; set; }

    public float FontSizeInPoints
    {
        get => _fontSizeInPoints;
        set
        {
            if (_fontSizeInPoints != value)
            {
                _fontSizeInPoints = value;
                OnFontSizeChanged();
            }
        }
    }

    public ScriptLang ScriptLang { get; set; }
    public PositionTechnique PositionTechnique { get; set; }
    public bool EnableLigature { get; set; }

    public virtual void GenerateGlyphPlan(
        char[] textBuffer,
        int startAt,
        int len,
        IUnscaledGlyphPlanList unscaledGlyphPlan)
    {
        var glyphLayout = GlyphLayoutMan;
        glyphLayout.Layout(textBuffer, startAt, len);
        glyphLayout.GenerateUnscaledGlyphPlans(unscaledGlyphPlan);
    }

    protected virtual void OnFontSizeChanged()
    {
    }

    /// <summary>
    ///     draw string at (xpos,ypos), depend on baseline
    /// </summary>
    /// <param name="textBuffer"></param>
    /// <param name="startAt"></param>
    /// <param name="len"></param>
    /// <param name="left"></param>
    /// <param name="top"></param>
    public abstract void DrawString(char[] textBuffer, int startAt, int len, float left, float top);

    /// <summary>
    ///     draw glyph plan list at (xpos,ypos) of baseline
    /// </summary>
    /// <param name="glyphPlanList"></param>
    /// <param name="left"></param>
    /// <param name="top"></param>
    public abstract void DrawFromGlyphPlans(GlyphPlanSequence glyphPlanList, int startAt, int len, float left,
        float top);


    //helper methods
    public void DrawString(char[] textBuffer, float left, float top)
    {
        DrawString(textBuffer, 0, textBuffer.Length, left, top);
    }

    public void DrawFromGlyphPlans(GlyphPlanSequence glyphPlanSeq, float left, float top)
    {
        DrawFromGlyphPlans(glyphPlanSeq, 0, glyphPlanSeq.Count, left, top);
    }
}