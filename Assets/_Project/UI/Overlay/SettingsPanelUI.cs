using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Minesweeper.UI
{
    public sealed class SettingsPanelUI : MonoBehaviour
    {
        [Header("Managers")]
        public UIOverlayManager overlayUI;
        public GameController gameController;
        public AudioManager audioManager;

        [Header("Panel")]
        public GameObject settingsPanel;
        public Button dimButton;
        public Button closeButton;
        public Button quitButton;

        [Header("Sliders")]
        public Slider bgmSlider;
        public Slider sfxSlider;
        public TMP_Text bgmValueText;
        public TMP_Text sfxValueText;

        const string KeyBgm = "VOL_BGM";
        const string KeySfx = "VOL_SFX";

        void Awake()
        {
            if (!audioManager) audioManager = FindObjectOfType<AudioManager>();

            if (dimButton) dimButton.onClick.AddListener(Close);
            if (closeButton) closeButton.onClick.AddListener(Close);
            if (quitButton) quitButton.onClick.AddListener(QuitToModeSelect);

            if (bgmSlider)
            {
                bgmSlider.minValue = 0f;
                bgmSlider.maxValue = 1f;
                bgmSlider.onValueChanged.AddListener(OnBgmChanged);
            }

            if (sfxSlider)
            {
                sfxSlider.minValue = 0f;
                sfxSlider.maxValue = 1f;
                sfxSlider.onValueChanged.AddListener(OnSfxChanged);
            }

            float bgm = PlayerPrefs.GetFloat(KeyBgm, 1f);
            float sfx = PlayerPrefs.GetFloat(KeySfx, 1f);

            if (bgmSlider) bgmSlider.SetValueWithoutNotify(bgm);
            if (sfxSlider) sfxSlider.SetValueWithoutNotify(sfx);

            ApplyVolumes(bgm, sfx);
            RefreshValueText(bgm, sfx);
        }

        public void Open()
        {
            if (!settingsPanel) return;
            settingsPanel.SetActive(true);
            settingsPanel.transform.SetAsLastSibling();

            float bgm = bgmSlider ? bgmSlider.value : PlayerPrefs.GetFloat(KeyBgm, 1f);
            float sfx = sfxSlider ? sfxSlider.value : PlayerPrefs.GetFloat(KeySfx, 1f);
            RefreshValueText(bgm, sfx);
        }

        public void Close()
        {
            if (!settingsPanel) return;
            settingsPanel.SetActive(false);
        }

        void OnBgmChanged(float v)
        {
            PlayerPrefs.SetFloat(KeyBgm, v);
            PlayerPrefs.Save();

            float sfx = sfxSlider ? sfxSlider.value : PlayerPrefs.GetFloat(KeySfx, 1f);
            ApplyVolumes(v, sfx);
            RefreshValueText(v, sfx);
        }

        void OnSfxChanged(float v)
        {
            PlayerPrefs.SetFloat(KeySfx, v);
            PlayerPrefs.Save();

            float bgm = bgmSlider ? bgmSlider.value : PlayerPrefs.GetFloat(KeyBgm, 1f);
            ApplyVolumes(bgm, v);
            RefreshValueText(bgm, v);
        }

        void RefreshValueText(float bgm, float sfx)
        {
            if (bgmValueText) bgmValueText.text = $"{Mathf.RoundToInt(bgm * 100f)}%";
            if (sfxValueText) sfxValueText.text = $"{Mathf.RoundToInt(sfx * 100f)}%";
        }

        void ApplyVolumes(float bgm, float sfx)
        {
            if (!audioManager) return;
            audioManager.SetBgmVolume(bgm);
            audioManager.SetSfxVolume(sfx);
        }

        void QuitToModeSelect()
        {
            Close();

            if (GameSession.I != null)
            {
                GameSession.I.Mode = GameMode.Classic;
                GameSession.I.CurrentLevelId = 1;
            }

            if (overlayUI) overlayUI.ShowModeSelect();
            //if (gameController) gameController.PauseForMenu();
        }
    }
}