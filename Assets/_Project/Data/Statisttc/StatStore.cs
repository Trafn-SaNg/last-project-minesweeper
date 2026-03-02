using System;
using System.Collections.Generic;
using UnityEngine;

public static class StatsStore
{
    // Keys
    const string K_TotalGames = "ST_TOTAL_GAMES";
    const string K_Wins = "ST_WINS";
    const string K_Losses = "ST_LOSSES";
    const string K_TotalPlaySeconds = "ST_TOTAL_PLAY_SECONDS";
    const string K_ChallengeWins = "ST_CH_WINS";
    const string K_ClassicWins = "ST_CL_WINS";

    // Optional history (last N records) stored as JSON in PlayerPrefs
    const string K_HistoryJson = "ST_HISTORY_JSON";
    const int HistoryMax = 10;

    [Serializable]
    public class HistoryItem
    {
        public string mode;      // "Classic" / "Challenge"
        public string detail;    // "Easy" / "Medium" / "Lv3"...
        public int seconds;      // duration
        public bool win;
        public string at;        // ISO time
    }

    [Serializable]
    class HistoryList
    {
        public List<HistoryItem> items = new List<HistoryItem>();
    }

    public static void RecordGame(GameMode mode, string detail, int seconds, bool win)
    {
        // Totals
        PlayerPrefs.SetInt(K_TotalGames, PlayerPrefs.GetInt(K_TotalGames, 0) + 1);
        if (win) PlayerPrefs.SetInt(K_Wins, PlayerPrefs.GetInt(K_Wins, 0) + 1);
        else PlayerPrefs.SetInt(K_Losses, PlayerPrefs.GetInt(K_Losses, 0) + 1);

        PlayerPrefs.SetInt(K_TotalPlaySeconds, PlayerPrefs.GetInt(K_TotalPlaySeconds, 0) + Mathf.Max(0, seconds));

        // Mode counters
        if (mode == GameMode.Classic && win)
            PlayerPrefs.SetInt(K_ClassicWins, PlayerPrefs.GetInt(K_ClassicWins, 0) + 1);

        if (mode == GameMode.Challenge && win)
            PlayerPrefs.SetInt(K_ChallengeWins, PlayerPrefs.GetInt(K_ChallengeWins, 0) + 1);

        // History
        AppendHistory(mode, detail, seconds, win);

        PlayerPrefs.Save();
    }

    static void AppendHistory(GameMode mode, string detail, int seconds, bool win)
    {
        var json = PlayerPrefs.GetString(K_HistoryJson, "");
        HistoryList list;

        if (string.IsNullOrEmpty(json))
            list = new HistoryList();
        else
        {
            try { list = JsonUtility.FromJson<HistoryList>(json) ?? new HistoryList(); }
            catch { list = new HistoryList(); }
        }

        list.items.Insert(0, new HistoryItem
        {
            mode = mode.ToString(),
            detail = detail,
            seconds = seconds,
            win = win,
            at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        });

        if (list.items.Count > HistoryMax)
            list.items.RemoveRange(HistoryMax, list.items.Count - HistoryMax);

        PlayerPrefs.SetString(K_HistoryJson, JsonUtility.ToJson(list));
    }

    public static int TotalGames => PlayerPrefs.GetInt(K_TotalGames, 0);
    public static int Wins => PlayerPrefs.GetInt(K_Wins, 0);
    public static int Losses => PlayerPrefs.GetInt(K_Losses, 0);
    public static int TotalPlaySeconds => PlayerPrefs.GetInt(K_TotalPlaySeconds, 0);
    public static int ClassicWins => PlayerPrefs.GetInt(K_ClassicWins, 0);
    public static int ChallengeWins => PlayerPrefs.GetInt(K_ChallengeWins, 0);

    public static List<HistoryItem> GetHistory()
    {
        var json = PlayerPrefs.GetString(K_HistoryJson, "");
        if (string.IsNullOrEmpty(json)) return new List<HistoryItem>();

        try
        {
            var list = JsonUtility.FromJson<HistoryList>(json);
            return list?.items ?? new List<HistoryItem>();
        }
        catch
        {
            return new List<HistoryItem>();
        }
    }

    public static string FormatHms(int totalSeconds)
    {
        totalSeconds = Mathf.Max(0, totalSeconds);
        int h = totalSeconds / 3600;
        int m = (totalSeconds % 3600) / 60;
        int s = totalSeconds % 60;
        return $"{h:00}:{m:00}:{s:00}";
    }
}