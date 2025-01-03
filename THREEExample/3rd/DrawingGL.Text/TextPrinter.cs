//MIT, 2017, Zou Wei(github/zwcloud), WinterDev

using System;
using Tesselate;
using Typography.Contours;
using Typography.OpenFont;
using Typography.TextLayout;

namespace DrawingGL.Text;

/// <summary>
///     text printer
/// </summary>
internal class TextPrinter : TextPrinterBase
{
    //
    // for tess
    // 
    private readonly SimpleCurveFlattener _curveFlattener;

    private readonly GlyphMeshCollection<ProcessedGlyph> _glyphMeshCollection = new();


    private readonly UnscaledGlyphPlanList _resuableGlyphPlanList = new();
    private readonly TessTool _tessTool;
    private string _currentFontFile;
    private GlyphOutlineBuilder _currentGlyphPathBuilder;

    private Typeface _currentTypeface;

    //funcs:
    //1. layout glyph
    //2. measure glyph
    //3. generate glyph runs into textrun 
    private GlyphTranslatorToPath _pathTranslator;

    //-------------
    public TextPrinter()
    {
        FontSizeInPoints = 14;

        ScriptLang = new ScriptLang("latn");

        //
        _curveFlattener = new SimpleCurveFlattener();

        _tessTool = new TessTool();
    }

    public override GlyphLayout GlyphLayoutMan { get; } = new();

    public override Typeface Typeface
    {
        get => _currentTypeface;
        set
        {
            _currentTypeface = value;
            GlyphLayoutMan.Typeface = value;
        }
    }

    /// <summary>
    ///     Font file path
    /// </summary>
    public string FontFilename
    {
        get => _currentFontFile;
        set
        {
            if (_currentFontFile != value)
            {
                _currentFontFile = value;

                //TODO: review here
                using (var stream = Utility.ReadFile(value))
                {
                    var reader = new OpenFontReader();
                    Typeface = reader.Read(stream);
                }

                //2. glyph builder
                _currentGlyphPathBuilder = new GlyphOutlineBuilder(Typeface);
                _currentGlyphPathBuilder.UseTrueTypeInstructions = false; //reset
                _currentGlyphPathBuilder.UseVerticalHinting = false; //reset
                switch (HintTechnique)
                {
                    case HintTechnique.TrueTypeInstruction:
                        _currentGlyphPathBuilder.UseTrueTypeInstructions = true;
                        break;
                    case HintTechnique.TrueTypeInstruction_VerticalOnly:
                        _currentGlyphPathBuilder.UseTrueTypeInstructions = true;
                        _currentGlyphPathBuilder.UseVerticalHinting = true;
                        break;
                    case HintTechnique.CustomAutoFit:
                        //custom agg autofit 
                        break;
                }

                //3. glyph translater
                _pathTranslator = new GlyphTranslatorToPath();

                //4. Update GlyphLayout
                GlyphLayoutMan.ScriptLang = ScriptLang;
                GlyphLayoutMan.PositionTechnique = PositionTechnique;
                GlyphLayoutMan.EnableLigature = EnableLigature;
            }
        }
    }


    public override void DrawFromGlyphPlans(GlyphPlanSequence glyphPlanList, int startAt, int len, float x, float y)
    {
        throw new NotImplementedException();
    }

    public MeasuredStringBox Measure(char[] textBuffer, int startAt, int len)
    {
        return GlyphLayoutMan.LayoutAndMeasureString(
            textBuffer, startAt, len,
            FontSizeInPoints
        );
    }

    /// <summary>
    ///     generate glyph run into a given textRun
    /// </summary>
    /// <param name="outputTextRun"></param>
    /// <param name="charBuffer"></param>
    /// <param name="start"></param>
    /// <param name="len"></param>
    public void GenerateGlyphRuns(TextRun outputTextRun, char[] charBuffer, int start, int len)
    {
        // layout glyphs with selected layout technique
        var sizeInPoints = FontSizeInPoints;
        outputTextRun.typeface = Typeface;
        outputTextRun.sizeInPoints = sizeInPoints;

        //in this version we store original glyph into the mesh collection
        //and then we scale it later, so I just specific font size=0 (you can use any value)
        _glyphMeshCollection.SetCacheInfo(Typeface, 0, HintTechnique);


        GlyphLayoutMan.Typeface = Typeface;
        GlyphLayoutMan.Layout(charBuffer, start, len);

        var pxscale = Typeface.CalculateScaleToPixelFromPointSize(sizeInPoints);

        _resuableGlyphPlanList.Clear();
        GenerateGlyphPlan(charBuffer, 0, charBuffer.Length, _resuableGlyphPlanList);

        // render each glyph 
        var planCount = _resuableGlyphPlanList.Count;
        for (var i = 0; i < planCount; ++i)
        {
            _pathTranslator.Reset();
            //----
            //glyph path 
            //---- 
            var glyphPlan = _resuableGlyphPlanList[i];
            //
            //1. check if we have this glyph in cache?
            //if yes, not need to build it again 


            if (!_glyphMeshCollection.TryGetCacheGlyph(glyphPlan.glyphIndex, out var processGlyph))
            {
                //if not found the  create a new one and register it
                var writablePath = new WritablePath();
                _pathTranslator.SetOutput(writablePath);
                _currentGlyphPathBuilder.BuildFromGlyphIndex(glyphPlan.glyphIndex, sizeInPoints);
                _currentGlyphPathBuilder.ReadShapes(_pathTranslator);

                //-------
                //do tess   
                var flattenPoints = _curveFlattener.Flatten(writablePath._points, out var endContours);

                var tessData = _tessTool.TessAsTriVertexArray(flattenPoints, endContours, out var vertexCount);
                processGlyph = new ProcessedGlyph(tessData, (ushort)vertexCount);

                _glyphMeshCollection.RegisterCachedGlyph(glyphPlan.glyphIndex, processGlyph);
            }

            outputTextRun.AddGlyph(
                new GlyphRun(glyphPlan,
                    processGlyph.tessData,
                    processGlyph.vertextCount));
        }
    }

    public override void DrawString(char[] textBuffer, int startAt, int len, float x, float y)
    {
    }

    //-------------
    private struct ProcessedGlyph
    {
        public readonly float[] tessData;
        public readonly ushort vertextCount;

        public ProcessedGlyph(float[] tessData, ushort vertextCount)
        {
            this.tessData = tessData;
            this.vertextCount = vertextCount;
        }
    }
}