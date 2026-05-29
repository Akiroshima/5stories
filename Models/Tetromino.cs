using System;
using System.Collections.Generic;
using System.Linq;

namespace DomModel.Models
{
    /// <summary>
    /// Перечисление типов тетромино
    /// </summary>
    public enum TetrominoType
    {
        I, O, T, S, Z, J, L
    }

    /// <summary>
    /// Класс, представляющий падающую фигуру Тетромино
    /// </summary>
    public class Tetromino
    {
        public TetrominoType Type { get; private set; }
        public Point Position { get; set; }
        public int Rotation { get; set; }
        public List<Point> Blocks { get; set; }

        private readonly Dictionary<TetrominoType, List<List<Point>>> _shapePatterns;

        public Tetromino(TetrominoType type, Point startPosition)
        {
            Type = type;
            Position = startPosition;
            Rotation = 0;
            _shapePatterns = InitializeShapePatterns();
            Blocks = GetBlocksForRotation(0);
        }

        /// <summary>
        /// Получить блоки фигуры с учётом текущей позиции и ротации
        /// </summary>
        public List<Point> GetAbsoluteBlocks()
        {
            return Blocks.Select(block => new Point(
                Position.X + block.X,
                Position.Y + block.Y
            )).ToList();
        }

        /// <summary>
        /// Повернуть фигуру на 90 градусов по часовой стрелке
        /// </summary>
        public void Rotate()
        {
            var nextRotation = (Rotation + 1) % 4;
            Blocks = GetBlocksForRotation(nextRotation);
            Rotation = nextRotation;
        }

        /// <summary>
        /// Переместить фигуру
        /// </summary>
        public void Move(int deltaX, int deltaY)
        {
            Position = new Point(Position.X + deltaX, Position.Y + deltaY);
        }

        private List<Point> GetBlocksForRotation(int rotation)
        {
            return new List<Point>(_shapePatterns[Type][rotation % 4]);
        }

        private Dictionary<TetrominoType, List<List<Point>>> InitializeShapePatterns()
        {
            return new Dictionary<TetrominoType, List<List<Point>>>
            {
                // I-тетромино
                {
                    TetrominoType.I, new List<List<Point>>
                    {
                        new List<Point> { new Point(0, 1), new Point(1, 1), new Point(2, 1), new Point(3, 1) },
                        new List<Point> { new Point(1, 0), new Point(1, 1), new Point(1, 2), new Point(1, 3) },
                        new List<Point> { new Point(0, 1), new Point(1, 1), new Point(2, 1), new Point(3, 1) },
                        new List<Point> { new Point(1, 0), new Point(1, 1), new Point(1, 2), new Point(1, 3) }
                    }
                },
                // O-тетромино (квадрат)
                {
                    TetrominoType.O, new List<List<Point>>
                    {
                        new List<Point> { new Point(0, 0), new Point(1, 0), new Point(0, 1), new Point(1, 1) },
                        new List<Point> { new Point(0, 0), new Point(1, 0), new Point(0, 1), new Point(1, 1) },
                        new List<Point> { new Point(0, 0), new Point(1, 0), new Point(0, 1), new Point(1, 1) },
                        new List<Point> { new Point(0, 0), new Point(1, 0), new Point(0, 1), new Point(1, 1) }
                    }
                },
                // T-тетромино
                {
                    TetrominoType.T, new List<List<Point>>
                    {
                        new List<Point> { new Point(1, 0), new Point(0, 1), new Point(1, 1), new Point(2, 1) },
                        new List<Point> { new Point(1, 0), new Point(1, 1), new Point(2, 1), new Point(1, 2) },
                        new List<Point> { new Point(0, 1), new Point(1, 1), new Point(2, 1), new Point(1, 2) },
                        new List<Point> { new Point(1, 0), new Point(0, 1), new Point(1, 1), new Point(1, 2) }
                    }
                },
                // S-тетромино
                {
                    TetrominoType.S, new List<List<Point>>
                    {
                        new List<Point> { new Point(1, 0), new Point(2, 0), new Point(0, 1), new Point(1, 1) },
                        new List<Point> { new Point(1, 0), new Point(1, 1), new Point(2, 1), new Point(2, 2) },
                        new List<Point> { new Point(1, 0), new Point(2, 0), new Point(0, 1), new Point(1, 1) },
                        new List<Point> { new Point(1, 0), new Point(1, 1), new Point(2, 1), new Point(2, 2) }
                    }
                },
                // Z-тетромино
                {
                    TetrominoType.Z, new List<List<Point>>
                    {
                        new List<Point> { new Point(0, 0), new Point(1, 0), new Point(1, 1), new Point(2, 1) },
                        new List<Point> { new Point(2, 0), new Point(1, 1), new Point(2, 1), new Point(1, 2) },
                        new List<Point> { new Point(0, 0), new Point(1, 0), new Point(1, 1), new Point(2, 1) },
                        new List<Point> { new Point(2, 0), new Point(1, 1), new Point(2, 1), new Point(1, 2) }
                    }
                },
                // J-тетромино
                {
                    TetrominoType.J, new List<List<Point>>
                    {
                        new List<Point> { new Point(0, 0), new Point(0, 1), new Point(1, 1), new Point(2, 1) },
                        new List<Point> { new Point(1, 0), new Point(2, 0), new Point(1, 1), new Point(1, 2) },
                        new List<Point> { new Point(0, 1), new Point(1, 1), new Point(2, 1), new Point(2, 0) },
                        new List<Point> { new Point(1, 0), new Point(1, 1), new Point(0, 2), new Point(1, 2) }
                    }
                },
                // L-тетромино
                {
                    TetrominoType.L, new List<List<Point>>
                    {
                        new List<Point> { new Point(2, 0), new Point(0, 1), new Point(1, 1), new Point(2, 1) },
                        new List<Point> { new Point(1, 0), new Point(1, 1), new Point(1, 2), new Point(2, 2) },
                        new List<Point> { new Point(0, 1), new Point(1, 1), new Point(2, 1), new Point(0, 0) },
                        new List<Point> { new Point(0, 0), new Point(1, 0), new Point(1, 1), new Point(1, 2) }
                    }
                }
            };
        }
    }
}
