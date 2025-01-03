using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class Skeleton : Object3D
{
    public Matrix4[] BoneInverses;

    public float[] BoneMatrices;

    public Bone[] Bones;

    public DataTexture BoneTexture;

    public int BoneTextureHeight;

    public int BoneTextureSize;

    public int BoneTextureWidth;

    public int Frame = -1;

    public Matrix4 IdentityMatrix;
    public bool UseVertexTexture;

    public Skeleton(Bone[] bones, Matrix4[] boneInverses = null)
    {
        Bones = bones;
        BoneMatrices = new float[bones.Length * 16];
        if (boneInverses != null)
        {
            CalculateInverses();
        }
        else
        {
            //if (this.Bones.Length == boneInverses.Length)
            //{
            //    Array.Copy(boneInverses, 0, this.BoneInverses, 0, boneInverses.Length);
            //}
            //else
            //{
            BoneInverses = new Matrix4[Bones.Length];
            var bCount = 0;
            for (var i = 0; i < Bones.Length; i++) BoneInverses[bCount++] = new Matrix4();
            //}
        }
    }

    public Skeleton(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public void CalculateInverses()
    {
        BoneInverses = new Matrix4[Bones.Length];
        var bCount = 0;
        for (var i = 0; i < Bones.Length; i++)
        {
            var inverse = new Matrix4();
            if (Bones[i] != null) inverse.GetInverse(Bones[i].MatrixWorld);
            BoneInverses[bCount++] = inverse;
        }
    }

    public void Pose()
    {
        Bone bone;
        for (var i = 0; i < Bones.Length; i++)
        {
            bone = Bones[i];
            if (bone != null) bone.MatrixWorld.GetInverse(BoneInverses[i]);
        }

        for (var i = 0; i < Bones.Length; i++)
        {
            bone = Bones[i];
            if (bone != null)
            {
                if (bone.Parent != null && bone.Parent is Bone)
                {
                    // Unsure is Bone.Matrix is the Right variable
                    bone.Matrix.GetInverse(bone.Parent.MatrixWorld);
                    bone.Matrix.Multiply(bone.MatrixWorld);
                }
                else
                {
                    // Unsure is Bone.Matrix is the Right variable
                    bone.Matrix.Copy(bone.MatrixWorld);
                }
            }

            // Unsure is Bone.Matrix is the Right variable
            bone.Matrix.Decompose(bone.Position, bone.Quaternion, bone.Scale);
        }
    }

    public void Update()
    {
        var offsetMatrix = new Matrix4();
        var identityMatrix = new Matrix4();

        for (var i = 0; i < Bones.Length; i++)
        {
            var matrix = Bones[i] != null ? Bones[i].MatrixWorld : identityMatrix;
            offsetMatrix.MultiplyMatrices(matrix, BoneInverses[i]);
            offsetMatrix.ToArray(BoneMatrices, i * 16);
        }

        if (BoneTexture != null) BoneTexture.NeedsUpdate = true;
    }


    public Skeleton Clone()
    {
        return new Skeleton(Bones, BoneInverses);
    }

    public Bone getBoneByName(string name)
    {
        for (var i = 0; i < Bones.Length; i++)
        {
            var bone = Bones[i];
            if (bone.Name.Equals(name)) return bone;
        }

        return null;
    }

    public Skeleton ComputeBoneTexture()
    {
        // layout (1 matrix = 4 pixels)
        //      RGBA RGBA RGBA RGBA (=> column1, column2, column3, column4)
        //  with  8x8  pixel texture max   16 bones * 4 pixels =  (8 * 8)
        //       16x16 pixel texture max   64 bones * 4 pixels = (16 * 16)
        //       32x32 pixel texture max  256 bones * 4 pixels = (32 * 32)
        //       64x64 pixel texture max 1024 bones * 4 pixels = (64 * 64)

        var size = (int)Math.Sqrt(Bones.Length * 4); // 4 pixels needed for 1 matrix
        size = (int)Math.Ceiling((decimal)size / 4) * 4;
        size = Math.Max(size, 4);

        var boneMatrices = new float[size * size * 4]; // 4 floats per RGBA pixel
        Array.Copy(BoneMatrices, boneMatrices, BoneMatrices.Length); // copy current values

        //Bitmap im = new Bitmap(size, size,size,System.Drawing.Imaging.PixelFormat.Format8bppIndexed,Marshal.UnsafeAddrOfPinnedArrayElement(ToByteArray(boneMatrices),0));
        var im = boneMatrices.ToByteArray().ToSKBitMap(size, size);
        var boneTexture = new DataTexture(im, size, size, Constants.RGBAFormat, Constants.FloatType);
        boneTexture.NeedsUpdate = true;

        BoneMatrices = boneMatrices;
        BoneTexture = boneTexture;

        return this;
    }
}