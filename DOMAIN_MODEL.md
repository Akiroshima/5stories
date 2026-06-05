# Доменная модель игры Tetris

## Обзор структуры проекта

```
DomModel/
├── Models/              # Доменные модели
│   ├── Point.cs        # Структура координаты
│   ├── Tetromino.cs    # Падающая фигура
│   ├── GameBoard.cs    # Игровое поле
│   ├── Score.cs        # Система очков
│   ├── Game.cs         # Главный класс игры (агрегирует модели)
│   ├── GameProgress.cs # Сохранённый прогресс
│   └── LeaderBoardEntry.cs # Запись в таблице рекордов
│
├── Interfaces/         # Контракты
│   └── IGamePersistence.cs # Интерфейсы для сохранения/загрузки
│
├── Services/           # Бизнес-логика
│   ├── GamePersistenceService.cs # Сохранение/загрузка в файлы
│   └── LeaderBoardService.cs    # Управление рекордами
│
├── ViewModels/         # MVVM для WPF
│   ├── ViewModelBase.cs       # Базовый класс с INotifyPropertyChanged
│   ├── RelayCommand.cs        # Команды для привязки к кнопкам
│   ├── GameViewModel.cs       # ViewModel для игры (реализует IGameObserver)
│   ├── LeaderBoardViewModel.cs # ViewModel для таблицы рекордов
│   └── Converters/
│       └── ValueConverters.cs  # Конвертеры для привязки данных
│
└── Views/              # WPF представления (XAML)
    ├── GameWindow.xaml    # Главное окно игры
    └── GameWindow.xaml.cs # Code-behind
```

## Ключевые особенности доменной модели

### 1. Наследование
- **IGameObserver** - интерфейс для наблюдателя
- **ViewModelBase** - базовый класс для всех ViewModel с реализацией `INotifyPropertyChanged`

### 2. Агрегация
- **Game** агрегирует:
  - `GameBoard` - состояние поля
  - `Score` - система очков
  - `Tetromino` - текущую фигуру
  - Список наблюдателей (`IGameObserver`)

### 3. Интерфейсы
- **IGamePersistence** - сохранение/загрузка игры
- **ILeaderBoardManager** - управление рекордами
- **IGameEngine** - управление движком (для расширения)
- **IGameObserver** - паттерн Observer для уведомлений

### 4. Паттерны
- **Observer** - Game уведомляет ViewModel об изменениях
- **MVVM** - разделение логики и UI через ViewModel
- **Dependency Injection** - готово к использованию сервисов
- **Repository Pattern** - GamePersistenceService и LeaderBoardService

## Привязка данных WPF (Data Binding)

### GameViewModel свойства, доступные для привязки:
```csharp
public bool[,] GridState           // Состояние игрового поля
public int CurrentScore            // Текущий счёт
public int LinesCleared           // Количество очищенных линий
public int Level                   // Текущий уровень
public string GameStatus           // Статус игры
public bool IsPaused              // Находится ли на паузе
public ObservableCollection<LeaderBoardEntry> LeaderBoard // Таблица рекордов
```

### Команды для привязки:
```csharp
StartGameCommand      // Начать новую игру
PauseGameCommand      // Пауза
ResumeGameCommand     // Продолжить
MoveLeftCommand       // Переместить влево
MoveRightCommand      // Переместить вправо
MoveDownCommand       // Переместить вниз
RotateCommand         // Повернуть фигуру
```

## Связи между классами

```
┌─────────────────────────────────────────────────────┐
│                    Presentation Layer               │
├─────────────────────────────────────────────────────┤
│  GameWindow (XAML) ←→ GameViewModel (INotifyPropertyChanged)
│                              ↑
│                    Реализует IGameObserver
├─────────────────────────────────────────────────────┤
│                     ViewModel Layer                  │
├─────────────────────────────────────────────────────┤
│                     Business Layer                   │
│  ┌─────────────────────────────────────────────────┐│
│  │ Game (главный класс)                           ││
│  │  ├─ GameBoard                                  ││
│  │  ├─ Score                                      ││
│  │  ├─ Tetromino (current)                        ││
│  │  ├─ Tetromino (next)                           ││
│  │  └─ List<IGameObserver>                        ││
│  └─────────────────────────────────────────────────┘│
├─────────────────────────────────────────────────────┤
│                      Service Layer                   │
│  GamePersistenceService     LeaderBoardService      │
│  ├─ SaveGameAsync()         ├─ GetLeaderBoardAsync()
│  └─ LoadGameAsync()         └─ SaveLeaderBoardAsync()
├─────────────────────────────────────────────────────┤
│                      Model Layer                     │
│  ├─ Tetromino               ├─ GameBoard
│  ├─ Score                   ├─ Point
│  ├─ GameProgress            └─ LeaderBoardEntry
└─────────────────────────────────────────────────────┘
```

## Пример использования

### Инициализация и запуск игры
```csharp
// Создать ViewModel
var gameVM = new GameViewModel();

// Запустить новую игру
gameVM.StartGame();

// Обработка ввода
gameVM.MoveLeftCommand.Execute(null);
gameVM.RotateCommand.Execute(null);

// Сохранение
var progress = gameVM.SaveGame("PlayerName");
await gamePersistence.SaveGameAsync(progress, "savegame.txt");

// Загрузка
var loaded = await gamePersistence.LoadGameAsync("savegame.txt");
gameVM.LoadGame(loaded);
```

### Работа с таблицей рекордов
```csharp
var leaderBoardService = new LeaderBoardService();
var entries = await leaderBoardService.GetLeaderBoardAsync("leaderboard.txt");

leaderBoardService.AddScore("Player1", 1500);
await leaderBoardService.SaveLeaderBoardAsync(entries, "leaderboard.txt");
```

## Доменные концепции

### TetrominoType
Перечисление типов фигур: `I, O, T, S, Z, J, L`

### GameState
Состояния игры: `NotStarted, Playing, Paused, GameOver`

### Point
Структура для координат на поле (X, Y)

### GameBoard (10×20)
- Проверка валидности позиций
- Проверка размещения фигур
- Очистка заполненных строк
- Обнаружение конца игры

## Расширяемость

### Добавление новых фич:
1. **Сложность уровней** - добавить параметр в `Game`
2. **Мультиплеер** - создать `MultiplayerGame` наследующий `Game`
3. **Анимации** - использовать `IGameObserver` для оповещений
4. **Звук** - добавить `IAudioService` в `Game`
5. **Статистика** - расширить `GameProgress`

## Соответствие историям

✅ История 1: Управление фигурами - `Game.MovePiece*()`, `Game.RotatePiece()`
✅ История 2: Сохранение - `GamePersistenceService.SaveGameAsync()`
✅ История 3: Загрузка - `GamePersistenceService.LoadGameAsync()`
✅ История 4: Таблица рекордов - `LeaderBoardService`
✅ История 5: Простая структура данных - текстовые файлы в `Services`
