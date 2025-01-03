using MIConvexHull;

namespace THREE;

[Serializable]
public class ConvexGeometry : Geometry
{
    public ConvexGeometry(Vector3[] points)
    {
        FromBufferGeometry(new ConvexBufferGeometry(points));
        MergeVertices();
    }
}

[Serializable]
public class ConvexBufferGeometry : BufferGeometry
{
    public ConvexBufferGeometry()
    {
    }

    public ConvexBufferGeometry(List<Vector3> points)
    {
        List<TVertex> vertices = new();

        for (var i = 0; i < points.Count; i++) vertices.Add(new TVertex(points[i].X, points[i].Y, points[i].Z));
        var convexHull = MIConvexHull.ConvexHull.Create<TVertex, TFace>(vertices);
        var faces = convexHull.Result.Faces.ToList();
        var (positions, normals) = ConvertThreeVertices(faces);

        SetAttribute("position", new BufferAttribute<float>(positions.ToArray(), 3));
        SetAttribute("normal", new BufferAttribute<float>(normals.ToArray(), 3));
    }

    public ConvexBufferGeometry(Vector3[] points) : this(points.ToList())
    {
    }

    private (List<float>, List<float>) ConvertThreeVertices(List<TFace> faces)
    {
        var tvertices = new List<float>();
        var tnormals = new List<float>();

        for (var i = 0; i < faces.Count; i++)
        {
            var face = faces[i];

            tvertices.Add((float)face.Vertices[0].Position[0], (float)face.Vertices[0].Position[1],
                (float)face.Vertices[0].Position[2]);
            tvertices.Add((float)face.Vertices[1].Position[0], (float)face.Vertices[1].Position[1],
                (float)face.Vertices[1].Position[2]);
            tvertices.Add((float)face.Vertices[2].Position[0], (float)face.Vertices[2].Position[1],
                (float)face.Vertices[2].Position[2]);

            tnormals.Add((float)face.Normal[0], (float)face.Normal[1], (float)face.Normal[2]);
            tnormals.Add((float)face.Normal[0], (float)face.Normal[1], (float)face.Normal[2]);
            tnormals.Add((float)face.Normal[0], (float)face.Normal[1], (float)face.Normal[2]);
        }

        return (tvertices, tnormals);
    }

    [Serializable]
    private class TVertex : IVertex
    {
        public TVertex(double x, double y, double z)
        {
            Position = new[] { x, y, z };
        }

        public double[] Position { get; set; }
    }

    [Serializable]
    private class TFace : ConvexFace<TVertex, TFace>
    {
    }
}