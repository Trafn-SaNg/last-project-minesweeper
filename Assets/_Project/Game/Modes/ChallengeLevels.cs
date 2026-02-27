using UnityEngine;

[System.Serializable]
public class LevelConfig
{
    public int id;
    public int width;
    public int height;
    public int mines;

    public LevelConfig(int id, int w, int h, int m)
    {
        this.id = id;
        width = w;
        height = h;
        mines = m;
    }
}

public static class ChallengeLevels
{
    public const int MaxLevel = 10;

    // Anh có thể chỉnh thông số cho hợp demo
    public static readonly LevelConfig[] Levels =
    {
        new LevelConfig(1,  8,  8, 10),
        new LevelConfig(2, 10, 10, 15),
        new LevelConfig(3, 12, 12, 25),
        new LevelConfig(4, 16, 12, 35),
        new LevelConfig(5, 16, 16, 45),

        new LevelConfig(6, 18, 16, 55),
        new LevelConfig(7, 20, 16, 70),
        new LevelConfig(8, 22, 18, 85),
        new LevelConfig(9, 24, 20, 99),
        new LevelConfig(10, 26, 20, 120),
    };

    public static LevelConfig Get(int levelId)
    {
        levelId = Mathf.Clamp(levelId, 1, MaxLevel);
        return Levels[levelId - 1];
    }
}