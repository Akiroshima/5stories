using DomModel.Interfaces;
using DomModel.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace DomModel.ViewModels
{
    /// <summary>
    /// ViewModel для управления игровым процессом
    /// Реализует паттерн MVVM с привязкой данных WPF
    /// </summary>
    public class GameViewModel : ViewModelBase, IGameObserver
    {
        private Game _game;
        private bool[,] _gridState;
        private int _currentScore;
        private int _linesCleared;
        private int _level;
        private string _gameStatus;
        private bool _isPaused;
        private ObservableCollection<LeaderBoardEntry> _leaderBoard;

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

        public ObservableCollection<LeaderBoardEntry> LeaderBoard
        {
            get => _leaderBoard;
            set => SetProperty(ref _leaderBoard, value);
        }

        public ICommand StartGameCommand { get; private set; }
        public ICommand PauseGameCommand { get; private set; }
        public ICommand ResumeGameCommand { get; private set; }
        public ICommand MoveLeftCommand { get; private set; }
        public ICommand MoveRightCommand { get; private set; }
        public ICommand MoveDownCommand { get; private set; }
        public ICommand RotateCommand { get; private set; }

        public GameViewModel()
        {
            _game = new Game();
            _game.Subscribe(this);
            _gridState = new bool[GameBoard.Height, GameBoard.Width];
            _leaderBoard = new ObservableCollection<LeaderBoardEntry>();
            GameStatus = "Готовы начать игру";

            InitializeCommands();
        }

        private void InitializeCommands()
        {
            StartGameCommand = new RelayCommand(_ => StartGame());
            PauseGameCommand = new RelayCommand(_ => PauseGame());
            ResumeGameCommand = new RelayCommand(_ => ResumeGame());
            MoveLeftCommand = new RelayCommand(_ => _game.MovePieceLeft());
            MoveRightCommand = new RelayCommand(_ => _game.MovePieceRight());
            MoveDownCommand = new RelayCommand(_ => _game.MovePieceDown());
            RotateCommand = new RelayCommand(_ => _game.RotatePiece());
        }

        public void StartGame()
        {
            _game.StartNewGame();
            GameStatus = "Игра начата!";
            IsPaused = false;
        }

        public void PauseGame()
        {
            _game.Pause();
            IsPaused = true;
            GameStatus = "Пауза";
        }

        public void ResumeGame()
        {
            _game.Resume();
            IsPaused = false;
            GameStatus = "Игра продолжается";
        }

        public void LoadGame(GameProgress progress)
        {
            _game.LoadGame(progress);
            UpdateGameState();
            GameStatus = "Игра загружена";
        }

        public GameProgress SaveGame(string playerName)
        {
            return _game.GetProgress(playerName);
        }

        // Реализация IGameObserver
        public void OnGameStateChanged()
        {
            UpdateGameState();
        }

        public void OnPieceSpawned(Tetromino piece)
        {
            OnGameStateChanged();
        }

        public void OnLineCleared(int linesCleared)
        {
            GameStatus = $"Очищено {linesCleared} линий!";
        }

        public void OnGameOver(int finalScore)
        {
            GameStatus = $"Игра окончена! Итоговый счёт: {finalScore}";
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
    }
}
