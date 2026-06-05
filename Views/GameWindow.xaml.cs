using DomModel.ViewModels;

namespace DomModel.Views
{
    public partial class GameWindow
    {
        public GameWindow()
        {
            InitializeComponent();
            DataContext = new GameViewModel();
        }
    }
}
