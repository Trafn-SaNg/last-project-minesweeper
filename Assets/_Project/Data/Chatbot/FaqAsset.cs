using System;
using System.Collections.Generic;
using UnityEngine;

namespace Minesweeper.Chatbot
{
    [Serializable]
    public class FaqEntry
    {
        [TextArea(1, 3)] public string question;
        [TextArea(2, 6)] public string answer;
        [Range(0f, 1f)] public float minScore = 0.55f;
        public List<string> keywords = new();
    }

    [CreateAssetMenu(menuName = "Minesweeper/Chatbot/FAQ", fileName = "FAQ")]
    public class FaqAsset : ScriptableObject
    {
        public List<FaqEntry> entries = new();
    }
}
