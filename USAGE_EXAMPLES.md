# Примеры использования доменной модели

## 1. Инициализация и запуск игры

```csharp
using DomModel.ViewModels;
using System.Windows;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // ViewModel с GameWindow в качестве DataContext
        var mainWindow = new GameWindow();
        mainWindow.Show();
    }
}
```

## 2. Управление игрой из ViewModel

```csharp
public class GameController
{
    private GameViewModel _gameVM;
    private GamePersistenceService _persistenceService;
    private LeaderBoardService _leaderBoardService;

    public GameController()
    {
        _gameVM = new GameViewModel();
        _persistenceService = new GamePersistenceService();
        _leaderBoardService = new LeaderBoardService();
    }

    // Начать новую игру
    public void PlayNewGame()
    {
        _gameVM.StartGame();
    }

    // Сохранить игру
    public async Task SaveGameAsync(string playerName)
    {
        var progress = _gameVM.SaveGame(playerName);
        await _persistenceService.SaveGameAsync(
            progress, 
            "./Saves/game_save.txt"
        );
    }

    // Загрузить игру
    public async Task LoadGameAsync()
    {
        var progress = await _persistenceService.LoadGameAsync(
            "./Saves/game_save.txt"
        );
        _gameVM.LoadGame(progress);
    }

    // Обновить таблицу рекордов
    public async Task UpdateLeaderBoardAsync()
    {
        var leaderBoard = await _leaderBoardService.GetLeaderBoardAsync(
            "./Data/leaderboard.txt"
        );
        var collection = new ObservableCollection<LeaderBoardEntry>(leaderBoard);
        _gameVM.UpdateLeaderBoard(collection);
    }
}
```

## 3. Обработка клавиш в GameWindow.xaml.cs

```csharp
public partial class GameWindow
{
    public GameWindow()
    {
        InitializeComponent();
        DataContext = new GameViewModel();
        
        // Обработка клавиш
        this.KeyDown += GameWindow_KeyDown;
    }

    private void GameWindow_KeyDown(object sender, KeyEventArgs e)
    {
        if (DataContext is GameViewModel viewModel)
        {
            switch (e.Key)
            {
                case Key.Left:
                    viewModel.MoveLeftCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.Right:
                    viewModel.MoveRightCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.Down:
                    viewModel.MoveDownCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.Space:
                    viewModel.RotateCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.P:
                    if (viewModel.IsPaused)
                        viewModel.ResumeGameCommand.Execute(null);
                    else
                        viewModel.PauseGameCommand.Execute(null);
                    e.Handled = true;
                    break;
            }
        }
    }
}
```

## 4. Отрисовка игрового поля в XAML

```xml
<!-- Приблизительный пример Canvas отрисовки -->
<Canvas Name="GameCanvas" Width="300" Height="600" Background="Black">
    <!-- Элементы отрисовываются динамически на основе GridState -->
</Canvas>
```

```csharp
// Code-behind для отрисовки
private void DrawGame(GameViewModel viewModel)
{
    GameCanvas.Children.Clear();
    
    if (viewModel.GridState == null)
        return;

    const int cellSize = 30;
    var grid = viewModel.GridState;

    for (int y = 0; y < grid.GetLength(0); y++)
    {
        for (int x = 0; x < grid.GetLength(1); x++)
        {
            if (grid[y, x])
            {
                var rect = new Rectangle
                {
                    Width = cellSize,
                    Height = cellSize,
                    Fill = Brushes.Blue,
                    Stroke = Brushes.DarkBlue
                };
                Canvas.SetLeft(rect, x * cellSize);
                Canvas.SetTop(rect, y * cellSize);
                GameCanvas.Children.Add(rect);
            }
        }
    }
}
```

## 5. Паттерн Observer в действии

```csharp
// ViewModel подписывается на изменения игры
public class GameViewModel : ViewModelBase, IGameObserver
{
    public void OnGameStateChanged()
    {
        // Обновить UI при изменении состояния
        UpdateGameState();
    }

    public void OnPieceSpawned(Tetromino piece)
    {
        // Обновить следующую фигуру в UI
        OnGameStateChanged();
    }

    public void OnLineCleared(int linesCleared)
    {
        // Показать эффект очистки линий
        GameStatus = $"Очищено {linesCleared} линий!";
    }

    public void OnGameOver(int finalScore)
    {
        // Показать диалог с итоговым счётом
        GameStatus = $"ИГРА ОКОНЧЕНА! Счёт: {finalScore}";
    }
}
```

## 6. Использование Dependency Injection (рекомендуется)

```csharp
// App.xaml.cs с DI контейнером
public partial class App : Application
{
    private IServiceProvider _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        var services = new ServiceCollection();
        
        // Регистрация сервисов
        services.AddSingleton<IGamePersistence, GamePersistenceService>();
        services.AddSingleton<ILeaderBoardManager, LeaderBoardService>();
        services.AddSingleton<GameViewModel>();
        services.AddSingleton<GameWindow>();
        
        _serviceProvider = services.BuildServiceProvider();
        
        var mainWindow = _serviceProvider.GetRequiredService<GameWindow>();
        mainWindow.Show();
    }
}
```

## 7. Тестирование доменной модели

```csharp
using Xunit;

public class GameTests
{
    [Fact]
    public void StartGame_ShouldInitializeBoard()
    {
        // Arrange
        var game = new Game();
        
        // Act
        game.StartNewGame();
        
        // Assert
        Assert.NotNull(game.CurrentPiece);
        Assert.NotNull(game.NextPiece);
        Assert.Equal(GameState.Playing, game.State);
    }

    [Fact]
    public void MovePieceDown_ShouldChangePiecePosition()
    {
        // Arrange
        var game = new Game();
        game.StartNewGame();
        var initialY = game.CurrentPiece.Position.Y;
        
        // Act
        game.MovePieceDown();
        
        // Assert
        Assert.True(game.CurrentPiece.Position.Y > initialY);
    }

    [Fact]
    public void ClearCompletedLines_ShouldRemoveFullLines()
    {
        // Arrange
        var board = new GameBoard();
        
        // Act & Assert
        // ... тест логики очистки
    }
}
```
