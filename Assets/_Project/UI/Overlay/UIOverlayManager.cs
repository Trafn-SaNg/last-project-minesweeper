using UnityEngine;

public sealed class UIOverlayManager : MonoBehaviour
{
    [Header("Overlay Root")]
    public GameObject overlayRoot; // Canvas/Overlay

    [Header("Panels")]
    public GameObject modeSelectPanel;
    public GameObject challengeSelectPanel;

    public GameObject winPanel;
    public GameObject losePanel;
    public GameObject helpPanel;
    public GameObject customPanel;
    public GameObject chatPanel;

    void Awake()
    {
        if (overlayRoot) overlayRoot.SetActive(true);
    }

    void Start()
    {
        // Màn hình đầu tiên khi vào game
        ShowModeSelect();
    }

    public void HideAllPanels()
    {
        if (modeSelectPanel) modeSelectPanel.SetActive(false);
        if (challengeSelectPanel) challengeSelectPanel.SetActive(false);
        if (winPanel) winPanel.SetActive(false);
        if (losePanel) losePanel.SetActive(false);
        if (helpPanel) helpPanel.SetActive(false);
        if (customPanel) customPanel.SetActive(false);
        if (chatPanel) chatPanel.SetActive(false);
    }

    void BringToFront(GameObject panel)
    {
        if (!panel) return;
        panel.transform.SetAsLastSibling();
    }

    public void ShowModeSelect()
    {
        if (overlayRoot) overlayRoot.SetActive(true);
        HideAllPanels();

        if (modeSelectPanel)
        {
            modeSelectPanel.SetActive(true);
            BringToFront(modeSelectPanel);
        }
    }

    public void ShowChallengeSelect()
    {
        if (overlayRoot) overlayRoot.SetActive(true);
        HideAllPanels();

        if (challengeSelectPanel)
        {
            challengeSelectPanel.SetActive(true);
            BringToFront(challengeSelectPanel);
        }
    }

    // Khi vào gameplay (Classic hoặc Challenge), ta ẩn overlay menu
    public void HideOverlayForGameplay()
    {
        if (overlayRoot) overlayRoot.SetActive(true); // giữ ON để Win/Lose bật được khi cần
        HideAllPanels();
    }

    public void ShowWin()
    {
        if (overlayRoot) overlayRoot.SetActive(true);
        if (winPanel)
        {
            winPanel.SetActive(true);
            BringToFront(winPanel);
        }
    }

    public void ShowLose()
    {
        if (overlayRoot) overlayRoot.SetActive(true);
        if (losePanel)
        {
            losePanel.SetActive(true);
            BringToFront(losePanel);
        }
    }
}