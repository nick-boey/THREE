namespace THREE;

[Serializable]
public class EllipseCurve : Curve
{
    public float AEndAngle;

    public float AStartAngle;
    public float Ax;

    public float Ay;

    public bool ClockWise;

    public float Rotation;

    public float XRadius;

    public float YRadius;


    public EllipseCurve(float? aX = null, float? aY = null, float? xRadius = null, float? yRadius = null,
        float? aStartAngle = null, float? aEndAngle = null, bool? clockwise = null, float? rotation = null)
    {
        Ax = aX != null ? aX.Value : 0;
        Ay = aY != null ? aY.Value : 0;

        XRadius = xRadius != null ? xRadius.Value : 1;
        YRadius = yRadius != null ? yRadius.Value : 1;

        AStartAngle = aStartAngle != null ? aStartAngle.Value : 0;
        AEndAngle = aEndAngle != null ? aEndAngle.Value : (float)(2 * Math.PI);

        ClockWise = clockwise != null ? clockwise.Value : false;

        Rotation = rotation != null ? rotation.Value : 0;
    }

    protected EllipseCurve(EllipseCurve source)
    {
        Ax = source.Ax;
        Ay = source.Ay;

        XRadius = source.XRadius;
        YRadius = source.YRadius;

        AStartAngle = source.AStartAngle;
        AEndAngle = source.AEndAngle;

        ClockWise = source.ClockWise;

        Rotation = source.Rotation;
    }

    public new object Clone()
    {
        return new EllipseCurve(this);
    }

    public override Vector3 GetPoint(float t, Vector3? optionalTarget = null)
    {
        var point = optionalTarget ?? new Vector3();

        const float twoPI = (float)Math.PI * 2;

        var deltaAngle = AEndAngle - AStartAngle;

        var samePoints = Math.Abs(deltaAngle) < float.Epsilon;

        while (deltaAngle < 0) deltaAngle += twoPI;
        while (deltaAngle > twoPI) deltaAngle -= twoPI;

        if (deltaAngle < float.Epsilon)
        {
            if (samePoints)
                deltaAngle = 0;
            else
                deltaAngle = twoPI;
        }

        if (ClockWise && !samePoints)
        {
            if (Math.Abs(deltaAngle - twoPI) < 1e-14)
                deltaAngle = -twoPI;
            else
                deltaAngle = deltaAngle - twoPI;
        }

        var angle = AStartAngle + t * deltaAngle;
        var x = Ax + XRadius * (float)Math.Cos(angle);
        var y = Ay + YRadius * (float)Math.Sin(angle);

        if (Rotation != 0)
        {
            var cos = (float)Math.Cos(Rotation);
            var sin = (float)Math.Sin(Rotation);

            var tx = x - Ax;
            var ty = y - Ay;

            // Rotate the point about the center of the ellipse.
            x = tx * cos - ty * sin + Ax;
            y = tx * sin + ty * cos + Ay;
        }

        return point.Set(x, y, 0);
    }
}