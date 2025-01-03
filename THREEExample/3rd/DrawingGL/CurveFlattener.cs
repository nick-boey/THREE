//MIT, 2016-present, WinterDev

using System;
using System.Collections.Generic;

namespace DrawingGL;

/// <summary>
///     Represents a cubic bezier curve with two anchor and two control points.
/// </summary>
//[Serializable]
internal readonly struct BezierCurveCubic
{
    /// <summary>
    ///     Start anchor point.
    /// </summary>
    public readonly Vector2 StartAnchor;

    /// <summary>
    ///     End anchor point.
    /// </summary>
    public readonly Vector2 EndAnchor;

    /// <summary>
    ///     First control point, controls the direction of the curve start.
    /// </summary>
    public readonly Vector2 FirstControlPoint;

    /// <summary>
    ///     Second control point, controls the direction of the curve end.
    /// </summary>
    public readonly Vector2 SecondControlPoint;


    /// <summary>
    ///     Constructs a new <see cref="BezierCurveCubic" />.
    /// </summary>
    /// <param name="startAnchor">The start anchor point.</param>
    /// <param name="endAnchor">The end anchor point.</param>
    /// <param name="firstControlPoint">The first control point.</param>
    /// <param name="secondControlPoint">The second control point.</param>
    public BezierCurveCubic(Vector2 startAnchor, Vector2 endAnchor, Vector2 firstControlPoint,
        Vector2 secondControlPoint)
    {
        StartAnchor = startAnchor;
        EndAnchor = endAnchor;
        FirstControlPoint = firstControlPoint;
        SecondControlPoint = secondControlPoint;
    }


    /// <summary>
    ///     Calculates the point with the specified t.
    /// </summary>
    /// <param name="t">The t value, between 0.0f and 1.0f.</param>
    /// <returns>Resulting point.</returns>
    public Vector2 CalculatePoint(float t)
    {
        var r = new Vector2();
        var c = 1.0f - t;
        r.X = StartAnchor.X * c * c * c + FirstControlPoint.X * 3 * t * c * c + SecondControlPoint.X * 3 * t * t * c
              + EndAnchor.X * t * t * t;
        r.Y = StartAnchor.Y * c * c * c + FirstControlPoint.Y * 3 * t * c * c + SecondControlPoint.Y * 3 * t * t * c
              + EndAnchor.Y * t * t * t;
        return r;
    }
}

internal enum CurveFlattenMethod
{
    Inc,
    Div
}

internal class SimpleCurveFlattener
{
    private readonly Curve4Div _curve4Div = new();
    private readonly List<int> _endPointList = new();
    private readonly List<float> _xyCoords = new();


    public int IncrementalStep
    {
        get;
        set;
        //TODO: value must >=1
    } = 7;

    public CurveFlattenMethod FlattenMethod { get; set; }


    public double DivCurveAngleTolerenceEpsilon
    {
        get => _curve4Div.CurveAngleTolerenceEpsilon;
        set => _curve4Div.CurveAngleTolerenceEpsilon = value;
    }

    public int DivCurveRecursiveLimit
    {
        get => _curve4Div.CurveRecursiveLimit;
        set => _curve4Div.CurveRecursiveLimit = value;
    }

    private void FlattenBezire(
        List<float> pointList,
        float x0, float y0,
        float x1, float y1,
        float x2, float y2,
        float x3, float y3)
    {
        if (IncrementalStep > 0)
        {
            //--------------------------------
            //don't add 1st point (x0, y0)
            //because we've added it.
            //--------------------------------

            var curve = new BezierCurveCubic(
                new Vector2(x0, y0),
                new Vector2(x3, y3),
                new Vector2(x1, y1),
                new Vector2(x2, y2));

            var eachstep = (float)1 / IncrementalStep;
            var stepSum = eachstep; //start

            var n = IncrementalStep - 1;
            for (var i = 1; i < n; ++i)
            {
                var vector2 = curve.CalculatePoint(stepSum);
                pointList.Add(vector2.X);
                pointList.Add(vector2.Y);
                stepSum += eachstep;
            }
        }

        pointList.Add(x3);
        pointList.Add(y3);
    }

    public float[] Flatten(List<PathPoint> points, out int[] endContours)
    {
        //reset
        _endPointList.Clear();
        _xyCoords.Clear();
        _curve4Div.SetOutput(_xyCoords);
        //----------
        var j = points.Count;
        if (j == 0)
        {
            endContours = null;
            return null;
        }

        //----------
        //first 
        var latest_x = points[0].x;
        var latest_y = points[0].y;


        for (var i = 1; i < j; ++i)
        {
            //we have point or curve4
            //no curve 3
            var p1 = points[i];
            switch (p1.kind)
            {
                default: throw new NotSupportedException();
                case PathPointKind.Point:
                {
                    _xyCoords.Add(latest_x = p1.x);
                    _xyCoords.Add(latest_y = p1.y);
                }
                    break;
                case PathPointKind.CloseFigure:
                {
                    //add stop mark point
                    _endPointList.Add(_xyCoords.Count - 1);
                }
                    break;
                case PathPointKind.CurveControl:
                {
                    //read next curve
                    //curve4

                    var p2 = points[i + 1];
                    var p3 = points[i + 2];
                    //--------------

                    if (FlattenMethod == CurveFlattenMethod.Inc)
                        FlattenBezire(
                            _xyCoords,
                            latest_x, latest_y,
                            p1.x, p1.y,
                            p2.x, p2.y,
                            latest_x = p3.x, latest_y = p3.y
                        );
                    else
                        //use sub div flatten
                        _curve4Div.Flatten(
                            latest_x, latest_y,
                            p1.x, p1.y,
                            p2.x, p2.y,
                            latest_x = p3.x, latest_y = p3.y);


                    //--------------
                    i += 2;
                }
                    break;
            }
            //close 
        }

        endContours = _endPointList.ToArray();
        return _xyCoords.ToArray();
    }
}

