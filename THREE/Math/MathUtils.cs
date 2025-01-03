﻿namespace THREE;

[Serializable]
public static class MathUtils
{
    public const float PI2 = 2 * 3.14159265358979323846f;

    public const float HalfPI = 3.14159265358979323846f / 2.0f;

    public const float SQRT1_2 = 0.7071067811865476f;

    public const float DEG2RAD = (float)Math.PI / 180;
    public const float RAD2DEG = 180 / (float)Math.PI;
    public static readonly Random random = new();

    public static Color NextColor()
    {
        return new Color((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
    }

    public static float NextFloat(float min, float max)
    {
        var val = random.NextDouble() * (max - min) + min;
        return (float)val;
    }

    public static int NextInt()
    {
        return (int)random.NextDouble();
    }

    public static float NextFloat()
    {
        return (float)random.NextDouble();
    }

    public static float RadToDeg(double rad)
    {
        return (float)(rad * 180.0f / Math.PI);
    }

    public static float RandFloat(float low, float high)
    {
        return low + (float)random.NextDouble() * (high - low);
    }

    public static float RandFloatSpread(float range)
    {
        return range * (float)(0.5f - random.NextDouble());
    }

    public static float DegToRad(double deg)
    {
        return (float)(Math.PI * deg / 180.0f);
    }

    public static float mapLinear(double x, double a1, double a2, double b1, double b2)
    {
        return (float)(b1 + (x - a1) * (b2 - b1) / (a2 - a1));
    }

    public static float CeilPowerOfTwo(float value)
    {
        return (float)Math.Pow(2, Math.Ceiling(Math.Log(value) / Math.Log(2)));
    }

    public static float FloorPowerOfTwo(float value)
    {
        return (float)Math.Pow(2, Math.Floor(Math.Log(value) / Math.Log(2)));
    }

    public static bool IsPowerOfTwo(int value)
    {
        return (value & (value - 1)) == 0 && value != 0;
    }

    public static float Lerp(float x, float y, float t)
    {
        return (1 - t) * x + t * y;
    }

    public static float Clamp(float val, float min, float max)
    {
        //if (val.CompareTo(min) < 0) return min;
        //else if (val.CompareTo(max) > 0) return max;
        //else return val;

        return Math.Max(min, Math.Min(max, val));
    }
}