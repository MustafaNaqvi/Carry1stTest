using UnityEngine;

public static class JsonHelper
{
    public static T ImportJson<T>(string path)
    {
        if (string.IsNullOrEmpty(path)) return default;
        path = path.Replace(".json", "");
        var textAsset = Resources.Load<TextAsset>(path);
        return JsonUtility.FromJson<T>(textAsset.text);
    }
}