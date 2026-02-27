using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Minesweeper.Core;

namespace Minesweeper.UI
{
    public sealed class GameController : MonoBehaviour
    {
        [Header("Board")]
        public RectTransform boardRoot;
        public GridLayoutGroup grid;
        public TileView tilePrefab;

        [Header("HUD")]
        public TMP_Text timerText;
        public TMP_Text minesLeftText;
        public TMP_Text bestText;
        public TMP_Dropdown difficultyDropdown;
        public Button newGameButton;

        [Header("Overlay Root + Panels")]
        public GameObject overlay; // Canvas/Overlay (root)
        public GameObject winPanel;
        public GameObject losePanel;
        public GameObject customPanel;

        [Header("Menus")]
        public GameObject modeSelectPanel;
        public GameObject challengeSelectPanel;

        [Header("Help")]
        public GameObject helpPanel;
        public Button helpButton;
        public Button helpCloseButton;

        [Header("Chat")]
        public ChatUI chatUI;
        public Button chatButton; // nút Chat ở HelpPanel

        [Header("Custom Inputs")]
        public TMP_InputField inputWidth;
        public TMP_InputField inputHeight;
        public TMP_InputField inputMines;
        public Button applyButton;
        public Button cancelButton;

        [Header("Win/Lose Buttons (Classic group)")]
        public Button winPlayAgainButton;
        public Button winCloseButton;
        public Button losePlayAgainButton;
        public Button loseCloseButton;

        [Header("Win UI")]
        public TMP_Text winInfo;

        [Header("Audio")]
        public AudioManager audioManager;

        [Header("Managers (Cách A)")]
        public UIOverlayManager overlayUI;          // object UIOverlayManager (Hierarchy)
        public WinLoseModeSwitch winLoseModeSwitch; // object UIFlow (WinLoseModeSwitch)


        private Board _board;
        private TileView[,] _tiles;

        private GameState _state = GameState.Ready;
        private float _time;
        private bool _firstClick;

        private int _w = 9, _h = 9, _m = 10;
        private string _difficultyKey = "Easy"; // Easy/Medium/Hard/Custom/Challenge_Lx

        private Coroutine _loseRoutine;
        private int _runId;

        void Awake()
        {
            if (!grid && boardRoot) grid = boardRoot.GetComponent<GridLayoutGroup>();

            // Ensure overlay root active (menus live under it)
            if (overlay) overlay.SetActive(true);

            // UI hooks
            if (newGameButton) newGameButton.onClick.AddListener(NewGame);
            if (difficultyDropdown) difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);

            if (applyButton) applyButton.onClick.AddListener(ApplyCustom);
            if (cancelButton) cancelButton.onClick.AddListener(CancelCustom);

            if (winPlayAgainButton) winPlayAgainButton.onClick.AddListener(PlayAgain);
            if (winCloseButton) winCloseButton.onClick.AddListener(CloseWinLose);
            if (losePlayAgainButton) losePlayAgainButton.onClick.AddListener(PlayAgain);
            if (loseCloseButton) loseCloseButton.onClick.AddListener(CloseWinLose);

            if (helpButton) helpButton.onClick.AddListener(ShowHelp);
            if (helpCloseButton) helpCloseButton.onClick.AddListener(CloseHelp);

            if (chatButton) chatButton.onClick.AddListener(OpenChatFromHelp);

            HideAllOverlayPanels(); // do not disable overlay root
            HideMenusOnly();
        }

        void Start()
        {
            if (!audioManager) audioManager = FindObjectOfType<AudioManager>();
            if (audioManager) audioManager.PlayBgm();

            // Màn đầu tiên: ModeSelect do ModeSelectUI/overlayUI đảm nhiệm.
            // Nếu anh không dùng ModeSelectPanel thì fallback chơi luôn.
            if (modeSelectPanel == null && overlayUI == null)
            {
                ApplyDifficultyPreset(0);
                NewGame();
            }
        }

        void Update()
        {
            if (_state == GameState.Playing)
            {
                _time += Time.deltaTime;
                UpdateTimerText();
            }
        }

        // ---------------- Difficulty / HUD ----------------

        public void OnDifficultyChanged(int value)
        {
            // 0 Easy, 1 Medium, 2 Hard, 3 Custom
            if (value == 3)
            {
                ShowCustom();
                return;
            }

            if (GameSession.I != null) GameSession.I.Mode = GameMode.Classic;

            ApplyDifficultyPreset(value);
            NewGame();
        }

