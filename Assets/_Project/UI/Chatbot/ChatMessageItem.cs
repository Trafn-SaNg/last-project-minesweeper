using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Minesweeper.Chatbot
{
    public sealed class ChatMessageItem : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private RectTransform bubbleRect;
        [SerializeField] private Image bubbleBg;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private HorizontalLayoutGroup row;
        [SerializeField] private LayoutElement bubbleLayout;

        [Header("Layout")]
        [SerializeField] private float maxBubbleWidth = 520f;
        [SerializeField] private float bubblePadding = 40f; // padding tổng (ước lượng)

        public void Set(string text, bool isUser)
        {
            if (messageText) messageText.text = text;

            // căn trái/phải
            if (row)
            {
                row.childAlignment = isUser ? TextAnchor.UpperRight : TextAnchor.UpperLeft;
                row.padding.left = isUser ? 80 : 0;
                row.padding.right = isUser ? 0 : 80;
            }

            // màu bong bóng (tuỳ anh đổi)
            if (bubbleBg)
                bubbleBg.color = isUser ? new Color(0.90f, 0.97f, 1f, 1f) : new Color(0.95f, 0.95f, 0.95f, 1f);

            // giới hạn width theo nội dung
            if (bubbleLayout && messageText)
            {
                Canvas.ForceUpdateCanvases();
                float preferred = messageText.preferredWidth + bubblePadding;
                bubbleLayout.preferredWidth = Mathf.Min(maxBubbleWidth, preferred);
            }
        }
    }
}
