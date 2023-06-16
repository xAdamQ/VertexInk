using UnityEngine;

public static class Extensions
{
    /// <summary>
    /// converts a color into v3, neglecting the alpha
    /// </summary>
    public static Vector3 ToVector3(this Color c) => new(c.r, c.g, c.b);
}