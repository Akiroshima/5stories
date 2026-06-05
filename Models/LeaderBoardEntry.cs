using System;

namespace DomModel.Models
{
    /// <summary>
    /// Запись в таблице рекордов
    /// </summary>
    public class LeaderBoardEntry : IComparable<LeaderBoardEntry>
    {
        public string PlayerName { get; set; }
        public int Score { get; set; }
        public int LinesCleared { get; set; }
        public int Level { get; set; }
        public DateTime AchievedDate { get; set; }

        public LeaderBoardEntry()
        {
            AchievedDate = DateTime.Now;
        }

        public LeaderBoardEntry(string playerName, int score, int linesCleared, int level, DateTime achievedDate)
        {
            PlayerName = playerName;
            Score = score;
            LinesCleared = linesCleared;
            Level = level;
            AchievedDate = achievedDate;
        }

        public int CompareTo(LeaderBoardEntry other)
        {
            if (other == null)
                return 1;

            // Сортировка по очкам в убывающем порядке
            return other.Score.CompareTo(this.Score);
        }

        public override string ToString()
        {
            return $"{PlayerName};{Score};{LinesCleared};{Level};{AchievedDate:yyyy-MM-dd HH:mm:ss}";
        }
    }
}
