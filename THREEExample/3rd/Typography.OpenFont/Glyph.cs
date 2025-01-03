//Apache2, 2017-present, WinterDev
//Apache2, 2014-2016, Samuel Carlsson, WinterDev

using System;
using System.Text;
using Typography.OpenFont.CFF;
using Typography.OpenFont.MathGlyphs;

namespace Typography.OpenFont;

public class Glyph
{
    //--------------------

    //cff  

    internal readonly Cff1GlyphData _cff1GlyphData; //NULL in  _onlyLayoutEssMode 

    /// <summary>
    ///     glyph info has only essential layout detail (this is our extension)
    /// </summary>
    private readonly bool _onlyLayoutEssMode;

    private bool _hasOrgAdvWidth; //FOUND in all mode
    private ushort _imgFormat; //FOUND in all mode (if font has this data)

    private uint _streamLen; //FOUND in all mode (if font has this data)

    internal Glyph(
        GlyphPointF[] glyphPoints,
        ushort[] contourEndPoints,
        Bounds bounds,
        byte[] glyphInstructions,
        ushort index)
    {
        //create from TTF 

#if DEBUG
        dbugId = s_debugTotalId++;
        if (dbugId == 444)
        {
        }
#endif
        GlyphPoints = glyphPoints;
        EndPoints = contourEndPoints;
        Bounds = bounds;
        GlyphInstructions = glyphInstructions;
        GlyphIndex = index;
    }

    internal Glyph(Cff1GlyphData cff1Glyph, ushort glyphIndex)
    {
#if DEBUG
        dbugId = s_debugTotalId++;
        cff1Glyph.dbugGlyphIndex = glyphIndex;
#endif
        //create from CFF 
        _cff1GlyphData = cff1Glyph;
        GlyphIndex = glyphIndex;
    }

    internal Glyph(ushort glyphIndex, uint streamOffset, uint streamLen, ushort imgFormat)
    {
        //_bmpGlyphSource = bmpGlyphSource;
        BitmapStreamOffset = streamOffset;
        _streamLen = streamLen;
        _imgFormat = imgFormat;
        GlyphIndex = glyphIndex;
    }

    private Glyph(ushort glyphIndex)
    {
        //for Clone_NO_BuildingInstructions()
        _onlyLayoutEssMode = true;
        GlyphIndex = glyphIndex;
    }

    public ushort GlyphIndex { get; } //FOUND in all mode
    public Bounds Bounds { get; internal set; } //FOUND in all mode
    public ushort OriginalAdvanceWidth { get; private set; } //FOUND in all mode
    internal ushort BitmapGlyphAdvanceWidth { get; set; } //FOUND in all mode

    //TrueTypeFont
    public ushort[] EndPoints { get; private set; } //NULL in  _onlyLayoutEssMode         
    public GlyphPointF[] GlyphPoints { get; private set; } //NULL in  _onlyLayoutEssMode         
    internal byte[] GlyphInstructions { get; set; } //NULL in _onlyLayoutEssMode 
    public bool HasGlyphInstructions => GlyphInstructions != null; //FALSE  n _onlyLayoutEssMode 

    //
    public GlyphClassKind GlyphClass { get; internal set; } //FOUND in all mode
    internal ushort MarkClassDef { get; set; } //FOUND in all mode

    public short MinX => Bounds.XMin;
    public short MaxX => Bounds.XMax;
    public short MinY => Bounds.YMin;
    public short MaxY => Bounds.YMax;

    public bool IsCffGlyph => _cff1GlyphData != null;

    //TODO: review here again
    public MathGlyphInfo MathGlyphInfo { get; internal set; } //FOUND in all mode (if font has this data)
    internal uint BitmapStreamOffset { get; private set; }
    internal uint BitmapFormat => _imgFormat;


    public static bool HasOriginalAdvancedWidth(Glyph glyph)
    {
        return glyph._hasOrgAdvWidth;
    }

    public static void SetOriginalAdvancedWidth(Glyph glyph, ushort advW)
    {
        glyph.OriginalAdvanceWidth = advW;
        glyph._hasOrgAdvWidth = true;
    }

