using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class Points : Object3D
{
    private Matrix4 inverseMatrix = new();
    private Vector3 position = new();
    private Ray ray = new();
    private Sphere sphere = new();

    public Points(Geometry geometry = null, Material material = null)
    {
        type = "Points";

        Geometry = geometry ?? new BufferGeometry();
        Material = material ?? new PointsMaterial { Color = new Color().SetHex(0xFFFFFF) };

        UpdateMorphTargets();
    }

    public Points(Geometry geometry = null, List<Material> materials = null)
    {
        type = "Points";

        Geometry = geometry ?? new BufferGeometry();

        if (materials == null)
        {
            Material = new PointsMaterial { Color = new Color().SetHex(0xFFFFFF) };
            Materials.Add(Material);
        }
        else
        {
            Materials = materials;
            if (Materials.Count > 0)
                Material = Materials[0];
        }
    }

    public Points(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public void UpdateMorphTargets()
    {
    }

    private void testPoint(Vector3 point, int index, float localThresholdSq, Matrix4 matrixWorld, Raycaster raycaster,
        List<Intersection> intersects, Object3D object3d)
    {
        var rayPointDistanceSq = ray.DistanceSqToPoint(point);

        if (rayPointDistanceSq < localThresholdSq)
        {
            var intersectPoint = new Vector3();
            ray.ClosestPointToPoint(point, intersectPoint);
            intersectPoint.ApplyMatrix4(matrixWorld);

            var distance = raycaster.ray.origin.DistanceTo(intersectPoint);

            if (distance < raycaster.near || distance > raycaster.far) return;

            intersects.Add(new Intersection
            {
                distance = distance,
                distanceToRay = (float)Math.Sqrt(rayPointDistanceSq),
                point = intersectPoint,
                index = index,
                face = null,
                object3D = object3d
            });
        }
    }

    public override void Raycast(Raycaster raycaster, List<Intersection> intersectionList)
    {
        var threshold = (float)raycaster.parameters.Points["threshold"];
        // Checking boundingSphere distance to ray
        if (Geometry.BoundingSphere == null) Geometry.ComputeBoundingSphere();

        sphere.Copy(Geometry.BoundingSphere);
        sphere.ApplyMatrix4(MatrixWorld);
        sphere.Radius += threshold;
        if (!raycaster.ray.IntersectsSphere(sphere)) return;

        inverseMatrix.GetInverse(MatrixWorld);
        ray.copy(raycaster.ray).ApplyMatrix4(inverseMatrix);

        var localThreshold = threshold / ((Scale.X + Scale.Y + Scale.Z) / 3);
        var localThresholdSq = localThreshold * localThreshold;
        if (Geometry is BufferGeometry)
        {
            var index = (Geometry as BufferGeometry).Index;
            var attributes = (Geometry as BufferGeometry).Attributes["position"] as BufferAttribute<float>;
            var positions = attributes.Array;

            if (index != null)
            {
                var indices = index.Array;
                for (var i = 0; i < indices.Length; i++)
                {
                    var a = indices[i];
                    position.FromArray(positions, a * 3);
                    testPoint(position, a, localThresholdSq, MatrixWorld, raycaster, intersectionList, this);
                }
            }
            else
            {
                for (var i = 0; i < positions.Length / 3; i++)
                {
                    position.FromArray(positions, i * 3);
                    testPoint(position, i, localThresholdSq, MatrixWorld, raycaster, intersectionList, this);
                }
            }
        }
        else
        {
            var vertices = Geometry.Vertices;
            for (var i = 0; i < vertices.Count; i++)
                testPoint(vertices[i], i, localThresholdSq, MatrixWorld, raycaster, intersectionList, this);
        }
    }
}