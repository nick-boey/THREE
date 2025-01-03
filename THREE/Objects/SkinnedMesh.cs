using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class SkinnedMesh : Mesh
{
    public Matrix4 BindMatrix;

    public Matrix4 BindMatrixInverse;
    public string BindMode;

    public Skeleton Skeleton;

    public SkinnedMesh(Geometry geometry, List<Material> material, bool? useVertexTexture = null) : base(geometry,
        material)
    {
        type = "SkinnedMesh";

        BindMode = "attached";

        BindMatrix = Matrix4.Identity();

        BindMatrixInverse = Matrix4.Identity();
    }

    public SkinnedMesh(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public void Bind(Skeleton skeleton, Matrix4 bindMatrix)
    {
        Skeleton = skeleton;

        if (bindMatrix == null)
        {
            UpdateMatrixWorld(true);

            Skeleton.CalculateInverses();

            bindMatrix = MatrixWorld;
        }

        BindMatrix.Copy(bindMatrix);
        BindMatrixInverse.GetInverse(bindMatrix);
    }

    public void Pose()
    {
        Skeleton.Pose();
    }

    public void NormalizeSkinWeights()
    {
        var vector = new Vector4();
        var skinWeight = (Geometry as BufferGeometry).Attributes["skinWeight"] as BufferAttribute<float>;

        for (var i = 0; i < skinWeight.count; i++)
        {
            vector.X = skinWeight.GetX(i);
            vector.Y = skinWeight.GetY(i);
            vector.Z = skinWeight.GetZ(i);
            vector.W = skinWeight.GetW(i);

            var scale = 1f / vector.ManhattanLength();

            if (scale != float.PositiveInfinity)
                vector.MultiplyScalar(scale);
            else
                vector.Set(1, 0, 0, 0); // do something reasonable

            skinWeight.SetXYZW(i, vector.X, vector.Y, vector.Z, vector.W);
        }
    }

    public override void UpdateMatrixWorld(bool force = false)
    {
        base.UpdateMatrixWorld(force);
        if (BindMode.Equals("attached"))
            BindMatrixInverse.GetInverse(MatrixWorld);
        else if (BindMode.Equals("detached")) BindMatrixInverse.GetInverse(BindMatrix);
        // Unrecognized BindMode
    }

    public Vector4 BoneTransform(int index, Vector4 target)
    {
        var basePosition = new Vector3();
        var skinIndex = new Vector4();
        var skinWeight = new Vector4();
        var vector = new Vector4();
        var matrix = new Matrix4();

        skinIndex.FromBufferAttribute((Geometry as BufferGeometry).Attributes["skinIndex"] as BufferAttribute<float>,
            index);
        skinWeight.FromBufferAttribute((Geometry as BufferGeometry).Attributes["skinWeight"] as BufferAttribute<float>,
            index);
        basePosition
            .FromBufferAttribute((Geometry as BufferGeometry).Attributes["position"] as BufferAttribute<float>, index)
            .ApplyMatrix4(BindMatrix);


        var basePosition1 = new Vector4();
        for (var i = 0; i < 4; i++)
        {
            var weight = skinWeight.GetComponent(i);
            if (weight != 0)
            {
                var boneIndex = (int)skinIndex.GetComponent(i);
                matrix.MultiplyMatrices(Skeleton.Bones[boneIndex].MatrixWorld, Skeleton.BoneInverses[boneIndex]);
                vector.Set(basePosition.X, basePosition.Y, basePosition.Z, 1);
                target.AddScaledVector(vector.ApplyMatrix4(matrix), weight);
            }
        }

        return target.ApplyMatrix4(BindMatrixInverse);
    }
}