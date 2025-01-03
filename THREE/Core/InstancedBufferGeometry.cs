namespace THREE;

[Serializable]
public struct InstancedGroups
{
    public int Start;

    public int Count;

    public int Instances;
}

[Serializable]
public class InstancedBufferGeometry : BufferGeometry
{
    public new List<InstancedGroups> Groups = new();

    public int InstanceCount = int.MaxValue;

    public int? MaxInstanceCount;

    public InstancedBufferGeometry()
    {
    }

    protected InstancedBufferGeometry(InstancedBufferGeometry source)
    {
        Copy(source);
    }

    public new InstancedBufferGeometry Clone()
    {
        return new InstancedBufferGeometry(this);
    }

    public InstancedBufferGeometry Copy(InstancedBufferGeometry source)
    {
        Groups = new List<InstancedGroups>(source.Groups);
        MaxInstanceCount = source.MaxInstanceCount;
        InstanceCount = source.InstanceCount;

        return this;
    }

    public override void AddGroup(int start, int count, int instances)
    {
        Groups.Add(new InstancedGroups { Start = start, Count = count, Instances = instances });
    }
}