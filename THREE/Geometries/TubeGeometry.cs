using System.Collections;

namespace THREE;

[Serializable]
public class TubeGeometry : Geometry
{
    public List<Vector3> binormals = new();

    public List<Vector3> normals = new();

    public Hashtable parameters;
    public List<Vector3> tangents = new();

    public TubeGeometry(Curve path, int? tubularSegments = null, float? radius = null, int? radialSegments = null,
        bool? closed = null)
    {
        type = "TubeGeometry";

        parameters = new Hashtable
        {
            { "path", path },
            { "tubularSegments", tubularSegments },
            { "radius", radius },
            { "radialSegments", radialSegments },
            { "closed", closed }
        };

        var bufferGeometry = new TubeBufferGeometry(path, tubularSegments, radius, radialSegments, closed);

        tangents = bufferGeometry.tangents;

        normals = bufferGeometry.normals;

        binormals = bufferGeometry.binormals;

        FromBufferGeometry(bufferGeometry);
        MergeVertices();
    }
}

[Serializable]
public class TubeBufferGeometry : BufferGeometry
{
    public List<Vector3> binormals = new();

    private bool closed;

    private List<int> indexList = new();

    private Vector3 normal = new();

    private List<float> normalList = new();

    public List<Vector3> normals = new();

    private Vector3 P = new();
    public Hashtable parameters;

    private Curve path;

    private int radialSegments;

    private float radius;

    public List<Vector3> tangents = new();

    private int tubularSegments;

    private Vector2 uv = new();

    private List<float> uvList = new();

    private Vector3 vertex = new();

    private List<float> verticeList = new();


    public TubeBufferGeometry(Curve path, int? tubularSegments = null, float? radius = null, int? radialSegments = null,
        bool? closed = null)
    {
        type = "TubeBufferGeometry";

        parameters = new Hashtable
        {
            { "path", path },
            { "tubularSegments", tubularSegments },
            { "radius", radius },
            { "radialSegments", radialSegments },
            { "closed", closed }
        };

        this.path = path;

        this.tubularSegments = tubularSegments != null ? (int)tubularSegments : 64;

        this.radius = radius != null ? (float)radius : 1;

        this.radialSegments = radialSegments != null ? (int)radialSegments : 8;

        this.closed = closed != null ? (bool)closed : false;

        var frames = path.ComputeFrenetFrames(this.tubularSegments, this.closed);

        tangents = (List<Vector3>)frames["tangents"];
        normals = (List<Vector3>)frames["normals"];
        binormals = (List<Vector3>)frames["binormals"];

        GenerateBufferData();

        SetIndex(indexList);

        SetAttribute("position", new BufferAttribute<float>(verticeList.ToArray(), 3));

        SetAttribute("normal", new BufferAttribute<float>(normalList.ToArray(), 3));

        SetAttribute("uv", new BufferAttribute<float>(uvList.ToArray(), 2));
    }

    private void GenerateBufferData()
    {
        for (var i = 0; i < tubularSegments; i++) GenerateSegment(i);

        // if the geometry is not closed, generate the last row of vertices and normals
        // at the regular position on the given path
        //
        // if the geometry is closed, duplicate the first row of vertices and normals (uvs will differ)

        GenerateSegment(closed == false ? tubularSegments : 0);

        // uvs are generated in a separate function.
        // this makes it easy compute correct values for closed geometries

        GenerateUVs();

        // finally create faces

        GenerateIndices();
    }

    private void GenerateSegment(int i)
    {
        // we use getPointAt to sample evenly distributed points from the given path

        P = path.GetPointAt(i / (float)tubularSegments, P);

        // retrieve corresponding normal and binormal

        var N = normals[i];
        var B = binormals[i];

        // generate normals and vertices for the current segment

        for (var j = 0; j <= radialSegments; j++)
        {
            var v = j / (float)radialSegments * (float)Math.PI * 2;

            var sin = (float)Math.Sin(v);
            var cos = -(float)Math.Cos(v);

            // normal

            normal.X = cos * N.X + sin * B.X;
            normal.Y = cos * N.Y + sin * B.Y;
            normal.Z = cos * N.Z + sin * B.Z;
            normal.Normalize();

            normalList.Add(normal.X, normal.Y, normal.Z);

            // vertex

            vertex.X = P.X + radius * normal.X;
            vertex.Y = P.Y + radius * normal.Y;
            vertex.Z = P.Z + radius * normal.Z;

            verticeList.Add(vertex.X, vertex.Y, vertex.Z);
        }
    }

    private void GenerateIndices()
    {
        for (var j = 1; j <= tubularSegments; j++)
        for (var i = 1; i <= radialSegments; i++)
        {
            var a = (radialSegments + 1) * (j - 1) + (i - 1);
            var b = (radialSegments + 1) * j + (i - 1);
            var c = (radialSegments + 1) * j + i;
            var d = (radialSegments + 1) * (j - 1) + i;

            // faces
            indexList.Add(a, b, d);
            indexList.Add(b, c, d);
        }
    }

    private void GenerateUVs()
    {
        for (var i = 0; i <= tubularSegments; i++)
        for (var j = 0; j <= radialSegments; j++)
        {
            uv.X = i / tubularSegments;
            uv.Y = j / radialSegments;

            uvList.Add(uv.X, uv.Y);
        }
    }
}