using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class Scene : Object3D
{
    public bool AutoUpdate = true;
    public object? Background = null;
    public bool ClearBeforeRender = true;
    public Texture? Environment;
    public Fog? Fog = null;
    public bool IsScene = true;
    public Material? OverrideMaterial = null;

    public Scene()
    {
        type = "Scene";
    }

    public Scene(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public virtual void Resize(float width, float height)
    {
    }
}