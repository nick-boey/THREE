namespace THREE;

[Serializable]
public class DataTexture2DArray : Texture
{
    public byte[] Data;
    public int Depth;
    public int Height;
    public int Width;

    public DataTexture2DArray(byte[] array, int width, int height, int depth)
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