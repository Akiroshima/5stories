using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DomModel.Models
{
    /// <summary>
    /// Паттерн STATE - управление состояниями игры
    /// Позволяет менять поведение игры в зависимости от состояния
    /// </summary>
    public interface IGameState
    {
        void OnEnter(GameEngine engine);
        void OnExit(GameEngine engine);
        void Update(GameEngine engine, double deltaTime);
        void HandleInput(GameEngine engine, GameInput input);
    }

    public class PlayingState : IGameState
    {
        private readonly Stopwatch _dropTimer = new Stopwatch();
        private readonly double _dropInterval = 1.0; // секунды

        public void OnEnter(GameEngine engine)
        {
            _dropTimer.Restart();
            Console.WriteLine("🎮 Игра начата - режим PLAYING");
        }

        public void OnExit(GameEngine engine)
        {
            _dropTimer.Stop();
        }

        public void Update(GameEngine engine, double deltaTime)
        {
            // Автоматическое падение фигур
            if (_dropTimer.Elapsed.TotalSeconds > _dropInterval)
            {
                engine.Game.MovePieceDown();
                _dropTimer.Restart();
            }
        }

        public void HandleInput(GameEngine engine, GameInput input)
        {
            switch (input.Type)
            {
                case InputType.MoveLeft:
                    engine.Game.MovePieceLeft();
                    break;
                case InputType.MoveRight:
                    engine.Game.MovePieceRight();
                    break;
                case InputType.MoveDown:
                    engine.Game.MovePieceDown();
                    break;
                case InputType.Rotate:
                    engine.Game.RotatePiece();
                    break;
                case InputType.Pause:
                    engine.ChangeState(new PausedState());
                    break;
            }
        }
    }

    public class PausedState : IGameState
    {
        public void OnEnter(GameEngine engine)
        {
            Console.WriteLine("⏸️ Игра на паузе");
        }

        public void OnExit(GameEngine engine)
        {
            Console.WriteLine("▶️ Игра продолжается");
        }

        public void Update(GameEngine engine, double deltaTime)
        {
            // Ничего не делаем на паузе
        }

        public void HandleInput(GameEngine engine, GameInput input)
        {
            if (input.Type == InputType.Pause)
            {
                engine.ChangeState(new PlayingState());
            }
        }
    }

    public class GameOverState : IGameState
    {
        public void OnEnter(GameEngine engine)
        {
            Console.WriteLine($"💀 ИГРА ОКОНЧЕНА! Финальный счёт: {engine.Game.Score.CurrentScore}");
        }

        public void OnExit(GameEngine engine)
        {
        }

        public void Update(GameEngine engine, double deltaTime)
        {
            // Ничего не делаем после окончания
        }

        public void HandleInput(GameEngine engine, GameInput input)
        {
            if (input.Type == InputType.Start)
            {
                engine.Game.StartNewGame();
                engine.ChangeState(new PlayingState());
            }
        }
    }

    /// <summary>
    /// Input для паттерна Command - инкапсулирует данные команды
    /// </summary>
    public enum InputType
    {
        MoveLeft,
        MoveRight,
        MoveDown,
        Rotate,
        Pause,
        Start
    }

    public class GameInput
    {
        public InputType Type { get; set; }
        public DateTime Timestamp { get; set; }

        public GameInput(InputType type)
        {
            Type = type;
            Timestamp = DateTime.Now;
        }
    }

    /// <summary>
    /// Паттерн FACTORY - создание Tetromino разных типов
    /// Абстрактная фабрика для создания фигур
    /// </summary>
    public interface ITetrominoFactory
    {
        Tetromino CreateTetromino(TetrominoType type, Point position);
    }

    public class TetrominoFactory : ITetrominoFactory
    {
        public Tetromino CreateTetromino(TetrominoType type, Point position)
        {
            var tetromino = new Tetromino(type, position);
            Console.WriteLine($"🎲 Создана фигура: {type}");
            return tetromino;
        }
    }

    /// <summary>
    /// GameEngine - главный класс с полной бизнес-логикой
    /// Реализует State паттерн для управления состояниями
    /// Работает с Command паттерном через GameInput
    /// Использует Factory для создания фигур
    /// </summary>
    public class GameEngine
    {
        private IGameState _currentState;
        private readonly ITetrominoFactory _tetrominoFactory;
        private readonly Queue<GameInput> _inputQueue;
        private readonly Stopwatch _gameTimer;

        public Game Game { get; private set; }
        public bool IsRunning { get; private set; }

        public GameEngine()
        {
            Game = new Game();
            _tetrominoFactory = new TetrominoFactory();
            _inputQueue = new Queue<GameInput>();
            _gameTimer = new Stopwatch();
            _currentState = new PlayingState();
        }

        /// <summary>
        /// Запустить игровой цикл
        /// </summary>
        public void Start()
        {
            IsRunning = true;
            _gameTimer.Restart();
            Game.StartNewGame();
            _currentState = new PlayingState();
            _currentState.OnEnter(this);

            Console.WriteLine("🎮 Игровой цикл запущен!");
        }

        /// <summary>
        /// Остановить игровой цикл
        /// </summary>
        public void Stop()
        {
            IsRunning = false;
            _gameTimer.Stop();
            _currentState.OnExit(this);
        }

        /// <summary>
        /// Запустить движок для загруженной игры без сброса доски
        /// </summary>
        public void StartLoaded()
        {
            IsRunning = true;
            _gameTimer.Restart();
            _currentState = new PlayingState();
            _currentState.OnEnter(this);
        }

        /// <summary>
        /// Обработать входные данные (Command паттерн)
        /// </summary>
        public void ProcessInput(GameInput input)
        {
            if (!IsRunning) return;

            _inputQueue.Enqueue(input);
            Console.WriteLine($"📥 Команда добавлена в очередь: {input.Type}");
        }

        /// <summary>
        /// Обновить состояние игры (игровой цикл)
        /// </summary>
        public void Update()
        {
            if (!IsRunning) return;

            double deltaTime = _gameTimer.Elapsed.TotalSeconds;
            _gameTimer.Restart();

            // Обработать все накопленные команды
            while (_inputQueue.Count > 0)
            {
                var input = _inputQueue.Dequeue();
                _currentState.HandleInput(this, input);
            }

            // Обновить текущее состояние
            _currentState.Update(this, deltaTime);

            // Проверить конец игры
            if (Game.Board.IsGameOver() && _currentState is PlayingState)
            {
                ChangeState(new GameOverState());
            }
        }

        /// <summary>
        /// Изменить состояние игры (State паттерн)
        /// </summary>
        public void ChangeState(IGameState newState)
        {
            if (_currentState != null)
            {
                _currentState.OnExit(this);
            }

            _currentState = newState;
            _currentState.OnEnter(this);
        }

        /// <summary>
        /// Получить текущее состояние
        /// </summary>
        public IGameState GetCurrentState() => _currentState;

        /// <summary>
        /// Получить информацию о состоянии игры
        /// </summary>
        public string GetGameStatus()
        {
            return $"Счёт: {Game.Score.CurrentScore} | " +
                   $"Линии: {Game.Score.LinesCleared} | " +
                   $"Уровень: {Game.Score.Level} | " +
                   $"Состояние: {_currentState.GetType().Name}";
        }
    }
}