    /// <summary>
    ///     TrueType outline, offset glyph points
    /// </summary>
    /// <param name="glyph"></param>
    /// <param name="dx"></param>
    /// <param name="dy"></param>
    internal static void TtfOffsetXY(Glyph glyph, short dx, short dy)
    {
        //change data on current glyph
        var glyphPoints = glyph.GlyphPoints;
        for (var i = glyphPoints.Length - 1; i >= 0; --i) glyphPoints[i] = glyphPoints[i].Offset(dx, dy);
        //-------------------------
        var orgBounds = glyph.Bounds;
        glyph.Bounds = new Bounds(
            (short)(orgBounds.XMin + dx),
            (short)(orgBounds.YMin + dy),
            (short)(orgBounds.XMax + dx),
            (short)(orgBounds.YMax + dy));
    }

    /// <summary>
    ///     TrueType outline, transform normal
    /// </summary>
    /// <param name="glyph"></param>
    /// <param name="m00"></param>
    /// <param name="m01"></param>
    /// <param name="m10"></param>
    /// <param name="m11"></param>
    internal static void TtfTransformWith2x2Matrix(Glyph glyph, float m00, float m01, float m10, float m11)
    {
        //http://stackoverflow.com/questions/13188156/whats-the-different-between-vector2-transform-and-vector2-transformnormal-i
        //http://www.technologicalutopia.com/sourcecode/xnageometry/vector2.cs.htm

        //change data on current glyph
        float new_xmin = 0;
        float new_ymin = 0;
        float new_xmax = 0;
        float new_ymax = 0;

        var glyphPoints = glyph.GlyphPoints;
        for (var i = 0; i < glyphPoints.Length; ++i)
        {
            var p = glyphPoints[i];
            var x = p.P.X;
            var y = p.P.Y;

            float newX, newY;
            //please note that this is transform normal***
            glyphPoints[i] = new GlyphPointF(
                newX = (float)Math.Round(x * m00 + y * m10),
                newY = (float)Math.Round(x * m01 + y * m11),
                p.onCurve);

            //short newX = xs[i] = (short)Math.Round((x * m00) + (y * m10));
            //short newY = ys[i] = (short)Math.Round((x * m01) + (y * m11));
            //------
            if (newX < new_xmin) new_xmin = newX;
            if (newX > new_xmax) new_xmax = newX;
            //------
            if (newY < new_ymin) new_ymin = newY;
            if (newY > new_ymax) new_ymax = newY;
        }

        //TODO: review here
        glyph.Bounds = new Bounds(
            (short)new_xmin, (short)new_ymin,
            (short)new_xmax, (short)new_ymax);
    }

    /// <summary>
    ///     TrueType outline glyph clone
    /// </summary>
    /// <param name="original"></param>
    /// <param name="newGlyphIndex"></param>
    /// <returns></returns>
    internal static Glyph TtfOutlineGlyphClone(Glyph original, ushort newGlyphIndex)
    {
        //for true type instruction glyph***
        return new Glyph(
            Utils.CloneArray(original.GlyphPoints),
            Utils.CloneArray(original.EndPoints),
            original.Bounds,
            original.GlyphInstructions != null ? Utils.CloneArray(original.GlyphInstructions) : null,
            newGlyphIndex);
    }

