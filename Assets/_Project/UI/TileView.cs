using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Minesweeper.UI
{
    public sealed class TileView : MonoBehaviour, IPointerClickHandler
    {
        [Header("Refs")]
        [SerializeField] private Image bg;                 // nền ô (Image ở root Tile)
        [SerializeField] private TMP_Text numberText;      // số
        [SerializeField] private TMP_Text iconText;        // icon dạng chữ (⚑, 💣)
        [SerializeField] private Image iconImage;          // icon dạng ảnh (optional)

        [Header("Sprites")]
        [SerializeField] private Sprite hiddenSprite;      // ô chưa mở
        [SerializeField] private Sprite revealedSprite;    // ô đã mở
        [SerializeField] private Sprite flagSprite;        // (optional) icon ảnh cờ
        [SerializeField] private Sprite mineSprite;        // (optional) icon ảnh bom

        [Header("Icon chars (if not using iconImage)")]
        [SerializeField] private string flagChar = "⚑";
        [SerializeField] private string mineChar = "💣";

        private int _x, _y;
        private Action<int, int> _onLeft;
        private Action<int, int> _onRight;

        private void Awake()
        {
            if (!bg) bg = GetComponent<Image>();

            // để text/icon không chặn click vào tile
            if (numberText) numberText.raycastTarget = false;
            if (iconText) iconText.raycastTarget = false;
            if (iconImage) iconImage.raycastTarget = false;

            // tránh tint làm sprite bị đổi màu (đặc biệt khi anh dùng ảnh trắng)
            if (bg) bg.color = Color.white;
        }

        public void Init(int x, int y, Action<int, int> onLeftClick, Action<int, int> onRightClick)
        {
            _x = x;
            _y = y;
            _onLeft = onLeftClick;
            _onRight = onRightClick;

            ShowHidden();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData == null) return;

            if (eventData.button == PointerEventData.InputButton.Left)
                _onLeft?.Invoke(_x, _y);
            else if (eventData.button == PointerEventData.InputButton.Right)
                _onRight?.Invoke(_x, _y);
        }

        // ---------------- Render ----------------

        public void ShowHidden()
        {
            SetBg(hiddenSprite);
            SetNumber("");
            SetIconNone();
        }

        public void ShowFlag()
        {
            SetBg(hiddenSprite);
            SetNumber("");

            // Ưu tiên icon ảnh nếu có sprite
            if (iconImage && flagSprite)
            {
                iconImage.enabled = true;
                iconImage.sprite = flagSprite;
                if (iconText) iconText.text = "";
            }
            else
            {
                if (iconText) iconText.text = flagChar;
                if (iconImage) iconImage.enabled = false;
            }
        }

        public void ShowMine()
        {
            SetBg(revealedSprite);
            SetNumber("");

            if (iconImage && mineSprite)
            {
                iconImage.enabled = true;
                iconImage.sprite = mineSprite;
                if (iconText) iconText.text = "";
            }
            else
            {
                if (iconText) iconText.text = mineChar;
                if (iconImage) iconImage.enabled = false;
            }
        }

        public void ShowNumber(int adjacentMines)
        {
            SetBg(revealedSprite);
            SetIconNone();

            if (!numberText) return;

            if (adjacentMines <= 0)
            {
                numberText.text = "";
            }
            else
            {
                numberText.text = adjacentMines.ToString();
                numberText.color = NumberColor(adjacentMines); // <<< MÀU SỐ 1..8
                // đảm bảo không bị alpha = 0
                var c = numberText.color; c.a = 1f; numberText.color = c;
            }
        }

        // ---------------- Helpers ----------------

        private void SetBg(Sprite sprite)
        {
            if (!bg) return;
            if (sprite) bg.sprite = sprite;

            // rất quan trọng: nếu bg.color không trắng => sprite trắng sẽ bị “đổi màu”
            bg.color = Color.white;
        }

        private void SetNumber(string s)
        {
            if (!numberText) return;
            numberText.text = s;

            // nếu là rỗng thì không cần đổi màu
            var c = numberText.color; c.a = 1f; numberText.color = c;
        }

        private void SetIconNone()
        {
            if (iconText) iconText.text = "";
            if (iconImage) iconImage.enabled = false;
        }

        // ---------------- Number Colors (Minesweeper-like) ----------------

        private static Color NumberColor(int n)
        {
            // Chuẩn màu phổ biến Minesweeper
            return n switch
            {
                1 => Hex("#1976D2"), // Blue
                2 => Hex("#2E7D32"), // Green
                3 => Hex("#D32F2F"), // Red
                4 => Hex("#0D47A1"), // Navy
                5 => Hex("#7B1F1F"), // Maroon
                6 => Hex("#00838F"), // Teal
                7 => Hex("#212121"), // Black
                8 => Hex("#616161"), // Gray
                _ => Hex("#000000")
            };
        }

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }
    }
}
