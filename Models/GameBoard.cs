using System;
using System.Collections.Generic;
using System.Linq;

namespace DomModel.Models
{
    /// <summary>
    /// Класс, представляющий игровое поле
    /// </summary>
    public class GameBoard
    {
        public const int Width = 10;
        public const int Height = 20;

        private bool[,] _grid;

        public GameBoard()
        {
            _grid = new bool[Height, Width];
            Clear();
        }

        /// <summary>
        /// Проверить, заполнена ли ячейка
        /// </summary>
        public bool IsOccupied(int x, int y)
        {
            if (!IsValidPosition(x, y))
                return true; // Границы считаются занятыми

            return _grid[y, x];
        }

        /// <summary>
        /// Установить ячейку как занятую
        /// </summary>
        public void SetBlock(int x, int y)
        {
            if (IsValidPosition(x, y))
                _grid[y, x] = true;
        }

        /// <summary>
        /// Проверить, может ли фигура занять позицию
        /// </summary>
        public bool CanPlacePiece(Tetromino piece)
        {
            return piece.GetAbsoluteBlocks().All(block => 
                IsValidPosition(block.X, block.Y) && !IsOccupied(block.X, block.Y));
        }

        /// <summary>
        /// Разместить фигуру на поле
        /// </summary>
        public void PlacePiece(Tetromino piece)
        {
            foreach (var block in piece.GetAbsoluteBlocks())
            {
                SetBlock(block.X, block.Y);
            }
        }

        /// <summary>
        /// Проверить и удалить заполненные строки
        /// </summary>
        public List<int> ClearCompletedLines()
        {
            var completedLines = new List<int>();

            for (int y = 0; y < Height; y++)
            {
                bool isComplete = true;
                for (int x = 0; x < Width; x++)
                {
                    if (!_grid[y, x])
                    {
                        isComplete = false;
                        break;
                    }
                }

                if (isComplete)
                    completedLines.Add(y);
            }

            if (completedLines.Count > 0)
            {
                foreach (var lineIndex in completedLines.OrderByDescending(x => x))
                {
                    RemoveLine(lineIndex);
                }
            }

            return completedLines;
        }

        /// <summary>
        /// Проверить, заполнено ли поле (игра окончена)
        /// </summary>
        public bool IsGameOver()
        {
            for (int x = 0; x < Width; x++)
            {
                if (_grid[0, x])
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Очистить поле
        /// </summary>
        public void Clear()
        {
            _grid = new bool[Height, Width];
        }

        /// <summary>
        /// Получить копию сетки для отображения
        /// </summary>
        public bool[,] GetGridCopy()
        {
            return (bool[,])_grid.Clone();
        }

        private void RemoveLine(int lineIndex)
        {
            for (int y = lineIndex; y > 0; y--)
            {
                for (int x = 0; x < Width; x++)
                {
                    _grid[y, x] = _grid[y - 1, x];
                }
            }

            for (int x = 0; x < Width; x++)
            {
                _grid[0, x] = false;
            }
        }

        private bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }
    }
}
