using DomModel.Interfaces;
using DomModel.Models;
using DomModel.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DomModel.ViewModels
{
    /// <summary>
    /// ViewModel для управления игровым процессом
    /// Реализует паттерн MVVM с привязкой данных WPF
    /// Использует GameLoopService для управления игровым циклом
    /// </summary>
    public class GameViewModel : ViewModelBase, IGameObserver
    {
        private readonly Game _game;
        private readonly GameLoopService _gameLoopService;
        private readonly GameAnalyticsService _analyticsService;
        private readonly InputService _inputService;
        private bool[,] _gridState;
        private int _currentScore;
        private int _linesCleared;
        private int _level;
        private string _gameStatus;
        private bool _isPaused;
        private ObservableCollection<LeaderBoardEntry> _leaderBoard;
        private bool _isGameRunning;

        public bool[,] GridState
        {
            get => _gridState;
            set => SetProperty(ref _gridState, value);
        }

        public int CurrentScore
        {
            get => _currentScore;
            set => SetProperty(ref _currentScore, value);
        }

        public int LinesCleared
        {
            get => _linesCleared;
            set => SetProperty(ref _linesCleared, value);
        }

        public int Level
        {
            get => _level;
            set => SetProperty(ref _level, value);
        }

        public string GameStatus
        {
            get => _gameStatus;
            set => SetProperty(ref _gameStatus, value);
        }

        public bool IsPaused
        {
            get => _isPaused;
            set => SetProperty(ref _isPaused, value);
        }

        public bool IsGameRunning
        {
            get => _isGameRunning;
            set => SetProperty(ref _isGameRunning, value);
        }

        public ObservableCollection<LeaderBoardEntry> LeaderBoard
        {
            get => _leaderBoard;
            set => SetProperty(ref _leaderBoard, value);
        }

        public ICommand StartGameCommand { get; private set; } = null!;
        public ICommand PauseGameCommand { get; private set; } = null!;
        public ICommand ResumeGameCommand { get; private set; } = null!;
        public ICommand MoveLeftCommand { get; private set; } = null!;
        public ICommand MoveRightCommand { get; private set; } = null!;
        public ICommand MoveDownCommand { get; private set; } = null!;
        public ICommand RotateCommand { get; private set; } = null!;

        /// <summary>
        /// Конструктор ViewModel
        /// </summary>
        public GameViewModel()
        {
            _isGameRunning = false;

            _gameLoopService = new GameLoopService();
            _analyticsService = new GameAnalyticsService(_gameLoopService.GetEngine());
            _inputService = new InputService(_gameLoopService);

            _game = _gameLoopService.GetEngine().Game;
            _game.Subscribe(this);

            _gridState = new bool[GameBoard.Height, GameBoard.Width];
            _gameStatus = "Нажмите Enter чтобы начать";
            _leaderBoard = new ObservableCollection<LeaderBoardEntry>();

            _gameLoopService.OnUpdate += UpdateGameView;
            _gameLoopService.OnStatusChanged += UpdateGameStatus;

            InitializeCommands();
        }

        private void InitializeCommands()
        {
            StartGameCommand = new RelayCommand(_ => StartGameAsync());
            PauseGameCommand = new RelayCommand(_ => PauseGame());
            ResumeGameCommand = new RelayCommand(_ => ResumeGame());
            MoveLeftCommand = new RelayCommand(_ => _inputService.HandleKeyPress("Left"));
            MoveRightCommand = new RelayCommand(_ => _inputService.HandleKeyPress("Right"));
            MoveDownCommand = new RelayCommand(_ => _inputService.HandleKeyPress("Down"));
            RotateCommand = new RelayCommand(_ => _inputService.HandleKeyPress("Space"));
        }

        public void StartGameAsync()
        {
            if (_isGameRunning) return;

            IsGameRunning = true;
            _gameLoopService.Start();
            GameStatus = "🎮 Игра начата!";
            IsPaused = false;

            _ = Task.Run(async () =>
            {
                try
                {
                    await _gameLoopService.RunAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Ошибка в игровом цикле: {ex.Message}");
                    RunOnUiThread(() => GameStatus = $"❌ Ошибка: {ex.Message}");
                }
                finally
                {
                    RunOnUiThread(() =>
                    {
                        IsGameRunning = false;
                        GameStatus = "⏹️ Игра остановлена";
                    });
                }
            });
        }

        public void ResumeGame()
        {
            _game.Resume();
            IsPaused = false;
            GameStatus = "▶️ Игра продолжается";
        }

        private void UpdateGameView(GameEngine engine)
        {
            RunOnUiThread(UpdateGameState);
        }

        private void UpdateGameStatus(string status)
        {
            RunOnUiThread(() => GameStatus = $"📊 {status}");
        }

        public void PauseGame()
        {
            _game.Pause();
            IsPaused = true;
            GameStatus = "Пауза";
        }

        public void LoadGame(GameProgress progress)
        {
            _gameLoopService.Stop();
            _game.LoadGame(progress);
            RunOnUiThread(UpdateGameState);
            GameStatus = "🎮 Игра загружена";
        }

        public GameProgress SaveGame(string playerName)
        {
            _gameLoopService.Stop();
            return _game.GetProgress(playerName);
        }

        public string GetAnalytics()
        {
            return _analyticsService.GetAnalytics();
        }

        public void OnGameStateChanged()
        {
            RunOnUiThread(UpdateGameState);
        }

        public void OnPieceSpawned(Tetromino piece)
        {
            Console.WriteLine($"\n🎲 Новая фигура спавнена!");
            Console.WriteLine($"   Тип: {piece.Type}");
            Console.WriteLine($"   Позиция: ({piece.Position.X}, {piece.Position.Y})");
            Console.WriteLine($"   Блоков: {piece.Blocks?.Count ?? 0}");

            if (piece.Blocks != null)
            {
                foreach (var block in piece.Blocks)
                {
                    Console.WriteLine($"      - ({block.X}, {block.Y})");
                }
            }

            RunOnUiThread(UpdateGameState);
        }

        public void OnLineCleared(int linesCleared)
        {
            RunOnUiThread(() =>
            {
                _analyticsService.OnLinesCleared(linesCleared);
                GameStatus = $"🎉 Очищено {linesCleared} линий!";
            });
        }

        public void OnGameOver(int finalScore)
        {
            RunOnUiThread(() =>
            {
                _analyticsService.ResetCombo();
                GameStatus = $"💀 ИГРА ОКОНЧЕНА! Финальный счёт: {finalScore}";
                Console.WriteLine(_analyticsService.GetAnalytics());
            });
        }

        private void UpdateGameState()
        {
            GridState = _game.Board.GetGridCopy();
            CurrentScore = _game.Score.CurrentScore;
            LinesCleared = _game.Score.LinesCleared;
            Level = _game.Score.Level;
        }

        public void UpdateLeaderBoard(ObservableCollection<LeaderBoardEntry> leaderBoard)
        {
            LeaderBoard = leaderBoard;
        }

        public Game GetGame() => _game;

        public InputService GetInputService() => _inputService;

        private void RunOnUiThread(Action action)
        {
            var dispatcher = Application.Current?.Dispatcher;

            if (dispatcher == null || dispatcher.CheckAccess())
            {
                action();
                return;
            }

            dispatcher.Invoke(action);
        }
    }
}
