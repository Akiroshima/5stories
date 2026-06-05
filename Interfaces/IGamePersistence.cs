using DomModel.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DomModel.Interfaces
{
    /// <summary>
    /// Интерфейс для сохранения и загрузки игры
    /// </summary>
    public interface IGamePersistence
    {
        Task SaveGameAsync(GameProgress gameProgress, string filePath);
        Task<GameProgress> LoadGameAsync(string filePath);
    }

    /// <summary>
    /// Интерфейс для управления таблицей рекордов
    /// </summary>
    public interface ILeaderBoardManager
    {
        Task<List<LeaderBoardEntry>> GetLeaderBoardAsync(string filePath);
        Task SaveLeaderBoardAsync(List<LeaderBoardEntry> entries, string filePath);
        void AddScore(string playerName, int score);
        bool IsHighScore(int score);
    }

    /// <summary>
    /// Интерфейс для игрового движка
    /// </summary>
    public interface IGameEngine
    {
        void Start();
        void Pause();
        void Resume();
        void Stop();
        void RotatePiece();
        void MovePieceLeft();
        void MovePieceRight();
        void MovePieceDown();
    }

    /// <summary>
    /// Интерфейс для оповещения об изменениях
    /// </summary>
    public interface IGameObserver
    {
        void OnGameStateChanged();
        void OnPieceSpawned(Tetromino piece);
        void OnLineCleared(int linesCleared);
        void OnGameOver(int finalScore);
    }
}
