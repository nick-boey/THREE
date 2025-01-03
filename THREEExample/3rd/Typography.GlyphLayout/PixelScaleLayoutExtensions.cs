//MIT, 2016-present, WinterDev

using System;

namespace Typography.TextLayout;

/// <summary>
///     scaled glyph plan to specfic font size.
///     offsetX,offsetY,advanceX are adjusted to fit with specific font size
/// </summary>
public readonly struct PxScaledGlyphPlan
{
    public readonly ushort input_cp_offset;
    public readonly ushort glyphIndex;

    public PxScaledGlyphPlan(ushort input_cp_offset, ushort glyphIndex, float advanceW, float offsetX, float offsetY)
    {
        this.input_cp_offset = input_cp_offset;
        this.glyphIndex = glyphIndex;
        OffsetX = offsetX;
        OffsetY = offsetY;
        AdvanceX = advanceW;
    }

    public readonly float AdvanceX;

    /// <summary>
    ///     x offset from current position
    /// </summary>
    public readonly float OffsetX;

    /// <summary>
    ///     y offset from current position
    /// </summary>
    public readonly float OffsetY;

    public bool AdvanceMoveForward => AdvanceX > 0;

#if DEBUG
    public override string ToString()
    {
        return " adv:" + AdvanceX;
    }
#endif
}

/// <summary>
///     scaled glyph plan
/// </summary>
public struct GlyphPlanSequencePixelScaleLayout
{
    private GlyphPlanSequence _seq;
    private readonly float _pxscale;
    private readonly int _end;

    public GlyphPlanSequencePixelScaleLayout(GlyphPlanSequence glyphPlans, float pxscale)
    {
        _seq = glyphPlans;
        _pxscale = pxscale;
        AccumWidth = 0;
        CurrentIndex = glyphPlans.startAt;
        _end = glyphPlans.startAt + glyphPlans.len;
        ExactX = ExactY = 0;
        CurrentGlyphIndex = 0;
    }

    //
    public int CurrentIndex { get; private set; }

    //
    public PxScaledGlyphPlan GlyphPlan
    {
        get
        {
            var unscale = _seq[CurrentIndex];
            var scaled_advW = unscale.AdvanceX * _pxscale;
            return new PxScaledGlyphPlan(
                unscale.input_cp_offset,
                unscale.glyphIndex,
                scaled_advW,
                unscale.OffsetX * _pxscale,
                unscale.OffsetY * _pxscale);
        }
    }

    public float AccumWidth { get; private set; }

    public float ExactX { get; private set; }

    public float ExactY { get; private set; }

    public ushort CurrentGlyphIndex { get; private set; }

    public bool Read()
    {
        if (CurrentIndex >= _end) return false;

        //read current 
        var unscale = _seq[CurrentIndex];

        var scaled_advW = unscale.AdvanceX * _pxscale;
        ExactX = AccumWidth + (unscale.AdvanceX + unscale.OffsetX) * _pxscale;
        ExactY = unscale.OffsetY * _pxscale;
        AccumWidth += scaled_advW;
        CurrentGlyphIndex = unscale.glyphIndex;
        CurrentIndex++;
        return true;
    }
}

/// <summary>
///     scaled glyph plan + snap-to-grid
/// </summary>
public struct GlyphPlanSequenceSnapPixelScaleLayout
{
    private GlyphPlanSequence _seq;
    private readonly float _pxscale;
    private readonly int _end;

    private readonly bool _rightToLeft;

    public GlyphPlanSequenceSnapPixelScaleLayout(GlyphPlanSequence glyphPlans, int start, int len, float pxscale)
    {
        _seq = glyphPlans;
        _pxscale = pxscale;
        AccumWidth = 0;
        CurrentIndex = start;
        _end = start + len;
        ExactX = ExactY = 0;
        CurrentGlyphIndex = 0;
        _limitW = 0;

        if (_rightToLeft = glyphPlans.IsRightToLeft) CurrentIndex = _end - 1;
    }

