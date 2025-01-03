using System.Collections;

namespace THREE;

[Serializable]
public class CylinderGeometry : Geometry
{
    public Hashtable parameters;

    public CylinderGeometry(float radiusTop, float radiusBottom, float height, int? radialSegments = null,
        int? heightSegments = null, bool? openEnded = null, float? thetaStart = null, float? thetaLength = null)
    {
        parameters = new Hashtable
        {
            { "radiusTop", radiusTop },
            { "radiusBottom", radiusBottom },
            { "height", height },
            { "radialSegments", radialSegments },
            { "heightSegments", heightSegments },
            { "openEnded", openEnded },
            { "thetaStart", thetaStart },
            { "thetaLength", thetaLength }
        };

        FromBufferGeometry(new CylinderBufferGeometry(radiusTop, radiusBottom, height, radialSegments, heightSegments,
            openEnded, thetaStart, thetaLength));
        MergeVertices();
    }
}

[Serializable]
public class CylinderBufferGeometry : BufferGeometry
{
    private int groupStart;

    private float halfHeight;

    public float Height;

    public int HeightSegments;

    private int index;

    private List<List<int>> indexArray = new();

    private List<int> indices = new();

    private List<float> normals = new();

    public bool OpenEnded;

    public Hashtable parameters;

    public int RadialSegments;

    public float RadiusBottom;
    public float RadiusTop;

    public float ThetaLength;

    public float ThetaStart;

    private List<float> uvs = new();

    private List<float> vertices = new();

    public CylinderBufferGeometry(float radiusTop, float radiusBottom, float height, int? radialSegments = null,
        int? heightSegments = null, bool? openEnded = null, float? thetaStart = null, float? thetaLength = null)
    {
        RadiusTop = radiusTop; //==0 ? 1:radiusTop;
        RadiusBottom = radiusBottom;
        Height = height != 0 ? height : 1;

        RadialSegments = radialSegments != null ? (int)Math.Floor((decimal)radialSegments) : 8;
        HeightSegments = heightSegments != null ? (int)Math.Floor((decimal)heightSegments) : 1;

        OpenEnded = openEnded != null ? (bool)openEnded : false;
        ThetaStart = thetaStart != null ? (float)thetaStart : 0.0f;
        ThetaLength = thetaLength != null ? (float)thetaLength : (float)Math.PI * 2;

        halfHeight = height / 2;

        parameters = new Hashtable
        {
            { "radiusTop", RadiusTop },
            { "radiusBottom", RadiusBottom },
            { "height", Height },
            { "radialSegments", RadialSegments },
            { "heightSegments", HeightSegments },
            { "openEnded", OpenEnded },
            { "thetaStart", ThetaStart },
            { "thetaLength", ThetaLength }
        };

        GenerateTorso();

        if (OpenEnded == false)
        {
            if (RadiusTop > 0) GenerateCap(true);
            if (RadiusBottom > 0) GenerateCap(false);
        }

        SetIndex(indices);
        SetAttribute("position", new BufferAttribute<float>(vertices.ToArray(), 3));
        SetAttribute("normal", new BufferAttribute<float>(normals.ToArray(), 3));
        SetAttribute("uv", new BufferAttribute<float>(uvs.ToArray(), 2));
    }

    private void GenerateTorso()
    {
        int x, y;
        var groupCount = 0;

        var normal = Vector3.Zero();
        var vertex = Vector3.Zero();

        var slope = (RadiusBottom - RadiusTop) / Height;

        for (y = 0; y <= HeightSegments; y++)
        {
            var indexRow = new List<int>();

            var v = y / (float)HeightSegments;

            var radius = v * (RadiusBottom - RadiusTop) + RadiusTop;

            for (x = 0; x <= RadialSegments; x++)
            {
                var u = x / (float)RadialSegments;
                var theta = u * ThetaLength + ThetaStart;

                var sinTheta = (float)Math.Sin(theta);
                var cosTheta = (float)Math.Cos(theta);

                //vertex

                vertex.X = radius * sinTheta;
                vertex.Y = -v * Height + halfHeight;
                vertex.Z = radius * cosTheta;
                vertices.Add(vertex.X);
                vertices.Add(vertex.Y);
                vertices.Add(vertex.Z);

                //normal

                normal.Set(sinTheta, slope, cosTheta).Normalize();
                normals.Add(normal.X);
                normals.Add(normal.Y);
                normals.Add(normal.Z);

                //uv

                uvs.Add(u);
                uvs.Add(1 - v);
                indexRow.Add(index++);
            }

            indexArray.Add(indexRow);
        }

        // generate indices

        for (x = 0; x < RadialSegments; x++)
        for (y = 0; y < HeightSegments; y++)
        {
            var a = indexArray[y][x];
            var b = indexArray[y + 1][x];
            var c = indexArray[y + 1][x + 1];
            var d = indexArray[y][x + 1];

            indices.Add(a);
            indices.Add(b);
            indices.Add(d);
            indices.Add(b);
            indices.Add(c);
            indices.Add(d);

            groupCount += 6;
        }

        AddGroup(groupStart, groupCount);

        groupStart += groupCount;
    }

    private void GenerateCap(bool top)
    {
        int x, centerIndexStart, centerIndexEnd;

        var uv = new Vector2();
        var vertex = new Vector3();

        var groupCount = 0;

        var radius = top ? RadiusTop : RadiusBottom;
        float sign = top ? 1 : -1;

        centerIndexStart = index;

        for (x = 1; x <= RadialSegments; x++)
        {
            vertices.Add(0);
            vertices.Add(halfHeight * sign);
            vertices.Add(0);
            normals.Add(0);
            normals.Add(sign);
            normals.Add(0);
            uvs.Add(0.5f);
            uvs.Add(0.5f);
            index++;
        }

        centerIndexEnd = index;

        for (x = 0; x <= RadialSegments; x++)
        {
            var u = x / (float)RadialSegments;
            var theta = u * ThetaLength + ThetaStart;

            var cosTheta = (float)Math.Cos(theta);
            var sinTheta = (float)Math.Sin(theta);

            //vertex

            vertex.X = radius * sinTheta;
            vertex.Y = halfHeight * sign;
            vertex.Z = radius * cosTheta;

            vertices.Add(vertex.X);
            vertices.Add(vertex.Y);
            vertices.Add(vertex.Z);

            //normal
            normals.Add(0);
            normals.Add(sign);
            normals.Add(0);

            //uv

            uv.X = cosTheta * 0.5f + 0.5f;
            uv.Y = sinTheta * 0.5f * sign + 0.5f;
            uvs.Add(uv.X);
            uvs.Add(uv.Y);

            index++;
        }

        //generate indices

        for (x = 0; x < RadialSegments; x++)
        {
            var c = centerIndexStart + x;
            var i = centerIndexEnd + x;

            if (top)
            {
                indices.Add(i);
                indices.Add(i + 1);
                indices.Add(c);
            }
            else
            {
                indices.Add(i + 1);
                indices.Add(i);
                indices.Add(c);
            }

            groupCount += 3;
        }

        AddGroup(groupStart, groupCount, top ? 1 : 2);

        groupStart += groupCount;
    }
}