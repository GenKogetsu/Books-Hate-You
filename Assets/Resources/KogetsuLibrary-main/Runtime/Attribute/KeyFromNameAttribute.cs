using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Kogetsu.Library.Attribute
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class KeyFromNameAttribute : System.Attribute
    {

    }

    public static class KeyFromNameSync
    {
        private const BindingFlags Flags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private const string EmptyKeyPrefix = "<empty>";

        private static readonly Dictionary<(object, string), Dictionary<string, UnityEngine.Object>> Snapshots = new();

        public static void Sync(object target)
        {
            if (target == null) return;

            FieldInfo[] fields = target.GetType().GetFields(Flags);

            foreach (FieldInfo field in fields)
            {
                if (field.GetCustomAttribute<KeyFromNameAttribute>() == null) continue;

                if (!IsStringKeyedUnityObjectDictionary(field.FieldType)) continue;

                if (field.GetValue(target) is not IDictionary dictionary) continue;

                SyncDictionary(dictionary, (target, field.Name));
            }
        }

        private static bool IsStringKeyedUnityObjectDictionary(Type type)
        {
            if (!type.IsGenericType) return false;
            if (type.GetGenericTypeDefinition() != typeof(Dictionary<,>)) return false;

            Type[] args = type.GetGenericArguments();
            return args[0] == typeof(string) && typeof(UnityEngine.Object).IsAssignableFrom(args[1]);
        }

        private static void SyncDictionary(IDictionary dictionary, (object, string) snapshotId)
        {
            if (dictionary.Count == 0)
            {
                Snapshots.Remove(snapshotId);
                return;
            }

            Snapshots.TryGetValue(snapshotId, out Dictionary<string, UnityEngine.Object> previous);

            List<KeyValuePair<string, UnityEngine.Object>> entries = new(dictionary.Count);

            foreach (DictionaryEntry entry in dictionary)
            {
                entries.Add(new KeyValuePair<string, UnityEngine.Object>(
                    (string)entry.Key,
                    entry.Value as UnityEngine.Object));
            }

            List<string> newKeys = BuildKeys(entries, previous);

            SortedDictionary<string, UnityEngine.Object> sorted = new(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < entries.Count; i++)
            {
                sorted[newKeys[i]] = entries[i].Value;
            }

            if (!IsSameAsCurrent(dictionary, sorted))
            {
                dictionary.Clear();

                foreach (KeyValuePair<string, UnityEngine.Object> pair in sorted)
                {
                    dictionary[pair.Key] = pair.Value;
                }
            }

            Snapshots[snapshotId] = new Dictionary<string, UnityEngine.Object>(sorted);
        }

        private static List<string> BuildKeys(
            List<KeyValuePair<string, UnityEngine.Object>> entries,
            Dictionary<string, UnityEngine.Object> previous)
        {
            List<string> keys = new(entries.Count);
            HashSet<string> used = new(StringComparer.OrdinalIgnoreCase);
            int emptyIndex = 0;

            foreach (KeyValuePair<string, UnityEngine.Object> entry in entries)
            {
                UnityEngine.Object value = entry.Value;

                if (!value)
                {
                    keys.Add(null);
                    continue;
                }

                bool valueChanged = previous == null
                                    || !previous.TryGetValue(entry.Key, out UnityEngine.Object old)
                                    || old != value;

                string baseKey = value.name;
                string key = baseKey;

                if (!valueChanged && used.Contains(baseKey))
                {
                    key = MakeUnique(baseKey, used);
                }
                else if (used.Contains(baseKey))
                {
                    key = MakeUnique(baseKey, used);
                }

                keys.Add(key);
                used.Add(key);
            }

            for (int i = 0; i < keys.Count; i++)
            {
                if (keys[i] != null) continue;

                string key;
                do
                {
                    key = $"{EmptyKeyPrefix}{emptyIndex++}";
                }
                while (used.Contains(key));

                keys[i] = key;
                used.Add(key);
            }

            return keys;
        }

        private static string MakeUnique(string baseKey, HashSet<string> used)
        {
            int suffix = 2;
            string key;

            do
            {
                key = $"{baseKey}#{suffix++}";
            }
            while (used.Contains(key));

            return key;
        }

        private static bool IsSameAsCurrent(
            IDictionary dictionary,
            SortedDictionary<string, UnityEngine.Object> sorted)
        {
            if (dictionary.Count != sorted.Count) return false;

            IDictionaryEnumerator current = dictionary.GetEnumerator();

            foreach (KeyValuePair<string, UnityEngine.Object> pair in sorted)
            {
                if (!current.MoveNext()) return false;
                if ((string)current.Key != pair.Key) return false;
                if (current.Value as UnityEngine.Object != pair.Value) return false;
            }

            return true;
        }
    }
}