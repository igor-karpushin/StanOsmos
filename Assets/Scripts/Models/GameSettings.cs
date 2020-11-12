using System;
using System.IO;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct UserSettings
{
    public Color Color;
    [Range(0.05f, 0.1f)] public float Capacity;
}

[Serializable]
public struct EnemySettings
{
    public int Count;
    public Gradient Gradient;
    public float2 Capacity;
    public float2 Velocity;

    public Color Lerp(float delta)
    {
        return Gradient.Evaluate(delta);
        //return Color.Lerp(Color1, Color2, delta);
    }
}

[Serializable]
public struct GameSettings
{
    const string FileName = "Settings.json";
    public UserSettings User;
    public EnemySettings Enemy;
    public float ForceCapacity;

    public void LoadFromFile()
    {
        if (File.Exists(FileName))
        {
            this = JsonUtility.FromJson<GameSettings>(File.ReadAllText(FileName));
        }
    }

    public void SaveToFile()
    {
        File.WriteAllText(FileName, JsonUtility.ToJson(this));
    }
}
