using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public static class TexUtils
{
    /// <summary>
    /// convert render texture into a texture2d
    /// </summary>
    public static Texture2D ToTexture(this RenderTexture renderTexture)
    {
        var active = RenderTexture.active;
        RenderTexture.active = renderTexture;

        // Create a new Texture2D and read the RenderTexture image into it
        var tex = new Texture2D(renderTexture.width, renderTexture.height);
        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);

        RenderTexture.active = active;

        return tex;
    }

    /// <summary>
    /// converts a texture2d into a sprite
    /// </summary>
    public static Sprite ToSprite(this Texture2D texture2D)
    {
        return Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
    }

    /// <summary>
    /// saves the texture into the specified path as PNG
    /// </summary>
    public static void SaveTexture(Texture2D texture2D, string path, string name)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        //save texture to the path
        var bytes = texture2D.EncodeToPNG();
        var fileName = name + ".png";
        File.WriteAllBytes(path + "/" + fileName, bytes);
    }

    /// <summary>
    /// loads all textures from a folder
    /// </summary>
    public static IEnumerable<Texture2D> LoadAllTexturesAt(string path)
    {
        var files = Directory.GetFiles(path);
        foreach (var file in files)
        {
            if (file.EndsWith(".png"))
            {
                var bytes = File.ReadAllBytes(file);
                var texture = new Texture2D(2, 2);
                texture.LoadImage(bytes);
                yield return texture;
            }
        }
    }
}