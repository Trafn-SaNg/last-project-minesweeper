using UnityEngine;
using UnityEngine.UI;
using Minesweeper.UI;

public sealed class ModeSelectUI : MonoBehaviour
{
    [Header("Refs")]
    public UIOverlayManager overlay;   // kéo object UIOverlayManager (Hierarchy) vào đây
    public GameController gameController;

    [Header("Buttons")]
    public Button classicButton;
    public Button challengeButton;

    void Awake()
    {
        if (classicButton) classicButton.onClick.AddListener(OnClassic);
        if (challengeButton) challengeButton.onClick.AddListener(OnChallenge);
    }

    void Start()
    {
        // Màn đầu tiên
        if (overlay) overlay.ShowModeSelect();
    }

    void OnClassic()
    {
        if (GameSession.I != null) GameSession.I.Mode = GameMode.Classic;

        if (overlay) overlay.HideOverlayForGameplay();
        if (gameController) gameController.StartClassic();
    }

    void OnChallenge()
    {
        if (GameSession.I != null) GameSession.I.Mode = GameMode.Challenge;

        if (overlay) overlay.ShowChallengeSelect();
        // Không start game ở đây, start ở ChallengeSelectUI khi chọn level
    }
}