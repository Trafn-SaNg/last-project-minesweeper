using UnityEngine;

namespace Minesweeper.Chatbot
{
    [CreateAssetMenu(menuName = "Minesweeper/Chatbot/Config", fileName = "ChatbotConfig")]
    public class ChatbotConfig : ScriptableObject
    {
        [Header("Server")]
        public string serverUrl = "http://localhost:8787/chat";
        public int timeoutSeconds = 20;

        [Header("Hybrid")]
        [Range(0f, 1f)] public float offlineMinScore = 0.60f;

        [Header("Prompt")]
        [TextArea(3, 8)]
        public string systemHint =
            "You are a helpful assistant for a Minesweeper game. Answer briefly and clearly.";
    }
}
