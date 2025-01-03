using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using THREE;
using Typography.Contours;
using Typography.OpenFont;
using Typography.TextLayout;

namespace THREEExample.Three.Loaders;

internal class GlyphTranslatorToPath : IGlyphTranslator
{
    private float _lastMoveX;
    private float _lastMoveY;
    private float _lastX;
    private float _lastY;
    private float offsetX;
    private float offsetY;
    private StringBuilder stbuilder;
    public ShapePath shapePath { get; set; }

    public void BeginRead(int contourCount)
    {
    }

    public void Curve3(float x1, float y1, float x2, float y2)
    {
        shapePath.QuadraticCurveTo(x1 + offsetX, y1 + offsetY, _lastX = x2 + offsetX, _lastY = y2 + offsetY);
        if (stbuilder != null)
            stbuilder.AppendLine(
                string.Format("quad_bezier_to c1({0:0.00}, {1:0.00}) end ({2:0.00}, {3:0.00})",
                    x1 + offsetX, y1 + offsetY, x2 + offsetX, y2 + offsetY));
    }

    public void Curve4(float x1, float y1, float x2, float y2, float x3, float y3)
    {
        shapePath.BezierCurveTo(x1 + offsetX, y1 + offsetY, x2 + offsetX, y2 + offsetY, _lastX = x3 + offsetX,
            _lastY = y3 + offsetY);
        if (stbuilder != null)
            stbuilder.AppendLine(
                string.Format(
                    "cubic_bezier_to c0 ({0:0.00}, {1:0.00}) c1 ({2:0.00}, {3:0.00}) end ({4:0.00}, {5:0.00})",
                    x1 + offsetX, y1 + offsetY, x2 + offsetX, y2 + offsetY, x3 + offsetX, y3 + offsetY));
    }

    public void EndRead()
    {
    }

    public void LineTo(float x1, float y1)
    {
        shapePath.LineTo(x1 + offsetX, y1 + offsetY);
    }

    public void MoveTo(float x0, float y0)
    {
        _lastX = _lastMoveX = x0 + offsetX;
        _lastY = _lastMoveY = y0 + offsetY;
        shapePath.MoveTo(x0 + offsetX, y0 + offsetY);
        if (stbuilder != null)
            stbuilder.AppendLine(string.Format("move_to ({0:0.00}, {1:0.00})", x0 + offsetX, y0 + offsetY));
    }

    public void CloseContour()
    {
    }

    public void SetOutput(ShapePath shapePath, StringBuilder stbuilder = null)
    {
        this.shapePath = shapePath;
        this.stbuilder = stbuilder;
    }

    public void CloseFigure()
    {
    }

    public void Reset()
    {
        shapePath = null;
        _lastMoveX = _lastMoveY = _lastX = _lastY;
    }

    internal void SetOffset(float nx, float ny)
    {
        offsetX = nx;
        offsetY = ny;
    }
}

public class TTFFont
{
    private readonly UnscaledGlyphPlanList resuableGlyphPlanList = new();

    private string fontFile;
    private float fontSizeInPoints;

    private GlyphOutlineBuilder glyphPathBuilder;

    private GlyphTranslatorToPath pathTranslator;
    private Typeface typeface;

    public TTFFont(string filePath)
    {
        FontFile = filePath;

#if DEBUG
        Debug.WriteLine("font.name {0}", typeface.NameEntry.FontName);
        Debug.WriteLine("ascender {0}", Asender);
        Debug.WriteLine("descender {0}", Descender);
        Debug.WriteLine("family name {0}", FamilyName);
        Debug.WriteLine("bounding box xmin {0}, xmax {1}, ymin {2}, ymax{3}", Bounds.XMin, Bounds.XMax, Bounds.YMin,
            Bounds.YMax);
        Debug.WriteLine("underlinePosition {0}", UnderlinePosition);
        Debug.WriteLine("underlineThickness {0}", UnderlineThickness);
#endif
    }

    public string FamilyName => typeface.FontSubFamily;

    public string styleName => typeface.FontSubFamily;

    public ushort UnitPerEm => typeface.UnitsPerEm;

    public int Asender => typeface.Ascender;

    public int Descender => typeface.Descender;

    public short UnderlinePosition => typeface.PostTable.UnderlinePosition;
    public short UnderlineThickness => typeface.PostTable.UnderlineThickness;

    public GlyphLayout GlyphLayoutMan { get; } = new();

    public Bounds Bounds => typeface.Bounds;

    public Typeface Typeface
    {
        get => typeface;
        set
        {
            typeface = value;
            GlyphLayoutMan.Typeface = value;
        }
    }

    public HintTechnique HintTechnique { get; set; }

    public ScriptLang ScriptLang { get; set; }
    public PositionTechnique PositionTechnique { get; set; }
    public bool EnableLigature { get; set; }

