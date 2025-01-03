using System.Collections;

namespace THREE;

[Serializable]
public class TetrahedronGeometry : Geometry
{
    public Hashtable parameters;

    public TetrahedronGeometry(float? radius = null, float? detail = null)
    {
        parameters = new Hashtable
        {
            { "radius", radius },
            { "detail", detail }
        };

        FromBufferGeometry(new TetrahedronBufferGeometry(radius, detail));
        MergeVertices();
    }
}

[Serializable]
public class TetrahedronBufferGeometry : PolyhedronBufferGeometry
{
    private static List<float> vertices = new()
    {
        1, 1, 1, -1, -1, 1, -1, 1, -1, 1, -1, -1
    };

    private static List<int> indices = new()
    {
        2, 1, 0, 0, 3, 2, 1, 3, 0, 2, 3, 1
    };

    public TetrahedronBufferGeometry(float? radius = null, float? detail = null) : base(vertices, indices, radius,
        detail)
    {
    }
}