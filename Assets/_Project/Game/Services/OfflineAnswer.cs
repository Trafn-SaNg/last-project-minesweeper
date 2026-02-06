using System;
using System.Linq;
using Minesweeper.Chatbot;
using UnityEngine;

namespace Minesweeper.Game.Services
{
    public static class OfflineAnswer
    {
        public static bool TryAnswer(string userText, FaqAsset faq, float minScore, out string answer)
        {
            answer = null;
            if (faq == null || string.IsNullOrWhiteSpace(userText)) return false;

            string q = Normalize(userText);

            float best = 0f;
            string bestA = null;

            foreach (var e in faq.entries)
            {
                if (e == null) continue;

                float score = Score(q, Normalize(e.question), e.keywords);
                float need = Mathf.Max(minScore, e.minScore);

                if (score >= need && score > best)
                {
                    best = score;
                    bestA = e.answer;
                }
            }

            if (bestA == null) return false;
            answer = bestA;
            return true;
        }

        static float Score(string user, string faqQ, System.Collections.Generic.List<string> keywords)
        {
            // 1) keyword match
            float kw = 0f;
            if (keywords != null && keywords.Count > 0)
            {
                int hit = 0;
                foreach (var k in keywords)
                {
                    var nk = Normalize(k);
                    if (string.IsNullOrEmpty(nk)) continue;
                    if (user.Contains(nk)) hit++;
                }
                kw = (float)hit / keywords.Count; // 0..1
            }

            // 2) token overlap (Jaccard)
            var ut = user.Split(' ', StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();
            var ft = faqQ.Split(' ', StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();
            int inter = ut.Intersect(ft).Count();
            int uni = ut.Union(ft).Count();
            float jac = uni == 0 ? 0 : (float)inter / uni;

            // 3) contains bonus
            float contains = (faqQ.Length > 0 && user.Contains(faqQ)) ? 0.25f : 0f;

            // tổng hợp
            return Mathf.Clamp01(jac * 0.6f + kw * 0.35f + contains);
        }

        static string Normalize(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            s = s.ToLowerInvariant();
            // bỏ ký tự đặc biệt cơ bản
            foreach (char c in ",.!?;:/\\()[]{}\"'`~@#$%^&*-_+=<>|")
                s = s.Replace(c.ToString(), " ");
            // gom khoảng trắng
            return string.Join(" ", s.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
