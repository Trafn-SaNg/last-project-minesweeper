using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Minesweeper.UI
{
    public sealed class StatisticsPanelUI : MonoBehaviour
    {
        [Header("Panel")]
        public GameObject statisticsPanel;
        public Button dimButton;
        public Button closeButton;

        [Header("Tabs")]
        public Button overallTabButton;
        public Button classicTabButton;
        public Button challengeTabButton;
        public Button historyTabButton;

        [Header("Sections")]
        public GameObject sectionOverall;
        public GameObject sectionClassic;
        public GameObject sectionChallenge;
        public GameObject sectionHistory;

        [Header("Text Outputs (Body)")]
        public TMP_Text overallText;
        public TMP_Text classicText;
        public TMP_Text challengeText;
        public TMP_Text historyText;

        void Awake()
        {
            if (dimButton) dimButton.onClick.AddListener(Close);
            if (closeButton) closeButton.onClick.AddListener(Close);

            if (overallTabButton) overallTabButton.onClick.AddListener(() => ShowTab(0));
            if (classicTabButton) classicTabButton.onClick.AddListener(() => ShowTab(1));
            if (challengeTabButton) challengeTabButton.onClick.AddListener(() => ShowTab(2));
            if (historyTabButton) historyTabButton.onClick.AddListener(() => ShowTab(3));
        }

        public void Open()
        {
            if (!statisticsPanel) return;
            statisticsPanel.SetActive(true);
            statisticsPanel.transform.SetAsLastSibling();

            RefreshAll();
            ShowTab(0); // default Overall
        }

        public void Close()
        {
            if (!statisticsPanel) return;
            statisticsPanel.SetActive(false);
        }

        void ShowTab(int tab)
        {
            if (sectionOverall) sectionOverall.SetActive(tab == 0);
            if (sectionClassic) sectionClassic.SetActive(tab == 1);
            if (sectionChallenge) sectionChallenge.SetActive(tab == 2);
            if (sectionHistory) sectionHistory.SetActive(tab == 3);
        }

        void RefreshAll()
        {
            RefreshOverall();
            RefreshClassic();
            RefreshChallenge();
            RefreshHistory();
        }

        void RefreshOverall()
        {
            if (!overallText) return;

            int total = StatsStore.TotalGames;
            int wins = StatsStore.Wins;
            int losses = StatsStore.Losses;
            float rate = total > 0 ? (wins * 100f / total) : 0f;

            var sb = new StringBuilder();
            sb.AppendLine($"Total Games: {total}");
            sb.AppendLine($"Wins: {wins}");
            sb.AppendLine($"Losses: {losses}");
            sb.AppendLine($"Win Rate: {rate:0.0}%");
            sb.AppendLine($"Total Play Time: {StatsStore.FormatHms(StatsStore.TotalPlaySeconds)}");
            sb.AppendLine($"Classic Wins: {StatsStore.ClassicWins}");
            sb.AppendLine($"Challenge Wins: {StatsStore.ChallengeWins}");

            overallText.text = sb.ToString();
        }

        void RefreshClassic()
        {
            if (!classicText) return;

            // Nếu anh muốn “Best theo difficulty”, hiện tại BestKey của anh phụ thuộc _difficultyKey + w/h/m.
            // Ở đây em hiển thị đơn giản theo preset phổ biến.
            // Nếu anh muốn đúng tuyệt đối theo key của anh, em sẽ map theo đúng BestKey.
            int bestEasy = HighScoreStore.GetBest("Easy_9x9_10");
            int bestMed = HighScoreStore.GetBest("Medium_16x16_40");
            int bestHard = HighScoreStore.GetBest("Hard_30x16_99");

            string fmt(int b) => b < 0 ? "--" : GameController.FormatTime(b);

            classicText.text =
                $"Best Easy (9x9/10): {fmt(bestEasy)}\n" +
                $"Best Medium (16x16/40): {fmt(bestMed)}\n" +
                $"Best Hard (30x16/99): {fmt(bestHard)}\n\n" +
                $"Custom Best";
        }

        void RefreshChallenge()
        {
            if (!challengeText) return;

            int unlocked = GameSession.I != null ? GameSession.I.HighestUnlockedLevel : 1;

            var sb = new StringBuilder();
            sb.AppendLine($"Highest Unlocked Level: {unlocked}/{ChallengeLevels.MaxLevel}");
            sb.AppendLine($"Current Level: {(GameSession.I != null ? GameSession.I.CurrentLevelId : 1)}");
            sb.AppendLine();
            sb.AppendLine("Levels:");

            // Hiển thị config level để người xem thấy rõ demo
            for (int lv = 1; lv <= ChallengeLevels.MaxLevel; lv++)
            {
                var cfg = ChallengeLevels.Get(lv);
                sb.AppendLine($"- Lv{lv}: {cfg.width}x{cfg.height}, mines {cfg.mines}");
            }

            challengeText.text = sb.ToString();
        }

        void RefreshHistory()
        {
            if (!historyText) return;

            var list = StatsStore.GetHistory();
            if (list.Count == 0)
            {
                historyText.text = "No history yet.";
                return;
            }

            var sb = new StringBuilder();
            foreach (var h in list)
            {
                string result = h.win ? "WIN" : "LOSE";
                sb.AppendLine($"{h.at} | {h.mode} {h.detail} | {result} | {GameController.FormatTime(h.seconds)}");
            }

            historyText.text = sb.ToString();
        }
    }
}