        void ApplyDifficultyPreset(int value)
        {
            if (value == 0) { _w = 9; _h = 9; _m = 10; _difficultyKey = "Easy"; }
            if (value == 1) { _w = 16; _h = 16; _m = 40; _difficultyKey = "Medium"; }
            if (value == 2) { _w = 30; _h = 16; _m = 99; _difficultyKey = "Hard"; }
        }

        string BestKey() => $"{_difficultyKey}_{_w}x{_h}_{_m}";

        void UpdateBestText()
        {
            if (!bestText) return;
            int best = HighScoreStore.GetBest(BestKey());
            bestText.text = best < 0 ? "Best: --" : $"Best: {FormatTime(best)}";
        }

        void UpdateTimerText()
        {
            if (!timerText) return;
            int sec = Mathf.Clamp(Mathf.FloorToInt(_time), 0, 9999);
            timerText.text = $"Time: {FormatTime(sec)}";
        }

        void UpdateMinesLeftText()
        {
            if (!minesLeftText) return;
            int flags = _board != null ? _board.CountFlags() : 0;
            int left = Mathf.Max(0, _m - flags);
            minesLeftText.text = $"Mines: {left}";
        }

        public static string FormatTime(int seconds)
        {
            int mm = seconds / 60;
            int ss = seconds % 60;
            return $"{mm:00}:{ss:00}";
        }

        // ---------------- Overlay helpers (Cách A) ----------------

        void HideAllOverlayPanels()
        {
            // NEVER set overlay root inactive here.
            if (winPanel) winPanel.SetActive(false);
            if (losePanel) losePanel.SetActive(false);
            if (customPanel) customPanel.SetActive(false);
            if (helpPanel) helpPanel.SetActive(false);
            // ChatPanel do ChatUI quản lý riêng (nếu cần thì đóng trong ChatUI)
        }

        void HideMenusOnly()
        {
            if (modeSelectPanel) modeSelectPanel.SetActive(false);
            if (challengeSelectPanel) challengeSelectPanel.SetActive(false);
        }

        void ApplyWinLoseButtonGroups()
        {
            bool isChallenge = GameSession.I != null && GameSession.I.Mode == GameMode.Challenge;
            if (winLoseModeSwitch) winLoseModeSwitch.ApplyMode(isChallenge);
        }

        // ---------------- Public API for ModeSelect / Challenge ----------------

        public void StartClassic()
        {
            if (GameSession.I != null) GameSession.I.Mode = GameMode.Classic;

            // hide menus + overlays
            HideMenusOnly();
            HideAllOverlayPanels();

            // keep dropdown stable: if currently Custom => back to Easy to avoid popping custom panel
            if (difficultyDropdown && difficultyDropdown.value == 3)
                difficultyDropdown.value = 0;

            if (difficultyDropdown) ApplyDifficultyPreset(difficultyDropdown.value);

            NewGame();
        }

        // Called by ChallengeGameBridge (Hướng A)
        public void StartChallengeGame(int width, int height, int mines, int levelId)
        {
            if (GameSession.I != null)
            {
                GameSession.I.Mode = GameMode.Challenge;
                GameSession.I.CurrentLevelId = levelId;
            }

            _difficultyKey = $"Challenge_L{levelId}";
            StartCustom(width, height, mines);
        }

        // Unified entry for Custom/Challenge
        public void StartCustom(int width, int height, int mines)
        {
            _w = width;
            _h = height;
            _m = mines;

            if (string.IsNullOrEmpty(_difficultyKey))
                _difficultyKey = "Custom";

            HideMenusOnly();
            HideAllOverlayPanels();

            NewGame();
        }


        // ---------------- Custom panel ----------------

        void ShowCustom()
        {
            if (overlayUI != null)
            {
                overlayUI.HideAllPanels();
                if (customPanel)
                {
                    customPanel.SetActive(true);
                    customPanel.transform.SetAsLastSibling();
                }
            }
            else
            {
                if (overlay) overlay.SetActive(true);
                HideAllOverlayPanels();
                if (customPanel) customPanel.SetActive(true);
            }

            if (inputWidth) inputWidth.text = _w.ToString();
            if (inputHeight) inputHeight.text = _h.ToString();
            if (inputMines) inputMines.text = _m.ToString();
        }

