using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class MeshPhysicalMaterial : MeshStandardMaterial
{
    public float Transparency = 0.0f;

    public MeshPhysicalMaterial()
    {
        type = "MeshPhysicalMaterial";

        //this.Defines.Add("STANDARD", ""); already inserted from MeshStandardMaterial
        Defines.Add("PHYSICAL", "");

        Clearcoat = 0.0f;
        ClearcoatRoughness = 0.0f;

        Reflectivity = 0.5f;

        ClearcoatNormalScale = new Vector2(1, 1);

        ClearcoatNormalMap = null;
    }

    public MeshPhysicalMaterial(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}