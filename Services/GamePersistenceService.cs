using DomModel.Interfaces;
using DomModel.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DomModel.Services
{
    /// <summary>
    /// Сервис для сохранения и загрузки прогресса игры в текстовый файл
    /// Реализует интерфейс IGamePersistence
    /// </summary>
    public class GamePersistenceService : IGamePersistence
    {
        public async Task SaveGameAsync(GameProgress gameProgress, string filePath)
        {
            try
            {
                var directory = Path.GetDirectoryName(filePath);
                if (directory != null && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var lines = new List<string>
                {
                    $"PLAYER:{gameProgress.PlayerName}",
                    $"SCORE:{gameProgress.CurrentScore}",
                    $"LINES:{gameProgress.LinesCleared}",
                    $"LEVEL:{gameProgress.Level}",
                    $"TIME:{gameProgress.SaveTime:yyyy-MM-dd HH:mm:ss}",
                    "BOARD:"
                };

                // Сохранить состояние поля
                for (int y = 0; y < GameBoard.Height; y++)
                {
                    var row = "";
                    for (int x = 0; x < GameBoard.Width; x++)
                    {
                        row += gameProgress.BoardState[y, x] ? "1" : "0";
                    }
                    lines.Add(row);
                }

                await File.WriteAllLinesAsync(filePath, lines);
            }
            catch (Exception ex)
            {
                throw new IOException($"Ошибка при сохранении игры: {ex.Message}", ex);
            }
        }

        public async Task<GameProgress> LoadGameAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"Файл не найден: {filePath}");

                var lines = await File.ReadAllLinesAsync(filePath);
                var progress = new GameProgress();
                int boardLineIndex = -1;

                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];

                    if (line.StartsWith("PLAYER:"))
                        progress.PlayerName = line.Substring(7);
                    else if (line.StartsWith("SCORE:"))
                        progress.CurrentScore = int.Parse(line.Substring(6));
                    else if (line.StartsWith("LINES:"))
                        progress.LinesCleared = int.Parse(line.Substring(6));
                    else if (line.StartsWith("LEVEL:"))
                        progress.Level = int.Parse(line.Substring(6));
                    else if (line.StartsWith("TIME:"))
                        progress.SaveTime = DateTime.Parse(line.Substring(5));
                    else if (line == "BOARD:")
                    {
                        boardLineIndex = i + 1;
                        break;
                    }
                }

                // Загрузить состояние поля
                if (boardLineIndex > 0)
                {
                    progress.BoardState = new bool[GameBoard.Height, GameBoard.Width];
                    for (int y = 0; y < GameBoard.Height && boardLineIndex + y < lines.Length; y++)
                    {
                        var row = lines[boardLineIndex + y];
                        for (int x = 0; x < Math.Min(row.Length, GameBoard.Width); x++)
                        {
                            progress.BoardState[y, x] = row[x] == '1';
                        }
                    }
                }

                return progress;
            }
            catch (Exception ex)
            {
                throw new IOException($"Ошибка при загрузке игры: {ex.Message}", ex);
            }
        }
    }
}
