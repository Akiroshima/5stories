# 🎮 Бизнес-логика приложения Tetris

## 📋 Обзор

Полная реализация бизнес-логики игры Tetris с использованием 3 паттернов Gang of Four (GoF):

1. **Command** - инкапсуляция команд пользователя
2. **Observer** - оповещение об изменениях состояния
3. **State** - управление состояниями игры
4. **Factory** - создание фигур (бонус)

---

## 🏗️ Архитектура бизнес-логики

### Слои приложения:

```
┌─────────────────────────────────────────────┐
│         PRESENTATION LAYER (WPF)            │
│  GameWindow (XAML) ←→ GameViewModel         │
└──────────────────┬──────────────────────────┘
                   │ Data Binding
┌──────────────────▼──────────────────────────┐
│    APPLICATION SERVICES LAYER                │
│  ├─ GameLoopService (игровой цикл)         │
│  ├─ InputService (обработка ввода)         │
│  ├─ GameAnalyticsService (статистика)      │
│  └─ PhysicsService (физика, коллизии)      │
└──────────────────┬──────────────────────────┘
                   │ использует
┌──────────────────▼──────────────────────────┐
│   BUSINESS LOGIC LAYER (Models)             │
│  ├─ GameEngine (State паттерн)              │
│  ├─ Game (главный класс)                    │
│  ├─ GameBoard (поле)                        │
│  ├─ Tetromino (фигуры + Factory)            │
│  └─ GameInput (Command паттерн)             │
└──────────────────┬──────────────────────────┘
                   │ File I/O
┌──────────────────▼──────────────────────────┐
│     DATA PERSISTENCE LAYER                   │
│  GamePersistenceService, LeaderBoardService │
└─────────────────────────────────────────────┘
```

---

## 🎯 Паттерны GoF и их применение

### 1️⃣ **COMMAND паттерн** - инкапсуляция команд

**Файлы:**
- `Models/GameEngine.cs` - `GameInput`, `InputType`
- `Services/GameLoopService.cs` - `InputService`
- `ViewModels/RelayCommand.cs` - `RelayCommand`

**Как работает:**

```csharp
// Пользователь нажимает кнопку
<Button Command="{Binding StartGameCommand}" />

// Кнопка вызывает RelayCommand
public class RelayCommand : ICommand
{
    public void Execute(object parameter)
    {
        _execute(parameter);  // ← Вызывает метод ViewModel
    }
}

// ViewModel создаёт команду
StartGameCommand = new RelayCommand(async _ => await StartGameAsync());

// Команда отправляет GameInput в InputService
_inputService.HandleKeyPress("Left");

// InputService создаёт команду и отправляет в очередь
GameInput input = new GameInput(InputType.MoveLeft);
_gameLoopService.SendCommand(input.Type);

// GameEngine обрабатывает команду в состоянии
_currentState.HandleInput(this, input);
```

**Преимущества Command:**
- ✅ Инкапсулирует действия
- ✅ Очередь команд
- ✅ Отмена/повтор (легко добавить)
- ✅ Макросы из команд

---

### 2️⃣ **STATE паттерн** - управление состояниями

**Файлы:**
- `Models/GameEngine.cs` - `IGameState`, `PlayingState`, `PausedState`, `GameOverState`

**Как работает:**