internal sealed class Curve4Div
{
    private const double CURVE_COLLINEARITY_EPSILON = 1e-30;

    //***
    //modified from PixelFarm source
    //see detail there


    private int _count;
    private double _cusp_limit;
    private double _distance_tolerance_square;
    private List<float> _points;

    public Curve4Div()
    {
        ApproximationScale = 1.0;
        AngleTolerance = 0.0;
        _cusp_limit = 0.0;
        _count = 0;
    }

    public Curve4Div(double x1, double y1,
        double x2, double y2,
        double x3, double y3,
        double x4, double y4)
    {
        ApproximationScale = 1.0;
        AngleTolerance = 0.0;
        _cusp_limit = 0.0;
        _count = 0;
        Flatten(x1, y1, x2, y2, x3, y3, x4, y4);
    }

    public double CurveAngleTolerenceEpsilon { get; set; } = 0.01;

    public int CurveRecursiveLimit { get; set; } = 32;

    public double ApproximationScale { get; set; }

    public double AngleTolerance { get; set; }

    public double CuspLimit
    {
        get => _cusp_limit == 0.0 ? 0.0 : Math.PI - _cusp_limit;
        set => _cusp_limit = value == 0.0 ? 0.0 : Math.PI - value;
    }

    public void SetOutput(List<float> outputPoints)
    {
        _points = outputPoints;
    }

    public List<float> GetInternalPoints()
    {
        return _points;
    }

    public void Flatten(double x1, double y1,
        double x2, double y2,
        double x3, double y3,
        double x4, double y4)
    {
        _distance_tolerance_square = 0.5 / ApproximationScale;
        _distance_tolerance_square *= _distance_tolerance_square;
        AddBezier(x1, y1, x2, y2, x3, y3, x4, y4);
        _count = 0;
    }


    private void AddBezier(double x1, double y1,
        double x2, double y2,
        double x3, double y3,
        double x4, double y4)
    {
        _points.Add((float)x1);
        _points.Add((float)y1);

        AddRecursiveBezier(x1, y1, x2, y2, x3, y3, x4, y4, 0);
        _points.Add((float)x4);
        _points.Add((float)y4);
    }


