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
    public static readonly LevelConfig[] Levels =
    {
        new LevelConfig(1,  8,  8, 10),
        new LevelConfig(2, 10, 10, 15),
        new LevelConfig(3, 12, 12, 25),
        new LevelConfig(4, 16, 12, 35),
        new LevelConfig(5, 16, 16, 45),
    };

    public static LevelConfig Get(int levelId)
    {
        levelId = Mathf.Clamp(levelId, 1, 5);
        return Levels[levelId - 1];
    }
}