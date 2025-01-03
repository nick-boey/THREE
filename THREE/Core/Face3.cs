namespace THREE;

[Serializable]
public class Face3 : ICloneable
{
    public int a;

    public int b;

    public int c;

    public Color Color;

    public int MaterialIndex;

    public Vector3 Normal = new();

    public List<Color> VertexColors = new();

    public List<Vector3> VertexNormals = new();

    public List<Vector4> VertexTangents = new();

    public Face3(int a, int b, int c)
    {
        this.a = a;
        this.b = b;
        this.c = c;
        Normal = new Vector3(1, 1, 1);
        Color = Color.ColorName(ColorKeywords.white);
        MaterialIndex = 0;
    }

    public Face3(int a, int b, int c, Vector3 normal)
    {
        this.a = a;
        this.b = b;
        this.c = c;
        Normal = normal;
    }

    public Face3(int a, int b, int c, Vector3 normal, Color color, int materialIndex = 0)
    {
        this.a = a;
        this.b = b;
        this.c = c;
        Normal = normal;
        Color = color;
        MaterialIndex = materialIndex;
    }

    public Face3(int a, int b, int c, List<Vector3> vertexNormals, List<Color> vertexColors, int materialIndex = 0)
    {
        this.a = a;
        this.b = b;
        this.c = c;
        VertexNormals = vertexNormals;
        VertexColors = vertexColors;
        MaterialIndex = materialIndex;
    }

    protected Face3(Face3 other)
    {
        throw new NotImplementedException();
    }

    public object Clone()
    {
        return new Face3(this);
    }
}