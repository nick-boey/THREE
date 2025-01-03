using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class MeshFaceMaterial : Material
{
    public List<Material> Materials;

    public MeshFaceMaterial()
    {
        type = "MeshFaceMaterial";
    }

    public MeshFaceMaterial(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}