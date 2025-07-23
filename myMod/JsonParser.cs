using System.IO;
using SimpleJSON;
using UnityEngine;

//i dont know why but i was getting errors trying to use jsonutility so simplejson it is
//literally just reading from a json file n turning it into usable data.. should probably sanity check for bad inputs/nodes

//for the love of god never write comments in the json files. I learned the hard way.

public static class PaintCapSettingsJsonParser
{
    [System.Serializable]
    public class Vector2Data
    {
        public float x;
        public float y;
    }

    [System.Serializable]
    public class Vector3Data
    {
        public float x;
        public float y;
        public float z;
    }

    [System.Serializable]
    public class ColorData
    {
        public float r;
        public float g;
        public float b;
        public float a;
    }

    [System.Serializable]
    public class PaintCapSettingsData
    {
        public string Name;
        public string Description;
        public string CapModelName;
        public Vector2Data ForceMinMax;
        public Vector2Data FocusMinMax;
        public Vector2Data SpaterStrengthMinMax;
        public Vector2Data SpaterSizeMinMax;
        public bool UsePressureToControlForce;
        public float PressureMutiplyer;
        public float maxRandomness;
        public float BrushWidth;
        public float BrushHeight;
        public float Yuge;
        public float randomCutoff;
        public float bigDot;
        public float litDot;
        public float maxSpatDist;
        public float minSpatDist;
        public float maxSpatWallDist;
        public Vector2Data BrushScaleMinMax;
        public float BrushScale;
        public float MaxDistance;
        public string DynamicBrushMaterialName;
        public string NoiseTextureName;
        public string NoiseSpriteName;

        public Vector3Data ModelRotation;   // optional
        public Vector3Data ModelScale;      // optional
        public ColorData ColorOverride;     // optional
    }

    public static PaintCapSettingsData LoadFromJson(string jsonPath)
    {
        if (!File.Exists(jsonPath))
        {
            Debug.LogError($"JSON file not found at {jsonPath}");
            return null;
        }

        string json = File.ReadAllText(jsonPath);
        var node = JSON.Parse(json);
        if (node == null)
        {
            Debug.LogError("Failed to parse JSON.");
            return null;
        }

        Vector3Data ParseVector3(string key)
        {
            if (node[key] == null || node[key].Count < 3)
                return null;
            return new Vector3Data
            {
                x = node[key][0].AsFloat,
                y = node[key][1].AsFloat,
                z = node[key][2].AsFloat
            };
        }

        ColorData ParseColor(string key)
        {
            if (node[key] == null || node[key].Count < 4)
                return null;
            return new ColorData
            {
                r = node[key][0].AsFloat,
                g = node[key][1].AsFloat,
                b = node[key][2].AsFloat,
                a = node[key][3].AsFloat
            };
        }

        return new PaintCapSettingsData
        {
            Name = node["Name"],
            Description = node["Description"],
            CapModelName = node["CapModelName"],
            ForceMinMax = new Vector2Data { x = node["ForceMinMax"][0].AsFloat, y = node["ForceMinMax"][1].AsFloat },
            FocusMinMax = new Vector2Data { x = node["FocusMinMax"][0].AsFloat, y = node["FocusMinMax"][1].AsFloat },
            SpaterStrengthMinMax = new Vector2Data { x = node["SpaterStrengthMinMax"][0].AsFloat, y = node["SpaterStrengthMinMax"][1].AsFloat },
            SpaterSizeMinMax = new Vector2Data { x = node["SpaterSizeMinMax"][0].AsFloat, y = node["SpaterSizeMinMax"][1].AsFloat },
            UsePressureToControlForce = node["UsePressureToControlForce"].AsBool,
            PressureMutiplyer = node["PressureMutiplyer"].AsFloat,
            maxRandomness = node["maxRandomness"].AsFloat,
            BrushWidth = node["BrushWidth"].AsFloat,
            BrushHeight = node["BrushHeight"].AsFloat,
            Yuge = node["Yuge"].AsFloat,
            randomCutoff = node["randomCutoff"].AsFloat,
            bigDot = node["bigDot"].AsFloat,
            litDot = node["litDot"].AsFloat,
            maxSpatDist = node["maxSpatDist"].AsFloat,
            minSpatDist = node["minSpatDist"].AsFloat,
            maxSpatWallDist = node["maxSpatWallDist"].AsFloat,
            BrushScaleMinMax = new Vector2Data { x = node["BrushScaleMinMax"][0].AsFloat, y = node["BrushScaleMinMax"][1].AsFloat },
            BrushScale = node["BrushScale"].AsFloat,
            MaxDistance = node["MaxDistance"].AsFloat,
            DynamicBrushMaterialName = node["DynamicBrushMaterialName"],
            NoiseTextureName = node["NoiseTextureName"],
            NoiseSpriteName = node["NoiseSpriteName"],
            ModelRotation = ParseVector3("ModelRotation"),
            ModelScale = ParseVector3("ModelScale"),
            ColorOverride = ParseColor("ColorOverride"),
        };
    }
}
