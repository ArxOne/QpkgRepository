using System.Collections.Generic;

namespace ArxOne.Qnap.Utility;

public static class DictionaryExtension
{
    public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
    {
        return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
    }

    public static string GetValueOrDefault(this IDictionary<string, string> dictionary, string key)
    {
        return dictionary.TryGetValue(key, out var value) ? value : string.Empty;
    }
}   