```csharp
public interface IGameState
{
    void OnEnter(GameEngine engine);      // Вход в состояние
    void OnExit(GameEngine engine);       // Выход из состояния
    void Update(GameEngine engine, double deltaTime);  // Обновление
    void HandleInput(GameEngine engine, GameInput input); // Обработка ввода
}

// PlayingState - основное состояние
public class PlayingState : IGameState
{
    public void Update(GameEngine engine, double deltaTime)
    {
        // Автоматическое падение фигур
        if (_dropTimer.Elapsed.TotalSeconds > _dropInterval)
        {
            if (!engine.Game.MovePieceDown())
                engine.Game.PlaceCurrentPiece();
        }
    }

    public void HandleInput(GameEngine engine, GameInput input)
    {
        switch (input.Type)
        {
            case InputType.MoveLeft:
                engine.Game.MovePieceLeft();
                break;
            case InputType.Pause:
                engine.ChangeState(new PausedState());  // ← Смена состояния
                break;
        }
    }
}

// PausedState - пауза
public class PausedState : IGameState
{
    public void Update(GameEngine engine, double deltaTime)
    {
        // Ничего не делаем на паузе
    }

    public void HandleInput(GameEngine engine, GameInput input)
    {
        if (input.Type == InputType.Pause)
            engine.ChangeState(new PlayingState());  // Вернуться в Playing
    }
}

// GameOverState - конец игры
public class GameOverState : IGameState
{
    public void OnEnter(GameEngine engine)
    {
        Console.WriteLine($"ИГРА ОКОНЧЕНА! Счёт: {engine.Game.Score.CurrentScore}");
    }
}

// Смена состояния
public void ChangeState(IGameState newState)
{
    _currentState.OnExit(this);
    _currentState = newState;
    _currentState.OnEnter(this);
}
```

**Преимущества State:**
- ✅ Разделение логики по состояниям
- ✅ Легко добавлять новые состояния
- ✅ Слабая связанность
- ✅ Чистый код

**Диаграмма переходов:**

```
┌──────────────┐
│  NotStarted  │
└──────┬───────┘
       │ Start
       ▼
┌──────────────┐
│   Playing    │◄──────┐
└──────┬───────┘       │
       │ Pause         │ Resume
       ▼               │
┌──────────────┐       │
│   Paused     ├───────┘
└──────────────┘
       │ Game Over
       ▼
┌──────────────┐
│  GameOver    │
└──────────────┘
```

---

### 3️⃣ **OBSERVER паттерн** - оповещение об изменениях

**Файлы:**
- `Interfaces/IGamePersistence.cs` - `IGameObserver`
- `Models/Game.cs` - Subject (издатель)
- `ViewModels/GameViewModel.cs` - Observer (подписчик)

**Как работает:**

```csharp
// IGameObserver - интерфейс наблюдателя
public interface IGameObserver
{
    void OnGameStateChanged();
    void OnPieceSpawned(Tetromino piece);
    void OnLineCleared(int linesCleared);
    void OnGameOver(int finalScore);
}

// Game - издатель (Subject)
public class Game
{
    private List<IGameObserver> _observers;

    public void Subscribe(IGameObserver observer)
    {
        _observers.Add(observer);
    }

    public void PlaceCurrentPiece()
    {
        var clearedLines = _gameBoard.ClearCompletedLines();
        if (clearedLines.Count > 0)
        {
            _score.AddLinePoints(clearedLines.Count);
            NotifyLinesCleared(clearedLines.Count);  // ← Уведомить
        }
        SpawnNextPiece();
        NotifyGameStateChanged();  // ← Уведомить
    }

    private void NotifyGameStateChanged()
    {
        _observers.ForEach(o => o.OnGameStateChanged());
    }

    private void NotifyLinesCleared(int count)
    {
        _observers.ForEach(o => o.OnLineCleared(count));
    }
}

// GameViewModel - наблюдатель (Observer)
public class GameViewModel : ViewModelBase, IGameObserver
{
    public GameViewModel()
    {
        _game = new Game();
        _game.Subscribe(this);  // ← Подписаться на события
    }

    public void OnGameStateChanged()
    {
        UpdateGameState();  // Обновить UI
    }

    public void OnLineCleared(int linesCleared)
    {
        _analyticsService.OnLinesCleared(linesCleared);
        GameStatus = $"🎉 Очищено {linesCleared} линий!";
    }

    public void OnGameOver(int finalScore)
    {
        GameStatus = $"💀 ИГРА ОКОНЧЕНА! Счёт: {finalScore}";
    }
}
```

