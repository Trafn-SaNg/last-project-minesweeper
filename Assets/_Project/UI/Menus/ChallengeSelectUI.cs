using UnityEngine;
using UnityEngine.UI;

public sealed class ChallengeSelectUI : MonoBehaviour
{
    [Header("Refs")]
    public UIOverlayManager overlay;        // kéo UIOverlayManager
    public ChallengeGameBridge bridge;      // kéo ChallengeBridge
    public Button backButton;

    [Header("Level Buttons (size 5)")]
    public Button[] levelButtons;           // LevelButton_1..5
    public GameObject[] lockIcons;          // optional

    void Awake()
    {
        if (backButton)
            backButton.onClick.AddListener(() =>
            {
                if (overlay) overlay.ShowModeSelect();
            });

        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelId = i + 1;
            if (levelButtons[i])
                levelButtons[i].onClick.AddListener(() => StartLevel(levelId));
        }
    }

    void OnEnable()
    {
        RefreshLocks();
    }

    public void RefreshLocks()
    {
        for (int i = 0; i < 5; i++)
        {
            int levelId = i + 1;
            bool unlocked = GameSession.I != null && GameSession.I.IsLevelUnlocked(levelId);

            if (levelButtons != null && i < levelButtons.Length && levelButtons[i])
                levelButtons[i].interactable = unlocked;

            if (lockIcons != null && i < lockIcons.Length && lockIcons[i])
                lockIcons[i].SetActive(!unlocked);
        }
    }

    void StartLevel(int levelId)
    {
        if (GameSession.I != null && !GameSession.I.IsLevelUnlocked(levelId))
            return;

        if (overlay) overlay.HideOverlayForGameplay();

        if (bridge) bridge.StartChallengeLevel(levelId);
        else Debug.LogWarning("[ChallengeSelectUI] Missing ChallengeGameBridge reference!");
    }
}