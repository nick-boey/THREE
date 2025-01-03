using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class Sprite : Object3D
{
    private Vector2 _alignedPosition = new();
    private Geometry _geometry;

    private Vector3 _intersectPoint = new();
    private Vector3 _mvPosition = new();
    private Vector2 _rotatedPosition = new();

    private Vector2 _uvA = new();
    private Vector2 _uvB = new();
    private Vector2 _uvC = new();

    private Vector3 _vA = new();
    private Vector3 _vB = new();
    private Vector3 _vC = new();
    private Matrix4 _viewWorldMatrix = new();
    private Vector3 _worldScale = new();

    public Vector2 Center;

    public Sprite(Material material)
    {
        type = "Sprite";

        if (_geometry == null)
        {
            _geometry = new BufferGeometry();

            var float32Array = new[]
            {
                -0.5f, -0.5f, 0, 0, 0,
                0.5f, -0.5f, 0, 1, 0,
                0.5f, 0.5f, 0, 1, 1,
                -0.5f, 0.5f, 0, 0, 1
            };

            var interleavedBuffer = new InterleavedBuffer<float>(float32Array, 5);

            (_geometry as BufferGeometry).SetIndex(new List<int> { 0, 1, 2, 0, 2, 3 });
            (_geometry as BufferGeometry).SetAttribute("position",
                new InterleavedBufferAttribute<float>(interleavedBuffer, 3, 0));
            (_geometry as BufferGeometry).SetAttribute("uv",
                new InterleavedBufferAttribute<float>(interleavedBuffer, 2, 3));
        }

        Geometry = _geometry;
        Material = material != null ? material : new SpriteMaterial();

        Center = new Vector2(0.5f, 0.5f);
    }

    public Sprite(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    protected Sprite(Sprite source)
    {
        if (source.Center != null) Center.Copy(source.Center);
    }

    public override void Raycast(Raycaster raycaster, List<Intersection> intersectionList)
    {
        var intersectPoint = new Vector3();
        var worldScale = new Vector3();
        var mvPosition = new Vector3();

        var alignedPosition = new Vector2();
        var rotatedPosition = new Vector2();
        var viewWorldMatrix = new Matrix4();

        var vA = new Vector3();
        var vB = new Vector3();
        var vC = new Vector3();

        var uvA = new Vector2();
        var uvB = new Vector2();
        var uvC = new Vector2();

        worldScale.SetFromMatrixScale(MatrixWorld);

        viewWorldMatrix.Copy(raycaster.camera.MatrixWorld);
        ModelViewMatrix.MultiplyMatrices(raycaster.camera.MatrixWorldInverse, MatrixWorld);

        mvPosition.SetFromMatrixPosition(ModelViewMatrix);

        if (raycaster.camera is PerspectiveCamera && !Materials[0].SizeAttenuation)
            worldScale.MultiplyScalar(-mvPosition.Z);

        var rotation = Material.Rotation;
        float sin = 0, cos = 0;
        if (rotation != 0)
        {
            cos = (float)Math.Cos(rotation);
            sin = (float)Math.Sin(rotation);
        }

        var scale = worldScale.ToVector2();
        TransformVertex(vA.Set(-0.5f, -0.5f, 0), mvPosition, Center, scale, sin, cos,
            alignedPosition, rotatedPosition, viewWorldMatrix);
        TransformVertex(vB.Set(0.5f, -0.5f, 0), mvPosition, Center, scale, sin, cos,
            alignedPosition, rotatedPosition, viewWorldMatrix);
        TransformVertex(vC.Set(0.5f, 0.5f, 0), mvPosition, Center, scale, sin, cos,
            alignedPosition, rotatedPosition, viewWorldMatrix);

        uvA.Set(0, 0);
        uvB.Set(1, 0);
        uvC.Set(1, 1);

        // check first triangle
        var intersect = raycaster.ray.IntersectTriangle(vA, vB, vC, false, intersectPoint);

        if (intersect == null)
        {
            // check second triangle
            TransformVertex(vB.Set(-0.5f, 0.5f, 0), mvPosition, Center, scale, sin, cos,
                alignedPosition, rotatedPosition, viewWorldMatrix);
            uvB.Set(0, 1);

            intersect = raycaster.ray.IntersectTriangle(vA, vC, vB, false, intersectPoint);
            if (intersect == null) return;
        }

        var distance = raycaster.ray.origin.DistanceTo(intersectPoint);

        if (distance < raycaster.near || distance > raycaster.far) return;

        var item = new Intersection();
        item.distance = distance;
        item.point = intersectPoint.Clone();
        item.uv = Triangle.GetUV(intersectPoint, vA, vB, vC, uvA, uvB, uvC, new Vector2());
        item.face = null;
        item.object3D = this;
        intersectionList.Add(item);
    }

    private void TransformVertex(Vector3 vertexPosition, Vector3 mvPosition, Vector2 center, Vector2 scale, float sin,
        float cos, Vector2 alignedPosition, Vector2 rotatedPosition, Matrix4 viewWorldMatrix)
    {
        // compute position in camera space
        var vPos = vertexPosition.ToVector2();
        alignedPosition.SubVectors(vPos, center).AddScalar(0.5f).Multiply(scale);

        // to check if rotation is not zero
        if (sin != 0)
        {
            rotatedPosition.X = cos * alignedPosition.X - sin * alignedPosition.Y;
            rotatedPosition.Y = sin * alignedPosition.X + cos * alignedPosition.Y;
        }
        else
        {
            rotatedPosition.Copy(alignedPosition);
        }


        vertexPosition.Copy(mvPosition);
        vertexPosition.X += rotatedPosition.X;
        vertexPosition.Y += rotatedPosition.Y;

        // transform to world space
        vertexPosition.ApplyMatrix4(viewWorldMatrix);
    }

    public new Sprite Clone()
    {
        return new Sprite(this);
    }
}