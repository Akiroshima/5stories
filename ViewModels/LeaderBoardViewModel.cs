using DomModel.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DomModel.ViewModels
{
    /// <summary>
    /// ViewModel для таблицы рекордов
    /// </summary>
    public class LeaderBoardViewModel : ViewModelBase
    {
        private ObservableCollection<LeaderBoardEntry> _leaderBoardEntries;

        public ObservableCollection<LeaderBoardEntry> LeaderBoardEntries
        {
            get => _leaderBoardEntries;
            set => SetProperty(ref _leaderBoardEntries, value);
        }

        public LeaderBoardViewModel()
        {
            _leaderBoardEntries = new ObservableCollection<LeaderBoardEntry>();
        }

        public void LoadLeaderBoard(List<LeaderBoardEntry> entries)
        {
            LeaderBoardEntries.Clear();
            foreach (var entry in entries)
            {
                LeaderBoardEntries.Add(entry);
            }
        }

        public void AddEntry(LeaderBoardEntry entry)
        {
            LeaderBoardEntries.Add(entry);
        }
    }
}
