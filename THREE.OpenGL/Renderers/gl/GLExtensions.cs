using OpenTK.Graphics.ES30;

namespace THREE;

[Serializable]
public class GLExtensions
{
    public Dictionary<string, int> Extensions = new();
    public List<string> ExtensionsName = new();

    public GLExtensions()
    {
        ExtensionsName = new List<string>(GL.GetString(StringName.Extensions).Split(' '));
    }

    public int Get(string name)
    {
        var index = -1;

        int value;

        if (Extensions.TryGetValue(name, out value)) return value;

        index = ExtensionsName.IndexOf(name);
        if (index >= 0) Extensions.Add(name, index);
        return index;
    }
}