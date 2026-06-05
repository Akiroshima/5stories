# 🎮 Паттерн Command в Tetris

## Определение паттерна Command (GoF)

**Command** - это поведенческий паттерн проектирования, который инкапсулирует **запрос как объект**, позволяя параметризовать клиентов с различными запросами, ставить запросы в очередь, регистрировать запросы и поддерживать отмену операций.

---

## 🎯 Где я использовал Command в проекте

### 1️⃣ Инкапсуляция команды - Класс `GameInput`

📍 **Файл**: [Models/GameEngine.cs](Models/GameEngine.cs) (строки 115-125)

```csharp
/// <summary>
/// Input для паттерна Command - инкапсулирует данные команды
/// </summary>
public enum InputType
{
    MoveLeft,      // Команда: движение влево
    MoveRight,     // Команда: движение вправо
    MoveDown,      // Команда: движение вниз
    Rotate,        // Команда: поворот фигуры
    Pause,         // Команда: пауза
    Start          // Команда: начало игры
}

public class GameInput  // ← ЭТО КОМАНДА (Command Object)
{
    public InputType Type { get; set; }
    public DateTime Timestamp { get; set; }

    public GameInput(InputType type)
    {
        Type = type;
        Timestamp = DateTime.Now;
    }
}
```

**Роль**: `GameInput` - это объект **Command**, который инкапсулирует пользовательский запрос.

---

### 2️⃣ Инициатор команды - Класс `InputService`

📍 **Файл**: [Services/GameLoopService.cs](Services/GameLoopService.cs) (строки 102-145)

```csharp
public class InputService  // ← ЭТО ИНИЦИАТОР (Command Invoker)
{
    private GameLoopService _gameLoopService;
    private Dictionary<string, InputType> _keyBindings;

    // Таблица привязок: клавиша → тип команды
    private void InitializeKeyBindings()
    {
        _keyBindings = new Dictionary<string, InputType>
        {
            { "Left", InputType.MoveLeft },      // Левая стрелка → MoveLeft
            { "Right", InputType.MoveRight },    // Правая стрелка → MoveRight
            { "Down", InputType.MoveDown },      // Нижняя стрелка → MoveDown
            { "Space", InputType.Rotate },       // Пробел → Rotate
            { "P", InputType.Pause },            // P → Pause
            { "Enter", InputType.Start }         // Enter → Start
        };
    }

    /// <summary>
    /// Обработать нажатие клавиши и отправить команду
    /// </summary>
    public void HandleKeyPress(string key)  // ← ИНИЦИИРУЕТ КОМАНДУ
    {
        if (_keyBindings.TryGetValue(key, out var inputType))
        {
            // СОЗДАНИЕ КОМАНДЫ
            _gameLoopService.SendCommand(inputType);
            Console.WriteLine($"⌨️ Клавиша обработана: {key} -> {inputType}");
        }
    }
}
```

**Роль**: `InputService` - это **Invoker** (инициатор), который создает команды и отправляет их.

---

### 3️⃣ Обработчик команд - Класс `GameLoopService`

📍 **Файл**: [Services/GameLoopService.cs](Services/GameLoopService.cs) (строки 49-54)

```csharp
public class GameLoopService
{
    private GameEngine _gameEngine;

    /// <summary>
    /// Отправить команду в игру
    /// </summary>
    public void SendCommand(InputType inputType)  // ← ПРИНИМАЕТ И ОТПРАВЛЯЕТ КОМАНДУ
    {
        var input = new GameInput(inputType);  // ← СОЗДАНИЕ ОБЪЕКТА КОМАНДЫ
        _gameEngine.ProcessInput(input);        // ← ПЕРЕДАЧА К ИСПОЛНИТЕЛЮ
    }

    /// <summary>
    /// Главный игровой цикл (асинхронно)
    /// </summary>
    public async Task RunAsync()
    {
        while (_isRunning)
        {
            _gameEngine.Update();      // Обновить состояние
            OnUpdate?.Invoke(_gameEngine);
            OnStatusChanged?.Invoke(_gameEngine.GetGameStatus());
            // ... синхронизация FPS
        }
    }
}
```

