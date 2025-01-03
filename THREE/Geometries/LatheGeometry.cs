using System.Collections;

namespace THREE;

[Serializable]
public class LatheGeometry : Geometry
{
    public Hashtable parameters;

    public LatheGeometry(Vector3[] points, float? segments = null, float? phiStart = null, float? phiLength = null)
    {
        parameters = new Hashtable
        {
            { "points", points },
            { "segments", segments },
            { "phiStart", phiStart },
            { "phiLength", phiLength }
        };

        FromBufferGeometry(new LatheBufferGeometry(points, segments, phiStart, phiLength));
        MergeVertices();
    }
}

[Serializable]
public class LatheBufferGeometry : BufferGeometry
{
    public Hashtable parameters;

    public LatheBufferGeometry(Vector3[] points, float? segments = null, float? phiStart = null,
        float? phiLength = null)
    {
        segments = segments != null ? (float)Math.Floor(segments.Value) : 12;
        phiStart = phiStart != null ? phiStart.Value : 0;
        phiLength = phiLength != null ? phiLength.Value : (float)Math.PI * 2;
        phiLength = phiLength.Value.Clamp(0, (float)Math.PI * 2);

        parameters = new Hashtable
        {
            { "points", points },
            { "segments", segments },
            { "phiStart", phiStart },
            { "phiLength", phiLength }
        };

        var indices = new List<int>();
        var vertices = new List<float>();
        var uvs = new List<float>();


        var inverseSegments = 1.0f / (float)segments;

        var vertex = new Vector3();
        var uv = new Vector2();
        int i, j;

        // generate vertices and uvs

        for (i = 0; i <= segments.Value; i++)
        {
            var phi = phiStart.Value + i * inverseSegments * phiLength.Value;

            var sin = (float)Math.Sin(phi);
            var cos = (float)Math.Cos(phi);

            for (j = 0; j <= points.Length - 1; j++)
            {
                // vertex

                vertex.X = points[j].X * sin;
                vertex.Y = points[j].Y;
                vertex.Z = points[j].X * cos;

                vertices.Add(vertex.X);
                vertices.Add(vertex.Y);
                vertices.Add(vertex.Z);

                // uv

                uv.X = i / segments.Value;
                uv.Y = j / (float)(points.Length - 1);

                uvs.Add(uv.X);
                uvs.Add(uv.Y);
            }
        }

        // indices

        for (i = 0; i < segments.Value; i++)
        for (j = 0; j < points.Length - 1; j++)
        {
            var ba = j + i * points.Length;

            var a = ba;
            var b = ba + points.Length;
            var c = ba + points.Length + 1;
            var d = ba + 1;

            // faces
            indices.Add(a);
            indices.Add(b);
            indices.Add(d);
            indices.Add(b);
            indices.Add(c);
            indices.Add(d);
        }

        SetIndex(indices);
        SetAttribute("position", new BufferAttribute<float>(vertices.ToArray(), 3));
        SetAttribute("uv", new BufferAttribute<float>(uvs.ToArray(), 2));

        ComputeVertexNormals();

        if (phiLength.Value == (float)Math.PI * 2)
        {
            var normals = (Attributes["normal"] as BufferAttribute<float>).Array;

            var n1 = new Vector3();
            var n2 = new Vector3();
            var n = new Vector3();

            // this is the buffer offset for the last line of vertices

            var ba = (int)segments.Value * points.Length * 3;

            for (i = 0, j = 0; i < points.Length; i++, j += 3)
            {
                // select the normal of the vertex in the first line

                n1.X = normals[j + 0];
                n1.Y = normals[j + 1];
                n1.Z = normals[j + 2];

                // select the normal of the vertex in the last line

                n2.X = normals[ba + j + 0];
                n2.Y = normals[ba + j + 1];
                n2.Z = normals[ba + j + 2];

                // average normals

                n.AddVectors(n1, n2).Normalize();

                // assign the new values to both normals

                normals[j + 0] = normals[ba + j + 0] = n.X;
                normals[j + 1] = normals[ba + j + 1] = n.Y;
                normals[j + 2] = normals[ba + j + 2] = n.Z;
            }
        }
    }
}