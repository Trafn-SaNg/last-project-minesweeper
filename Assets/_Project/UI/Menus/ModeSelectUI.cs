using UnityEngine;
using UnityEngine.UI;
using Minesweeper.UI;

public sealed class ModeSelectUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject modeSelectPanel;
    public GameObject challengeSelectPanel;

    [Header("Buttons")]
    public Button classicButton;
    public Button challengeButton;

    [Header("Refs")]
    public GameController gameController;

    private void Awake()
    {
        if (classicButton)
            classicButton.onClick.AddListener(OnClassicClicked);

        if (challengeButton)
            challengeButton.onClick.AddListener(OnChallengeClicked);
    }

    private void OnEnable()
    {
        // đảm bảo panel challenge đang tắt khi mở mode select
        if (challengeSelectPanel)
            challengeSelectPanel.SetActive(false);

        if (modeSelectPanel)
            modeSelectPanel.SetActive(true);
    }

    private void OnClassicClicked()
    {
        if (!gameController)
        {
            Debug.LogError("GameController reference missing in ModeSelectUI");
            return;
        }

        // ẩn panel menu
        if (modeSelectPanel)
            modeSelectPanel.SetActive(false);

        // bắt đầu Classic mode
        gameController.StartClassic();
    }

    private void OnChallengeClicked()
    {
        if (!gameController)
        {
            Debug.LogError("GameController reference missing in ModeSelectUI");
            return;
        }

        // ẩn panel mode select
        if (modeSelectPanel)
            modeSelectPanel.SetActive(false);

        // hiển thị panel challenge select thông qua GameController
        gameController.ShowChallengeSelect();
    }
}