**Роль**: `GameLoopService` - это **Command Queue & Dispatcher**, передает команду исполнителю.

---

### 4️⃣ Исполнитель команды - Класс `GameEngine`

📍 **Файл**: [Models/GameEngine.cs](Models/GameEngine.cs) (строки 180-250)

```csharp
public class GameEngine
{
    public Game Game { get; private set; }
    private IGameState _currentState;
    private Queue<GameInput> _inputQueue;  // ← ОЧЕРЕДЬ КОМАНД!

    public GameEngine()
    {
        Game = new Game();
        _inputQueue = new Queue<GameInput>();
    }

    /// <summary>
    /// Обработать входящую команду
    /// </summary>
    public void ProcessInput(GameInput input)  // ← ПОЛУЧИТЬ КОМАНДУ
    {
        _inputQueue.Enqueue(input);  // ← ПОСТАВИТЬ В ОЧЕРЕДЬ
        Console.WriteLine($"📤 Команда в очередь: {input.Type}");
    }

    /// <summary>
    /// Обновить состояние игры (вызывается каждый кадр)
    /// </summary>
    public void Update()
    {
        // ОБРАБОТАТЬ ВСЕ КОМАНДЫ ИЗ ОЧЕРЕДИ
        while (_inputQueue.Count > 0)
        {
            var input = _inputQueue.Dequeue();  // ← ДОСТАТЬ КОМАНДУ
            _currentState.HandleInput(this, input);  // ← ИСПОЛНИТЬ КОМАНДУ
        }

        _currentState.Update(this, 0.016);  // 60 FPS = 16ms per frame
    }
}
```

**Роль**: `GameEngine` - это **Receiver** (получатель команды), исполняет команду через текущее состояние.

---

### 5️⃣ Обработчик команды по состояниям - `PlayingState`

📍 **Файл**: [Models/GameEngine.cs](Models/GameEngine.cs) (строки 20-66)

```csharp
public class PlayingState : IGameState
{
    /// <summary>
    /// Обработать команду игрока
    /// </summary>
    public void HandleInput(GameEngine engine, GameInput input)  // ← ИСПОЛНИТЬ КОМАНДУ
    {
        switch (input.Type)  // ← ИНТЕРПРЕТИРОВАТЬ ТИП КОМАНДЫ
        {
            case InputType.MoveLeft:
                engine.Game.MovePieceLeft();      // ДЕЙСТВИЕ: сдвинуть влево
                break;
            case InputType.MoveRight:
                engine.Game.MovePieceRight();     // ДЕЙСТВИЕ: сдвинуть вправо
                break;
            case InputType.MoveDown:
                engine.Game.MovePieceDown();      // ДЕЙСТВИЕ: сдвинуть вниз
                break;
            case InputType.Rotate:
                engine.Game.RotatePiece();        // ДЕЙСТВИЕ: повернуть
                break;
            case InputType.Pause:
                engine.ChangeState(new PausedState());  // ДЕЙСТВИЕ: смена состояния
                break;
        }
    }
}
```

**Роль**: `PlayingState` - это **Concrete Command Handler**, исполняет конкретную команду.

---

## 📊 Диаграмма потока Command

```
Пользователь нажимает клавишу
       ↓
InputService.HandleKeyPress("Left")
       ↓
        ┌─────────────────────────────────┐
        │  CreateCommand: GameInput        │
        │  Type: InputType.MoveLeft        │ ← ИНКАПСУЛЯЦИЯ
        │  Timestamp: DateTime.Now         │
        └─────────────────────────────────┘
       ↓
GameLoopService.SendCommand(InputType.MoveLeft)
       ↓
        ┌─────────────────────────────────┐
        │  _inputQueue.Enqueue(command)   │ ← ОЧЕРЕДЬ КОМАНД
        └─────────────────────────────────┘
       ↓
GameEngine.Update() (каждый кадр 60 раз в сек)
       ↓
while (_inputQueue.Count > 0)
       ↓
_currentState.HandleInput(engine, input)
       ↓
PlayingState.HandleInput() / PausedState.HandleInput() и т.д.
       ↓
Game.MovePieceLeft() или Game.Pause() или другое действие
       ↓
Observer уведомляет ViewModel о изменении
       ↓
WPF UI обновляет дисплей
```

