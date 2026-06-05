# 📑 Индекс файлов доменной модели Tetris

## 🎯 Быстрая навигация

### Models (Доменные модели - бизнес-логика)
| Файл | Описание | Ключевые классы |
|------|---------|-----------------|
| `Point.cs` | Структура координат | `Point` |
| `Tetromino.cs` | Падающие фигуры | `Tetromino`, `TetrominoType` |
| `GameBoard.cs` | Игровое поле | `GameBoard` |
| `Score.cs` | Система очков | `Score` |
| `Game.cs` | Главный класс | `Game`, `GameState` |
| `GameProgress.cs` | Сохранённый прогресс | `GameProgress` |
| `LeaderBoardEntry.cs` | Запись в рекордах | `LeaderBoardEntry` |

### Services (Сервисы - работа с данными)
| Файл | Описание | Интерфейсы |
|------|---------|-----------|
| `GamePersistenceService.cs` | Сохранение/загрузка | `IGamePersistence` |
| `LeaderBoardService.cs` | Управление рекордами | `ILeaderBoardManager` |

### Interfaces (Контракты)
| Файл | Описание | Интерфейсы |
|------|---------|-----------|
| `IGamePersistence.cs` | Контракты для сервисов | `IGamePersistence`, `ILeaderBoardManager`, `IGameEngine`, `IGameObserver` |

### ViewModels (MVVM для WPF)
| Файл | Описание | Базовые классы |
|------|---------|-----------------|
| `ViewModelBase.cs` | Базовый класс | `INotifyPropertyChanged` |
| `RelayCommand.cs` | Команды для UI | `ICommand` |
| `GameViewModel.cs` | ViewModel игры | `ViewModelBase`, `IGameObserver` |
| `LeaderBoardViewModel.cs` | ViewModel рекордов | `ViewModelBase` |
| `Converters/ValueConverters.cs` | Конвертеры данных | `IValueConverter` |

### Views (WPF UI)
| Файл | Описание | Назначение |
|------|---------|-----------|
| `GameWindow.xaml` | XAML разметка | UI главного окна |
| `GameWindow.xaml.cs` | Code-behind | Логика окна |

### Документация
| Файл | Описание | Для кого |
|------|---------|---------|
| `README.md` | Главная документация | Все |
| `DOMAIN_MODEL.md` | Подробное описание модели | Разработчики |
| `CLASS_DIAGRAM.txt` | ASCII диаграмма классов | Архитектура |
| `USAGE_EXAMPLES.md` | Примеры кода | Разработчики |
| `SETUP_INSTRUCTIONS.md` | Настройка проекта | Новички |
| `FILE_INDEX.md` | Этот файл | Навигация |

---

## 🔍 Описание каждого файла

### 🎯 Models/Point.cs
```
Назначение: Представление координат на поле игры
Содержит: Структура Point с X, Y координатами
Используется: Везде, где нужны координаты (фигуры, поле)
Количество кода: ~40 строк
Зависимости: Нет
```

### 🎯 Models/Tetromino.cs
```
Назначение: Представление падающих фигур
Содержит: 
  - enum TetrominoType (7 типов фигур)
  - class Tetromino (логика ротации и движения)
  - Шаблоны всех 7 типов фигур
Используется: Game, GameBoard
Количество кода: ~180 строк
Ключевые методы:
  - GetAbsoluteBlocks(): Получить абсолютные координаты блоков
  - Rotate(): Повернуть на 90 градусов
  - Move(dx, dy): Переместить фигуру
```

### 🎯 Models/GameBoard.cs
```
Назначение: Представление игрового поля 10x20
Содержит:
  - bool[,] сетка (10 ширина, 20 высота)
  - Логика размещения фигур
  - Логика очистки заполненных строк
  - Проверка конца игры
Используется: Game
Количество кода: ~140 строк
Ключевые методы:
  - CanPlacePiece(piece): Может ли разместиться фигура
  - PlacePiece(piece): Разместить фигуру
  - ClearCompletedLines(): Очистить полные линии
  - IsGameOver(): Проверить конец игры
```

### 🎯 Models/Score.cs
```
Назначение: Управление очками и уровнем
Содержит:
  - Текущий счёт
  - Количество очищенных линий
  - Расчёт уровня
  - Система подсчёта очков
Используется: Game, GameViewModel
Количество кода: ~70 строк
Ключевые методы:
  - AddDropPoints(height): Очки за падение
  - AddLinePoints(count): Очки за линии
  - Reset(): Сброс
```