        public void CancelCustom()
        {
            HideAllOverlayPanels();

            // revert dropdown back to Easy
            if (difficultyDropdown && difficultyDropdown.value == 3)
                difficultyDropdown.value = 0;
        }

        public void ApplyCustom()
        {
            int w = ParseInt(inputWidth, 9);
            int h = ParseInt(inputHeight, 9);
            int m = ParseInt(inputMines, 10);

            w = Mathf.Clamp(w, 5, 60);
            h = Mathf.Clamp(h, 5, 40);
            int maxMines = w * h - 9;
            m = Mathf.Clamp(m, 1, Mathf.Max(1, maxMines));

            if (GameSession.I != null) GameSession.I.Mode = GameMode.Classic;

            _w = w; _h = h; _m = m;
            _difficultyKey = "Custom";

            HideAllOverlayPanels();
            NewGame();
        }

        static int ParseInt(TMP_InputField field, int fallback)
        {
            if (!field) return fallback;
            return int.TryParse(field.text, out var v) ? v : fallback;
        }

        // ---------------- Win/Lose panel controls ----------------

        public void CloseWinLose()
        {
            HideAllOverlayPanels();
        }

        public void PlayAgain()
        {
            HideAllOverlayPanels();
            NewGame();
        }

        // ---------------- Help / Chat ----------------

        void ShowHelp()
        {
            if (overlayUI != null)
            {
                overlayUI.HideAllPanels();
                if (helpPanel)
                {
                    helpPanel.SetActive(true);
                    helpPanel.transform.SetAsLastSibling();
                }
                return;
            }

            if (overlay) overlay.SetActive(true);
            HideAllOverlayPanels();
            if (helpPanel) helpPanel.SetActive(true);
        }

        void CloseHelp()
        {
            if (helpPanel) helpPanel.SetActive(false);
            // do NOT disable overlay root
        }

        void OpenChatFromHelp()
        {
            if (helpPanel) helpPanel.SetActive(false);
            if (chatUI) chatUI.Open();
        }

        // ---------------- Game flow ----------------

        public void NewGame()
        {
            _runId++;
            if (_loseRoutine != null)
            {
                StopCoroutine(_loseRoutine);
                _loseRoutine = null;
            }

            HideAllOverlayPanels();

            _board = new Board(_w, _h, _m);
            _board.Reset();

            _tiles = new TileView[_w, _h];

            _state = GameState.Ready;
            _time = 0f;
            _firstClick = true;

            UpdateTimerText();
            UpdateMinesLeftText();
            UpdateBestText();

            BuildGrid();
            RenderAll();
        }

        void BuildGrid()
        {
            if (!boardRoot) return;

            for (int i = boardRoot.childCount - 1; i >= 0; i--)
                Destroy(boardRoot.GetChild(i).gameObject);

            if (grid)
            {
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = _w;
                FitGridToBoardRoot();
            }

            for (int y = 0; y < _h; y++)
                for (int x = 0; x < _w; x++)
                {
                    var t = Instantiate(tilePrefab, boardRoot);
                    t.Init(x, y, OnLeftClick, OnRightClick);
                    _tiles[x, y] = t;
                }

            StartCoroutine(FitNextFrame());
        }

        void OnLeftClick(int x, int y)
        {
            if (_state == GameState.Won || _state == GameState.Lost) return;
            if (_board == null) return;

            if (_firstClick)
            {
                _board.Generate(x, y);
                _firstClick = false;
                _state = GameState.Playing;
            }

            bool hitMine;
            bool changed = _board.Reveal(x, y, out hitMine);
            if (!changed) return;

            if (hitMine)
            {
                _state = GameState.Lost;

                if (_loseRoutine != null) StopCoroutine(_loseRoutine);
                _loseRoutine = StartCoroutine(LoseSequence(x, y, _runId));
                return;
            }

            RenderAll();

            if (_board.CheckWin())
            {
                _state = GameState.Won;

                int seconds = Mathf.Clamp(Mathf.FloorToInt(_time), 0, 9999);

                // Save best for this mode/key
                HighScoreStore.TrySetBest(BestKey(), seconds);
                UpdateBestText();

                // Unlock next challenge level
                if (GameSession.I != null && GameSession.I.Mode == GameMode.Challenge)
                {
                    GameSession.I.MarkLevelCleared(GameSession.I.CurrentLevelId);
                }

                // Update Win info
                if (winInfo)
                {
                    int best = HighScoreStore.GetBest(BestKey());
                    string bestStr = best < 0 ? "--" : FormatTime(best);
                    winInfo.text = $"Time: {FormatTime(seconds)}  |  Best: {bestStr}";
                }

                if (audioManager) audioManager.PlayVictory();

                ApplyWinLoseButtonGroups();

                if (overlayUI != null) overlayUI.ShowWin();
                else ShowWinFallback();
            }
        }