    public ushort CurrentGlyphIndex { get; private set; }

    public int CurrentIndex { get; private set; }

    //
    public bool Read()
    {
        if (_rightToLeft)
        {
            if (CurrentIndex < 0) return false;

            //read current 
            var unscale = _seq[CurrentIndex];

            var scaled_advW = (short)Math.Round(unscale.AdvanceX * _pxscale);
            var scaled_offsetX = (short)Math.Round(unscale.OffsetX * _pxscale);
            var scaled_offsetY = (short)Math.Round(unscale.OffsetY * _pxscale);

            ExactX = AccumWidth + scaled_offsetX;
            ExactY = scaled_offsetY;
            AccumWidth += scaled_advW;

            CurrentGlyphIndex = unscale.glyphIndex;
            CurrentIndex--;
            return true;
        }
        else
        {
            if (CurrentIndex >= _end) return false;

            //read current 
            var unscale = _seq[CurrentIndex];

            var scaled_advW = (short)Math.Round(unscale.AdvanceX * _pxscale);
            var scaled_offsetX = (short)Math.Round(unscale.OffsetX * _pxscale);
            var scaled_offsetY = (short)Math.Round(unscale.OffsetY * _pxscale);

            ExactX = AccumWidth + scaled_offsetX;
            ExactY = scaled_offsetY;
            AccumWidth += scaled_advW;

            CurrentGlyphIndex = unscale.glyphIndex;
            CurrentIndex++;
            return true;
        }
    }

    public void ReadToEnd()
    {
        while (Read()) ;
    }

    public int AccumWidth { get; private set; }

    public int ExactX { get; private set; }

    public int ExactY { get; private set; }

    private int _limitW;

    public void ReadWidthLimitWidth(int limitWidth)
    {
        _limitW = limitWidth;
        while (ReadWidthLimitWidth()) ;
    }

    private bool ReadWidthLimitWidth()
    {
        if (_rightToLeft)
        {
            if (CurrentIndex < 0) return false;

            //read current 
            var unscale = _seq[CurrentIndex];

            var scaled_advW = (short)Math.Round(unscale.AdvanceX * _pxscale);
            var scaled_offsetX = (short)Math.Round(unscale.OffsetX * _pxscale);
            var scaled_offsetY = (short)Math.Round(unscale.OffsetY * _pxscale);

            if (AccumWidth + scaled_advW > _limitW)
                //stop
                return false;

            ExactX = AccumWidth + scaled_offsetX;
            ExactY = scaled_offsetY;
            AccumWidth += scaled_advW;

            CurrentGlyphIndex = unscale.glyphIndex;
            CurrentIndex--;
            return true;
        }
        else
        {
            if (CurrentIndex >= _end) return false;

            //read current 
            var unscale = _seq[CurrentIndex];

            var scaled_advW = (short)Math.Round(unscale.AdvanceX * _pxscale);
            var scaled_offsetX = (short)Math.Round(unscale.OffsetX * _pxscale);
            var scaled_offsetY = (short)Math.Round(unscale.OffsetY * _pxscale);

            if (AccumWidth + scaled_advW > _limitW)
                //stop
                return false;

            ExactX = AccumWidth + scaled_offsetX;
            ExactY = scaled_offsetY;
            AccumWidth += scaled_advW;

            CurrentGlyphIndex = unscale.glyphIndex;
            CurrentIndex++;
            return true;
        }
    }
}

