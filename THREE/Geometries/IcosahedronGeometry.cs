namespace THREE;

[Serializable]
public class IcosahedronGeometry : Geometry
{
    public IcosahedronGeometry(float radius, float? detail = null)
    {
        FromBufferGeometry(new IcosahedronBufferGeometry(radius, detail));
        MergeVertices();
    }
}

[Serializable]
public class IcosahedronBufferGeometry : PolyhedronBufferGeometry
{
    private static float t = (1 + (float)Math.Sqrt(5)) / 2;

    private static List<float> vertices = new()
    {
        -1, t, 0, 1, t, 0, -1, -t, 0, 1, -t, 0,
        0, -1, t, 0, 1, t, 0, -1, -t, 0, 1, -t,
        t, 0, -1, t, 0, 1, -t, 0, -1, -t, 0, 1
    };


    private static List<int> indices = new()
    {
        0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11,
        1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8,
        3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9,
        4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1
    };

    public IcosahedronBufferGeometry(float radius, float? detail = null) : base(vertices, indices, radius, detail)
    {
    }
}