using UnityEngine;

public static class HighScoreStore
{
    public static int GetBest(string key)
    {
        return PlayerPrefs.GetInt("BEST_" + key, -1);
    }

    public static bool TrySetBest(string key, int seconds)
    {
        int cur = GetBest(key);
        if (cur < 0 || seconds < cur)
        {
            PlayerPrefs.SetInt("BEST_" + key, seconds);
            PlayerPrefs.Save();
            return true;
        }
        return false;
    }
}