using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class Line : Object3D
{
    public Vector3 End = Vector3.Zero();

    public int LinePieces = 1;
    public int LineStrip = 0;

    public int Mode;

    public Vector3 Start = Vector3.Zero();


    public Line()
    {
    }

    public Line(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public Line(Geometry geometry = null, Material material = null, int? type = null)
    {
        this.type = "Line";

        Geometry = geometry ?? new Geometry();
        Material = material ?? new LineBasicMaterial { Color = new Color().SetHex(0xFFFFFF) };

        Mode = Constants.LineStrip;
        if (null != type) Mode = type.Value;
    }

    public Line(Geometry geometry = null, List<Material> materials = null, int? type = null)
    {
        this.type = "Line";

        Geometry = geometry ?? new Geometry();

        if (materials == null)
        {
            Material = new MeshBasicMaterial { Color = new Color().SetHex(0xffffff) };
            Materials.Add(Material);
        }
        else
        {
            Materials = materials;
            if (Materials.Count > 0)
                Material = Materials[0];
        }

        Mode = Constants.LineStrip;
        if (null != type) Mode = type.Value;
    }

    public void InitGeometry(Geometry geometry, Material material)
    {
        type = "Line";

        Geometry = geometry ?? new Geometry();

        if (material == null)
        {
            Material = new MeshBasicMaterial { Color = new Color().SetHex(0xffffff) };
            Materials.Add(Material);
        }
        else
        {
            Material = material;
            Materials.Add(material);
        }

        Mode = Constants.LineStrip;
    }

    public void InitGeometry(Geometry geometry, List<Material> materials)
    {
        type = "Line";

        Geometry = geometry ?? new Geometry();

        if (materials == null)
        {
            Material = new MeshBasicMaterial { Color = new Color().SetHex(0xffffff) };
            Materials.Add(Material);
        }
        else
        {
            Materials = materials;
            if (Materials.Count > 0)
                Material = Materials[0];
        }

        Mode = Constants.LineStrip;
    }

    public virtual Line ComputeLineDistances()
    {
        var geometry = Geometry;

        var _start = new Vector3();
        var _end = new Vector3();
        if (geometry is BufferGeometry)
        {
            // we assume non-indexed geometry

            if ((geometry as BufferGeometry).Index == null)
            {
                var positionAttribute = (BufferAttribute<float>)(geometry as BufferGeometry).Attributes["position"];
                var lineDistances = new List<float>();
                lineDistances.Add(0);

                for (var i = 1; i < positionAttribute.count; i++)
                {
                    _start.FromBufferAttribute(positionAttribute, i - 1);
                    _end.FromBufferAttribute(positionAttribute, i);

                    //lineDistances[i] = lineDistances[i - 1];

                    lineDistances.Add(lineDistances[i - 1]);

                    lineDistances[i] += _start.DistanceTo(_end);
                }

                (geometry as BufferGeometry).SetAttribute("lineDistance",
                    new BufferAttribute<float>(lineDistances.ToArray(), 1));
            }
            else
            {
                Console.WriteLine(
                    "THREE.Line.computeLineDistances(): Computation only possible with non-indexed BufferGeometry.");
            }
        }
        else if (geometry is Geometry)
        {
            var vertices = geometry.Vertices;
            var lineDistances = geometry.LineDistances;

            lineDistances.Clear();

            lineDistances.Add(0);

            for (var i = 1; i < vertices.Count; i++)
            {
                lineDistances.Add(lineDistances[i - 1]);

                lineDistances[i] += vertices[i - 1].DistanceTo(vertices[i]);
            }
        }

        return this;
    }

    public void RayCast()
    {
        throw new NotImplementedException();
    }
}