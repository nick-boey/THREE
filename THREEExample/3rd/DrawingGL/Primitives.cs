//MIT, 2017, Zou Wei(github/zwcloud), WinterDev

namespace DrawingGL;

internal readonly struct Size
{
    private readonly float _width;
    private readonly float _height;

    public Size(float width, float height)
    {
        _width = width;
        _height = height;
    }
}

public struct Vector2
{
    public Vector2(float x, float y)
    {
        X = x;
        Y = y;
    }

    public float X { get; set; }


    public float Y { get; set; }

    public static Vector2 operator -(Vector2 v0, Vector2 v1)
    {
        return new Vector2(v0.X - v1.X, v0.Y - v1.Y);
    }
}