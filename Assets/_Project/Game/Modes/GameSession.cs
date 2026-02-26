using UnityEngine;

public enum GameMode { Classic, Challenge }

public sealed class GameSession : MonoBehaviour
{
    public static GameSession I { get; private set; }

    [Header("Runtime")]
    public GameMode Mode = GameMode.Classic;
    public int CurrentLevelId = 1; // 1..5

    [Header("Challenge Progress")]
    public int HighestUnlockedLevel = 1; // mặc định mở Level 1
    public float[] BestTimeByLevel = new float[6]; // index 1..5, 0 bỏ

    const string KeyUnlocked = "CH_UNLOCKED";
    const string KeyBestPrefix = "CH_BEST_"; // + levelId

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        LoadChallengeProgress();
    }

    public void LoadChallengeProgress()
    {
        HighestUnlockedLevel = PlayerPrefs.GetInt(KeyUnlocked, 1);
        if (BestTimeByLevel == null || BestTimeByLevel.Length < 6) BestTimeByLevel = new float[6];

        for (int lv = 1; lv <= 5; lv++)
            BestTimeByLevel[lv] = PlayerPrefs.GetFloat(KeyBestPrefix + lv, 0f);
    }

    public void SaveChallengeProgress()
    {
        PlayerPrefs.SetInt(KeyUnlocked, HighestUnlockedLevel);
        for (int lv = 1; lv <= 5; lv++)
            PlayerPrefs.SetFloat(KeyBestPrefix + lv, BestTimeByLevel[lv]);
        PlayerPrefs.Save();
    }

    public bool IsLevelUnlocked(int levelId)
        => levelId <= HighestUnlockedLevel;

    public void MarkLevelCleared(int levelId, float timeSeconds)
    {
        // best time
        var best = BestTimeByLevel[levelId];
        if (best <= 0f || timeSeconds < best)
            BestTimeByLevel[levelId] = timeSeconds;

        // unlock next
        if (levelId >= HighestUnlockedLevel && levelId < 5)
            HighestUnlockedLevel = levelId + 1;

        SaveChallengeProgress();
    }

    public static string FormatTime(float t)
    {
        if (t <= 0f) return "--";
        int sec = Mathf.RoundToInt(t);
        int m = sec / 60;
        int s = sec % 60;
        return $"{m:00}:{s:00}";
    }
}