using DomModel.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DomModel.Services
{
    /// <summary>
    /// Сервис управления игровым циклом
    /// Реализует логику обновления игры и рендеринга
    /// </summary>
    public class GameLoopService
    {
        private readonly GameEngine _gameEngine;
        private bool _isRunning;
        private const int TargetFPS = 60; // 60 кадров в секунду
        private readonly double _frameTime = 1.0 / TargetFPS;

        public event Action<GameEngine>? OnUpdate;
        public event Action<string>? OnStatusChanged;

        public GameLoopService()
        {
            _gameEngine = new GameEngine();
        }

        /// <summary>
        /// Запустить игровой цикл
        /// </summary>
        public void Start()
        {
            _isRunning = true;
            _gameEngine.Start();
            OnStatusChanged?.Invoke(_gameEngine.GetGameStatus());
        }

        /// <summary>
        /// Остановить игровой цикл
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
            _gameEngine.Stop();
        }

        /// <summary>
        /// Отправить команду в игру
        /// </summary>
        public void SendCommand(InputType inputType)
        {
            var input = new GameInput(inputType);
            _gameEngine.ProcessInput(input);
        }

        /// <summary>
        /// Запустить главный игровой цикл (асинхронно)
        /// </summary>
        public async Task RunAsync()
        {
            var stopwatch = Stopwatch.StartNew();

            while (_isRunning)
            {
                // Обновить состояние игры
                _gameEngine.Update();

                // Вызвать обработчик обновления
                OnUpdate?.Invoke(_gameEngine);
                OnStatusChanged?.Invoke(_gameEngine.GetGameStatus());

                // Синхронизация с целевой частотой кадров
                var elapsedMs = stopwatch.Elapsed.TotalMilliseconds;
                var targetMs = _frameTime * 1000;
                var sleepMs = (int)(targetMs - elapsedMs);

                if (sleepMs > 0)
                {
                    await Task.Delay(sleepMs);
                }

                stopwatch.Restart();
            }
        }

        /// <summary>
        /// Получить ссылку на двигатель игры
        /// </summary>
        public GameEngine GetEngine() => _gameEngine;

        /// <summary>
        /// Получить текущее состояние игры
        /// </summary>
        public Game GetGame() => _gameEngine.Game;
    }

    /// <summary>
    /// Сервис для обработки пользовательского ввода
    /// Реализует паттерн Command для инкапсуляции команд
    /// </summary>
    public class InputService
    {
        private readonly GameLoopService _gameLoopService;
        private Dictionary<string, InputType> _keyBindings = new Dictionary<string, InputType>();

        public InputService(GameLoopService gameLoopService)
        {
            _gameLoopService = gameLoopService;
            InitializeKeyBindings();
        }

        /// <summary>
        /// Инициализировать привязки клавиш
        /// </summary>
        private void InitializeKeyBindings()
        {
            _keyBindings = new Dictionary<string, InputType>
            {
                { "Left", InputType.MoveLeft },
                { "Right", InputType.MoveRight },
                { "Down", InputType.MoveDown },
                { "Space", InputType.Rotate },
                { "P", InputType.Pause },
                { "Enter", InputType.Start }
            };
        }

        /// <summary>
        /// Обработать нажатие клавиши
        /// </summary>
        public void HandleKeyPress(string key)
        {
            if (_keyBindings.TryGetValue(key, out var inputType))
            {
                _gameLoopService.SendCommand(inputType);
                Console.WriteLine($"⌨️ Клавиша обработана: {key} -> {inputType}");
            }
        }

        /// <summary>
        /// Получить список привязанных клавиш
        /// </summary>
        public Dictionary<string, InputType> GetKeyBindings() => _keyBindings;
    }

    /// <summary>
    /// Сервис физики и коллизий
    /// Проверяет столкновения и валидирует позиции
    /// </summary>
    public class PhysicsService
    {
        private GameBoard _gameBoard;

        public PhysicsService(GameBoard gameBoard)
        {
            _gameBoard = gameBoard;
        }

        /// <summary>
        /// Проверить столкновение фигуры с границами или другими блоками
        /// </summary>
        public bool CheckCollision(Tetromino piece, int offsetX = 0, int offsetY = 0)
        {
            var testPiece = new Tetromino(piece.Type, new Point(
                piece.Position.X + offsetX,
                piece.Position.Y + offsetY
            ))
            {
                Rotation = piece.Rotation,
                Blocks = piece.Blocks
            };

            return !_gameBoard.CanPlacePiece(testPiece);
        }

        /// <summary>
        /// Проверить, пересекается ли фигура с границами поля
        /// </summary>
        public bool IsOutOfBounds(Tetromino piece)
        {
            var blocks = piece.GetAbsoluteBlocks();
            return blocks.Any(b => b.X < 0 || b.X >= GameBoard.Width || b.Y >= GameBoard.Height);
        }

        /// <summary>
        /// Получить расстояние до земли
        /// </summary>
        public int GetDistanceToFloor(Tetromino piece)
        {
            int distance = 0;
            while (!CheckCollision(piece, 0, distance + 1))
            {
                distance++;
            }
            return distance;
        }
    }

    /// <summary>
    /// Сервис для анализа и статистики игры
    /// </summary>
    public class GameAnalyticsService
    {
        private GameEngine _gameEngine;
        private int _totalLinesCleared;
        private int _maxCombo;
        private int _currentCombo;
        private DateTime _gameStartTime;

        public GameAnalyticsService(GameEngine gameEngine)
        {
            _gameEngine = gameEngine;
            _gameStartTime = DateTime.Now;
        }

        /// <summary>
        /// Обновить статистику при очистке линий
        /// </summary>
        public void OnLinesCleared(int lineCount)
        {
            _totalLinesCleared += lineCount;
            _currentCombo++;

            if (_currentCombo > _maxCombo)
            {
                _maxCombo = _currentCombo;
            }

            Console.WriteLine($"🔥 Комбо: x{_currentCombo} | Всего линий: {_totalLinesCleared}");
        }

        /// <summary>
        /// Сбросить комбо
        /// </summary>
        public void ResetCombo()
        {
            _currentCombo = 0;
        }

        /// <summary>
        /// Получить общую статистику
        /// </summary>
        public string GetAnalytics()
        {
            var elapsedTime = DateTime.Now - _gameStartTime;
            return $"⏱️ Время: {elapsedTime:mm\\:ss} | " +
                   $"Очищено линий: {_totalLinesCleared} | " +
                   $"Макс комбо: x{_maxCombo} | " +
                   $"Счёт: {_gameEngine.Game.Score.CurrentScore}";
        }
    }
}
