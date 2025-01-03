using System.Diagnostics;

namespace THREE;

[Serializable]
public class LineSegmentsGeometry : InstancedBufferGeometry
{
    private Box3 _box = new();
    private Vector3 _vector = new();

    public LineSegmentsGeometry()
    {
        var positions = new float[] { -1, 2, 0, 1, 2, 0, -1, 1, 0, 1, 1, 0, -1, 0, 0, 1, 0, 0, -1, -1, 0, 1, -1, 0 };
        var uvs = new float[] { -1, 2, 1, 2, -1, 1, 1, 1, -1, -1, 1, -1, -1, -2, 1, -2 };
        var index = new List<int> { 0, 2, 1, 2, 3, 1, 2, 4, 3, 4, 5, 3, 4, 6, 5, 6, 7, 5 };

        SetIndex(index);
        SetAttribute("position", new BufferAttribute<float>(positions, 3));
        SetAttribute("uv", new BufferAttribute<float>(uvs, 2));
    }

    public new LineSegmentsGeometry ApplyMatrix(Matrix4 matrix)
    {
        return ApplyMatrix4(matrix);
    }

    public new LineSegmentsGeometry ApplyMatrix4(Matrix4 matrix)
    {
        var start = Attributes["instanceStart"] as InterleavedBufferAttribute<float>;
        var end = Attributes["instanceEnd"] as InterleavedBufferAttribute<float>;
        if (start != null && end != null)
        {
            start.ApplyMatrix4(matrix);

            end.ApplyMatrix4(matrix);

            start.NeedsUpdate = true;
        }

        if (BoundingBox != null) ComputeBoundingBox();

        if (BoundingSphere != null) ComputeBoundingSphere();
        return this;
    }

    public virtual LineSegmentsGeometry SetPositions(float[] array)
    {
        var instanceBuffer = new InstancedInterleavedBuffer<float>(array, 6, 1); // xyz, xyz

        SetAttribute("instanceStart", new InterleavedBufferAttribute<float>(instanceBuffer, 3, 0)); // xyz
        SetAttribute("instanceEnd", new InterleavedBufferAttribute<float>(instanceBuffer, 3, 3)); // xyz

        //

        ComputeBoundingBox();
        ComputeBoundingSphere();

        return this;
    }

    public LineSegmentsGeometry SetColors(float[] array)
    {
        var instanceColorBuffer = new InstancedInterleavedBuffer<float>(array, 6, 1); // rgb, rgb

        SetAttribute("instanceColorStart", new InterleavedBufferAttribute<float>(instanceColorBuffer, 3, 0)); // rgb
        SetAttribute("instanceColorEnd", new InterleavedBufferAttribute<float>(instanceColorBuffer, 3, 3)); // rgb

        return this;
    }

    public LineSegmentsGeometry FromWireframeGeometry(BufferGeometry geometry)
    {
        SetPositions((geometry.Attributes["position"] as BufferAttribute<float>).Array);

        return this;
    }

    public LineSegmentsGeometry FromEdgesGeometry(BufferGeometry geometry)
    {
        SetPositions((geometry.Attributes["position"] as BufferAttribute<float>).Array);

        return this;
    }

    public LineSegmentsGeometry FromMesh(Mesh mesh)
    {
        FromWireframeGeometry(mesh.Geometry as BufferGeometry);

        return this;
    }

    public LineSegmentsGeometry FromLineSegments(LineSegments lineSegments)
    {
        var geometry = lineSegments.Geometry as BufferGeometry;
        SetPositions((geometry.Attributes["position"] as BufferAttribute<float>).Array); // assumes non-indexed
        // set colors, maybe
        return this;
    }

    public new void ComputeBoundingBox()
    {
        if (BoundingBox == null) BoundingBox = new Box3();

        var start = Attributes["instanceStart"] as InterleavedBufferAttribute<float>;
        var end = Attributes["instanceEnd"] as InterleavedBufferAttribute<float>;

        if (start != null && end != null)
        {
            BoundingBox.SetFromBufferAttribute(start);

            _box.SetFromBufferAttribute(end);

            BoundingBox.Union(_box);
        }
    }

    public new void ComputeBoundingSphere()
    {
        if (BoundingSphere == null) BoundingSphere = new Sphere();

        if (BoundingBox == null) ComputeBoundingBox();

        var start = Attributes["instanceStart"] as InterleavedBufferAttribute<float>;
        var end = Attributes["instanceEnd"] as InterleavedBufferAttribute<float>;

        if (start != null && end != null)
        {
            var center = BoundingSphere.Center;

            BoundingBox.GetCenter(center);

            float maxRadiusSq = 0;

            for (int i = 0, il = start.count; i < il; i++)
            {
                _vector.FromBufferAttribute(start, i);
                maxRadiusSq = Math.Max(maxRadiusSq, center.DistanceToSquared(_vector));

                _vector.FromBufferAttribute(end, i);
                maxRadiusSq = Math.Max(maxRadiusSq, center.DistanceToSquared(_vector));
            }

            BoundingSphere.Radius = (float)Math.Sqrt(maxRadiusSq);

            if (BoundingSphere.Radius == float.NaN)
            {
                Debug.WriteLine(
                    "THREE.LineSegmentsGeometry.computeBoundingSphere(): Computed radius is NaN. The instanced position data is likely to have NaN values.");
                Environment.Exit(-1);
            }
        }
    }
}