using BepInEx.Logging;
using System.IO;
using UnityEngine;

//this is where the meat is

public static class ApplyCustomCap
{
    public static void CreateCustomCapFromJson(string jsonPath, ManualLogSource logger)
    {
        logger.LogInfo("Running CreateCustomCapFromJson");

        var data = PaintCapSettingsJsonParser.LoadFromJson(jsonPath);
        if (data == null)
        {
            logger.LogInfo("Failed to load cap settings from json");
            return;
        }

        GameObject capObject = new GameObject(data.Name);
        capObject.hideFlags = HideFlags.HideAndDontSave; //i dont think i actually need this but it's what the game does so we will too

        PaintCapSettings capSettings = capObject.AddComponent<PaintCapSettings>(); //isnt that crazy? u can just add components that exist in the game? holy frick. 

        capSettings.Name = data.Name;
        capSettings.Description = data.Description;

        var model = UnityAssetResolver.FindModelByName(data.CapModelName);

        if (model == null)
        {
            logger.LogError($"CapModel '{data.CapModelName}' not found!");
        }
        else
        {
            //FIX THE STUPID PIVOT
            GameObject pivotFixer = new GameObject($"{data.Name}_PivotFixer");
            pivotFixer.hideFlags = HideFlags.HideAndDontSave;
            pivotFixer.transform.localPosition = Vector3.zero;
            pivotFixer.transform.localRotation = Quaternion.identity;
            pivotFixer.transform.localScale = Vector3.one;

            GameObject container = new GameObject($"{data.Name}_ModelContainer");
            container.hideFlags = HideFlags.HideAndDontSave;
            container.transform.parent = pivotFixer.transform;
            container.transform.localPosition = Vector3.zero;
            container.transform.localRotation = Quaternion.identity;
            container.transform.localScale = Vector3.one;

            GameObject modelInstance = Object.Instantiate(model, container.transform);
            modelInstance.name = $"{data.Name}_ModelInstance";
            modelInstance.hideFlags = HideFlags.HideAndDontSave;
            modelInstance.SetActive(true);


            //fixing stupid pivot point AGAIN so we can resize it
            modelInstance.transform.localPosition = Vector3.zero;

            Vector3 rotation = data.ModelRotation != null ? new Vector3(data.ModelRotation.x, data.ModelRotation.y, data.ModelRotation.z) : Vector3.zero;
            modelInstance.transform.localRotation = Quaternion.Euler(rotation);

            Vector3 scale = data.ModelScale != null ? new Vector3(data.ModelScale.x, data.ModelScale.y, data.ModelScale.z) : Vector3.one;
            modelInstance.transform.localScale = scale;

            capSettings.CapModel = pivotFixer;

            //add our pretty custom colors here if it exists
            if (data.ColorOverride != null)
            {
                Color col = new Color(data.ColorOverride.r, data.ColorOverride.g, data.ColorOverride.b, data.ColorOverride.a);
                if (col.a > 0f)
                {
                    Renderer[] renderers = modelInstance.GetComponentsInChildren<Renderer>(true);
                    foreach (var renderer in renderers)
                    {
                        foreach (var mat in renderer.materials)
                        {
                            if (mat.HasProperty("_Color"))
                                mat.color = col;
                        }
                    }
                }
            }
        }

        //load custom noise texture from folder
        Texture2D customNoiseTex = null;
        string pluginFolder = Path.GetDirectoryName(typeof(CustomCapPlugin).Assembly.Location);
        string texturesFolder = Path.Combine(pluginFolder, "CustomTextures");

        if (!string.IsNullOrEmpty(data.NoiseTextureName))
        {
            string texturePath = Path.Combine(texturesFolder, data.NoiseTextureName + ".png");
            if (File.Exists(texturePath))
            {
                customNoiseTex = UnityAssetResolver.LoadTextureFromFile(texturePath, logger);
                if (customNoiseTex != null)
                {
                    logger.LogInfo($"Loaded custom noise texture from: {data.NoiseTextureName}.png");
                }
            }
        }

        //use custom noise texture if loaded, otherwise fallback to default
        capSettings.NoiseTexture = customNoiseTex ?? UnityAssetResolver.FindTextureByName(data.NoiseTextureName);

        //todo: add custom override for sprites. i just dont know how to make sprites so i didnt add it..
        capSettings.NoiseSprite = UnityAssetResolver.FindSpriteByName(data.NoiseSpriteName);

        //alter our capsettings class with everything from da json file
        capSettings.ForceMinMax = new Vector2(data.ForceMinMax.x, data.ForceMinMax.y);
        capSettings.FocusMinMax = new Vector2(data.FocusMinMax.x, data.FocusMinMax.y);
        capSettings.SpaterStrengthMinMax = new Vector2(data.SpaterStrengthMinMax.x, data.SpaterStrengthMinMax.y);
        capSettings.SpaterSizeMinMax = new Vector2(data.SpaterSizeMinMax.x, data.SpaterSizeMinMax.y);
        capSettings.PressureCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        capSettings.UsePressureToControlForce = data.UsePressureToControlForce;
        capSettings.PressureMutiplyer = data.PressureMutiplyer;
        capSettings.maxRandomness = data.maxRandomness;
        capSettings.BrushWidth = data.BrushWidth;
        capSettings.BrushHeight = data.BrushHeight;
        capSettings.Yuge = data.Yuge;
        capSettings.randomCutoff = data.randomCutoff;
        capSettings.bigDot = data.bigDot;
        capSettings.litDot = data.litDot;
        capSettings.maxSpatDist = data.maxSpatDist;
        capSettings.minSpatDist = data.minSpatDist;
        capSettings.maxSpatWallDist = data.maxSpatWallDist;
        capSettings.BrushScaleMinMax = new Vector2(data.BrushScaleMinMax.x, data.BrushScaleMinMax.y);
        capSettings.BrushScale = data.BrushScale;
        capSettings.MaxDistance = data.MaxDistance;
        capSettings.DynamicBrushMaterial = UnityAssetResolver.FindMaterialByName(data.DynamicBrushMaterialName);

        //we want to append our new cap to the capbrushes array
        //if already exists for some ungodly reason just replace it so u dont have to hunt down dead references

        PaintCapSettings[] oldCaps = PaintTexture.Instance.CapBrushes;
        int existingIndex = -1;

        for (int i = 0; i < oldCaps.Length; i++)
        {
            if (oldCaps[i] != null && oldCaps[i].Name == data.Name)
            {
                existingIndex = i;
                break;
            }
        }

        PaintCapSettings[] newCaps;

        if (existingIndex >= 0)
        {
            //replacing
            newCaps = new PaintCapSettings[oldCaps.Length];
            for (int i = 0; i < oldCaps.Length; i++)
                newCaps[i] = oldCaps[i];

            newCaps[existingIndex] = capSettings;

            logger.LogInfo($"Replaced existing cap '{data.Name}' at index {existingIndex}.");
        }
        else
        {
            //thank god it doesnt exist add it
            newCaps = new PaintCapSettings[oldCaps.Length + 1];
            for (int i = 0; i < oldCaps.Length; i++)
                newCaps[i] = oldCaps[i];

            newCaps[oldCaps.Length] = capSettings;

            logger.LogInfo($"Added new cap '{data.Name}'.");
        }

        PaintTexture.Instance.CapBrushes = newCaps;

        //set current active cap... we dont really need this. 
        //PaintTexture.Instance.SetCapBrush(existingIndex >= 0 ? existingIndex : oldCaps.Length);

        logger.LogInfo("Custom cap added"); //Isnt that SICK
    }
}