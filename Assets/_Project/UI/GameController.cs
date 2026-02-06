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

        [Header("Overlay")]
        public GameObject overlay;
        public GameObject winPanel;
        public GameObject losePanel;
        public GameObject customPanel;

        [Header("Custom Inputs")]
        public TMP_InputField inputWidth;
        public TMP_InputField inputHeight;
        public TMP_InputField inputMines;
        public Button applyButton;
        public Button cancelButton;

        [Header("Win/Lose Buttons")]
        public Button winPlayAgainButton;
        public Button winCloseButton;
        public Button losePlayAgainButton;
        public Button loseCloseButton;

        [Header("Help")]
        public GameObject helpPanel;
        public Button helpButton;
        public Button helpCloseButton;

        [Header("Audio")]
        public AudioManager audio;

        [Header("Win UI")]
        public TMP_Text winInfo;

        [Header("Chat")]
        public ChatUI chatUI;
        public Button chatButton; // nút Chat ở HelpPanel




        private Board _board;
        private TileView[,] _tiles;

        private GameState _state = GameState.Ready;
        private float _time;
        private bool _firstClick;

        private int _w = 10, _h = 10, _m = 10;
        private string _difficultyKey = "Easy";

        private void Awake()
        {
            if (!grid && boardRoot) grid = boardRoot.GetComponent<GridLayoutGroup>();

            // UI hooks (safe even if already set in inspector)
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

            HideAllOverlays();
        }

        private void Start()
        {
            if (!audio) audio = FindObjectOfType<AudioManager>();
            if (audio) audio.PlayBgm();

            ApplyDifficultyPreset(0);
            NewGame();
        }


        private void Update()
        {
            if (_state == GameState.Playing)
            {
                _time += Time.deltaTime;
                UpdateTimerText();
            }
        }

        // ---------------- UI ----------------

        public void OnDifficultyChanged(int value)
        {
            if (value == 3) // Custom
            {
                ShowCustom();
                return;
            }

            ApplyDifficultyPreset(value);
            NewGame();
        }

        private void ApplyDifficultyPreset(int value)
        {
            // 0 Easy, 1 Medium, 2 Hard, 3 Custom
            if (value == 0) { _w = 9;  _h = 9;  _m = 10; _difficultyKey = "Easy"; }
            if (value == 1) { _w = 16; _h = 16; _m = 40; _difficultyKey = "Medium"; }
            if (value == 2) { _w = 30; _h = 16; _m = 99; _difficultyKey = "Hard"; }
        }

        private string BestKey() => $"{_difficultyKey}_{_w}x{_h}_{_m}";

        private void UpdateBestText()
        {
            int best = HighScoreStore.GetBest(BestKey());
            bestText.text = best < 0 ? "Best: --" : $"Best: {FormatTime(best)}";
        }

        private void UpdateTimerText()
        {
            int sec = Mathf.Clamp(Mathf.FloorToInt(_time), 0, 9999);
            timerText.text = $"Time: {FormatTime(sec)}";
        }

        private void UpdateMinesLeftText()
        {
            int flags = _board != null ? _board.CountFlags() : 0;
            int left = Mathf.Max(0, _m - flags);
            minesLeftText.text = $"Mines: {left}";
        }

        private static string FormatTime(int seconds)
        {
            int mm = seconds / 60;
            int ss = seconds % 60;
            return $"{mm:00}:{ss:00}";
        }

        // ---------------- Overlay ----------------

        private void HideAllOverlays()
        {
            if (overlay) overlay.SetActive(false);
            if (winPanel) winPanel.SetActive(false);
            if (losePanel) losePanel.SetActive(false);
            if (customPanel) customPanel.SetActive(false);
            if (helpPanel) helpPanel.SetActive(false);
        }

        private void ShowWin()
        {
            if (overlay) overlay.SetActive(true);
            if (winPanel) winPanel.SetActive(true);
            if (losePanel) losePanel.SetActive(false);
            if (customPanel) customPanel.SetActive(false);
        }

        private void ShowLose()
        {
            if (overlay) overlay.SetActive(true);
            if (losePanel) losePanel.SetActive(true);
            if (winPanel) winPanel.SetActive(false);
            if (customPanel) customPanel.SetActive(false);
        }

        private void ShowCustom()
        {
            if (overlay) overlay.SetActive(true);
            if (customPanel) customPanel.SetActive(true);
            if (winPanel) winPanel.SetActive(false);
            if (losePanel) losePanel.SetActive(false);

            // prefill
            if (inputWidth) inputWidth.text = _w.ToString();
            if (inputHeight) inputHeight.text = _h.ToString();
            if (inputMines) inputMines.text = _m.ToString();
        }

        public void CloseWinLose()
        {
            HideAllOverlays();
        }

        public void PlayAgain()
        {
            HideAllOverlays();
            NewGame();
        }

        public void CancelCustom()
        {
            HideAllOverlays();
            // revert dropdown back to previous preset (if it was on Custom)
            if (difficultyDropdown && difficultyDropdown.value == 3)
                difficultyDropdown.value = 0; // default back to Easy
        }

        public void ApplyCustom()
        {
            int w = ParseInt(inputWidth, 9);
            int h = ParseInt(inputHeight, 9);
            int m = ParseInt(inputMines, 10);

            // clamp safe ranges
            w = Mathf.Clamp(w, 5, 40);
            h = Mathf.Clamp(h, 5, 30);
            int maxMines = w * h - 9; // keep first-click safe zone possible
            m = Mathf.Clamp(m, 1, Mathf.Max(1, maxMines));

            _w = w; _h = h; _m = m;
            _difficultyKey = "Custom";

            HideAllOverlays();
            NewGame();
        }

        private static int ParseInt(TMP_InputField field, int fallback)
        {
            if (!field) return fallback;
            return int.TryParse(field.text, out var v) ? v : fallback;
        }

        // ---------------- Game ----------------

        public void NewGame()
        {
            _runId++;
            if (_loseRoutine != null)
            {
                StopCoroutine(_loseRoutine);
                _loseRoutine = null;
            }
            HideAllOverlays();
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

        private void BuildGrid()
        {
            // clear old
            for (int i = boardRoot.childCount - 1; i >= 0; i--)
                Destroy(boardRoot.GetChild(i).gameObject);

            // update grid constraint
            if (grid)
            {
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = _w;
                FitGridToBoardRoot();
            }

            // instantiate tiles
            for (int y = 0; y < _h; y++)
            for (int x = 0; x < _w; x++)
            {
                var t = Instantiate(tilePrefab, boardRoot);
                t.Init(x, y, OnLeftClick, OnRightClick);
                _tiles[x, y] = t;
            }
            StartCoroutine(FitNextFrame());
        }

        private void OnLeftClick(int x, int y)
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

                if (overlay) overlay.SetActive(true);
                if (losePanel) losePanel.SetActive(false);
                if (winPanel) winPanel.SetActive(false);
                if (customPanel) customPanel.SetActive(false);
                if (helpPanel) helpPanel.SetActive(false);

                if (_loseRoutine != null) StopCoroutine(_loseRoutine);
                _loseRoutine = StartCoroutine(LoseSequence(x, y, _runId));
                return;
            }



            RenderAll();

            if (_board.CheckWin())
            {
                _state = GameState.Won;

                int seconds = Mathf.Clamp(Mathf.FloorToInt(_time), 0, 9999);

                // Lưu best trước
                HighScoreStore.TrySetBest(BestKey(), seconds);

                // Cập nhật HUD bên trái
                UpdateBestText();

                // BƠM THÔNG TIN CHO WIN PANEL
                // BƠM THÔNG TIN CHO WIN PANEL
                if (winInfo)
                {
                    int best = HighScoreStore.GetBest(BestKey());
                    string bestStr = best < 0 ? "--" : FormatTime(best);
                    winInfo.text = $"Time: {FormatTime(seconds)}  |  Best: {bestStr}";
                }


                // ÂM THANH VICTORY (SFX)
                if (audio) audio.PlayVictory();

                ShowWin();
            }

        }

        private void OnRightClick(int x, int y)
        {
            if (_state == GameState.Won || _state == GameState.Lost) return;
            if (_board == null) return;

            if (_firstClick)
            {
                // allow flag before first click, still OK
                _state = GameState.Ready;
            }

            if (_board.ToggleFlag(x, y))
            {
                UpdateMinesLeftText();
                RenderCell(x, y);
            }
        }

        private void RenderAll()
        {
            for (int y = 0; y < _h; y++)
            for (int x = 0; x < _w; x++)
                RenderCell(x, y);
        }

        private void RenderCell(int x, int y)
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
        private void FitGridToBoardRoot()
        {
            if (!boardRoot || !grid) return;

            // Lấy kích thước vùng hiển thị thật của BoardRoot
            float availW = boardRoot.rect.width - grid.padding.left - grid.padding.right;
            float availH = boardRoot.rect.height - grid.padding.top - grid.padding.bottom;

            // spacing hiện tại
            float sx = grid.spacing.x;
            float sy = grid.spacing.y;

            // Tính cell size tối đa để vừa cả width lẫn height
            float cellW = (availW - sx * (_w - 1)) / _w;
            float cellH = (availH - sy * (_h - 1)) / _h;

            float cell = Mathf.Floor(Mathf.Min(cellW, cellH));

            // Giới hạn để không quá nhỏ / quá to
            cell = Mathf.Clamp(cell, 24f, 96f);

            grid.cellSize = new Vector2(cell, cell);
        }
        private void OnRectTransformDimensionsChange()
        {
            if (_board == null) return;
            if (!boardRoot || !grid) return;
            FitGridToBoardRoot();
        }
        private System.Collections.IEnumerator FitNextFrame()
        {
            yield return null; // đợi 1 frame để layout tính xong rect
            FitGridToBoardRoot();
        }
        private void ShowHelp()
        {
            if (overlay) overlay.SetActive(true);

            // tắt các panel khác nếu muốn (đỡ chồng)
            if (winPanel) winPanel.SetActive(false);
            if (losePanel) losePanel.SetActive(false);
            if (customPanel) customPanel.SetActive(false);

            if (helpPanel) helpPanel.SetActive(true);
        }

        private void CloseHelp()
        {
            if (helpPanel) helpPanel.SetActive(false);
            // nếu không còn modal nào khác thì tắt overlay
            if (overlay) overlay.SetActive(false);
        }

        private void OpenChatFromHelp()
        {
            // muốn giữ HelpPanel đang mở hay tắt đi tuỳ anh:
            // Option A: tắt HelpPanel khi mở chat
            if (helpPanel) helpPanel.SetActive(false);

            // đảm bảo overlay đang bật (chat có dim riêng cũng được)
            if (overlay) overlay.SetActive(true);

            if (chatUI) chatUI.Open();
        }

        private Coroutine _loseRoutine;
        private int _runId; // tăng mỗi lần NewGame để hủy các animation cũ

        private System.Collections.IEnumerator LoseSequence(int hitX, int hitY, int runId)
        {
            // gom mine + sort gần -> xa
            var mines = new System.Collections.Generic.List<(int x, int y, int d2)>();
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

            // nổ lan
            const float stepDelay = 0.06f;
            for (int i = 0; i < mines.Count; i++)
            {
                if (runId != _runId) yield break;

                var (mx, my, _) = mines[i];

                // KHÔNG cần RevealMineOnly để khỏi sửa Core
                // Chỉ show UI mine (anh đã có sprite bomb đẹp rồi)
                _tiles[mx, my].ShowMine();

                if (audio) audio.PlayBoom();
                yield return new WaitForSeconds(stepDelay);
            }

            // đợi 1.5s rồi mới hiện LosePanel
            float afterDelay = 1.5f;
            float t = 0f;
            while (t < afterDelay)
            {
                if (runId != _runId) yield break;
                t += Time.deltaTime;
                yield return null;
            }
            if (runId != _runId) yield break;

            if (audio) audio.PlayLose();
            ShowLose();

            _loseRoutine = null;
        }


    }
}
