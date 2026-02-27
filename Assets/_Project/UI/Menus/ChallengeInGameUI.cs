using UnityEngine;
using UnityEngine.UI;

public sealed class ChallengeInGameUI : MonoBehaviour
{
    public GameObject challengeSelectPanel;   // Canvas/Overlay/ChallengeSelectPanel
    public ChallengeGameBridge bridge;        // object ChallengeBridge

    [Header("Win Buttons")]
    public Button nextButton;
    public Button winRetryButton;
    public Button winLevelSelectButton;

    [Header("Lose Buttons")]
    public Button loseRetryButton;
    public Button loseLevelSelectButton;

    void Awake()
    {
        if (nextButton) nextButton.onClick.AddListener(Next);
        if (winRetryButton) winRetryButton.onClick.AddListener(Retry);
        if (loseRetryButton) loseRetryButton.onClick.AddListener(Retry);

        if (winLevelSelectButton) winLevelSelectButton.onClick.AddListener(LevelSelect);
        if (loseLevelSelectButton) loseLevelSelectButton.onClick.AddListener(LevelSelect);
    }

    void Next()
    {
        if (GameSession.I == null) return;

        int nextLv = Mathf.Clamp(GameSession.I.CurrentLevelId + 1, 1, 5);
        if (!GameSession.I.IsLevelUnlocked(nextLv)) return;

        bridge.StartChallengeLevel(nextLv);
    }

    void Retry()
    {
        if (GameSession.I == null) return;
        bridge.StartChallengeLevel(GameSession.I.CurrentLevelId);
    }

    void LevelSelect()
    {
        if (challengeSelectPanel) challengeSelectPanel.SetActive(true);
    }
}