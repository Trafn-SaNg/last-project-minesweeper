using UnityEngine;

public enum GameMode { Classic, Challenge }

public sealed class GameSession : MonoBehaviour
{
    public static GameSession I { get; private set; }

    [Header("Runtime")]
    public GameMode Mode = GameMode.Classic;
    public int CurrentLevelId = 1; // 1..5

    [Header("Challenge Progress")]
    public int HighestUnlockedLevel = 1; // mặc định mở level 1

    const string KeyUnlocked = "CH_UNLOCKED";

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        HighestUnlockedLevel = PlayerPrefs.GetInt(KeyUnlocked, 1);
        HighestUnlockedLevel = Mathf.Clamp(HighestUnlockedLevel, 1, 5);
    }

    public bool IsLevelUnlocked(int levelId)
        => levelId <= HighestUnlockedLevel;

    public void MarkLevelCleared(int levelId)
    {
        levelId = Mathf.Clamp(levelId, 1, 5);

        if (levelId >= HighestUnlockedLevel && levelId < 5)
            HighestUnlockedLevel = levelId + 1;

        PlayerPrefs.SetInt(KeyUnlocked, HighestUnlockedLevel);
        PlayerPrefs.Save();
    }

    // Debug tiện test
    [ContextMenu("Reset Challenge Progress")]
    public void ResetChallengeProgress()
    {
        HighestUnlockedLevel = 1;
        PlayerPrefs.SetInt(KeyUnlocked, HighestUnlockedLevel);
        PlayerPrefs.Save();
    }
}