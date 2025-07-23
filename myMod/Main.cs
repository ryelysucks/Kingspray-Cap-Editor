using BepInEx;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

[BepInPlugin("com.skante.capmod", "da cap mod", "1.0.0")]
public class CustomCapPlugin : BaseUnityPlugin
{
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    //should probably check we're not loading into a scene without paint... oh well!
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Logger.LogInfo($"Scene loaded: {scene.name}");
        LoadAllJsonCaps();
    }

    private void LoadAllJsonCaps()
    {
        string pluginFolder = Path.GetDirectoryName(typeof(CustomCapPlugin).Assembly.Location);
        string customCapsFolder = Path.Combine(pluginFolder, "CustomCaps");
        string[] jsonFiles = Directory.GetFiles(customCapsFolder, "*.json", SearchOption.TopDirectoryOnly);

        Logger.LogInfo($"Found {jsonFiles.Length} JSON files");

        if (jsonFiles.Length == 0)
        {
            Logger.LogWarning("No cap .json files found in CustomCaps folder.");
            return;
        }

        foreach (string jsonFile in jsonFiles)
        {
            Logger.LogInfo($"Loading cap config: {Path.GetFileName(jsonFile)}");
            try
            {
                ApplyCustomCap.CreateCustomCapFromJson(jsonFile, Logger);
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Exception loading {Path.GetFileName(jsonFile)}: {ex}");
            }
        }
    }
}
