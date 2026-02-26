using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Minesweeper.UI;

public sealed class ChallengeInGameUI : MonoBehaviour
{
    [Header("Refs")]
    public GameController gameController;

    [Header("UI")]
    public TMP_Text challengeLabel;     // ví dụ: "Challenge L3 - 16x16 - 40"
    public Button backToModeButton;     // nếu anh muốn quay lại mode select
    public GameObject modeSelectPanel;  // panel mode select

    private void Awake()
    {
        if (backToModeButton)
            backToModeButton.onClick.AddListener(BackToModeSelect);
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (!gameController || !challengeLabel) return;

        // Label chỉ để hiển thị, không ảnh hưởng gameplay
        // (Nếu anh muốn “level hiện tại” chính xác, anh có thể tự lưu levelId ở UI/Session sau)
        challengeLabel.text = "Challenge Mode";
    }

    private void BackToModeSelect()
    {
        if (modeSelectPanel) modeSelectPanel.SetActive(true);
        gameObject.SetActive(false);
    }
}