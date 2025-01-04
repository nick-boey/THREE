namespace THREE;

[Serializable]
public class BoxGeometry : Geometry
{
    public float Depth { get; set; }
    public int DepthSegments { get; set; }
    public float Height { get; set; }
    public int HeightSegments { get; set; }
    public float Width { get; set; }
    public int WidthSegments { get; set; }

    public BoxGeometry(float width = 1, float height = 1, float depth = 1, int widthSegments = 1,
        int heightSegments = 1, int depthSegments = 1)
    {
        Type = "BoxGeometry";

        Width = width;

        Height = height;

        Depth = depth;

        WidthSegments = widthSegments;

        HeightSegments = heightSegments;

        DepthSegments = depthSegments;

        FromBufferGeometry(new BoxBufferGeometry(Width, Height, Depth, WidthSegments, HeightSegments, DepthSegments));

        MergeVertices();
    }
}

[Serializable]
public class BoxBufferGeometry : BufferGeometry
{
    public float Depth;

    public int DepthSegments;

    private int groupStart;

    public float Height;

    public int HeightSegments;

    private List<int> indices = new();

    private List<float> normals = new();

    private int numberOfVertices;

    private List<float> uvs = new();

    private List<float> vertices = new();
    public float Width;

    public int WidthSegments;

    public BoxBufferGeometry(float width = 1, float height = 1, float depth = 1, int widthSegments = 1,
        int heightSegments = 1, int depthSegments = 1)
    {
        Width = width;

        Height = height;

        Depth = depth == 0 ? 1 : depth;

        // segments

        WidthSegments = Math.Floor((float)widthSegments) > 0 ? widthSegments : 1;

        HeightSegments = Math.Floor((float)heightSegments) > 0 ? heightSegments : 1;

        DepthSegments = Math.Floor((float)depthSegments) > 0 ? depthSegments : 1;

        // build each side of the box geometry

        BuildPlane('z', 'y', 'x', -1, -1, Depth, Height, Width, DepthSegments, HeightSegments, 0); // px
        BuildPlane('z', 'y', 'x', 1, -1, Depth, Height, -Width, DepthSegments, HeightSegments, 1); // nx
        BuildPlane('x', 'z', 'y', 1, 1, Width, Depth, Height, WidthSegments, DepthSegments, 2); // py
        BuildPlane('x', 'z', 'y', 1, -1, Width, Depth, -Height, WidthSegments, DepthSegments, 3); // ny
        BuildPlane('x', 'y', 'z', 1, -1, Width, Height, Depth, WidthSegments, HeightSegments, 4); // pz
        BuildPlane('x', 'y', 'z', -1, -1, Width, Height, -Depth, WidthSegments, HeightSegments, 5); // nz

        SetIndex(indices);
        SetAttribute("position", new BufferAttribute<float>(vertices.ToArray(), 3));
        SetAttribute("normal", new BufferAttribute<float>(normals.ToArray(), 3));
        SetAttribute("uv", new BufferAttribute<float>(uvs.ToArray(), 2));
    }

    private void BuildPlane(char u, char v, char w, int udir, int vdir, float width, float height, float depth,
        int gridX, int gridY, int materialIndex)
    {
        var segmentWidth = width / gridX;
        var segmentHeight = height / gridY;

        var widthHalf = width / 2;
        var heightHalf = height / 2;
        var depthHalf = depth / 2;

        var gridX1 = gridX + 1;
        var gridY1 = gridY + 1;

        var vertexCounter = 0;
        var groupCount = 0;

        int ix, iy;

        var vector = new Vector3();

        // generate vertices, normals and uvs

        for (iy = 0; iy < gridY1; iy++)
        {
            var y = iy * segmentHeight - heightHalf;

            for (ix = 0; ix < gridX1; ix++)
            {
                var x = ix * segmentWidth - widthHalf;


                // set values to correct vector component

                vector[u] = x * udir;
                vector[v] = y * vdir;
                vector[w] = depthHalf;

                // now apply vector to vertex buffer

                vertices.Add(vector.X);
                vertices.Add(vector.Y);
                vertices.Add(vector.Z);

                // set values to correct vector component

                vector[u] = 0;
                vector[v] = 0;
                vector[w] = depth > 0 ? 1 : -1;

                // now apply vector to normal buffer
                normals.Add(vector.X);
                normals.Add(vector.Y);
                normals.Add(vector.Z);


                // uvs
                uvs.Add(ix / gridX);
                uvs.Add(1 - iy / gridY);

                // counters

                vertexCounter += 1;
            }
        }


        // indices

        // 1. you need three indices to draw a single face
        // 2. a single segment consists of two faces
        // 3. so we need to generate six (2*3) indices per segment

        for (iy = 0; iy < gridY; iy++)
        for (ix = 0; ix < gridX; ix++)
        {
            var a = numberOfVertices + ix + gridX1 * iy;
            var b = numberOfVertices + ix + gridX1 * (iy + 1);
            var c = numberOfVertices + ix + 1 + gridX1 * (iy + 1);
            var d = numberOfVertices + ix + 1 + gridX1 * iy;

            // faces
            indices.Add(a);
            indices.Add(b);
            indices.Add(d);

            indices.Add(b);
            indices.Add(c);
            indices.Add(d);

            // increase counter
            groupCount += 6;
        }

        // add a group to the geometry. this will ensure multi material support

        AddGroup(groupStart, groupCount, materialIndex);

        // calculate new start value for groups

        groupStart += groupCount;

        // update total number of vertices

        numberOfVertices += vertexCounter;
    }
}