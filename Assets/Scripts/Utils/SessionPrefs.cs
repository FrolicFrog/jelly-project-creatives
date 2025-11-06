using System.Collections.Generic;

public static class SessionPrefs
{
    private static Dictionary<string, object> _store = new();

    public static void Set<T>(string key, T value)
    {
        _store[key] = value;
    }

    public static T Get<T>(string key, T defaultValue = default)
    {
        if (_store.TryGetValue(key, out var value) && value is T t)
        {
            return t;
        }
        return defaultValue;
    }

    public static bool HasKey(string key)
    {
        return _store.ContainsKey(key);
    }

    public static void DeleteKey(string key)
    {
        _store.Remove(key);
    }

    public static void Clear()
    {
        _store.Clear();
    }
}