    private void AddRecursiveBezier(double x1, double y1,
        double x2, double y2,
        double x3, double y3,
        double x4, double y4,
        int level)
    {
        //recursive
        if (level > CurveRecursiveLimit) return;

        // Calculate all the mid-points of the line segments
        //----------------------
        var x12 = (x1 + x2) / 2;
        var y12 = (y1 + y2) / 2;
        var x23 = (x2 + x3) / 2;
        var y23 = (y2 + y3) / 2;
        var x34 = (x3 + x4) / 2;
        var y34 = (y3 + y4) / 2;
        var x123 = (x12 + x23) / 2;
        var y123 = (y12 + y23) / 2;
        var x234 = (x23 + x34) / 2;
        var y234 = (y23 + y34) / 2;
        var x1234 = (x123 + x234) / 2;
        var y1234 = (y123 + y234) / 2;
        // Try to approximate the full cubic curve by a single straight line
        //------------------
        var dx = x4 - x1;
        var dy = y4 - y1;
        var d2 = Math.Abs((x2 - x4) * dy - (y2 - y4) * dx);
        var d3 = Math.Abs((x3 - x4) * dy - (y3 - y4) * dx);
        double da1, da2, k;
        var SwitchCase = 0;
        if (d2 > CURVE_COLLINEARITY_EPSILON) SwitchCase = 2;
        if (d3 > CURVE_COLLINEARITY_EPSILON) SwitchCase++;

        switch (SwitchCase)
        {
            case 0:
                // All collinear OR p1==p4
                //----------------------
                k = dx * dx + dy * dy;
                if (k == 0)
                {
                    d2 = calc_sq_distance(x1, y1, x2, y2);
                    d3 = calc_sq_distance(x4, y4, x3, y3);
                }
                else
                {
                    k = 1 / k;
                    da1 = x2 - x1;
                    da2 = y2 - y1;
                    d2 = k * (da1 * dx + da2 * dy);
                    da1 = x3 - x1;
                    da2 = y3 - y1;
                    d3 = k * (da1 * dx + da2 * dy);
                    if (d2 > 0 && d2 < 1 && d3 > 0 && d3 < 1)
                        // Simple collinear case, 1---2---3---4
                        // We can leave just two endpoints
                        return;
                    if (d2 <= 0) d2 = calc_sq_distance(x2, y2, x1, y1);
                    else if (d2 >= 1) d2 = calc_sq_distance(x2, y2, x4, y4);
                    else d2 = calc_sq_distance(x2, y2, x1 + d2 * dx, y1 + d2 * dy);
                    if (d3 <= 0) d3 = calc_sq_distance(x3, y3, x1, y1);
                    else if (d3 >= 1) d3 = calc_sq_distance(x3, y3, x4, y4);
                    else d3 = calc_sq_distance(x3, y3, x1 + d3 * dx, y1 + d3 * dy);
                }

                if (d2 > d3)
                {
                    if (d2 < _distance_tolerance_square)
                    {
                        _points.Add((float)x2);
                        _points.Add((float)y2);
                        return;
                    }
                }
                else
                {
                    if (d3 < _distance_tolerance_square)
                    {
                        AddPoint(x3, y3);
                        return;
                    }
                }

                break;
            case 1:
                // p1,p2,p4 are collinear, p3 is significant
                //----------------------
                if (d3 * d3 <= _distance_tolerance_square * (dx * dx + dy * dy))
                {
                    if (AngleTolerance < CurveAngleTolerenceEpsilon)
                    {
                        AddPoint(x23, y23);
                        return;
                    }

                    // Angle Condition
                    //----------------------
                    da1 = Math.Abs(Math.Atan2(y4 - y3, x4 - x3) - Math.Atan2(y3 - y2, x3 - x2));
                    if (da1 >= Math.PI) da1 = 2 * Math.PI - da1;
                    if (da1 < AngleTolerance)
                    {
                        AddPoint(x2, y2);
                        AddPoint(x3, y3);
                        return;
                    }

                    if (_cusp_limit != 0.0)
                        if (da1 > _cusp_limit)
                        {
                            AddPoint(x3, y3);
                            return;
                        }
                }

                break;
            case 2:
                // p1,p3,p4 are collinear, p2 is significant
                //----------------------
                if (d2 * d2 <= _distance_tolerance_square * (dx * dx + dy * dy))
                {
                    if (AngleTolerance < CurveAngleTolerenceEpsilon)
                    {
                        AddPoint(x23, y23);
                        return;
                    }

                    // Angle Condition
                    //----------------------
                    da1 = Math.Abs(Math.Atan2(y3 - y2, x3 - x2) - Math.Atan2(y2 - y1, x2 - x1));
                    if (da1 >= Math.PI) da1 = 2 * Math.PI - da1;
                    if (da1 < AngleTolerance)
                    {
                        AddPoint(x2, y2);
                        AddPoint(x3, y3);
                        return;
                    }

                    if (_cusp_limit != 0.0)
                        if (da1 > _cusp_limit)
                        {
                            AddPoint(x2, y2);
                            return;
                        }
                }

                break;
            case 3:
                // Regular case
                //-----------------
                if ((d2 + d3) * (d2 + d3) <= _distance_tolerance_square * (dx * dx + dy * dy))
                {
                    // If the curvature doesn't exceed the distance_tolerance value
                    // we tend to finish subdivisions.
                    //----------------------
                    if (AngleTolerance < CurveAngleTolerenceEpsilon)
                    {
                        AddPoint(x23, y23);
                        return;
                    }

                    // Angle & Cusp Condition
                    //----------------------
                    k = Math.Atan2(y3 - y2, x3 - x2);
                    da1 = Math.Abs(k - Math.Atan2(y2 - y1, x2 - x1));
                    da2 = Math.Abs(Math.Atan2(y4 - y3, x4 - x3) - k);
                    if (da1 >= Math.PI) da1 = 2 * Math.PI - da1;
                    if (da2 >= Math.PI) da2 = 2 * Math.PI - da2;
                    if (da1 + da2 < AngleTolerance)
                    {
                        // Finally we can stop the recursion
                        //----------------------

                        AddPoint(x23, y23);
                        return;
                    }

                    if (_cusp_limit != 0.0)
                    {
                        if (da1 > _cusp_limit)
                        {
                            AddPoint(x2, y2);
                            return;
                        }

                        if (da2 > _cusp_limit)
                        {
                            AddPoint(x3, y3);
                            return;
                        }
                    }
                }

                break;
        }

        // Continue subdivision
        //----------------------
        AddRecursiveBezier(x1, y1, x12, y12, x123, y123, x1234, y1234, level + 1);
        AddRecursiveBezier(x1234, y1234, x234, y234, x34, y34, x4, y4, level + 1);
    }

    private static double calc_sq_distance(double x1, double y1, double x2, double y2)
    {
        var dx = x2 - x1;
        var dy = y2 - y1;
        return dx * dx + dy * dy;
    }

    private void AddPoint(double x, double y)
    {
        _points.Add((float)x);
        _points.Add((float)y);
    }

    private void AddPoint(float x, float y)
    {
        _points.Add(x);
        _points.Add(y);
    }
}