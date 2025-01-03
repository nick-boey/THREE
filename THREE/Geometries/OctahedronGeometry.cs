using System.Collections;

namespace THREE;

[Serializable]
public class OctahedronGeometry : Geometry
{
    public Hashtable parameters;

    public OctahedronGeometry(float? radius = null, float? detail = null)
    {
        parameters = new Hashtable
        {
            { "radius", radius },
            { "detail", detail }
        };

        FromBufferGeometry(new OctahedronBufferGeometry(radius, detail));
        MergeVertices();
    }
}

[Serializable]
public class OctahedronBufferGeometry : PolyhedronBufferGeometry
{
    private static List<float> vertices = new()
    {
        1, 0, 0, -1, 0, 0, 0, 1, 0,
        0, -1, 0, 0, 0, 1, 0, 0, -1
    };

    private static List<int> indices = new()
    {
        0, 2, 4, 0, 4, 3, 0, 3, 5,
        0, 5, 2, 1, 2, 5, 1, 5, 3,
        1, 3, 4, 1, 4, 2
    };

    public OctahedronBufferGeometry(float? radius = null, float? detail = null) : base(vertices, indices, radius,
        detail)
    {
    }
}