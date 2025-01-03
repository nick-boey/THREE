namespace THREE;

[Serializable]
public class ImmediateRenderObject : Object3D
{
    public ImmediateRenderObject(Material material)
    {
        Material = material;
    }

    protected ImmediateRenderObject(ImmediateRenderObject other) : base(other)
    {
        Material = (Material)other.Material.Clone();
    }

    public void Render(Action renderCAllback)
    {
    }
}