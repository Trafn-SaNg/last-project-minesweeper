using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Minesweeper.Core
{
    [Serializable]
    public class BestEntry
    {
        public string key;
        public int bestSeconds;
    }

    [Serializable]
    public class BestData
    {
        public List<BestEntry> entries = new List<BestEntry>();
    }

    public static class HighScoreStore
    {
        private static string FilePath => Path.Combine(Application.persistentDataPath, "minesweeper_best.json");

        public static int GetBest(string key)
        {
            var data = Load();
            foreach (var e in data.entries)
                if (e.key == key) return e.bestSeconds;
            return -1;
        }

        public static bool TrySetBest(string key, int seconds)
        {
            var data = Load();
            for (int i = 0; i < data.entries.Count; i++)
            {
                if (data.entries[i].key == key)
                {
                    if (seconds < data.entries[i].bestSeconds)
                    {
                        data.entries[i].bestSeconds = seconds;
                        Save(data);
                        return true;
                    }
                    return false;
                }
            }

            data.entries.Add(new BestEntry { key = key, bestSeconds = seconds });
            Save(data);
            return true;
        }

        private static BestData Load()
        {
            try
            {
                if (!File.Exists(FilePath)) return new BestData();
                var json = File.ReadAllText(FilePath);
                return JsonUtility.FromJson<BestData>(json) ?? new BestData();
            }
            catch
            {
                return new BestData();
            }
        }

        private static void Save(BestData data)
        {
            try
            {
                var json = JsonUtility.ToJson(data, true);
                File.WriteAllText(FilePath, json);
            }
            catch { /* ignore */ }
        }
    }
}
