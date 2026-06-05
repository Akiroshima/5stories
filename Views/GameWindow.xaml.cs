using DomModel.Models;
using DomModel.Services;
using DomModel.ViewModels;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DomModel.Views
{
    public partial class GameWindow
    {
        private GameViewModel _viewModel;
        private const int BLOCK_SIZE = 20;
        private const int BOARD_COLS = 10;
        private const int BOARD_ROWS = 20;
        
        public GameWindow()
        {
            InitializeComponent();
            _viewModel = new GameViewModel();
            DataContext = _viewModel;

            // Подписаться на обновления игры
            this.PreviewKeyDown += GameWindow_KeyDown;

            // Таймер для отрисовки (60 FPS)
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(16);
            timer.Tick += (s, e) => RedrawGame();
            timer.Start();
        }

        /// <summary>
        /// Обработчик нажатия клавиши
        /// </summary>
        private void GameWindow_KeyDown(object sender, KeyEventArgs e)
        {
            string key = e.Key.ToString();
            
            switch (key)
            {
                case "Left":
                    _viewModel.MoveLeftCommand?.Execute(null);
                    e.Handled = true;
                    break;
                case "Right":
                    _viewModel.MoveRightCommand?.Execute(null);
                    e.Handled = true;
                    break;
                case "Down":
                    _viewModel.MoveDownCommand?.Execute(null);
                    e.Handled = true;
                    break;
                case "Space":
                    _viewModel.RotateCommand?.Execute(null);
                    e.Handled = true;
                    break;
                case "P":
                    if (_viewModel.IsGameRunning)
                    {
                        if (_viewModel.IsPaused)
                            _viewModel.ResumeGameCommand?.Execute(null);
                        else
                            _viewModel.PauseGameCommand?.Execute(null);
                    }
                    e.Handled = true;
                    break;
                case "Return":
                    if (!_viewModel.IsGameRunning)
                        _viewModel.StartGameCommand?.Execute(null);
                    e.Handled = true;
                    break;
            }
        }

        /// <summary>
        /// Перерисовать игровое поле
        /// </summary>
        private void RedrawGame()
        {
            try
            {
                GameCanvas.Children.Clear();

                if (_viewModel?.GridState == null)
                    return;

                // Нарисовать сетку игрового поля
                DrawGrid();

                // Нарисовать размещённые блоки
                DrawPlacedBlocks();

                // Нарисовать текущую падающую фигуру
                DrawCurrentPiece();

                // Нарисовать следующую фигуру (если нужно)
                DrawNextPiece();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Ошибка в отрисовке: {ex.Message}");
            }
        }

        /// <summary>
        /// Нарисовать сетку игрового поля
        /// </summary>
        private void DrawGrid()
        {
            var gridPen = new Pen(Brushes.Gray, 0.5);
            gridPen.Freeze();

            // Вертикальные линии
            for (int x = 0; x <= BOARD_COLS; x++)
            {
                var line = new System.Windows.Shapes.Line
                {
                    X1 = x * BLOCK_SIZE,
                    Y1 = 0,
                    X2 = x * BLOCK_SIZE,
                    Y2 = GameCanvas.Height,
                    Stroke = gridPen.Brush,
                    StrokeThickness = 0.5
                };
                GameCanvas.Children.Add(line);
            }

            // Горизонтальные линии
            for (int y = 0; y <= BOARD_ROWS; y++)
            {
                var line = new System.Windows.Shapes.Line
                {
                    X1 = 0,
                    Y1 = y * BLOCK_SIZE,
                    X2 = GameCanvas.Width,
                    Y2 = y * BLOCK_SIZE,
                    Stroke = gridPen.Brush,
                    StrokeThickness = 0.5
                };
                GameCanvas.Children.Add(line);
            }
        }

        /// <summary>
        /// Нарисовать размещённые блоки
        /// </summary>
        private void DrawPlacedBlocks()
        {
            if (_viewModel?.GridState == null)
                return;

            // GridState это bool[Height, Width], т.е. [rows, cols]
            // GetLength(0) = Height, GetLength(1) = Width
            for (int y = 0; y < BOARD_ROWS && y < _viewModel.GridState.GetLength(0); y++)
            {
                for (int x = 0; x < BOARD_COLS && x < _viewModel.GridState.GetLength(1); x++)
                {
                    if (_viewModel.GridState[y, x])  // ПРАВИЛЬНЫЙ порядок: [row, col]
                    {
                        DrawBlock(x, y, GetColorForBlock(x, y));
                    }
                }
            }
        }

        /// <summary>
        /// Нарисовать текущую падающую фигуру
        /// </summary>
        private void DrawCurrentPiece()
        {
            var game = _viewModel?.GetGame();
            if (game?.CurrentPiece == null)
                return;

            var piece = game.CurrentPiece;
            if (piece.Blocks == null || piece.Blocks.Count == 0)
                return;

            var color = GetTetrominoColor(piece.Type);

            foreach (var block in piece.Blocks)
            {
                int screenX = piece.Position.X + block.X;
                int screenY = piece.Position.Y + block.Y;

                if (screenX >= 0 && screenX < BOARD_COLS && screenY >= 0 && screenY < BOARD_ROWS)
                {
                    DrawBlock(screenX, screenY, color);
                }
            }
        }

        /// <summary>
        /// Нарисовать следующую фигуру
        /// </summary>
        private void DrawNextPiece()
        {
            // Это можно реализовать позже в отдельной панели
        }

        /// <summary>
        /// Нарисовать один блок
        /// </summary>
        private void DrawBlock(int x, int y, Brush color)
        {
            var rect = new System.Windows.Shapes.Rectangle
            {
                Width = BLOCK_SIZE - 1,
                Height = BLOCK_SIZE - 1,
                Fill = color,
                Stroke = Brushes.DarkGray,
                StrokeThickness = 1
            };

            Canvas.SetLeft(rect, x * BLOCK_SIZE);
            Canvas.SetTop(rect, y * BLOCK_SIZE);
            GameCanvas.Children.Add(rect);
        }

        /// <summary>
        /// Получить цвет для Tetromino типа
        /// </summary>
        private Brush GetTetrominoColor(TetrominoType type)
        {
            return type switch
            {
                TetrominoType.I => Brushes.Cyan,        // Голубой
                TetrominoType.O => Brushes.Yellow,      // Жёлтый
                TetrominoType.T => Brushes.Magenta,     // Фиолетовый
                TetrominoType.S => Brushes.LimeGreen,   // Зелёный
                TetrominoType.Z => Brushes.Red,         // Красный
                TetrominoType.J => Brushes.Blue,        // Синий
                TetrominoType.L => Brushes.Orange,      // Оранжевый
                _ => Brushes.White
            };
        }

        /// <summary>
        /// Получить цвет для блока на доске
        /// </summary>
        private Brush GetColorForBlock(int x, int y)
        {
            // Чередующиеся цвета для визуального эффекта
            return ((x + y) % 2 == 0) ? Brushes.DarkGray : Brushes.Gray;
        }
    }
}

