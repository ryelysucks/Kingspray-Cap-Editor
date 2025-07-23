using BepInEx.Logging;
using System.IO;
using System.Linq;
using UnityEngine;

//helper class for getting kingspray/custom assets during runtime

public static class UnityAssetResolver
{
    public static Material FindMaterialByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        Material mat = Resources.Load<Material>(name);
        if (mat != null) return mat;
        return Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(m => m.name == name);
    }

    public static Texture2D FindTextureByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        Texture2D tex = Resources.Load<Texture2D>(name);
        if (tex != null) return tex;
        return Resources.FindObjectsOfTypeAll<Texture2D>().FirstOrDefault(t => t.name == name);
    }

    public static Sprite FindSpriteByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        Sprite sprite = Resources.Load<Sprite>(name);
        if (sprite != null) return sprite;
        return Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(s => s.name == name);
    }

    public static GameObject FindModelByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        return Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(go => go.name == name);
    }

    //i really hate that u have to pass around the bepinex logger
    public static Texture2D LoadTextureFromFile(string filePath, ManualLogSource logger)
    {
        if (!File.Exists(filePath))
            return null;

        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D tex = new Texture2D(2, 2);
        if (!tex.LoadImage(fileData))
        {
            logger.LogWarning($"Failed to load image: {filePath}");
            return null;
        }
        return tex;
    }
}
