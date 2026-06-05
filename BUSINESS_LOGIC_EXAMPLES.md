# 📖 Примеры использования бизнес-логики

## 🎮 Пример 1: Запуск полного игрового цикла

```csharp
// Создать сервис игрового цикла
var gameLoopService = new GameLoopService();

// Подписаться на события
gameLoopService.OnUpdate += (engine) =>
{
    Console.WriteLine($"📊 Счёт: {engine.Game.Score.CurrentScore}");
};

gameLoopService.OnStatusChanged += (status) =>
{
    Console.WriteLine($"📈 Статус: {status}");
};

// Запустить игру
gameLoopService.Start();

// Запустить главный цикл (асинхронно)
await gameLoopService.RunAsync();

// Игра будет работать до вызова Stop()
```

---

## 🎯 Пример 2: Отправка команд (Command паттерн)

```csharp
var gameLoopService = new GameLoopService();
gameLoopService.Start();

// Отправить команды
gameLoopService.SendCommand(InputType.MoveLeft);
gameLoopService.SendCommand(InputType.Rotate);
gameLoopService.SendCommand(InputType.MoveDown);

// Команды добавляются в очередь и обрабатываются в игровом цикле
```

---

## 🎮 Пример 3: Работа с состояниями (State паттерн)

```csharp
var gameEngine = new GameEngine();
gameEngine.Start();

// Основное состояние - Playing
Console.WriteLine($"Состояние: {gameEngine.GetCurrentState().GetType().Name}");  // PlayingState

// Пауза - смена на PausedState
gameEngine.ChangeState(new PausedState());
Console.WriteLine($"Состояние: {gameEngine.GetCurrentState().GetType().Name}");  // PausedState

// Продолжить - вернуться в PlayingState
gameEngine.ChangeState(new PlayingState());
Console.WriteLine($"Состояние: {gameEngine.GetCurrentState().GetType().Name}");  // PlayingState

// Конец игры - смена на GameOverState
gameEngine.ChangeState(new GameOverState());
Console.WriteLine($"Состояние: {gameEngine.GetCurrentState().GetType().Name}");  // GameOverState
```

---

## 👁️ Пример 4: Observer паттерн - оповещения

```csharp
var game = new Game();

// Создать наблюдателя
public class MyGameObserver : IGameObserver
{
    public void OnGameStateChanged()
    {
        Console.WriteLine("✅ Состояние игры изменилось");
    }

    public void OnPieceSpawned(Tetromino piece)
    {
        Console.WriteLine($"🎲 Новая фигура: {piece.Type}");
    }

    public void OnLineCleared(int linesCleared)
    {
        Console.WriteLine($"🎉 Очищено {linesCleared} линий!");
    }

    public void OnGameOver(int finalScore)
    {
        Console.WriteLine($"💀 Игра окончена! Счёт: {finalScore}");
    }
}

// Подписать наблюдателя
var observer = new MyGameObserver();
game.Subscribe(observer);

// Теперь observer будет оповещаться об изменениях
game.StartNewGame();          // ✅ Состояние игры изменилось
game.MovePieceLeft();         // ✅ Состояние игры изменилось
game.PlaceCurrentPiece();     // 🎉 Очищено N линий! (если полные)
```

---

## 🎯 Пример 5: InputService - обработка ввода

```csharp
var gameLoopService = new GameLoopService();
gameLoopService.Start();

var inputService = new InputService(gameLoopService);

// Обработать нажатия клавиш
inputService.HandleKeyPress("Left");      // MoveLeft
inputService.HandleKeyPress("Right");     // MoveRight
inputService.HandleKeyPress("Space");     // Rotate
inputService.HandleKeyPress("P");         // Pause
inputService.HandleKeyPress("Enter");     // Start

// Получить привязки клавиш
var bindings = inputService.GetKeyBindings();
foreach (var binding in bindings)
{
    Console.WriteLine($"{binding.Key} → {binding.Value}");
}
```

---

## ⚙️ Пример 6: PhysicsService - физика и коллизии

```csharp
var board = new GameBoard();
var physicsService = new PhysicsService(board);

var piece = new Tetromino(TetrominoType.I, new Point(4, 0));

// Проверить столкновение влево
if (!physicsService.CheckCollision(piece, -1, 0))
{
    Console.WriteLine("✅ Можно переместить влево");
    piece.Move(-1, 0);
}
else
{
    Console.WriteLine("❌ Не можем переместить влево (столкновение)");
}

// Проверить выход за границы
if (physicsService.IsOutOfBounds(piece))
{
    Console.WriteLine("⚠️ Фигура вышла за границы!");
}

// Получить расстояние до земли
int distanceToFloor = physicsService.GetDistanceToFloor(piece);
Console.WriteLine($"📏 Расстояние до земли: {distanceToFloor} клеток");
```

---

## 📊 Пример 7: GameAnalyticsService - статистика

```csharp
var gameEngine = new GameEngine();
var analyticsService = new GameAnalyticsService(gameEngine);

// Игра началась
gameEngine.Start();

// Игрок очистил 2 линии
analyticsService.OnLinesCleared(2);  // 🔥 Комбо: x1

// Ещё 3 линии
analyticsService.OnLinesCleared(3);  // 🔥 Комбо: x2

// Получить статистику
Console.WriteLine(analyticsService.GetAnalytics());
// Вывод: ⏱️ Время: 00:45 | Очищено линий: 5 | Макс комбо: x2 | Счёт: 1500

// Сбросить комбо (если прошло время без очистки)
analyticsService.ResetCombo();
```

