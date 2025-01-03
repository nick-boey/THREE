namespace THREE;

[Serializable]
public class Line3
{
    public Vector3 End = Vector3.Zero();
    public Vector3 Start = Vector3.Zero();

    public Line3()
    {
    }

    public Line3(Vector3 start, Vector3 end)
    {
        Start = start;

        End = end;
    }

    public void Set(Vector3 start, Vector3 end)
    {
        Start = start;

        End = end;
    }

    public Vector3 GetCenter()
    {
        return (Start + End) * 0.5f;
    }

    public Vector3 Delta(Vector3 target = null)
    {
        if (target == null) target = new Vector3();
        return target.SubVectors(End, Start);
    }

    public float DistanceSq()
    {
        return Start.DistanceToSquared(End);
    }

    public float Distance()
    {
        return Start.DistanceTo(End);
    }

    public Vector3 At(float t, Vector3 target = null)
    {
        if (target == null) target = new Vector3();
        target.Copy(Delta() * t + Start);
        return target;
    }

    public float ClosestPointToPointParameter(Vector3 point, bool clampToLine = false)
    {
        var startP = point - Start;
        var startEnd = End - Start;

        var startEnd2 = Vector3.Dot(startEnd, startEnd);
        var startEnd_startP = Vector3.Dot(startEnd, startP);

        var t = startEnd_startP / startEnd2;

        if (clampToLine) t = t.Clamp(0, 1);
        return t;
    }

    public Vector3 ClosestPointToPoint(Vector3 point, bool clampToLine = false, Vector3 target = null)
    {
        var t = ClosestPointToPointParameter(point, clampToLine);

        if (target == null) target = new Vector3();
        return Delta(target).MultiplyScalar(t).Add(Start);
    }

    public void ApplyMatrix4(Matrix4 matrix)
    {
        Start.ApplyMatrix4(matrix);
        End.ApplyMatrix4(matrix);
    }

    public override bool Equals(object obj)
    {
        var line = obj as Line3;

        return line.Start.Equals(Start) && line.End.Equals(End);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}