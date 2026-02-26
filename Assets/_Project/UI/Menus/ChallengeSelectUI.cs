using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Minesweeper.UI;

public sealed class ChallengeSelectUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject challengeSelectPanel;
    public GameObject modeSelectPanel;

    [Header("Header Buttons")]
    public Button backButton;

    [Header("Level Buttons (size 5)")]
    public Button[] levelButtons;           // 0..4 => level 1..5
    public TMP_Text[] levelInfoTexts;       // hiển thị WxH - mines
    public TMP_Text[] levelBestTexts;       // hiển thị best

    [Header("Refs")]
    public GameController gameController;   // kéo GameController vào

    private void Awake()
    {
        if (backButton) backButton.onClick.AddListener(BackToModeSelect);

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

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (!gameController) return;

        // cập nhật text + khóa/mở
        for (int i = 0; i < 5; i++)
        {
            int levelId = i + 1;

            if (gameController.TryGetChallengeConfig(levelId, out var cfg))
            {
                if (levelInfoTexts != null && i < levelInfoTexts.Length && levelInfoTexts[i])
                    levelInfoTexts[i].text = $"{cfg.width}x{cfg.height} - {cfg.mines} mines";
            }

            // Best theo level
            int best = HighScoreStore.GetBest(gameController.ChallengeBestKey(levelId));
            if (levelBestTexts != null && i < levelBestTexts.Length && levelBestTexts[i])
                levelBestTexts[i].text = best < 0 ? "Best: --" : $"Best: {GameController.FormatTime(best)}";

            // Unlock đơn giản: level 1 luôn mở, level sau mở khi level trước có best
            bool unlocked = (levelId == 1) || HighScoreStore.GetBest(gameController.ChallengeBestKey(levelId - 1)) >= 0;

            if (levelButtons != null && i < levelButtons.Length && levelButtons[i])
                levelButtons[i].interactable = unlocked;
        }
    }

    private void StartLevel(int levelId)
    {
        if (!gameController) return;

        if (challengeSelectPanel) challengeSelectPanel.SetActive(false);
        gameController.StartChallengeGame(levelId);
    }

    private void BackToModeSelect()
    {
        if (challengeSelectPanel) challengeSelectPanel.SetActive(false);
        if (modeSelectPanel) modeSelectPanel.SetActive(true);
    }
}