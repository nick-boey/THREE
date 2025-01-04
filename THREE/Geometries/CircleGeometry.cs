using System.Collections;

namespace THREE;

[Serializable]
public class CircleGeometry : Geometry
{
    public Hashtable parameters;

    public CircleGeometry(float? radius = null, float? segments = null, float? thetaStart = null,
        float? thetaLength = null)
    {
        Type = "CircleGeometry";

        parameters = new Hashtable
        {
            { "radius", radius },
            { "segments", segments },
            { "thetaStart", thetaStart },
            { "thetaLength", thetaLength }
        };

        FromBufferGeometry(new CircleBufferGeometry(radius, segments, thetaStart, thetaLength));

        MergeVertices();
    }
}

[Serializable]
public class CircleBufferGeometry : BufferGeometry
{
    public Hashtable parameters;

    public CircleBufferGeometry(float? radius = null, float? segments = null, float? thetaStart = null,
        float? thetaLength = null)
    {
        Type = "CircleBufferGeometry";

        parameters = new Hashtable
        {
            { "radius", radius },
            { "segments", segments },
            { "thetaStart", thetaStart },
            { "thetaLength", thetaLength }
        };

        if (radius == null) radius = 1;

        segments = segments != null ? Math.Max(3, segments.Value) : 8;

        thetaStart = thetaStart != null ? thetaStart : 0;
        thetaLength = thetaLength != null ? thetaLength : (float)Math.PI * 2;

        var indices = new List<int>();

        var vertices = new List<float>();

        var normals = new List<float>();

        var uvs = new List<float>();

        vertices.Add(0, 0, 0);

        normals.Add(0, 0, 1);

        uvs.Add(0.5f, 0.5f);

        var vertex = new Vector3();

        var uv = new Vector2();

        for (int s = 0, i = 3; s <= segments; s++, i += 3)
        {
            var segment = thetaStart + s / segments * thetaLength;

            // vertex

            vertex.X = (float)(radius * Math.Cos(segment.Value));
            vertex.Y = (float)(radius * Math.Sin(segment.Value));

            vertices.Add(vertex.X, vertex.Y, vertex.Z);

            // normal

            normals.Add(0, 0, 1);

            // uvs

            uv.X = (vertices[i] / radius.Value + 1) / 2.0f;
            uv.Y = (vertices[i + 1] / radius.Value + 1) / 2.0f;

            uvs.Add(uv.X, uv.Y);
        }

        // indices

        for (var i = 1; i <= segments; i++) indices.Add(i, i + 1, 0);

        // build geometry

        SetIndex(indices);

        SetAttribute("position", new BufferAttribute<float>(vertices.ToArray(), 3));


        SetAttribute("normal", new BufferAttribute<float>(normals.ToArray(), 3));


        SetAttribute("uv", new BufferAttribute<float>(uvs.ToArray(), 2));
    }
}