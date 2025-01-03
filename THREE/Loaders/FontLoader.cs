namespace THREE;

[Serializable]
public class FontLoader
{
    public static Font Load(string jsonFile)
    {
        return new Font(jsonFile);
    }
}