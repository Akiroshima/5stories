namespace DomModel.Models
{
    /// <summary>
    /// Класс для управления очками в игре
    /// </summary>
    public class Score
    {
        private int _currentScore;
        private int _linesCleared;

        public int CurrentScore
        {
            get => _currentScore;
            set => _currentScore = value >= 0 ? value : 0;
        }

        public int LinesCleared
        {
            get => _linesCleared;
            set => _linesCleared = value >= 0 ? value : 0;
        }

        public int Level => (LinesCleared / 10) + 1;

        public Score()
        {
            Reset();
        }

        /// <summary>
        /// Добавить очки за установленную фигуру
        /// </summary>
        public void AddDropPoints(int height)
        {
            CurrentScore += height * 10;
        }

        /// <summary>
        /// Добавить очки за очищенные строки
        /// </summary>
        public void AddLinePoints(int linesClearedCount)
        {
            LinesCleared += linesClearedCount;

            int points = linesClearedCount switch
            {
                1 => 100,
                2 => 300,
                3 => 500,
                4 => 800,
                _ => 0
            };

            CurrentScore += points * Level;
        }

        /// <summary>
        /// Сбросить счёт
        /// </summary>
        public void Reset()
        {
            CurrentScore = 0;
            LinesCleared = 0;
        }
    }
}
