using System.Collections;

namespace THREE;

[Serializable]
public class RingGeometry : Geometry
{
    public Hashtable parameter;

    public RingGeometry(float? innerRadius = null, float? outerRadius = null, float? thetaSegments = null,
        float? phiSegments = null, float? thetaStart = null, float? thetaLength = null)
    {
        Type = "RingGeometry";

        parameter = new Hashtable
        {
            { "innerRadius", innerRadius },
            { "outerRadius", outerRadius },
            { "thetaSegments", thetaSegments },
            { "phiSegments", phiSegments },
            { "thetaStart", thetaStart },
            { "thetaLength", thetaLength }
        };

        FromBufferGeometry(new RingBufferGeometry(innerRadius, outerRadius, thetaSegments, phiSegments, thetaStart,
            thetaLength));
        MergeVertices();
    }
}

[Serializable]
public class RingBufferGeometry : BufferGeometry
{
    public Hashtable parameter;

    public RingBufferGeometry(float? innerRadius = null, float? outerRadius = null, float? thetaSegments = null,
        float? phiSegments = null, float? thetaStart = null, float? thetaLength = null)
    {
        Type = "RingGeometry";

        parameter = new Hashtable
        {
            { "innerRadius", innerRadius },
            { "outerRadius", outerRadius },
            { "thetaSegments", thetaSegments },
            { "phiSegments", phiSegments },
            { "thetaStart", thetaStart },
            { "thetaLength", thetaLength }
        };

        innerRadius = innerRadius != null ? innerRadius : 0.5f;
        outerRadius = outerRadius != null ? outerRadius : 1;

        thetaStart = thetaStart != null ? thetaStart : 0;
        thetaLength = thetaLength != null ? thetaLength : (float)Math.PI * 2;

        thetaSegments = thetaSegments != null ? Math.Max(3, thetaSegments.Value) : 8;
        phiSegments = phiSegments != null ? Math.Max(1, phiSegments.Value) : 1;

        // buffers

        var indices = new List<int>();
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();

        // some helper variables

        float segment;
        var radius = innerRadius;
        var radiusStep = (outerRadius - innerRadius) / phiSegments;
        var vertex = new Vector3();
        var uv = new Vector2();

        // generate vertices, normals and uvs

        for (var j = 0; j <= phiSegments; j++)
        {
            for (var i = 0; i <= thetaSegments; i++)
            {
                // values are generate from the inside of the ring to the outside

                segment = thetaStart.Value + i / thetaSegments.Value * thetaLength.Value;

                // vertex

                vertex.X = (float)(radius * Math.Cos(segment));
                vertex.Y = (float)(radius * Math.Sin(segment));

                vertices.Add((Vector3)vertex.Clone());

                // normal

                normals.Add(new Vector3(0, 0, 1));

                // uv

                uv.X = (vertex.X / outerRadius.Value + 1) / 2;
                uv.Y = (vertex.Y / outerRadius.Value + 1) / 2;

                uvs.Add((Vector2)uv.Clone());
            }

            // increase the radius for next row of vertices

            radius += radiusStep;
        }

        // indices

        for (var j = 0; j < phiSegments; j++)
        {
            var thetaSegmentLevel = j * (thetaSegments.Value + 1);

            for (var i = 0; i < thetaSegments; i++)
            {
                segment = i + thetaSegmentLevel;

                var a = Convert.ToInt32(segment);
                var b = Convert.ToInt32(segment + thetaSegments + 1);
                var c = Convert.ToInt32(segment + thetaSegments + 2);
                var d = Convert.ToInt32(segment + 1);

                // faces

                indices.Add(a, b, d);
                indices.Add(b, c, d);
            }
        }

        // build geometry

        SetIndex(indices);

        var positions = new BufferAttribute<float>();
        positions.ItemSize = 3;
        positions.Type = typeof(float);

        SetAttribute("position", positions.CopyVector3sArray(vertices.ToArray()));

        var normalAttributes = new BufferAttribute<float>();
        normalAttributes.ItemSize = 3;
        normalAttributes.Type = typeof(float);
        SetAttribute("normal", normalAttributes.CopyVector3sArray(normals.ToArray()));

        var uvAttributes = new BufferAttribute<float>();
        uvAttributes.ItemSize = 2;
        uvAttributes.Type = typeof(float);
        SetAttribute("uv", uvAttributes.CopyVector2sArray(uvs.ToArray()));
    }
}