### 🎯 Models/Game.cs
```
Назначение: ГЛАВНЫЙ класс игры (агрегирует все компоненты)
Содержит:
  - GameBoard, Score, Tetromino (текущая и следующая)
  - enum GameState
  - Список наблюдателей (Observer паттерн)
  - Вся логика игры
Используется: GameViewModel
Количество кода: ~250 строк
Ключевые методы:
  - StartNewGame(), LoadGame(), Pause(), Resume(), EndGame()
  - MovePiece*()/RotatePiece()
  - PlaceCurrentPiece()
  - Subscribe()/Unsubscribe() для Observer паттерна
```

### 🎯 Models/GameProgress.cs
```
Назначение: Представление сохранённого состояния игры
Содержит: Данные для сохранения/загрузки
  - PlayerName, CurrentScore, LinesCleared, Level
  - SaveTime, BoardState
Используется: GamePersistenceService, GameViewModel
Количество кода: ~25 строк
```

### 🎯 Models/LeaderBoardEntry.cs
```
Назначение: Запись в таблице рекордов
Содержит:
  - PlayerName, Score, LinesCleared, Level, AchievedDate
  - IComparable<T> для сортировки
Используется: LeaderBoardService, LeaderBoardViewModel
Количество кода: ~45 строк
```

### 🎯 Services/GamePersistenceService.cs
```
Назначение: Сохранение и загрузка игры в текстовые файлы
Реализует: IGamePersistence
Содержит:
  - SaveGameAsync(): Сохранить в текстовый файл
  - LoadGameAsync(): Загрузить из текстового файла
Используется: GameViewModel, главное приложение
Количество кода: ~100 строк
Формат: Текстовые файлы с ключ=значение и битовой картой поля
```

### 🎯 Services/LeaderBoardService.cs
```
Назначение: Управление таблицей рекордов TOP-10
Реализует: ILeaderBoardManager
Содержит:
  - GetLeaderBoardAsync(): Получить рекорды
  - SaveLeaderBoardAsync(): Сохранить рекорды
  - AddScore(): Добавить новый результат
  - IsHighScore(): Проверить, рекордный ли результат
Используется: Приложение, UI
Количество кода: ~80 строк
```

### 🎯 Interfaces/IGamePersistence.cs
```
Назначение: Определение контрактов для сервисов
Содержит интерфейсы:
  - IGamePersistence (сохранение/загрузка)
  - ILeaderBoardManager (рекорды)
  - IGameEngine (опция для расширения)
  - IGameObserver (паттерн Observer)
Используется: Services, Game, ViewModel
Количество кода: ~60 строк
```

### 🎯 ViewModels/ViewModelBase.cs
```
Назначение: Базовый класс для всех ViewModels
Реализует: INotifyPropertyChanged
Содержит:
  - OnPropertyChanged(name)
  - SetProperty<T>(ref storage, value)
Используется: Все ViewModel классы
Количество кода: ~35 строк
Назначение: Привязка данных WPF
```

### 🎯 ViewModels/RelayCommand.cs
```
Назначение: Реализация ICommand для WPF
Содержит:
  - RelayCommand (общий)
  - RelayCommand<T> (типизированный)
Используется: GameViewModel для кнопок
Количество кода: ~60 строк
Назначение: Привязка кнопок к методам ({Binding Command})
```

### 🎯 ViewModels/GameViewModel.cs
```
Назначение: ГЛАВНЫЙ ViewModel для UI игры
Наследует: ViewModelBase
Реализует: IGameObserver
Содержит:
  - Свойства для привязки (CurrentScore, Level и т.д.)
  - Команды для кнопок (StartGame, Pause и т.д.)
  - Логика обновления UI при изменениях игры
Используется: GameWindow (XAML)
Количество кода: ~150 строк
Ключевые свойства для привязки:
  - CurrentScore, LinesCleared, Level, GameStatus, IsPaused
  - GridState, LeaderBoard
```

### 🎯 ViewModels/LeaderBoardViewModel.cs
```
Назначение: ViewModel для таблицы рекордов
Наследует: ViewModelBase
Содержит:
  - ObservableCollection<LeaderBoardEntry> для привязки в DataGrid
  - Методы загрузки и добавления записей
Используется: GameWindow (для отображения рекордов)
Количество кода: ~35 строк
```