public static class PixelScaleLayoutExtensions
{
    private static float MeasureGlyphPlans(this GlyphLayout glyphLayout,
        float pxscale,
        bool snapToGrid)
    {
        //user can implement this with some 'PixelScaleEngine'  
        var glyphPositions = glyphLayout.ResultUnscaledGlyphPositions;
        float accumW = 0; //acummulate Width

        if (snapToGrid)
        {
            var finalGlyphCount = glyphPositions.Count;
            for (var i = 0; i < finalGlyphCount; ++i)
            {
                //all from pen-pos 
                var glyphIndex = glyphPositions.GetGlyph(i,
                    out var input_offset,
                    out var offsetX,
                    out var offsetY,
                    out var advW);
                accumW += (short)Math.Round(advW * pxscale);
            }
        }
        else
        {
            //not snap to grid
            //scaled but not snap to grid
            var finalGlyphCount = glyphPositions.Count;
            for (var i = 0; i < finalGlyphCount; ++i)
            {
                //all from pen-pos 
                var glyphIndex = glyphPositions.GetGlyph(i,
                    out var input_offset,
                    out var offsetX,
                    out var offsetY,
                    out var advW);
                accumW += advW * pxscale;
            }
        }

        return accumW;
    }

    private static float MeasureGlyphPlanWithLimitWidth(this GlyphLayout glyphLayout,
        float pxscale,
        float limitWidth,
        bool snapToGrid,
        out int stopAtGlyphIndex)
    {
        //user can implement this with some 'PixelScaleEngine'  
        var glyphPositions = glyphLayout.ResultUnscaledGlyphPositions;
        float accumW = 0; //acummulate Width
        stopAtGlyphIndex = 0;

        if (snapToGrid)
        {
            var finalGlyphCount = glyphPositions.Count;
            for (var i = 0; i < finalGlyphCount; ++i)
            {
                //all from pen-pos
                var glyphIndex = glyphPositions.GetGlyph(i,
                    out var input_offset,
                    out var offsetX,
                    out var offsetY,
                    out var advW);

                stopAtGlyphIndex = i; //***
                //
                var w = (short)Math.Round(advW * pxscale);
                if (accumW + w > limitWidth)
                    //stop           
                    break;

                accumW += w;
            }
        }
        else
        {
            //not snap to grid
            //scaled but not snap to grid
            var finalGlyphCount = glyphPositions.Count;
            for (var i = 0; i < finalGlyphCount; ++i)
            {
                //all from pen-pos
                var glyphIndex = glyphPositions.GetGlyph(i,
                    out var input_offset,
                    out var offsetX,
                    out var offsetY,
                    out var advW);


                stopAtGlyphIndex = i; //***

                var w = advW * pxscale;
                if (accumW + w > limitWidth)
                    //stop           
                    break;

                accumW += w;
            }
        }

        return accumW;


        ////measure string 
        //if (str.Length < 1)
        //{
        //    charFitWidth = 0;
        //}

        //_reusableMeasureBoxList.Clear(); //reset 


        //float pxscale = _currentTypeface.CalculateScaleToPixelFromPointSize(_fontSizeInPts);
        ////NOET:at this moment, simple operation
        ////may not be simple...  
        ////-------------------
        ////input string may contain more than 1 script lang
        ////user can parse it by other parser
        ////but in this code, we use our Typography' parser
        ////-------------------
        ////user must setup the CustomBreakerBuilder before use         

        //int cur_startAt = startAt;
        //float accumW = 0;

        //float acc_x = 0;//accum_x
        //float acc_y = 0;//accum_y
        //float g_x = 0;
        //float g_y = 0;
        //float x = 0;
        //float y = 0;
        //foreach (Typography.TextLayout.BreakSpan breakSpan in BreakToLineSegments(str, startAt, len))
        //{

        //    //measure string at specific px scale 
        //    _glyphLayout.Layout(str, breakSpan.startAt, breakSpan.len);
        //    //

        //    _reusableGlyphPlanList.Clear();
        //    _glyphLayout.GenerateUnscaledGlyphPlans(_reusableGlyphPlanList);
        //    //measure ...


        //    //measure each glyph
        //    //limit at specific width
        //    int glyphCount = _reusableGlyphPlanList.Count;


        //    for (int i = 0; i < glyphCount; ++i)
        //    {
        //        UnscaledGlyphPlan glyphPlan = _reusableGlyphPlanList[i];

        //        float ngx = acc_x + (float)Math.Round(glyphPlan.OffsetX * pxscale);
        //        float ngy = acc_y + (float)Math.Round(glyphPlan.OffsetY * pxscale);
        //        //NOTE:
        //        // -glyphData.TextureXOffset => restore to original pos
        //        // -glyphData.TextureYOffset => restore to original pos 
        //        //--------------------------
        //        g_x = (float)(x + (ngx)); //ideal x
        //        g_y = (float)(y + (ngy));
        //        float g_w = (float)Math.Round(glyphPlan.AdvanceX * pxscale);
        //        acc_x += g_w;
        //        //g_x = (float)Math.Round(g_x);
        //        g_y = (float)Math.Floor(g_y);

        //        float right = g_x + g_w;

        //        if (right >= accumW)
        //        {
        //            //stop here at this glyph
        //            charFit = i - 1;
        //            //TODO: review this
        //            charFitWidth = (int)System.Math.Round(accumW);
        //            return;
        //        }
        //        else
        //        {
        //            accumW = right;
        //        }
        //    }
        //}

        //charFit = 0;
        //charFitWidth = 0;
    }


