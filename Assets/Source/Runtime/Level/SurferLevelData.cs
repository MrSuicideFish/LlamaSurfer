using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class SurferLevelData
{
    private const string LevelDataAssetName = "LevelData.json";

    public Dictionary<Vector3, LevelObjectInfo> worldObjects;
    
    private static string GetLevelDataPath()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.IsValid() && !string.IsNullOrEmpty(currentScene.path))
        {
            string path = currentScene.path.Substring(0, currentScene.path.LastIndexOf('/'));
            string sceneName = Path.GetFileNameWithoutExtension(currentScene.path);
            path = $"{path}/{sceneName}/{LevelDataAssetName}";

            return path;
        }

        return string.Empty;
    }

    public SurferLevelData()
    {
        worldObjects = new Dictionary<Vector3, LevelObjectInfo>();
    }
    
    public static SurferLevelData Load()
    {
        string path = Path.GetFullPath(GetLevelDataPath());
        if (!File.Exists(path))
        {
            Save(new SurferLevelData());
        }

        string json = File.ReadAllText(path);
        SurferLevelData data = JsonConvert.DeserializeObject<SurferLevelData>(json);
        return data;
    }

    public static void Save(SurferLevelData data)
    {
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(data, Formatting.Indented);
        string path = GetLevelDataPath();
        File.WriteAllText(path, json);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}