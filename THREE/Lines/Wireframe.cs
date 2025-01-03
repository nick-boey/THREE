namespace THREE;

[Serializable]
public class Wireframe : Mesh
{
    private Vector3 _end = new();
    private Vector3 _start = new();

    public Wireframe()
    {
        InitGeometry(null, null);
    }

    public Wireframe(Geometry geometry = null, Material material = null) : base(geometry, material)
    {
    }

    public override void InitGeometry(Geometry geometry, Material material)
    {
        type = "Wireframe";

        if (geometry == null)
            Geometry = new LineSegmentsGeometry();
        else
            Geometry = geometry;

        if (material == null)
        {
            Material = new LineMaterial { Color = new Color().SetHex(0xffffff) };
        }
        else
        {
            Materials.Clear();
            Material = material;
        }

        Materials.Add(Material);

        UpdateMorphTargets();
    }

    public Wireframe ComputeLineDistances()
    {
        var geometry = Geometry as BufferGeometry;

        var instanceStart = geometry.Attributes["instanceStart"] as InterleavedBufferAttribute<float>;
        var instanceEnd = geometry.Attributes["instanceEnd"] as InterleavedBufferAttribute<float>;

        var lineDistances = new float[2 * instanceStart.count];

        for (int i = 0, j = 0, l = instanceStart.count; i < l; i++, j += 2)
        {
            _start.FromBufferAttribute(instanceStart, i);
            _end.FromBufferAttribute(instanceEnd, i);

            lineDistances[j] = j == 0 ? 0 : lineDistances[j - 1];
            lineDistances[j + 1] = lineDistances[j] + _start.DistanceTo(_end);
        }

        var instanceDistanceBuffer = new InstancedInterleavedBuffer<float>(lineDistances, 2, 1); // d0, d1

        geometry.SetAttribute("instanceDistanceStart",
            new InterleavedBufferAttribute<float>(instanceDistanceBuffer, 1, 0)); // d0
        geometry.SetAttribute("instanceDistanceEnd",
            new InterleavedBufferAttribute<float>(instanceDistanceBuffer, 1, 1)); // d1

        return this;
    }
}