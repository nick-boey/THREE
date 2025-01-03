namespace THREE;

[Serializable]
public class DataTexture3D : Texture
{
    public byte[] Data;
    public int Depth;
    public int Height;
    public int Width;

    public DataTexture3D(byte[] array, int width, int height, int depth)
    {
        Data = array;
        Width = width;
        Height = height;

        WrapR = Constants.ClampToEdgeWrapping;

        GenerateMipmaps = false;

        flipY = false;

        NeedsUpdate = true;
    }
}