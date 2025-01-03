using System.Collections;

namespace THREE;

[Serializable]
public class SphereGeometry : Geometry
{
    public Hashtable parameters;

    public SphereGeometry(float radius, float? widthSegments = null, float? heightSegments = null,
        float? phiStart = null, float? phiLength = null, float? thetaStart = null, float? thetaLength = null)
    {
        parameters = new Hashtable
        {
            { "radius", radius },
            { "withSegments", widthSegments },
            { "heightSegments", heightSegments },
            { "phiStart", phiStart != null ? (float)phiStart : 0 },
            { "phiLength", phiLength != null ? (float)phiLength : 2 * (float)Math.PI },
            { "thetaStart", thetaStart != null ? (float)thetaStart : 0 },
            { "thetaLength", thetaLength != null ? (float)thetaLength : (float)Math.PI }
        };

        FromBufferGeometry(new SphereBufferGeometry(radius, widthSegments, heightSegments, phiStart, phiLength,
            thetaStart, thetaLength));
        MergeVertices();
    }
}

[Serializable]
public class SphereBufferGeometry : BufferGeometry
{
    public Hashtable parameters;

    public SphereBufferGeometry(float radius, float? widthSegments = null, float? heightSegments = null,
        float? phiStart = null, float? phiLength = null, float? thetaStart = null, float? thetaLength = null)
    {
        radius = radius != 0 ? radius : 1;

        if (widthSegments == null) widthSegments = 8;
        if (heightSegments == null) heightSegments = 6;

        widthSegments = (float)Math.Max(3, Math.Floor(widthSegments.Value));

        heightSegments = (float)Math.Max(2, Math.Floor(heightSegments.Value));

        phiStart = phiStart != null ? (float)phiStart : 0;
        phiLength = phiLength != null ? (float)phiLength : 2 * (float)Math.PI;
        ;
        thetaStart = thetaStart != null ? (float)thetaStart : 0;
        thetaLength = thetaLength != null ? (float)thetaLength : (float)Math.PI;

        parameters = new Hashtable
        {
            { "radius", radius },
            { "withSegments", widthSegments },
            { "heightSegments", heightSegments },
            { "phiStart", phiStart != null ? (float)phiStart : 0 },
            { "phiLength", phiLength != null ? (float)phiLength : 2 * (float)Math.PI },
            { "thetaStart", thetaStart != null ? (float)thetaStart : 0 },
            { "thetaLength", thetaLength != null ? (float)thetaLength : (float)Math.PI }
        };

        var thetaEnd = (float)Math.Min((float)(thetaStart + thetaLength), Math.PI);

        var indices = new List<int>();
        var vertices = new List<float>();
        var normals = new List<float>();
        var uvs = new List<float>();
        List<List<int>> grid = new();

        var index = 0;

        var vertex = new Vector3();
        var normal = new Vector3();

        for (var iy = 0; iy <= heightSegments; iy++)
        {
            var verticesRow = new List<int>();

            var v = iy / (float)heightSegments;

            // special case for the poles

            var uOffset = 0.0f;

            if (iy == 0 && thetaStart == 0)
                uOffset = 0.5f / (float)widthSegments;
            else if (iy == heightSegments && thetaEnd == (float)Math.PI) uOffset = -0.5f / (float)widthSegments;

            for (var ix = 0; ix <= widthSegments; ix++)
            {
                var u = ix / (float)widthSegments;

                // vertex

                vertex.X = (float)(-radius * Math.Cos((float)phiStart + u * (float)phiLength) *
                                   Math.Sin((float)thetaStart + v * (float)thetaLength));
                vertex.Y = (float)(radius * Math.Cos((float)thetaStart + v * (float)thetaLength));
                vertex.Z = (float)(radius * Math.Sin((float)phiStart + u * (float)phiLength) *
                                   Math.Sin((float)thetaStart + v * (float)thetaLength));

                vertices.Add(vertex.X);
                vertices.Add(vertex.Y);
                vertices.Add(vertex.Z);

                // normal

                normal.Copy(vertex).Normalize();
                normals.Add(normal.X);
                normals.Add(normal.Y);
                normals.Add(normal.Z);

                // uv

                uvs.Add(u + uOffset);
                uvs.Add(1 - v);

                verticesRow.Add(index++);
            }

            grid.Add(verticesRow);
        }

        // indices

        for (var iy = 0; iy < (int)heightSegments; iy++)
        for (var ix = 0; ix < (int)widthSegments; ix++)
        {
            var a = grid[iy][ix + 1];
            var b = grid[iy][ix];
            var c = grid[iy + 1][ix];
            var d = grid[iy + 1][ix + 1];

            if (iy != 0 || thetaStart > 0)
            {
                indices.Add(a);
                indices.Add(b);
                indices.Add(d);
            }

            if (iy != (int)(heightSegments - 1) || thetaEnd < (float)Math.PI)
            {
                indices.Add(b);
                indices.Add(c);
                indices.Add(d);
            }
        }

        // build geometry

        SetIndex(indices);
        SetAttribute("position", new BufferAttribute<float>(vertices.ToArray(), 3));
        SetAttribute("normal", new BufferAttribute<float>(normals.ToArray(), 3));
        SetAttribute("uv", new BufferAttribute<float>(uvs.ToArray(), 2));
    }
}