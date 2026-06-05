using System;

namespace DomModel.Models
{
    /// <summary>
    /// Класс для сохранения прогресса игры
    /// </summary>
    public class GameProgress
    {
        public int CurrentScore { get; set; }
        public int LinesCleared { get; set; }
        public int Level { get; set; }
        public DateTime SaveTime { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public bool[,] BoardState { get; set; }

        public GameProgress()
        {
            SaveTime = DateTime.Now;
            BoardState = new bool[GameBoard.Height, GameBoard.Width];
        }

        public override string ToString()
        {
            return $"{PlayerName};{CurrentScore};{LinesCleared};{Level};{SaveTime:yyyy-MM-dd HH:mm:ss}";
        }
    }
}
