using UnityEngine;
using Minesweeper.UI;

public sealed class ChallengeGameBridge : MonoBehaviour
{
    public GameController gameController;

    public void StartChallengeLevel(int levelId)
    {
        var cfg = ChallengeLevels.Get(levelId);

        if (GameSession.I != null)
        {
            GameSession.I.Mode = GameMode.Challenge;
            GameSession.I.CurrentLevelId = levelId;
        }

        if (gameController != null)
            gameController.StartChallengeGame(cfg.width, cfg.height, cfg.mines, levelId);
        else
            Debug.LogWarning("[ChallengeGameBridge] Missing GameController reference!");
        Debug.Log("[Bridge] StartChallengeLevel " + levelId);
    }
}