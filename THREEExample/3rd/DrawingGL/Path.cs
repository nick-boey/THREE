﻿//MIT, 2016-present, WinterDev

using System.Collections.Generic;
using Typography.OpenFont;
using Typography.TextLayout;

namespace DrawingGL;

internal enum PathPointKind : byte
{
    Point,
    CurveControl,
    CloseFigure
}

internal readonly struct PathPoint
{
    public readonly float x;
    public readonly float y;
    public readonly PathPointKind kind;

    public PathPoint(float x, float y, PathPointKind k)
    {
        this.x = x;
        this.y = y;
        kind = k;
    }

#if DEBUG
    public override string ToString()
    {
        return "(" + x + "," + y + ")" + (kind == PathPointKind.Point ? " p " : "c");
    }
#endif
}

internal interface IWritablePath
{
    void CloseFigure();

    /// <summary>
    ///     add curve4 from latest point (x0,y0)
    /// </summary>
    /// <param name="x1"></param>
    /// <param name="y1"></param>
    /// <param name="x2"></param>
    /// <param name="y2"></param>
    /// <param name="x3"></param>
    /// <param name="y3"></param>
    void BezireTo(float x1, float y1, float x2, float y2, float x3, float y3);

    void LineTo(float x1, float y1);
    void MoveTo(float x0, float y0);
}

internal class WritablePath : IWritablePath
{
    private bool _addMoveTo;
    private float _lastMoveX;
    private float _lastMoveY;

    private float _latestX;

    private float _latestY;

    //record all cmd 
    internal List<PathPoint> _points = new();

    public void MoveTo(float x0, float y0)
    {
        _latestX = _lastMoveX = x0;
        _latestY = _lastMoveY = y0;
        _addMoveTo = true;

        //_points.Add(new PathPoint(_latestX = x0, _latestY = y0, PathPointKind.Point));
    }

    public void BezireTo(float x1, float y1, float x2, float y2, float x3, float y3)
    {
        if (_addMoveTo)
        {
            _points.Add(new PathPoint(_latestX, _latestY, PathPointKind.Point));
            _addMoveTo = false;
        }

        _points.Add(new PathPoint(x1, y1, PathPointKind.CurveControl));
        _points.Add(new PathPoint(x2, y2, PathPointKind.CurveControl));
        _points.Add(new PathPoint(_latestX = x3, _latestY = y3, PathPointKind.Point));
    }

    public void CloseFigure()
    {
        if (_lastMoveX != _latestX ||
            _lastMoveY != _latestY)
            _points.Add(new PathPoint(_lastMoveX, _lastMoveY, PathPointKind.Point));
        //_lastMoveX = _latestX;
        //_lastMoveY = _latestY;

        //add curve
        _points.Add(new PathPoint(_lastMoveX, _lastMoveY, PathPointKind.CloseFigure));


        _latestX = _lastMoveX;
        _latestY = _lastMoveY;
    }

    public void LineTo(float x1, float y1)
    {
        if (_addMoveTo)
        {
            _points.Add(new PathPoint(_latestX, _latestY, PathPointKind.Point));
            _addMoveTo = false;
        }

        _points.Add(new PathPoint(_latestX = x1, _latestY = y1, PathPointKind.Point));
    }
    //-------------------- 
}

public readonly struct GlyphRun
{
    //glyph run contains...
    //1.
    public readonly float[] _tessData; //4
    public readonly ushort _vertextCount;

    internal GlyphRun(UnscaledGlyphPlan glyphPlan, float[] tessData, ushort vertextCount)
    {
        GlyphPlan = glyphPlan;
        _tessData = tessData;
        _vertextCount = vertextCount;
    }

    public UnscaledGlyphPlan GlyphPlan { get; }
}

public class TextRun
{
    //each text run has TextFormat information

    internal List<GlyphRun> _glyphs = new();
    internal float sizeInPoints;
    internal Typeface typeface;

    public void AddGlyph(GlyphRun glyph)
    {
        _glyphs.Add(glyph);
    }

    public float CalculateToPixelScaleFromPointSize(float sizeInPoint)
    {
        return typeface.CalculateScaleToPixelFromPointSize(sizeInPoint);
    }
}