**Преимущества Observer:**
- ✅ Слабая связанность
- ✅ Динамическая подписка/отписка
- ✅ Много наблюдателей
- ✅ Отправителю не нужно знать о получателях

---

### 4️⃣ **FACTORY паттерн** (бонус) - создание фигур

**Файлы:**
- `Models/GameEngine.cs` - `ITetrominoFactory`, `TetrominoFactory`

**Как работает:**

```csharp
public interface ITetrominoFactory
{
    Tetromino CreateTetromino(TetrominoType type, Point position);
}

public class TetrominoFactory : ITetrominoFactory
{
    public Tetromino CreateTetromino(TetrominoType type, Point position)
    {
        var tetromino = new Tetromino(type, position);
        Console.WriteLine($"🎲 Создана фигура: {type}");
        return tetromino;
    }
}

// Использование
private ITetrominoFactory _tetrominoFactory = new TetrominoFactory();
var piece = _tetrominoFactory.CreateTetromino(TetrominoType.I, startPos);
```

**Преимущества Factory:**
- ✅ Инкапсулирует создание
- ✅ Легко расширять (новые типы фигур)
- ✅ Логирование и валидация

---

## 🔄 Игровой цикл

### Полный процесс выполнения команды:

```
┌─────────────────────────────────────┐
│  1. Пользователь нажимает клавишу   │
│     (например, "Левая стрелка")     │
└─────────────────┬───────────────────┘
                  │
┌─────────────────▼───────────────────┐
│  2. WPF вызывает RelayCommand        │
│     Command={Binding MoveLeftCommand}│
└─────────────────┬───────────────────┘
                  │
┌─────────────────▼───────────────────┐
│  3. RelayCommand.Execute()           │
│     → _inputService.HandleKeyPress() │
└─────────────────┬───────────────────┘
                  │
┌─────────────────▼───────────────────┐
│  4. InputService создаёт GameInput   │
│     GameInput(InputType.MoveLeft)    │
└─────────────────┬───────────────────┘
                  │
┌─────────────────▼───────────────────┐
│  5. InputService отправляет команду  │
│     _gameLoopService.SendCommand()   │
└─────────────────┬───────────────────┘
                  │
┌─────────────────▼───────────────────┐
│  6. GameLoopService добавляет        │
│     команду в очередь (_inputQueue) │
└─────────────────┬───────────────────┘
                  │
┌─────────────────▼───────────────────┐
│  7. Игровой цикл (60 FPS)            │
│     Update() обрабатывает очередь    │
└─────────────────┬───────────────────┘
                  │
┌─────────────────▼───────────────────┐
│  8. GameEngine обрабатывает команду  │
│     _currentState.HandleInput(input) │
└─────────────────┬───────────────────┘
                  │
┌─────────────────▼───────────────────┐
│  9. PlayingState.HandleInput()       │
│     → Game.MovePieceLeft()           │
└─────────────────┬───────────────────┘
                  │
┌─────────────────▼───────────────────┐
│  10. Game вызывает Observer методы   │
│     → NotifyGameStateChanged()       │
└─────────────────┬───────────────────┘
                  │
┌─────────────────▼───────────────────┐
│  11. GameViewModel.OnGameStateChanged│
│     → UpdateGameState()              │
└─────────────────┬───────────────────┘
                  │
┌─────────────────▼───────────────────┐
│  12. SetProperty() вызывает          │
│     PropertyChanged (INotifyPropChanged)
└─────────────────┬───────────────────┘
                  │
┌─────────────────▼───────────────────┐
│  13. WPF обновляет UI                │
│     {Binding CurrentScore} обновляется
└─────────────────────────────────────┘
```

---

## 📊 Основные сервисы

