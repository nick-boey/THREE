using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class InstancedMesh : Mesh
{
    private List<Intersection> _instanceIntersects = new();
    private Matrix4 _instanceLocalMatrix = new();
    private Matrix4 _instanceWorldMatrix = new();
    private Mesh _mesh = new();

    public BufferAttribute<float> InstanceColor;

    public int InstanceCount; // count in three.js

    public BufferAttribute<float> InstanceMatrix;

    public InstancedMesh()
    {
    }

    public InstancedMesh(Geometry geometry, Material material, int count) : base(geometry, material)
    {
        type = "InstancedMesh";
        InstanceMatrix = new BufferAttribute<float>(new float[count * 16], 16);
        InstanceColor = null;
        InstanceCount = count;
    }

    public InstancedMesh(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public override object Clone()
    {
        return this.DeepCopy();
    }

    public InstancedMesh Copy(InstancedMesh mesh)
    {
        return mesh.DeepCopy();
    }

    public Color GetColorAt(int index, Color color)
    {
        return color.FromArray(InstanceColor.Array, index * 3);
    }

    public Matrix4 GetMatrixAt(int index, Matrix4 matrix)
    {
        return matrix.FromArray(InstanceMatrix.Array, index * 16);
    }

    public void SetColorAt(int index, Color color)
    {
        if (InstanceColor == null) InstanceColor = new InstancedBufferAttribute<float>(new float[InstanceCount * 3], 3);
        color.ToArray(InstanceColor.Array, index * 3);
    }

    public void SetMatrixAt(int index, Matrix4 matrix)
    {
        matrix.ToArray(InstanceMatrix.Array, index * 16);
    }

    public override void Raycast(Raycaster raycaster, List<Intersection> intersects)
    {
        var matrixWorld = MatrixWorld;
        var raycastTimes = InstanceCount;

        _mesh.Geometry = Geometry;
        _mesh.Material = Material;

        if (_mesh.Material == null) return;

        for (var instanceId = 0; instanceId < raycastTimes; instanceId++)
        {
            // calculate the world matrix for each instance

            GetMatrixAt(instanceId, _instanceLocalMatrix);

            _instanceWorldMatrix.MultiplyMatrices(matrixWorld, _instanceLocalMatrix);

            // the mesh represents this single instance

            _mesh.MatrixWorld = _instanceWorldMatrix;

            _mesh.Raycast(raycaster, _instanceIntersects);

            // process the result of raycast

            for (int i = 0, l = _instanceIntersects.Count; i < l; i++)
            {
                var intersect = _instanceIntersects[i];
                intersect.instanceId = instanceId;
                intersect.object3D = this;
                intersects.Add(intersect);
            }

            _instanceIntersects.Clear();
        }
    }
}