    public string FontFile
    {
        get => fontFile;
        set
        {
            if (fontFile != value)
            {
                fontFile = value;

                //TODO: review here
                using (var stream = new FileStream(value, FileMode.Open))
                {
                    var reader = new OpenFontReader();
                    Typeface = reader.Read(stream);
                }

                //2. glyph builder
                glyphPathBuilder = new GlyphOutlineBuilder(Typeface);
                glyphPathBuilder.UseTrueTypeInstructions = false; //reset
                glyphPathBuilder.UseVerticalHinting = false; //reset
                switch (HintTechnique)
                {
                    case HintTechnique.TrueTypeInstruction:
                        glyphPathBuilder.UseTrueTypeInstructions = true;
                        break;
                    case HintTechnique.TrueTypeInstruction_VerticalOnly:
                        glyphPathBuilder.UseTrueTypeInstructions = true;
                        glyphPathBuilder.UseVerticalHinting = true;
                        break;
                    case HintTechnique.CustomAutoFit:
                        //custom agg autofit 
                        break;
                }

                //3. glyph translater
                pathTranslator = new GlyphTranslatorToPath();

                //4. Update GlyphLayout
                GlyphLayoutMan.ScriptLang = ScriptLang;
                GlyphLayoutMan.PositionTechnique = PositionTechnique;
                GlyphLayoutMan.EnableLigature = EnableLigature;
            }
        }
    }

    public float FontSizeInPoints
    {
        get => fontSizeInPoints;
        set
        {
            if (fontSizeInPoints != value) fontSizeInPoints = value;
        }
    }

    public static TTFFont Load(string ttfFilePath)
    {
        return new TTFFont(ttfFilePath);
    }

    public TextBufferGeometry CreateTextGeometry(string text, Hashtable parameters)
    {
        if (parameters.ContainsKey("size")) FontSizeInPoints = Convert.ToSingle((int)parameters["size"]);
        if (!parameters.ContainsKey("bevelThickness") || parameters["bevelThickness"] == null)
            parameters["bevelThickness"] = 10;
        if (!parameters.ContainsKey("bevelSize") || parameters["bevelSize"] == null) parameters["bevelSize"] = 8;
        if (!parameters.ContainsKey("bevelEnabled") || parameters["bevelEnabled"] == null)
            parameters["bevelEnabled"] = false;
        parameters["depth"] = parameters.ContainsKey("height") ? (int)parameters["height"] : 50;

        var shapes = GenerateShapes(text, FontSizeInPoints);

        var geometry = new TextBufferGeometry();

        geometry.Init(shapes, parameters);

        return geometry;
    }

    public List<Shape> GenerateShapes(string text, float? size)
    {
        text = text.Replace("\\n", "\n");
        if (size == null) FontSizeInPoints = 100;
        else FontSizeInPoints = size.Value;

        var shapes = new List<Shape>();
        var paths = CreatePaths(text.ToCharArray(), 0, text.ToCharArray().Length);

        for (int p = 0, pl = paths.Count; p < pl; p++)
        {
            var path = paths[p].ToShapes(false, false);
            shapes = shapes.Concat(path).ToList();
        }

        return shapes;
    }

    public List<ShapePath> CreatePaths(char[] charBuffer, int start, int len)
    {
        var paths = new List<ShapePath>();
        GlyphLayoutMan.Typeface = Typeface;
        GlyphLayoutMan.Layout(charBuffer, start, len);

        resuableGlyphPlanList.Clear();
        var glyphLayout = GlyphLayoutMan;
        glyphLayout.Layout(charBuffer, start, len);
        glyphLayout.GenerateUnscaledGlyphPlans(resuableGlyphPlanList);

        var pxscale = typeface.CalculateScaleToPixelFromPointSize(FontSizeInPoints);
        var planCount = resuableGlyphPlanList.Count;

        float accX = 0;
        float accY = 0;
        float x = 0;
        float y = 0;
        var nx = x;
        var ny = y;

        var yMax = Bounds.YMax;
        var yMin = Bounds.YMin;

        var line_height = (yMax - yMin + UnderlineThickness) * pxscale;

        for (var i = 0; i < planCount; ++i)
        {
            pathTranslator.Reset();
            var glyphPlan = resuableGlyphPlanList[i];

            if (charBuffer[i] == '\n')
            {
                nx = 0;
                ny -= line_height;
                accX = 0;
                accY -= line_height;
            }
            else
            {
                nx = x + accX + glyphPlan.OffsetX * pxscale;
                ny = y + accY + glyphPlan.OffsetY * pxscale;
            }

            var path = new ShapePath();
            pathTranslator.SetOffset(nx, ny);
            pathTranslator.SetOutput(path);
            glyphPathBuilder.BuildFromGlyphIndex(glyphPlan.glyphIndex, FontSizeInPoints);
            glyphPathBuilder.ReadShapes(pathTranslator);

            paths.Add(pathTranslator.shapePath);

            accX += glyphPlan.AdvanceX * pxscale;
        }

        return paths;
    }
}