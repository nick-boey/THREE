/*
Based on an optimized c++ solution in
 - http://stackoverflow.com/questions/9489736/catmull-rom-curve-with-no-cusps-and-no-self-intersections/
 - http://ideone.com/NoEbVM

This CubicPoly class could be used for reusing some variables and calculations,
but for three.js curve use, it could be possible inlined and flatten into a single function call
which can be placed in CurveUtils.
*/



/**
* @author zz85 https://github.com/zz85
*
* Centripetal CatmullRom Curve - which is useful for avoiding
* cusps and self-intersections in non-uniform catmull rom curves.
* http://www.cemyuksel.com/research/catmullrom_param/catmullrom.pdf
*
* curve.type accepts centripetal(default), chordal and catmullrom
* curve.tension is used for catmullrom which defaults to 0.5
*/
namespace THREE;

[Serializable]
internal class CubicPoly
{
    private float c0;
    private float c1;
    private float c2;
    private float c3;

    /*
     * Compute coefficients for a cubic polynomial
     *   p(s) = c0 + c1*s + c2*s^2 + c3*s^3
     * such that
     *   p(0) = x0, p(1) = x1
     *  and
     *   p'(0) = t0, p'(1) = t1.
     */
    public void Init(float x0, float x1, float t0, float t1)
    {
        c0 = x0;
        c1 = t0;
        c2 = -3 * x0 + 3 * x1 - 2 * t0 - t1;
        c3 = 2 * x0 - 2 * x1 + t0 + t1;
        c0 = x0;
    }

    public void InitCatmullRom(float x0, float x1, float x2, float x3, float tension)
    {
        Init(x1, x2, tension * (x2 - x0), tension * (x3 - x1));
    }

    public void InitNonuniformCatmullRom(float x0, float x1, float x2, float x3, float dt0, float dt1, float dt2)
    {
        // compute tangents when parameterized in [t1,t2]
        var t1 = (x1 - x0) / dt0 - (x2 - x0) / (dt0 + dt1) + (x2 - x1) / dt1;
        var t2 = (x2 - x1) / dt1 - (x3 - x1) / (dt1 + dt2) + (x3 - x2) / dt2;

        // rescale tangents for parametrization in [0,1]
        t1 *= dt1;
        t2 *= dt1;

        Init(x1, x2, t1, t2);
    }

    public float Calc(float t)
    {
        var t2 = t * t;
        var t3 = t2 * t;

        return c0 + c1 * t + c2 * t2 + c3 * t3;
    }
}

[Serializable]
public class CatmullRomCurve3 : Curve
{
    public bool Closed;

    public string CurveType;

    public List<Vector3> Points;

    private CubicPoly px = new();

    private CubicPoly py = new();

    private CubicPoly pz = new();

    public float Tension;
    private Vector3 tmp = new();

    public CatmullRomCurve3(List<Vector3> points = null, bool? closed = null, string curveType = null,
        float? tension = null)
    {
        Points = points != null ? points : new List<Vector3>();

        Closed = closed != null ? closed.Value : false;

        CurveType = curveType != null ? curveType : "centripetal";

        Tension = tension != null ? tension.Value : 0.5f;
    }

    protected CatmullRomCurve3(CatmullRomCurve3 source)
    {
        Points = new List<Vector3>();

        for (var i = 0; i < source.Points.Count; i++)
        {
            var point = source.Points[i];
            Points.Add((Vector3)point.Clone());
        }

        Closed = source.Closed;
        CurveType = source.CurveType;
        Tension = source.Tension;
    }

    public new object Clone()
    {
        return new CatmullRomCurve3(this);
    }

    public override Vector3 GetPoint(float t, Vector3 optionalTarget = null)
    {
        var point = optionalTarget != null ? optionalTarget : new Vector3();

        var points = Points;
        var l = points.Count;

        var p = (l - (Closed ? 0 : 1)) * t;
        var intPoint = (int)Math.Floor(p);
        var weight = p - intPoint;

        if (Closed)
        {
            intPoint += intPoint > 0 ? 0 : (int)(Math.Floor((decimal)(Math.Abs(intPoint) / l)) + 1) * l;
        }
        else if (weight == 0 && intPoint == l - 1)
        {
            intPoint = l - 2;
            weight = 1;
        }

        Vector3 p0, p1, p2, p3; // 4 points

        if (Closed || intPoint > 0)
        {
            p0 = points[(intPoint - 1) % l];
        }
        else
        {
            // extrapolate first point
            tmp.SubVectors(points[0], points[1]).Add(points[0]);
            p0 = tmp;
        }

        p1 = points[intPoint % l];
        p2 = points[(intPoint + 1) % l];

        if (Closed || intPoint + 2 < l)
        {
            p3 = points[(intPoint + 2) % l];
        }
        else
        {
            // extrapolate last point
            tmp.SubVectors(points[l - 1], points[l - 2]).Add(points[l - 1]);
            p3 = tmp;
        }

        if (CurveType == "centripetal" || CurveType == "chordal")
        {
            // init Centripetal / Chordal Catmull-Rom
            var pow = CurveType == "chordal" ? 0.5f : 0.25f;
            var dt0 = (float)Math.Pow(p0.DistanceToSquared(p1), pow);
            var dt1 = (float)Math.Pow(p1.DistanceToSquared(p2), pow);
            var dt2 = (float)Math.Pow(p2.DistanceToSquared(p3), pow);

            // safety check for repeated points
            if (dt1 < 1e-4) dt1 = 1.0f;
            if (dt0 < 1e-4) dt0 = dt1;
            if (dt2 < 1e-4) dt2 = dt1;

            px.InitNonuniformCatmullRom(p0.X, p1.X, p2.X, p3.X, dt0, dt1, dt2);
            py.InitNonuniformCatmullRom(p0.Y, p1.Y, p2.Y, p3.Y, dt0, dt1, dt2);
            pz.InitNonuniformCatmullRom(p0.Z, p1.Z, p2.Z, p3.Z, dt0, dt1, dt2);
        }
        else if (CurveType == "catmullrom")
        {
            px.InitCatmullRom(p0.X, p1.X, p2.X, p3.X, Tension);
            py.InitCatmullRom(p0.Y, p1.Y, p2.Y, p3.Y, Tension);
            pz.InitCatmullRom(p0.Z, p1.Z, p2.Z, p3.Z, Tension);
        }

        point.Set(
            px.Calc(weight),
            py.Calc(weight),
            pz.Calc(weight)
        );

        return point;
    }
}