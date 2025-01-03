using System.Collections;

namespace THREE;

[Serializable]
public class ShapeGeometry : Geometry
{
    public Hashtable parameter;

    public ShapeGeometry(List<Shape> shapes, float? curveSegments = null)
    {
        parameter = new Hashtable
        {
            { "shapes", shapes },
            { "curveSegments", curveSegments }
        };

        FromBufferGeometry(new ShapeBufferGeometry(shapes, curveSegments));
        MergeVertices();
    }
}

[Serializable]
public class ShapeBufferGeometry : BufferGeometry
{
    private float CurveSegments;

    private int groupCount;

    private int groupStart;

    private List<int> indices = new();
    private List<float> normals = new();
    public Hashtable parameter;
    private List<float> uvs = new();
    private List<float> vertices = new();

    public ShapeBufferGeometry(Shape shape, float? curveSegments = null)
    {
        parameter = new Hashtable
        {
            { "shapes", shape },
            { "curveSegments", curveSegments }
        };

        CurveSegments = curveSegments != null ? curveSegments.Value : 12;

        AddShape(shape);

        SetIndex(indices);

        var positions = new BufferAttribute<float>(vertices.ToArray(), 3);


        SetAttribute("position", positions);

        var normalAttributes = new BufferAttribute<float>(normals.ToArray(), 3);

        SetAttribute("normal", normalAttributes);

        var uvAttributes = new BufferAttribute<float>(uvs.ToArray(), 2);

        SetAttribute("uv", uvAttributes);
    }

    public ShapeBufferGeometry(List<Shape> shapes, float? curveSegments = null)
    {
        parameter = new Hashtable
        {
            { "shapes", shapes },
            { "curveSegments", curveSegments }
        };

        CurveSegments = curveSegments != null ? curveSegments.Value : 12;


        // helper variables


        if (shapes.Count == 1)
            AddShape(shapes[0]);
        else
            for (var i = 0; i < shapes.Count; i++)
            {
                AddShape(shapes[i]);
                AddGroup(groupStart, groupCount, i);

                groupStart += groupCount;
                groupCount = 0;
            }

        SetIndex(indices);

        var positions = new BufferAttribute<float>(vertices.ToArray(), 3);
        //positions.ItemSize = 3;
        //positions.Type = typeof(float);

        SetAttribute("position", positions);

        var normalAttributes = new BufferAttribute<float>(normals.ToArray(), 3);
        //normalAttributes.ItemSize = 3;
        //normalAttributes.Type = typeof(float);
        SetAttribute("normal", normalAttributes);

        var uvAttributes = new BufferAttribute<float>(uvs.ToArray(), 2);
        //uvAttributes.ItemSize = 2;
        //uvAttributes.Type = typeof(float);
        SetAttribute("uv", uvAttributes);
    }

    private void AddShape(Shape shape)
    {
        int i, l;

        List<Vector3> shapeHole = null;

        var indexOffset = vertices.Count / 3;
        var points = shape.ExtractPoints(CurveSegments);

        var shapeVertices = (List<Vector3>)points["shape"];
        var shapeHoles = (List<List<Vector3>>)points["holes"];

        // check direction of vertices

        if (ShapeUtils.IsClockWise(shapeVertices) == false)
        {
            shapeVertices.Reverse();


            for (i = 0, l = shapeHoles.Count; i < l; i++)
            {
                shapeHole = shapeHoles[i];

                if (ShapeUtils.IsClockWise(shapeHole))
                {
                    shapeHole.Reverse();
                    shapeHoles[i] = shapeHole;
                }
            }
        }

        var faces = ShapeUtils.TriangulateShape(shapeVertices, shapeHoles);

        // join vertices of inner and outer paths to a single array

        for (i = 0, l = shapeHoles.Count; i < l; i++)
        {
            shapeHole = shapeHoles[i];
            shapeVertices = shapeVertices.Concat(shapeHole);
        }

        // vertices, normals, uvs

        for (i = 0, l = shapeVertices.Count; i < l; i++)
        {
            var vertex = shapeVertices[i];

            vertices.Add(vertex.X, vertex.Y, vertex.Z);
            normals.Add(0, 0, 1);
            uvs.Add(vertex.X, vertex.Y); // world uvs
        }

        // incides

        for (i = 0, l = faces.Count; i < l; i++)
        {
            var face = faces[i];

            var a = face[0] + indexOffset;
            var b = face[1] + indexOffset;
            var c = face[2] + indexOffset;

            indices.Add(a, b, c);
            groupCount += 3;
        }
    }
}