### 🎯 ViewModels/Converters/ValueConverters.cs
```
Назначение: Конвертеры для привязки данных WPF
Реализует: IValueConverter
Содержит:
  - CellColorConverter: bool → цвет ячейки
  - GameStatusConverter: статус → текст
  - BoolToVisibilityConverter: bool → видимость
Используется: GameWindow.xaml (в Bindings)
Количество кода: ~60 строк
```

### 🎯 Views/GameWindow.xaml
```
Назначение: XAML разметка главного окна
Содержит:
  - Canvas для игрового поля
  - TextBlocks для счёта, уровня, статуса
  - Кнопки управления
  - DataGrid для таблицы рекордов
  - Привязка данных ({Binding ...})
  - Привязка команд ({Binding Command})
Используется: Главное окно приложения
Количество кода: ~120 строк
DataContext: Автоматически устанавливается в xaml.cs
```

### 🎯 Views/GameWindow.xaml.cs
```
Назначение: Code-behind для GameWindow
Содержит:
  - Инициализация DataContext = new GameViewModel()
  - Обработка клавиш KeyDown (опционально)
  - Отрисовка игрового поля (опционально)
Используется: WPF framework
Количество кода: ~20 строк
```

---

## 🏗️ Архитектурные слои

```
┌─────────────────────────────────────┐
│   PRESENTATION LAYER (Views)        │
│  GameWindow.xaml + GameWindow.xaml.cs
└─────────────────────────────────────┘
                   ↓
         ════════════════════
         Data Binding (XAML)
         ════════════════════
                   ↓
┌─────────────────────────────────────┐
│   VIEWMODEL LAYER                   │
│  ViewModelBase, GameViewModel,      │
│  RelayCommand, Converters           │
└─────────────────────────────────────┘
                   ↓
┌─────────────────────────────────────┐
│   BUSINESS LOGIC LAYER (Models)     │
│  Game, GameBoard, Score, Tetromino  │
└─────────────────────────────────────┘
                   ↓
┌─────────────────────────────────────┐
│   SERVICE LAYER                     │
│  GamePersistenceService,            │
│  LeaderBoardService                 │
└─────────────────────────────────────┘
                   ↓
┌─────────────────────────────────────┐
│   DATA LAYER                        │
│  Текстовые файлы (сохранения)       │
└─────────────────────────────────────┘
```

---

## 📊 Статистика проекта

| Категория | Количество |
|-----------|-----------|
| Файлов классов | 15 |
| Строк кода (всего) | ~1500 |
| Интерфейсов | 4 |
| Перечислений (enum) | 2 |
| Классов | 13 |
| Структур (struct) | 1 |
| Методов (всего) | ~80 |
| Свойств для привязки | 8 |
| Команд для UI | 7 |
| Конвертеров | 3 |

---

## 🎓 Обучающие концепции по файлам

| Файл | Концепция |
|------|-----------|
| Tetromino.cs | Enum, Dictionary, Полиморфизм |
| GameBoard.cs | 2D массивы, Валидация, Алгоритмы |
| Game.cs | Агрегация, Observer паттерн |
| Services | Async/Await, File I/O |
| ViewModels | MVVM, Data Binding, INotifyPropertyChanged |
| Converters | IValueConverter, типизация |
| GameWindow.xaml | XAML синтаксис, Binding, Commands |

---

## 🚀 С чего начать

1. **Изучить модели**: `Point.cs` → `Tetromino.cs` → `GameBoard.cs` → `Game.cs`
2. **Понять сервисы**: `GamePersistenceService.cs` → `LeaderBoardService.cs`
3. **MVVM слой**: `ViewModelBase.cs` → `RelayCommand.cs` → `GameViewModel.cs`
4. **UI**: `GameWindow.xaml` → `GameWindow.xaml.cs`
5. **Запустить** и тестировать

---

## 📚 Рекомендуемое чтение

1. Сначала `README.md` - для обзора
2. Затем `DOMAIN_MODEL.md` - для понимания структуры
3. Потом `CLASS_DIAGRAM.txt` - для визуализации
4. Наконец `USAGE_EXAMPLES.md` - для практики

---

Готово! 🎉 Доменная модель Tetris полностью создана и задокументирована.