---

## 🎓 Элементы паттерна Command в моем коде

| Элемент GoF | Класс в коде | Описание |
|-------------|-------------|---------|
| **Command** | `GameInput` | Объект, инкапсулирующий команду |
| **Invoker** | `InputService` | Инициирует создание команды |
| **Receiver** | `GameEngine` | Получает команду из очереди |
| **ConcreteCommand** | `InputType` enum | Конкретные типы команд |
| **CommandQueue** | `Queue<GameInput>` | Буферизирует команды |
| **Handler** | `PlayingState`, `PausedState` | Исполняет команду в зависимости от состояния |

---

## ✨ Преимущества использования Command

### 1. Инкапсуляция
```csharp
// ДО (без Command):
if (key == "Left") MovePieceLeft();
if (key == "Right") MovePieceRight();
// Жесткая связь между вводом и действием

// ПОСЛЕ (с Command):
var command = new GameInput(InputType.MoveLeft);
// Ввод отделен от действия
```

### 2. Очередь команд
```csharp
// Легко обрабатывать несколько команд в очереди
while (_inputQueue.Count > 0)
{
    var command = _inputQueue.Dequeue();
    _currentState.HandleInput(this, command);
}
// Даже если игрок быстро нажимает кнопки,
// все команды будут обработаны по очереди
```

### 3. Разные действия в разных состояниях
```csharp
// В PlayingState:
case InputType.MoveLeft:
    engine.Game.MovePieceLeft();  // Двигаем фигуру

// В PausedState:
case InputType.MoveLeft:
    // Ничего не происходит (игнорируем команду)
    break;

// В GameOverState:
case InputType.MoveLeft:
    // Ничего не происходит
    break;
```

### 4. Легко добавлять новые команды
```csharp
// Добавить новый тип команды:
public enum InputType
{
    // ... существующие
    HardDrop,     // ← НОВАЯ КОМАНДА
    Undo          // ← НОВАЯ КОМАНДА
}

// Обработать в состояниях:
public void HandleInput(GameEngine engine, GameInput input)
{
    switch (input.Type)
    {
        case InputType.HardDrop:
            engine.Game.HardDropPiece();
            break;
    }
}
```

### 5. История команд для логирования
```csharp
// Можно легко логировать все команды:
private List<GameInput> _commandHistory = new();

public void ProcessInput(GameInput input)
{
    _inputQueue.Enqueue(input);
    _commandHistory.Add(input);  // ← ИСТОРИЯ
}

// Потом можно проанализировать игру или сделать replay
```

---

## 🔍 Где еще используется Command в коде

### В RelayCommand (MVVM)
📍 **Файл**: [ViewModels/RelayCommand.cs](ViewModels/RelayCommand.cs)

```csharp
public class RelayCommand<T> : ICommand
{
    private Action<T> _execute;

    public void Execute(object parameter)
    {
        _execute.Invoke((T)parameter);  // ← ВЫПОЛНИТЬ КОМАНДУ
    }
}
```

**Использование в ViewModel**:
```csharp
StartGameCommand = new RelayCommand(_ => StartGameAsync());
PauseGameCommand = new RelayCommand(_ => PauseGame());
MoveLeftCommand = new RelayCommand(_ => _inputService.HandleKeyPress("Left"));
RotateCommand = new RelayCommand(_ => _inputService.HandleKeyPress("Space"));
```

Это ТОЖЕ Command паттерн (WPF MVVM вариант)!

---

## 📝 Итог

**Command паттерн в моем Tetris:**

1. ✅ **Инкапсулирован** в объект `GameInput` с типом из `InputType` enum
2. ✅ **Инициирован** через `InputService.HandleKeyPress()`
3. ✅ **Поставлен в очередь** в `GameLoopService` и `GameEngine`
4. ✅ **Обработан** в `PlayingState`, `PausedState`, `GameOverState`
5. ✅ **Исполнен** через методы `Game` класса

Это полная реализация паттерна **Command** из Gang of Four! 🎉
