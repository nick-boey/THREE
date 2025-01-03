namespace THREE;

public struct KeyPressEventArgs
{
    public string Key { get; set; }

    public KeyPressEventArgs(string key)
    {
        Key = key;
    }
}