    //static void ConcatMeasureBox(ref float accumW, ref float accumH, ref MeasuredStringBox measureBox)
    //{
    //    accumW += measureBox.width;
    //    float h = measureBox.CalculateLineHeight();
    //    if (h > accumH)
    //    {
    //        accumH = h;
    //    }
    //}


    public static MeasuredStringBox LayoutAndMeasureString(
        this GlyphLayout glyphLayout,
        char[] textBuffer,
        int startAt,
        int len,
        float fontSizeInPoints,
        float limitW = -1, //-1 unlimit scaled width (px)
        bool snapToGrid = true)
    {
        //1. unscale layout, in design unit
        glyphLayout.Layout(textBuffer, startAt, len);

        //2. scale  to specific font size           

        var typeface = glyphLayout.Typeface;
        var pxscale = typeface.CalculateScaleToPixelFromPointSize(fontSizeInPoints);

        //....
        float scaled_accumX = 0;
        if (limitW < 0)
        {
            //no limit
            scaled_accumX = MeasureGlyphPlans(
                glyphLayout,
                pxscale,
                snapToGrid);

            return new MeasuredStringBox(
                scaled_accumX,
                typeface.Ascender,
                typeface.Descender,
                typeface.LineGap,
                typeface.ClipedAscender,
                typeface.ClipedDescender,
                pxscale);
        }

        if (limitW > 0)
        {
            scaled_accumX = MeasureGlyphPlanWithLimitWidth(
                glyphLayout,
                pxscale,
                limitW,
                snapToGrid,
                out var stopAtChar);

            var mstrbox = new MeasuredStringBox(
                scaled_accumX,
                typeface.Ascender,
                typeface.Descender,
                typeface.LineGap,
                typeface.ClipedAscender,
                typeface.ClipedDescender,
                pxscale);

            mstrbox.StopAt = (ushort)stopAtChar;
            return mstrbox;
        }

        return new MeasuredStringBox(
            0,
            typeface.Ascender,
            typeface.Descender,
            typeface.LineGap,
            typeface.ClipedAscender,
            typeface.ClipedDescender,
            pxscale);
    }

#if DEBUG
    public static float dbugSnapToFitInteger(float value)
    {
        var floor_value = (int)value;
        return value - floor_value >= 1f / 2f ? floor_value + 1 : floor_value;
    }

    public static float dbugSnapHalf(float value)
    {
        var floor_value = (int)value;
        //round to int 0, 0.5,1.0
        return value - floor_value >= 2f / 3f ? floor_value + 1 : //else->
            value - floor_value >= 1f / 3f ? floor_value + 0.5f : floor_value;
    }

    private static int dbugSnapUpper(float value)
    {
        var floor_value = (int)value;
        return floor_value + 1;
    }
#endif
}