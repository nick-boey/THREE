using System.Collections;

namespace THREE;

[Serializable]
public class TorusGeometry : Geometry
{
    public Hashtable parameters;

    public TorusGeometry(float? radius = null, float? tube = null, float? radialSegments = null,
        float? tubularSegments = null, float? arc = null)
    {
        parameters = new Hashtable
        {
            { "radius", radius },
            { "tube", radius },
            { "radialSegments", radius },
            { "tubularSegments", radius },
            { "arc", radius }
        };

        FromBufferGeometry(new TorusBufferGeometry(radius, tube, radialSegments, tubularSegments, arc));
        MergeVertices();
    }
}

[Serializable]
public class TorusBufferGeometry : BufferGeometry
{
    public Hashtable parameters;

    public TorusBufferGeometry(float? radius = null, float? tube = null, float? radialSegments = null,
        float? tubularSegments = null, float? arc = null)
    {
        radius = radius != null ? radius : 1;
        tube = tube != null ? tube : 1;
        radialSegments = radialSegments != null ? (float)Math.Floor(radialSegments.Value) : 8;
        tubularSegments = tubularSegments != null ? (float)Math.Floor(tubularSegments.Value) : 6;
        arc = arc != null ? arc : (float)Math.PI * 2;
        parameters = new Hashtable
        {
            { "radius", radius },
            { "tube", radius },
            { "radialSegments", radius },
            { "tubularSegments", radius },
            { "arc", radius }
        };

        var indices = new List<int>();
        var vertices = new List<float>();
        var normals = new List<float>();
        var uvs = new List<float>();

        // helper variables

        var center = new Vector3();
        var vertex = new Vector3();
        var normal = new Vector3();

        int j, i;

        // generate vertices, normals and uvs

        for (j = 0; j <= radialSegments; j++)
        for (i = 0; i <= tubularSegments; i++)
        {
            var u = i / (float)tubularSegments * (float)arc;
            var v = j / (float)radialSegments * (float)Math.PI * 2;

            // vertex
            vertex.X = (float)((radius + tube * Math.Cos(v)) * Math.Cos(u));
            vertex.Y = (float)((radius + tube * Math.Cos(v)) * Math.Sin(u));
            vertex.Z = (float)(tube * Math.Sin(v));

            vertices.Add(vertex.X, vertex.Y, vertex.Z);

            // normal
            center.X = radius.Value * (float)Math.Cos(u);
            center.Y = radius.Value * (float)Math.Sin(u);
            normal.SubVectors(vertex, center).Normalize();

            normals.Add(normal.X, normal.Y, normal.Z);

            // uv
            uvs.Add(i / tubularSegments.Value);
            uvs.Add(j / radialSegments.Value);
        }

        // generate indices

        for (j = 1; j <= radialSegments; j++)
        for (i = 1; i <= tubularSegments; i++)
        {
            // indices
            var a = ((int)tubularSegments + 1) * j + i - 1;
            var b = ((int)tubularSegments + 1) * (j - 1) + i - 1;
            var c = ((int)tubularSegments + 1) * (j - 1) + i;
            var d = ((int)tubularSegments + 1) * j + i;

            // faces
            indices.Add(a, b, d);
            indices.Add(b, c, d);
        }

        // build geometry

        SetIndex(indices);
        SetAttribute("position", new BufferAttribute<float>(vertices.ToArray(), 3));
        SetAttribute("normal", new BufferAttribute<float>(normals.ToArray(), 3));
        SetAttribute("uv", new BufferAttribute<float>(uvs.ToArray(), 2));
    }
}