        void OnRightClick(int x, int y)
        {
            if (_state == GameState.Won || _state == GameState.Lost) return;
            if (_board == null) return;

            if (_board.ToggleFlag(x, y))
            {
                UpdateMinesLeftText();
                RenderCell(x, y);
            }
        }

        void RenderAll()
        {
            for (int y = 0; y < _h; y++)
                for (int x = 0; x < _w; x++)
                    RenderCell(x, y);
        }

        void RenderCell(int x, int y)
        {
            var c = _board.Cells[_board.Index(x, y)];
            var t = _tiles[x, y];

            switch (c.State)
            {
                case CellState.Hidden:
                    t.ShowHidden();
                    break;
                case CellState.Flagged:
                    t.ShowFlag();
                    break;
                case CellState.Revealed:
                    if (c.IsMine) t.ShowMine();
                    else t.ShowNumber(c.AdjacentMines);
                    break;
            }
        }

        void FitGridToBoardRoot()
        {
            if (!boardRoot || !grid) return;

            float availW = boardRoot.rect.width - grid.padding.left - grid.padding.right;
            float availH = boardRoot.rect.height - grid.padding.top - grid.padding.bottom;

            float sx = grid.spacing.x;
            float sy = grid.spacing.y;

            float cellW = (availW - sx * (_w - 1)) / _w;
            float cellH = (availH - sy * (_h - 1)) / _h;

            float cell = Mathf.Floor(Mathf.Min(cellW, cellH));
            cell = Mathf.Clamp(cell, 24f, 96f);

            grid.cellSize = new Vector2(cell, cell);
        }

        void OnRectTransformDimensionsChange()
        {
            if (_board == null) return;
            FitGridToBoardRoot();
        }

        IEnumerator FitNextFrame()
        {
            yield return null;
            FitGridToBoardRoot();
        }

        // Fallback show panels if overlayUI not assigned
        void ShowWinFallback()
        {
            if (overlay) overlay.SetActive(true);
            HideAllOverlayPanels();
            if (winPanel)
            {
                winPanel.SetActive(true);
                winPanel.transform.SetAsLastSibling();
            }
        }

        void ShowLoseFallback()
        {
            if (overlay) overlay.SetActive(true);
            HideAllOverlayPanels();
            if (losePanel)
            {
                losePanel.SetActive(true);
                losePanel.transform.SetAsLastSibling();
            }
        }

        IEnumerator LoseSequence(int hitX, int hitY, int runId)
        {
            var mines = new List<(int x, int y, int d2)>();
            for (int y = 0; y < _h; y++)
                for (int x = 0; x < _w; x++)
                {
                    var c = _board.Cells[_board.Index(x, y)];
                    if (!c.IsMine) continue;
                    int dx = x - hitX;
                    int dy = y - hitY;
                    mines.Add((x, y, dx * dx + dy * dy));
                }
            mines.Sort((a, b) => a.d2.CompareTo(b.d2));

            const float stepDelay = 0.06f;
            for (int i = 0; i < mines.Count; i++)
            {
                if (runId != _runId) yield break;

                var (mx, my, _) = mines[i];
                _tiles[mx, my].ShowMine();

                if (audioManager) audioManager.PlayBoom();
                yield return new WaitForSeconds(stepDelay);
            }

            float afterDelay = 1.5f;
            float t = 0f;
            while (t < afterDelay)
            {
                if (runId != _runId) yield break;
                t += Time.deltaTime;
                yield return null;
            }
            if (runId != _runId) yield break;

            if (audioManager) audioManager.PlayLose();

            ApplyWinLoseButtonGroups();

            if (overlayUI != null) overlayUI.ShowLose();
            else ShowLoseFallback();

            _loseRoutine = null;
        }
    }
}