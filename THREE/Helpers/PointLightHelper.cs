using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class PointLightHelper : Mesh
{
    public Color? Color;
    public Light Light;

    public PointLightHelper(Light light, float? sphereSize = null, Color? color = null)
    {
        Light = light;
        Light.UpdateMatrixWorld();


        Color = color;


        var geometry = new SphereBufferGeometry(sphereSize != null ? sphereSize.Value : 1, 4, 2);
        var material = new MeshBasicMaterial { Wireframe = true, Fog = false };


        InitGeometry(geometry, material);


        Matrix = Light.MatrixWorld;
        MatrixAutoUpdate = false;

        Update();
    }

    public PointLightHelper(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public void Update()
    {
        if (Color != null)
            Material.Color = Color;
        else
            Material.Color = Light.Color;
    }
}