    /// <summary>
    ///     append data from src to dest, dest data will changed***
    /// </summary>
    /// <param name="src"></param>
    /// <param name="dest"></param>
    internal static void TtfAppendGlyph(Glyph dest, Glyph src)
    {
        var org_dest_len = dest.EndPoints.Length;
#if DEBUG
        var src_contour_count = src.EndPoints.Length;
#endif
        if (org_dest_len == 0)
        {
            //org is empty glyph

            dest.GlyphPoints = Utils.ConcatArray(dest.GlyphPoints, src.GlyphPoints);
            dest.EndPoints = Utils.ConcatArray(dest.EndPoints, src.EndPoints);
        }
        else
        {
            var org_last_point = (ushort)(dest.EndPoints[org_dest_len - 1] + 1); //since start at 0 

            dest.GlyphPoints = Utils.ConcatArray(dest.GlyphPoints, src.GlyphPoints);
            dest.EndPoints = Utils.ConcatArray(dest.EndPoints, src.EndPoints);
            //offset latest append contour  end points
            var newlen = dest.EndPoints.Length;
            for (var i = org_dest_len; i < newlen; ++i) dest.EndPoints[i] += org_last_point;
        }


        //calculate new bounds
        var destBound = dest.Bounds;
        var srcBound = src.Bounds;
        var newXmin = Math.Min(destBound.XMin, srcBound.XMin);
        var newYMin = Math.Min(destBound.YMin, srcBound.YMin);
        var newXMax = Math.Max(destBound.XMax, srcBound.XMax);
        var newYMax = Math.Max(destBound.YMax, srcBound.YMax);

        dest.Bounds = new Bounds(newXmin, newYMin, newXMax, newYMax);
    }

    public Cff1GlyphData GetCff1GlyphData()
    {
        return _cff1GlyphData;
    }

    internal static void CopyExistingGlyphInfo(Glyph src, Glyph dst)
    {
        dst.Bounds = src.Bounds;
        dst._hasOrgAdvWidth = src._hasOrgAdvWidth;
        dst.OriginalAdvanceWidth = src.OriginalAdvanceWidth;
        dst.BitmapGlyphAdvanceWidth = src.BitmapGlyphAdvanceWidth;
        dst.GlyphClass = src.GlyphClass;
        dst.MarkClassDef = src.MarkClassDef;

        //ttf: NO EndPoints, GlyphPoints, HasGlyphInstructions

        //cff:  NO _cff1GlyphData

        //math-font:
        dst.MathGlyphInfo = src.MathGlyphInfo;
        dst.BitmapStreamOffset = src.BitmapStreamOffset;
        dst._streamLen = src._streamLen;
        dst._imgFormat = src._imgFormat;
    }

    internal static Glyph Clone_NO_BuildingInstructions(Glyph src)
    {
        //a new glyph has only detail about glyph layout
        //NO information about glyph building instructions
        //1. if src if ttf
        //2. if src is cff
        //3. if src is svg
        //4. if src is bitmap

        var newclone = new Glyph(src.GlyphIndex);
        CopyExistingGlyphInfo(src, newclone);
        return newclone;
    }

#if DEBUG
    public readonly int dbugId;
    private static int s_debugTotalId;
    public override string ToString()
    {
        var stbuilder = new StringBuilder();
        if (IsCffGlyph)
        {
            stbuilder.Append("cff");
            stbuilder.Append(",index=" + GlyphIndex);
            stbuilder.Append(",name=" + _cff1GlyphData.Name);
        }
        else
        {
            stbuilder.Append("ttf");
            stbuilder.Append(",index=" + GlyphIndex);
            stbuilder.Append(",class=" + GlyphClass);
            if (MarkClassDef != 0) stbuilder.Append(",mark_class=" + MarkClassDef);
        }

        return stbuilder.ToString();
    }
#endif
}

//https://docs.microsoft.com/en-us/typography/opentype/spec/gdef
public enum GlyphClassKind : byte
{
    //1 	Base glyph (single character, spacing glyph)
    //2 	Ligature glyph (multiple character, spacing glyph)
    //3 	Mark glyph (non-spacing combining glyph)
    //4 	Component glyph (part of single character, spacing glyph)
    //
    // The font developer does not have to classify every glyph in the font, 
    //but any glyph not assigned a class value falls into Class zero (0). 
    //For instance, class values might be useful for the Arabic glyphs in a font, but not for the Latin glyphs. 
    //Then the GlyphClassDef table will list only Arabic glyphs, and-by default-the Latin glyphs will be assigned to Class 0. 
    //Component glyphs can be put together to generate ligatures. 
    //A ligature can be generated by creating a glyph in the font that references the component glyphs, 
    //or outputting the component glyphs in the desired sequence. 
    //Component glyphs are not used in defining any GSUB or GPOS formats.
    //
    Zero = 0, //class0, classZero
    Base,
    Ligature,
    Mark,
    Component
}