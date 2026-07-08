using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeStorage : DraftUtils.SingletonDontDestroyOnLoadMonoBehaviour<RuntimeStorage>
{
    [ShowInInspector] [ReadOnly] private readonly Dictionary<string, object> _storage = new();

    public void Set<T>(string key, T value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            Debug.LogWarning("RuntimeStorage.Set called with an empty key.");
            return;
        }

        _storage[key] = value;
    }

    public bool TryGet<T>(string key, out T value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            value = default;
            return false;
        }

        if (_storage.TryGetValue(key, out var storedValue))
        {
            if (storedValue is null)
            {
                value = default;
                return default(T) is null;
            }

            if (storedValue is T typedValue)
            {
                value = typedValue;
                return true;
            }
        }

        value = default;
        return false;
    }

    public T Get<T>(string key, T defaultValue = default)
    {
        return TryGet(key, out T value) ? value : defaultValue;
    }

    public bool ContainsKey(string key)
    {
        return !string.IsNullOrWhiteSpace(key) && _storage.ContainsKey(key);
    }

    public void Remove(string key)
    {
        if (!string.IsNullOrWhiteSpace(key))
        {
            _storage.Remove(key);
        }
    }

    public void Clear()
    {
        _storage.Clear();
    }

    public int Count => _storage.Count;
}
