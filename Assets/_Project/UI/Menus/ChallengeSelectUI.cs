using UnityEngine;
using UnityEngine.UI;

public sealed class ChallengeSelectUI : MonoBehaviour
{
    [Header("Refs")]
    public UIOverlayManager overlay;
    public ChallengeGameBridge bridge;
    public Button backButton;

    [Header("Level Buttons")]
    public Button[] levelButtons;     // kéo đủ 10 button
    public GameObject[] lockIcons;    // optional, nếu có kéo đủ 10

    void Awake()
    {
        if (backButton)
            backButton.onClick.AddListener(() =>
            {
                if (overlay) overlay.ShowModeSelect();
            });

        if (levelButtons != null)
        {
            for (int i = 0; i < levelButtons.Length; i++)
            {
                int levelId = i + 1;
                if (levelButtons[i])
                    levelButtons[i].onClick.AddListener(() => StartLevel(levelId));
            }
        }
    }

    void OnEnable()
    {
        RefreshLocks();
    }

    public void RefreshLocks()
    {
        int n = (levelButtons != null) ? levelButtons.Length : 0;

        for (int i = 0; i < n; i++)
        {
            int levelId = i + 1;
            bool unlocked = GameSession.I != null && GameSession.I.IsLevelUnlocked(levelId);

            if (levelButtons[i]) levelButtons[i].interactable = unlocked;

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