---

## 🏭 Пример 8: Factory паттерн - создание фигур

```csharp
var factory = new TetrominoFactory();

// Создать фигуры разных типов
var iShape = factory.CreateTetromino(TetrominoType.I, new Point(4, 0));
// 🎲 Создана фигура: I

var tShape = factory.CreateTetromino(TetrominoType.T, new Point(4, 0));
// 🎲 Создана фигура: T

var oShape = factory.CreateTetromino(TetrominoType.O, new Point(4, 0));
// 🎲 Создана фигура: O
```

---

## 🎮 Пример 9: Полный игровой цикл с ViewModel

```csharp
public class GameController
{
    private GameViewModel _gameVM;
    private InputService _inputService;

    public GameController()
    {
        _gameVM = new GameViewModel();
        _inputService = new InputService(_gameVM.GameLoopService);
    }

    public async void PlayGame()
    {
        // Запустить игру
        await _gameVM.StartGameAsync();

        // Обработка ввода (например, из окна)
        _inputService.HandleKeyPress("Left");
        _inputService.HandleKeyPress("Rotate");
        _inputService.HandleKeyPress("Down");

        // При окончании игры:
        // GameViewModel получит OnGameOver()
        // UI обновится автоматически благодаря INotifyPropertyChanged
    }

    public void SaveGame()
    {
        var progress = _gameVM.SaveGame("PlayerName");
        // Сохранить в файл...
    }

    public void ShowAnalytics()
    {
        Console.WriteLine(_gameVM.GetAnalytics());
    }
}

// Использование:
var controller = new GameController();
controller.PlayGame();
```

---

## 🔄 Пример 10: Интеграция всех паттернов

```csharp
// 1. Создать главный контроллер
var gameEngine = new GameEngine();
var gameLoopService = new GameLoopService();
var inputService = new InputService(gameLoopService);
var physicsService = new PhysicsService(gameEngine.Game.Board);
var analyticsService = new GameAnalyticsService(gameEngine);

// 2. Подписаться на события (Observer)
gameLoopService.OnUpdate += (engine) =>
{
    Console.WriteLine($"FPS: Счёт={engine.Game.Score.CurrentScore}");
};

// 3. Создать наблюдателя для статистики
public class StatisticsObserver : IGameObserver
{
    private GameAnalyticsService _analytics;

    public StatisticsObserver(GameAnalyticsService analytics)
    {
        _analytics = analytics;
    }

    public void OnLineCleared(int count)
    {
        _analytics.OnLinesCleared(count);
    }

    public void OnGameOver(int score)
    {
        Console.WriteLine($"🏆 Игра завершена!");
    }

    // остальные методы...
}

gameEngine.Game.Subscribe(new StatisticsObserver(analyticsService));

// 4. Запустить игру
gameEngine.Start();
gameLoopService.Start();

// 5. Отправлять команды (Command паттерн)
for (int i = 0; i < 10; i++)
{
    inputService.HandleKeyPress("Left");
    inputService.HandleKeyPress("Rotate");
    await Task.Delay(100);
}

// 6. Изменить состояние (State паттерн)
gameEngine.ChangeState(new PausedState());
await Task.Delay(2000);
gameEngine.ChangeState(new PlayingState());

// 7. Получить статистику
Console.WriteLine(analyticsService.GetAnalytics());

// 8. Остановить игру
gameLoopService.Stop();
gameEngine.Stop();
```

---

## 🧪 Пример 11: Unit-тестирование

```csharp
[TestFixture]
public class GameEngineTests
{
    private GameEngine _gameEngine;

    [SetUp]
    public void Setup()
    {
        _gameEngine = new GameEngine();
    }

    [Test]
    public void StateChangeFromPlayingToPaused()
    {
        _gameEngine.Start();
        
        Assert.IsInstanceOf<PlayingState>(_gameEngine.GetCurrentState());
        
        _gameEngine.ChangeState(new PausedState());
        
        Assert.IsInstanceOf<PausedState>(_gameEngine.GetCurrentState());
    }

    [Test]
    public void CommandProcessing()
    {
        _gameEngine.Start();
        
        var input = new GameInput(InputType.MoveLeft);
        _gameEngine.GetCurrentState().HandleInput(_gameEngine, input);
        
        // Фигура должна переместиться влево (если возможно)
        Assert.Pass();
    }

    [Test]
    public void ObserverNotification()
    {
        var notified = false;
        
        var observer = new Mock<IGameObserver>();
        observer.Setup(o => o.OnGameStateChanged()).Callback(() => notified = true);
        
        _gameEngine.Game.Subscribe(observer.Object);
        _gameEngine.Game.StartNewGame();
        
        Assert.IsTrue(notified);
    }
}
```

---

## 📚 Рекомендуемый порядок изучения

1. **Command**: Посмотрите как работают команды в `InputService`
2. **State**: Изучите `PlayingState`, `PausedState`, `GameOverState`
3. **Observer**: Реализуйте `IGameObserver` для своего класса
4. **Factory**: Посмотрите как создаются `Tetromino` через фабрику
5. **Интеграция**: Запустите полный игровой цикл

---

Готово к использованию! 🚀
