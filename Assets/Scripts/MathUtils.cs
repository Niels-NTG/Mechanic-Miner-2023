using UnityEngine;

public static class MathUtils
{
    public static int HashVector2Int(Vector2Int vector2Int)
    {
        // The built-in hashcode function for Vector2Int is broken since it has lots of hash collisions.
        // Source: https://stackoverflow.com/a/21549302
        return (vector2Int.x << 16) | (vector2Int.y & 0xFFFF);
    }

    private static int Cantor(int a, int b)
    {
        // Source: https://stackoverflow.com/a/73089718
        return (a + b + 1) * (a + b) / 2 + b;
    }

    public static int Cantor(int a, int b, int c)
    {
        return Cantor(a, Cantor(b, c));
    }

    public static int Cantor(int a, int b, int c, int d)
    {
        return Cantor(Cantor(a, b), Cantor(c, d));
    }
}
