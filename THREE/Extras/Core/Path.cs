namespace THREE;

[Serializable]
public class Path : CurvePath
{
    public Vector3 CurrentPoint = new();

    public Path(List<Vector3> points = null)
    {
        if (points != null) SetFromPoints(points);
    }

    protected Path(Path source)
    {
        CurrentPoint.Copy(source.CurrentPoint);
    }

    public new object Clone()
    {
        return new Path(this);
    }

    public Path SetFromPoints(List<Vector3> points)
    {
        MoveTo(points[0].X, points[0].Y, points[0].Z);

        for (var i = 1; i < points.Count; i++) LineTo(points[i].X, points[i].Y, points[i].Z);

        return this;
    }

    public Path MoveTo(float x, float y, float z)
    {
        CurrentPoint.Set(x, y, z);

        return this;
    }

    public Path MoveTo(float x, float y)
    {
        CurrentPoint.Set(x, y, 0);
        return this;
    }

    public Path LineTo(float x, float y, float z)
    {
        var curve = new LineCurve3(CurrentPoint.Clone(), new Vector3(x, y, z));
        Curves.Add(curve);

        CurrentPoint.Set(x, y, z);

        return this;
    }

    public Path LineTo(float x, float y)
    {
        var curve = new LineCurve3(CurrentPoint.Clone(), new Vector3(x, y, 0));
        Curves.Add(curve);

        CurrentPoint.Set(x, y, 0);

        return this;
    }

    public Path QuadraticCurveTo(float aCPx, float aCPy, float aX, float aY)
    {
        var curve = new QuadraticBezierCurve3(
            CurrentPoint.Clone(),
            new Vector3(aCPx, aCPy, 0),
            new Vector3(aX, aY, 0)
        );

        Curves.Add(curve);

        CurrentPoint.Set(aX, aY, 0);

        return this;
    }

    public Path BezierCurveTo(float aCP1x, float aCP1y, float aCP2x, float aCP2y, float aX, float aY)
    {
        var curve = new CubicBezierCurve3(
            CurrentPoint.Clone(),
            new Vector3(aCP1x, aCP1y, 0),
            new Vector3(aCP2x, aCP2y, 0),
            new Vector3(aX, aY, 0)
        );

        Curves.Add(curve);

        CurrentPoint.Set(aX, aY, 0);

        return this;
    }

    public Path SplineThru(List<Vector3> pts /*Array of Vector*/)
    {
        var npts = new List<Vector3> { CurrentPoint.Clone() };
        npts.AddRange(pts);
        //[this.currentPoint.clone()].concat(pts);

        var curve = new SplineCurve(npts);
        Curves.Add(curve);

        CurrentPoint.Copy(pts[pts.Count - 1]);

        return this;
    }

    public Path Arc(float aX, float aY, float aRadius, float aStartAngle, float aEndAngle, bool aClockwise)
    {
        var x0 = CurrentPoint.X;
        var y0 = CurrentPoint.Y;

        AbsArc(aX + x0, aY + y0, aRadius,
            aStartAngle, aEndAngle, aClockwise);

        return this;
    }

    public Path AbsArc(float aX, float aY, float aRadius, float aStartAngle, float aEndAngle, bool aClockwise)
    {
        AbsEllipse(aX, aY, aRadius, aRadius, aStartAngle, aEndAngle, aClockwise);

        return this;
    }

    public Path Ellipse(float aX, float aY, float xRadius, float yRadius, float aStartAngle, float aEndAngle,
        bool aClockwise, float aRotation)
    {
        var x0 = CurrentPoint.X;
        var y0 = CurrentPoint.Y;

        AbsEllipse(aX + x0, aY + y0, xRadius, yRadius, aStartAngle, aEndAngle, aClockwise, aRotation);

        return this;
    }

    public Path AbsEllipse(float aX, float aY, float xRadius, float yRadius, float aStartAngle, float aEndAngle,
        bool aClockwise, float? aRotation = null)
    {
        var curve = new EllipseCurve(aX, aY, xRadius, yRadius, aStartAngle, aEndAngle, aClockwise, aRotation);

        if (Curves.Count > 0)
        {
            // if a previous curve is present, attempt to join
            var firstPoint = curve.GetPoint(0);

            if (!firstPoint.Equals(CurrentPoint)) LineTo(firstPoint.X, firstPoint.Y, 0);
        }

        Curves.Add(curve);

        var lastPoint = curve.GetPoint(1);
        CurrentPoint.Copy(lastPoint);

        return this;
    }
}