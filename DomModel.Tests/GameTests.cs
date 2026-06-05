using DomModel.Models;
using Xunit;

namespace DomModel.Tests
{
    public class GameTests
    {
        // Тест 1: Point.Equals возвращает true для одинаковых координат
        [Fact]
        public void Point_Equals_SameCoordinates_ReturnsTrue()
        {
            var p1 = new Point(3, 5);
            var p2 = new Point(3, 5);

            Assert.Equal(p1, p2);
            Assert.True(p1 == p2);
        }

        // Тест 2: Score.AddDropPoints добавляет height * 10 очков
        [Fact]
        public void Score_AddDropPoints_AddsCorrectAmount()
        {
            var score = new Score();

            score.AddDropPoints(7);

            Assert.Equal(70, score.CurrentScore);
        }

        // Тест 3: Score.AddLinePoints за 1 линию даёт 100 * Level очков
        [Fact]
        public void Score_AddLinePoints_SingleLine_Adds100PointsAtLevel1()
        {
            var score = new Score();

            score.AddLinePoints(1);

            Assert.Equal(100, score.CurrentScore);
            Assert.Equal(1, score.LinesCleared);
        }

        // Тест 4: GameBoard.IsOccupied возвращает true для позиции за границами поля
        [Fact]
        public void GameBoard_IsOccupied_OutOfBounds_ReturnsTrue()
        {
            var board = new GameBoard();

            bool occupiedLeft  = board.IsOccupied(-1, 0);
            bool occupiedRight = board.IsOccupied(GameBoard.Width, 0);
            bool occupiedBelow = board.IsOccupied(0, GameBoard.Height);

            Assert.True(occupiedLeft);
            Assert.True(occupiedRight);
            Assert.True(occupiedBelow);
        }

        // Тест 5: Tetromino.Move сдвигает позицию фигуры на заданный вектор
        [Fact]
        public void Tetromino_Move_UpdatesPosition()
        {
            var tetromino = new Tetromino(TetrominoType.O, new Point(4, 0));

            tetromino.Move(2, 3);

            Assert.Equal(new Point(6, 3), tetromino.Position);
        }
    }
}
