namespace THREE;

[Serializable]
public struct RenderItem
{
    public int Id;

    public Object3D Object3D;

    public BufferGeometry Geometry;

    public Material Material;

    public GLProgram Program;

    public int GroupOrder;

    public int RenderOrder;

    public float Z;

    public DrawRange? Group;
}

[Serializable]
public class GLRenderList
{
    private GLProgram defaultProgram = new() { Id = -1 };
    public List<RenderItem> Opaque = new();

    private GLProperties properties;

    public List<RenderItem> renderItems = new();

    public int renderItemsIndex;

    public List<RenderItem> Transmissive = new();

    public List<RenderItem> Transparent = new();

    public GLRenderList(GLProperties properties)
    {
        this.properties = properties;
    }


    private RenderItem GetNextRenderItem(Object3D object3D, BufferGeometry geometry, Material material, int groupOrder,
        float z, DrawRange? group)
    {
        RenderItem renderItem;
        var materialProperties = properties.Get(material);
        var program = (GLProgram)materialProperties["program"];
        if (renderItemsIndex > renderItems.Count - 1)
        {
            renderItem = new RenderItem
            {
                Id = object3D.Id,
                Object3D = object3D,
                Geometry = geometry,
                Material = material,
                Program = program != null ? program : defaultProgram,
                GroupOrder = groupOrder,
                RenderOrder = object3D.RenderOrder,
                Z = z,
                Group = group
            };
            renderItems.Add(renderItem);
        }
        else
        {
            renderItem = renderItems[renderItemsIndex];
            renderItem.Id = object3D.Id;
            renderItem.Object3D = object3D;
            renderItem.Geometry = geometry;
            renderItem.Material = material;
            renderItem.Program = program != null ? program : defaultProgram;
            renderItem.GroupOrder = groupOrder;
            renderItem.RenderOrder = object3D.RenderOrder;
            renderItem.Z = z;
            renderItem.Group = group;
        }

        renderItemsIndex++;

        return renderItem;
    }

    public void Push(Object3D object3D, BufferGeometry geometry, Material material, int groupOrder, float z,
        DrawRange? group)
    {
        var renderItem = GetNextRenderItem(object3D, geometry, material, groupOrder, z, group);
        if (material.Transmission > 0.0f)
            Transmissive.Add(renderItem);
        else if (material.Transparent)
            Transparent.Add(renderItem);
        else
            Opaque.Add(renderItem);
    }

    public void Unshift(Object3D object3D, BufferGeometry geometry, Material material, int groupOrder, float z,
        DrawRange? group)
    {
        var renderItem = GetNextRenderItem(object3D, geometry, material, groupOrder, z, group);
        if (material.Transmission > 0.0f)
            Transmissive.Insert(0, renderItem);
        else if (material.Transparent)
            Transparent.Insert(0, renderItem);
        else
            Opaque.Insert(0, renderItem);
    }

    public void Init()
    {
        Opaque.Clear();
        Transparent.Clear();
        Transmissive.Clear();
        renderItems.Clear();
        renderItemsIndex = 0;
    }

    public void Finish()
    {
        renderItems.Clear();
        renderItemsIndex = 0;
    }

    public void Sort()
    {
        if (Opaque.Count > 0)
            Opaque.Sort(delegate(RenderItem a, RenderItem b)
            {
                if (a.GroupOrder != b.GroupOrder) return a.GroupOrder - b.GroupOrder;

                if (a.RenderOrder != b.RenderOrder) return a.RenderOrder - b.RenderOrder;

                if (a.Program != b.Program) return a.Program.Id - b.Program.Id;

                if (a.Material.Id != b.Material.Id) return a.Material.Id - b.Material.Id;

                if (a.Z != b.Z) return (int)(a.Z - b.Z);

                return a.Id - b.Id;
            });
        if (Transparent.Count > 0)
            Transparent.Sort(delegate(RenderItem a, RenderItem b)
            {
                if (a.GroupOrder != b.GroupOrder) return a.GroupOrder - b.GroupOrder;

                if (a.RenderOrder != b.RenderOrder) return a.RenderOrder - b.RenderOrder;

                if (a.Z != b.Z) return (int)(b.Z - a.Z);

                return a.Id - b.Id;
            });
    }
}