### GameLoopService
```csharp
public class GameLoopService
{
    // Запустить игровой цикл
    public void Start();
    
    // Остановить цикл
    public void Stop();
    
    // Отправить команду в игру (Command паттерн)
    public void SendCommand(InputType inputType);
    
    // Главный игровой цикл (60 FPS)
    public async Task RunAsync();
}
```

### InputService
```csharp
public class InputService
{
    // Обработать нажатие клавиши и создать команду
    public void HandleKeyPress(string key);
    
    // Получить привязки клавиш
    public Dictionary<string, InputType> GetKeyBindings();
}
```

### PhysicsService
```csharp
public class PhysicsService
{
    // Проверить столкновение
    public bool CheckCollision(Tetromino piece, int offsetX = 0, int offsetY = 0);
    
    // Проверить выход за границы
    public bool IsOutOfBounds(Tetromino piece);
    
    // Расстояние до земли
    public int GetDistanceToFloor(Tetromino piece);
}
```

### GameAnalyticsService
```csharp
public class GameAnalyticsService
{
    // Обновить статистику при очистке линий
    public void OnLinesCleared(int lineCount);
    
    // Получить общую статистику
    public string GetAnalytics();
}
```

---

## 🎓 Взаимодействие паттернов

```
┌─────────────────────────────────────────────────────┐
│              COMMAND PATTERN                        │
│  RelayCommand → GameInput → InputService → Queue    │
│          Инкапсуляция команд пользователя           │
└────────────────────┬────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────┐
│              STATE PATTERN                          │
│  IGameState → PlayingState, PausedState, GameOverState
│          Управление состояниями игры                │
└────────────────────┬────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────┐
│              OBSERVER PATTERN                       │
│  Game → NotifyGameStateChanged → GameViewModel     │
│          Оповещение об изменениях                   │
└─────────────────────────────────────────────────────┘
```

---

## 🚀 Как это использует приложение

### Пример: Нажата клавиша влево

1. **Command**: Кнопка вызывает `MoveLeftCommand`
2. **Command**: Создаётся `GameInput(InputType.MoveLeft)`
3. **Command**: Команда добавляется в очередь
4. **State**: `PlayingState.HandleInput()` обрабатывает команду
5. **State**: Вызывает `Game.MovePieceLeft()`
6. **Observer**: Game уведомляет ViewModel
7. **Observer**: ViewModel обновляет UI (счёт, поле)
8. **WPF Binding**: UI перерисовывается

---

## 🔧 Расширяемость

### Добавить новое состояние:
```csharp
public class SlowMotionState : IGameState
{
    public void Update(GameEngine engine, double deltaTime)
    {
        // Медленное падение фигур
    }
}

// Использование:
engine.ChangeState(new SlowMotionState());
```

### Добавить новый тип ввода:
```csharp
public enum InputType
{
    // ... существующие ...
    HardDrop,  // ← Новый
    Rotate180  // ← Новый
}

// Обработка в State:
case InputType.HardDrop:
    // Реализация
    break;
```

### Добавить новый наблюдатель:
```csharp
public class SoundService : IGameObserver
{
    public void OnLineCleared(int count)
    {
        PlaySound("line_clear.mp3");
    }
}

game.Subscribe(new SoundService());
```

---

## ✅ Итоговая статистика

| Паттерн | Количество | Назначение |
|---------|-----------|-----------|
| Command | RelayCommand + GameInput | Инкапсуляция команд |
| State | 3 состояния | Управление ФСМ игры |
| Observer | 1 субъект + 2 наблюдателя | Оповещение об изменениях |
| Factory | TetrominoFactory | Создание фигур |

**Общая бизнес-логика:**
- ✅ Полный игровой цикл (60 FPS)
- ✅ Обработка ввода через Command паттерн
- ✅ Управление состояниями через State паттерн
- ✅ Оповещение UI через Observer паттерн
- ✅ Физика и коллизии
- ✅ Система очков и уровней
- ✅ Аналитика игры

---

**Архитектура готова к использованию и расширению!** 🚀
