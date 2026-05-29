using System;
using System.Collections.Generic;
using DomModel.Interfaces;

namespace DomModel.Models
{
    /// <summary>
    /// Основной класс, управляющий состоянием игры
    /// Агрегирует GameBoard, Score и Tetromino
    /// </summary>
    public class Game
    {
        private GameBoard _gameBoard;
        private Score _score;
        private Tetromino _currentPiece;
        private Tetromino _nextPiece;
        private Random _random;
        private GameState _gameState;
        private List<IGameObserver> _observers;

        public GameState State => _gameState;
        public GameBoard Board => _gameBoard;
        public Score Score => _score;
        public Tetromino CurrentPiece => _currentPiece;
        public Tetromino NextPiece => _nextPiece;

        public Game()
        {
            _gameBoard = new GameBoard();
            _score = new Score();
            _random = new Random();
            _gameState = GameState.NotStarted;
            _observers = new List<IGameObserver>();
        }

        /// <summary>
        /// Запустить новую игру
        /// </summary>
        public void StartNewGame()
        {
            _gameBoard.Clear();
            _score.Reset();
            _gameState = GameState.Playing;
            SpawnNextPiece();
            SpawnNextPiece();
            NotifyGameStateChanged();
        }

        /// <summary>
        /// Загрузить сохранённую игру
        /// </summary>
        public void LoadGame(GameProgress progress)
        {
            _gameBoard.Clear();
            _score.CurrentScore = progress.CurrentScore;
            _score.LinesCleared = progress.LinesCleared;

            // Восстановить состояние поля
            for (int y = 0; y < GameBoard.Height; y++)
            {
                for (int x = 0; x < GameBoard.Width; x++)
                {
                    if (progress.BoardState[y, x])
                        _gameBoard.SetBlock(x, y);
                }
            }

            _gameState = GameState.Playing;
            SpawnNextPiece();
            NotifyGameStateChanged();
        }

        /// <summary>
        /// Пауза
        /// </summary>
        public void Pause()
        {
            if (_gameState == GameState.Playing)
                _gameState = GameState.Paused;
        }

        /// <summary>
        /// Продолжить после паузы
        /// </summary>
        public void Resume()
        {
            if (_gameState == GameState.Paused)
                _gameState = GameState.Playing;
        }

        /// <summary>
        /// Завершить игру
        /// </summary>
        public void EndGame()
        {
            _gameState = GameState.GameOver;
            NotifyGameOver();
        }

        /// <summary>
        /// Переместить текущую фигуру влево
        /// </summary>
        public bool MovePieceLeft()
        {
            if (_gameState != GameState.Playing || _currentPiece == null)
                return false;

            _currentPiece.Move(-1, 0);
            if (!_gameBoard.CanPlacePiece(_currentPiece))
            {
                _currentPiece.Move(1, 0);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Переместить текущую фигуру вправо
        /// </summary>
        public bool MovePieceRight()
        {
            if (_gameState != GameState.Playing || _currentPiece == null)
                return false;

            _currentPiece.Move(1, 0);
            if (!_gameBoard.CanPlacePiece(_currentPiece))
            {
                _currentPiece.Move(-1, 0);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Переместить текущую фигуру вниз
        /// </summary>
        public bool MovePieceDown()
        {
            if (_gameState != GameState.Playing || _currentPiece == null)
                return false;

            _currentPiece.Move(0, 1);
            if (!_gameBoard.CanPlacePiece(_currentPiece))
            {
                _currentPiece.Move(0, -1);
                PlaceCurrentPiece();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Повернуть текущую фигуру
        /// </summary>
        public bool RotatePiece()
        {
            if (_gameState != GameState.Playing || _currentPiece == null)
                return false;

            var originalRotation = _currentPiece.Rotation;
            _currentPiece.Rotate();

            if (!_gameBoard.CanPlacePiece(_currentPiece))
            {
                _currentPiece.Rotation = originalRotation;
                _currentPiece.Blocks = GetBlocksForTetrominoType(_currentPiece.Type, originalRotation);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Разместить текущую фигуру и спавнить новую
        /// </summary>
        public void PlaceCurrentPiece()
        {
            if (_currentPiece == null)
                return;

            _gameBoard.PlacePiece(_currentPiece);
            _score.AddDropPoints(_currentPiece.Position.Y);

            var clearedLines = _gameBoard.ClearCompletedLines();
            if (clearedLines.Count > 0)
            {
                _score.AddLinePoints(clearedLines.Count);
                NotifyLinesCleared(clearedLines.Count);
            }

            SpawnNextPiece();

            if (_gameBoard.IsGameOver())
            {
                EndGame();
            }

            NotifyGameStateChanged();
        }

        /// <summary>
        /// Получить сохранённый прогресс
        /// </summary>
        public GameProgress GetProgress(string playerName)
        {
            var progress = new GameProgress
            {
                PlayerName = playerName,
                CurrentScore = _score.CurrentScore,
                LinesCleared = _score.LinesCleared,
                Level = _score.Level,
                SaveTime = DateTime.Now,
                BoardState = _gameBoard.GetGridCopy()
            };
            return progress;
        }

        /// <summary>
        /// Подписать наблюдателя на события игры
        /// </summary>
        public void Subscribe(IGameObserver observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
        }

        /// <summary>
        /// Отписать наблюдателя
        /// </summary>
        public void Unsubscribe(IGameObserver observer)
        {
            _observers.Remove(observer);
        }

        private void SpawnNextPiece()
        {
            _currentPiece = _nextPiece ?? CreateRandomPiece();
            _nextPiece = CreateRandomPiece();
            NotifyPieceSpawned(_currentPiece);
        }

        private Tetromino CreateRandomPiece()
        {
            var types = Enum.GetValues(typeof(TetrominoType));
            var randomType = (TetrominoType)types.GetValue(_random.Next(types.Length));
            return new Tetromino(randomType, new Point(GameBoard.Width / 2 - 1, 0));
        }

        private List<Point> GetBlocksForTetrominoType(TetrominoType type, int rotation)
        {
            // Вспомогательный метод для получения блоков определённого типа и ротации
            var piece = new Tetromino(type, new Point(0, 0));
            for (int i = 0; i < rotation; i++)
                piece.Rotate();
            return piece.Blocks;
        }

        private void NotifyGameStateChanged() => _observers.ForEach(o => o.OnGameStateChanged());
        private void NotifyPieceSpawned(Tetromino piece) => _observers.ForEach(o => o.OnPieceSpawned(piece));
        private void NotifyLinesCleared(int count) => _observers.ForEach(o => o.OnLineCleared(count));
        private void NotifyGameOver() => _observers.ForEach(o => o.OnGameOver(_score.CurrentScore));
    }

    public enum GameState
    {
        NotStarted,
        Playing,
        Paused,
        GameOver
    }
}
