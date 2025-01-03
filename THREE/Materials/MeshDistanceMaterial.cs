using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class MeshDistanceMaterial : Material
{
    public float FarDistance = 1000;

    public float NearDistance = 1;
    public Vector3 ReferencePosition = Vector3.Zero();


    public MeshDistanceMaterial()
    {
        type = "MeshDistanceMaterial";

        Skinning = false;

        MorphTargets = false;

        DisplacementMap = null;

        DisplacementScale = 1;

        DisplacementBias = 0;

        Fog = false;
    }

    public MeshDistanceMaterial(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}