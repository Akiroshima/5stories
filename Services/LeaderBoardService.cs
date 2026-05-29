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
    /// Сервис для управления таблицей рекордов
    /// Реализует интерфейс ILeaderBoardManager
    /// </summary>
    public class LeaderBoardService : ILeaderBoardManager
    {
        private List<LeaderBoardEntry> _leaderBoard;
        private const int MaxEntries = 10;

        public LeaderBoardService()
        {
            _leaderBoard = new List<LeaderBoardEntry>();
        }

        public async Task<List<LeaderBoardEntry>> GetLeaderBoardAsync(string filePath)
        {
            try
            {
                _leaderBoard.Clear();

                if (File.Exists(filePath))
                {
                    var lines = await File.ReadAllLinesAsync(filePath);
                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        var parts = line.Split(';');
                        if (parts.Length >= 5)
                        {
                            var entry = new LeaderBoardEntry(
                                parts[0],
                                int.Parse(parts[1]),
                                int.Parse(parts[2]),
                                int.Parse(parts[3]),
                                DateTime.Parse(parts[4])
                            );
                            _leaderBoard.Add(entry);
                        }
                    }
                }

                _leaderBoard.Sort();
                return new List<LeaderBoardEntry>(_leaderBoard);
            }
            catch (Exception ex)
            {
                throw new IOException($"Ошибка при загрузке таблицы рекордов: {ex.Message}", ex);
            }
        }

        public async Task SaveLeaderBoardAsync(List<LeaderBoardEntry> entries, string filePath)
        {
            try
            {
                var directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                entries.Sort();
                var top10 = entries.Take(MaxEntries).ToList();

                var lines = top10.Select(e => e.ToString()).ToList();
                await File.WriteAllLinesAsync(filePath, lines);
            }
            catch (Exception ex)
            {
                throw new IOException($"Ошибка при сохранении таблицы рекордов: {ex.Message}", ex);
            }
        }

        public void AddScore(string playerName, int score)
        {
            var entry = new LeaderBoardEntry(playerName, score, 0, 0, DateTime.Now);
            _leaderBoard.Add(entry);
            _leaderBoard.Sort();

            if (_leaderBoard.Count > MaxEntries)
                _leaderBoard = _leaderBoard.Take(MaxEntries).ToList();
        }

        public bool IsHighScore(int score)
        {
            if (_leaderBoard.Count < MaxEntries)
                return true;

            return score > _leaderBoard.Last().Score;
        }
    }
}
