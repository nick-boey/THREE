namespace THREE;

[Serializable]
public class Layers
{
    public int Mask = 1 | 0;

    public void Set(int channel)
    {
        Mask = (1 << channel) | 0;
    }

    public void Enable(int channel)
    {
        Mask |= (1 << channel) | 0;
    }

    public void EnableAll()
    {
        Mask = Convert.ToInt32(0xffffffff | 0);
    }

    public void Toggle(int channel)
    {
        Mask ^= (1 << channel) | 0;
    }

    public void Disable(int channel)
    {
        Mask &= ~((1 << channel) | 0);
    }

    public void DisableAll()
    {
        Mask = 0;
    }

    public bool Test(Layers layers)
    {
        return (Mask & layers.Mask) != 0;
    }
}