using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Minesweeper.Chatbot;
using Minesweeper.Game.Services;

namespace Minesweeper.UI
{
    public sealed class ChatUI : MonoBehaviour
    {
        [Header("Panels")]
        [Tooltip("Nếu để trống sẽ dùng chính GameObject đang gắn script (khuyến nghị gắn script lên ChatPanel).")]
        public GameObject chatPanel;
        [Tooltip("Tuỳ chọn: nếu anh mở chat từ HelpPanel và muốn ẩn HelpPanel khi chat mở.")]
        public GameObject helpPanel;

        [Header("Buttons")]
        [Tooltip("Kéo HelpButton (LeftPanel) hoặc ChatButton (Ask me?) vào đây để mở chat.")]
        public Button openFromHelpButton;
        public Button closeButton;
        [Tooltip("Optional: bấm nền Dim để đóng. (Dim có thể là Button hoặc có Button riêng DimButton)")]
        public Button dimButton;

        [Header("UI")]
        public TMP_InputField inputField;
        public Button sendButton;
        public ScrollRect scrollRect;
        public Transform contentRoot;

        [Header("Prefabs")]
        public ChatMessageItem messagePrefab;

        [Header("Data")]
        public FaqAsset faq;
        public ChatbotConfig config;

        private bool _waitingOnline;

        private void Awake()
        {
            // Nếu anh gắn script lên ChatPanel thì để auto
            if (!chatPanel) chatPanel = gameObject;

            if (openFromHelpButton) openFromHelpButton.onClick.AddListener(Open);
            if (sendButton) sendButton.onClick.AddListener(Send);
            if (closeButton) closeButton.onClick.AddListener(Close);
            if (dimButton) dimButton.onClick.AddListener(Close);

            if (inputField)
            {
                // TMP_InputField onSubmit sẽ bắn khi Enter (tuỳ setting)
                inputField.onSubmit.AddListener(_ => Send());
            }

            // Mặc định tắt chat khi vào scene
            if (chatPanel) chatPanel.SetActive(false);
        }

        public void Open()
        {
            _waitingOnline = false;

            if (helpPanel) helpPanel.SetActive(false);
            if (chatPanel) chatPanel.SetActive(true);

            // Focus input
            if (inputField)
            {
                inputField.text = "";
                inputField.ActivateInputField();
                inputField.Select();
            }

            // (Optional) nếu muốn mỗi lần mở chat có lời chào
            AddMessage("Chào anh! Em có thể giúp gì về Minesweeper?", isUser: false);
            ScrollToBottom();
        }

        public void Close()
        {
            _waitingOnline = false;
            if (chatPanel) chatPanel.SetActive(false);

            // Nếu anh muốn đóng chat quay lại HelpPanel:
            if (helpPanel) helpPanel.SetActive(true);
        }

        private void Send()
        {
            if (_waitingOnline) return;
            if (!inputField) return;

            string text = inputField.text.Trim();
            if (string.IsNullOrEmpty(text)) return;

            inputField.text = "";
            inputField.ActivateInputField();
            inputField.Select();

            AddMessage(text, isUser: true);

            // 1) OFFLINE trước (FAQ)
            float minScore = config ? config.offlineMinScore : 0.6f;
            if (OfflineAnswer.TryAnswer(text, faq, minScore, out var offlineAns))
            {
                AddMessage(offlineAns, isUser: false);
                return;
            }

            // 2) ONLINE fallback
            if (config == null || string.IsNullOrWhiteSpace(config.serverUrl))
            {
                AddMessage("Hiện chưa cấu hình AI online. Anh có thể thêm câu hỏi vào FAQ hoặc set serverUrl trong ChatbotConfig.", false);
                return;
            }

            StartCoroutine(AskOnline(text));
        }

        private IEnumerator AskOnline(string userText)
        {
            _waitingOnline = true;

            // typing message
            var typing = AddMessage("Đang trả lời...", isUser: false);

            string reply = null;
            yield return OnlineChatService.Ask(
                config.serverUrl,
                config.timeoutSeconds,
                config.systemHint,
                userText,
                r => reply = r
            );

            // remove typing
            if (typing) Destroy(typing.gameObject);

            if (string.IsNullOrWhiteSpace(reply))
                AddMessage("Xin lỗi, hiện em không kết nối được AI online. Anh thử lại sau hoặc hỏi theo cách khác nhé.", false);
            else
                AddMessage(reply, false);

            _waitingOnline = false;
            ScrollToBottom();
        }

        private ChatMessageItem AddMessage(string text, bool isUser)
        {
            if (!messagePrefab || !contentRoot) return null;

            var item = Instantiate(messagePrefab, contentRoot);
            item.Set(text, isUser);

            ScrollToBottom();
            return item;
        }

        private void ScrollToBottom()
        {
            // Đảm bảo layout tính xong trước khi set scroll
            Canvas.ForceUpdateCanvases();
            if (scrollRect) scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
