namespace THREE;

[Serializable]
public class CurvePath : Curve
{
    public bool AutoClose;

    private List<float> cacheLengths;
    public List<Curve> Curves = new();

    public CurvePath()
    {
    }

    protected CurvePath(CurvePath source) : this()
    {
        Curves = new List<Curve>();

        for (var i = 0; i < source.Curves.Count; i++)
        {
            var curve = source.Curves[i];

            Curves.Add((Curve)curve.Clone());
        }

        AutoClose = source.AutoClose;
    }

    public new object Clone()
    {
        return new CurvePath(this);
    }

    public void Add(Curve curve)
    {
        Curves.Add(curve);
    }

    public void ClosePath()
    {
        var startPoint = Curves[0].GetPoint(0);
        var endPoint = Curves[Curves.Count - 1].GetPoint(1);

        if (!startPoint.Equals(endPoint)) Curves.Add(new LineCurve3(endPoint, startPoint));
    }

    public override Vector3 GetPoint(float t, Vector3 optionalTarget = null)
    {
        var d = t * GetLength();
        var curveLengths = GetCurveLengths();
        var i = 0;

        // To think about boundaries points.

        while (i < curveLengths.Count)
        {
            if (curveLengths[i] >= d)
            {
                var diff = curveLengths[i] - d;
                var curve = Curves[i];

                var segmentLength = curve.GetLength();
                var u = segmentLength == 0 ? 0 : 1 - diff / segmentLength;


                return curve.GetPointAt(u);
            }

            i++;
        }

        return null;
    }

    public override float GetLength()
    {
        var lens = GetCurveLengths();
        return lens[lens.Count - 1];
    }

    public List<float> GetCurveLengths()
    {
        // We use cache values if curves and cache array are same length

        if (cacheLengths != null && cacheLengths.Count == Curves.Count) return cacheLengths;

        // Get length of sub-curve
        // Push sums into cached array

        var lengths = new List<float>();
        var sums = 0.0f;

        for (var i = 0; i < Curves.Count; i++)
        {
            sums += Curves[i].GetLength();
            lengths.Add(sums);
        }

        cacheLengths = lengths;

        return lengths;
    }

    public override List<Vector3> GetSpacedPoints(float? divisions = null)
    {
        if (divisions == null) divisions = 40;

        var points = new List<Vector3>();

        for (var i = 0; i <= divisions; i++) points.Add(GetPoint(i / divisions.Value));

        if (AutoClose) points.Add(points[0]);

        return points;
    }

    public override List<Vector3> GetPoints(float? divisions = null)
    {
        divisions = divisions != null ? divisions : 12;

        var points = new List<Vector3>();
        Vector3 last = null;

        for (var i = 0; i < Curves.Count; i++)
        {
            var curve = Curves[i];
            var resolution = curve != null && curve is EllipseCurve ? divisions * 2
                : curve != null && (curve is LineCurve || curve is LineCurve3) ? 1
                : curve != null && curve is SplineCurve ? divisions * (curve as SplineCurve).Points.Count
                : divisions;

            List<Vector3> pts = null;

            pts = curve.GetPoints(resolution);
            for (var j = 0; j < pts.Count; j++)
            {
                var point = pts[j];

                if (last != null && last.Equals(point)) continue; // ensures no consecutive points are duplicates

                points.Add(point);
                last = point;
            }
        }

        if (AutoClose && points.Count > 1 && !points[points.Count - 1].Equals(points[0])) points.Add(points[0]);

        return points;
    }

    public override void UpdateArcLengths()
    {
        NeedsUpdate = true;

        cacheLengths = null;

        GetCurveLengths();
    }
}