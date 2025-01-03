using System.Collections;

namespace THREE;

[Serializable]
public class Shape : Path
{
    public List<Path> Holes;
    public Guid Uuid = Guid.NewGuid();

    public Shape(List<Vector3> points = null) : base(points)
    {
        Holes = new List<Path>();
    }

    protected Shape(Shape source)
    {
        Holes = new List<Path>();
        for (var i = 0; i < source.Holes.Count; i++)
        {
            var hole = source.Holes[i];

            Holes.Add(hole.Clone() as Path);
        }
    }

    public new object Clone()
    {
        return new Shape(this);
    }

    public List<List<Vector3>> GetPointsHoles(float divisions)
    {
        var holePts = new List<List<Vector3>>();

        for (var i = 0; i < Holes.Count; i++)
        {
            if (Holes[i] == null) continue;
            holePts.Add(Holes[i].GetPoints(divisions));
        }

        return holePts;
    }

    public Hashtable ExtractPoints(float divisions)
    {
        return new Hashtable
        {
            { "shape", GetPoints(divisions) },
            { "holes", GetPointsHoles(divisions) }
        };
    }
}