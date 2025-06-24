using System.Numerics;

public static class Easing
{
    private static Dictionary<string, Func<float, float>> _curves = new();

    public static Vector2 Interpolate(Vector2 from, Vector2 to, float t, string curveName)
    {
        if (!_curves.TryGetValue(curveName, out var f)) f = t => t; // fallback: linear
        return Vector2.Lerp(from, to, f(t));
    }
}
