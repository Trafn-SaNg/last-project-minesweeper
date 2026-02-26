using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Minesweeper.Chatbot; // để dùng FaqAsset

public sealed class ChatPanelController : MonoBehaviour
{
    [Header("Root")]
    public GameObject chatPanel;
    public GameObject helpPanel;   // kéo HelpPanel vào đây (Canvas/Overlay/HelpPanel)

    [Header("UI")]
    public TMP_InputField inputField;
    public Transform contentRoot;
    public TMP_Text messageTextPrefab;

    [Header("Buttons")]
    public Button dimButton;
    public Button closeButton;
    public Button sendButton;

    [Header("FAQ")]
    public FaqAsset faqAsset;

    void Awake()
    {
        // Nếu anh không muốn hook bằng Inspector, vẫn có hook auto ở đây
        if (dimButton) dimButton.onClick.AddListener(Close);
        if (closeButton) closeButton.onClick.AddListener(Close);
        if (sendButton) sendButton.onClick.AddListener(Send);

        if (inputField)
            inputField.onSubmit.AddListener(_ => Send());
    }

    public void Open()
    {
        chatPanel.SetActive(true);
        if (helpPanel) helpPanel.SetActive(false);
        inputField?.ActivateInputField();
    }

    public void Close()
    {
        chatPanel.SetActive(false);
        if (helpPanel) helpPanel.SetActive(true);
    }

    // Để Button OnClick gọi thẳng được
    public void Send()
    {
        if (!chatPanel || !chatPanel.activeSelf) return;
        if (!inputField) return;

        var q = (inputField.text ?? "").Trim();
        if (q.Length == 0) return;

        Append("You: " + q);
        Append("Trafn SaNg: " + AnswerFromFaq(q));

        inputField.text = "";
        inputField.ActivateInputField();
    }

    string AnswerFromFaq(string userText)
    {
        if (!faqAsset || faqAsset.entries == null || faqAsset.entries.Count == 0)
            return "(FAQ trống) Anh chưa gắn FAQ.asset vào controller.";

        var t = userText.ToLowerInvariant();

        // Match theo keywords trước (dễ demo nhất)
        foreach (var e in faqAsset.entries)
        {
            if (e == null) continue;

            if (e.keywords != null)
            {
                foreach (var kw in e.keywords)
                {
                    if (string.IsNullOrWhiteSpace(kw)) continue;
                    if (t.Contains(kw.Trim().ToLowerInvariant()))
                        return e.answer;
                }
            }

            // fallback: contains question
            if (!string.IsNullOrWhiteSpace(e.question) &&
                t.Contains(e.question.Trim().ToLowerInvariant()))
                return e.answer;
        }

        return "Trafn SaNg chưa tìm thấy trong FAQ. Bạn thử hỏi bằng từ khóa như: \"mở ô\", \"cờ\", \"thắng\", \"số\".";
    }

    void Append(string msg)
    {
        if (!contentRoot || !messageTextPrefab) return;
        var item = Instantiate(messageTextPrefab, contentRoot);
        item.text = msg;
    }
}