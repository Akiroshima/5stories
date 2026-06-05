# Tetris Game - Доменная модель и WPF приложение

## 📋 Описание проекта

Полная реализация доменной модели для игры Tetris на C# с пользовательским интерфейсом WPF, реализующим паттерн MVVM и привязку данных.

Проект соответствует всем требованиям, указанным в пользовательских историях:
- ✅ Управление падающими фигурами
- ✅ Сохранение прогресса в текстовый файл
- ✅ Загрузка сохранённой игры
- ✅ Таблица рекордов
- ✅ Простая и расширяемая структура хранения данных

---

## 📁 Структура проекта

```
DomModel/
├── Models/                          # Доменные модели (классы для бизнес-логики)
│   ├── Point.cs                     # Структура для координат (X, Y)
│   ├── Tetromino.cs                 # Падающая фигура (7 типов)
│   ├── GameBoard.cs                 # Игровое поле (10x20)
│   ├── Score.cs                     # Система подсчёта очков и уровней
│   ├── Game.cs                      # Главный класс (агрегирует все модели)
│   ├── GameProgress.cs              # Сохранённый прогресс игры
│   └── LeaderBoardEntry.cs          # Запись в таблице рекордов
│
├── Interfaces/                      # Контракты (интерфейсы)
│   └── IGamePersistence.cs          # Интерфейсы для сохранения и загрузки
│
├── Services/                        # Сервисы (бизнес-логика работы с файлами)
│   ├── GamePersistenceService.cs    # Сохранение/загрузка игры в текстовые файлы
│   └── LeaderBoardService.cs        # Управление таблицей рекордов
│
├── ViewModels/                      # MVVM View Models (для WPF привязки)
│   ├── ViewModelBase.cs             # Базовый класс с INotifyPropertyChanged
│   ├── RelayCommand.cs              # Команды для привязки кнопок
│   ├── GameViewModel.cs             # View Model для игры (реализует IGameObserver)
│   ├── LeaderBoardViewModel.cs      # View Model для таблицы рекордов
│   └── Converters/
│       └── ValueConverters.cs       # Конвертеры для привязки данных WPF
│
├── Views/                           # WPF представления (XAML)
│   ├── GameWindow.xaml              # Главное окно игры
│   └── GameWindow.xaml.cs           # Code-behind
│
├── DOMAIN_MODEL.md                  # Подробная документация доменной модели
├── CLASS_DIAGRAM.txt                # ASCII диаграмма классов
├── USAGE_EXAMPLES.md                # Примеры использования
├── SETUP_INSTRUCTIONS.md            # Инструкции по настройке
└── README.md                        # Этот файл
```

---

## 🎮 Основные компоненты

### 1. **Models** - Доменные модели
- `Tetromino` - Падающие фигуры (I, O, T, S, Z, J, L) с ротацией
- `GameBoard` - Игровое поле 10x20 с логикой размещения фигур и очистки линий
- `Score` - Система расчёта очков с поддержкой уровней
- `Game` - Главный класс, агрегирующий все компоненты и реализующий Observer паттерн

### 2. **Services** - Сервисы
- `GamePersistenceService` - Сохранение/загрузка прогресса в текстовые файлы
- `LeaderBoardService` - Управление таблицей рекордов (TOP 10)

### 3. **ViewModels** - MVVM компоненты
- `GameViewModel` - Управляет состоянием UI, реализует паттерны:
  - **Observer** - подписан на события Game
  - **INotifyPropertyChanged** - привязка данных WPF
  - **Command Pattern** - команды для кнопок
- `ViewModelBase` - Базовый класс для всех ViewModel
- `RelayCommand` - Реализация ICommand для WPF

### 4. **Views** - WPF представления
- `GameWindow.xaml` - Главное окно с:
  - Игровым полем (Canvas)
  - Отображением счёта и уровня
  - Кнопками управления
  - Таблицей рекордов (DataGrid)
  - Привязкой данных к GameViewModel

---

## 🔄 Архитектурные паттерны

### 1. **MVVM** (Model-View-ViewModel)
```
GameWindow.xaml (View)
      ↓ (Data Binding)
GameViewModel (ViewModel)
      ↓ (использует)
Game + Services (Model)
```

### 2. **Observer** (Наблюдатель)
```
Game (Subject)
  ├─ Subscribe(observer)
  └─ Notify observers on:
      ├─ OnGameStateChanged()
      ├─ OnPieceSpawned()
      ├─ OnLineCleared()
      └─ OnGameOver()

GameViewModel (Observer)
  └─ Реализует IGameObserver
     └─ Обновляет UI при изменениях
```

### 3. **Aggregation** (Агрегация)
```
Game
  ├─ contains GameBoard (сост. поля)
  ├─ contains Score (очки)
  ├─ contains Tetromino (текущая)
  ├─ contains Tetromino (следующая)
  └─ contains List<IGameObserver>
```

### 4. **Dependency Injection**
```
Services регистрируются в контейнере DI
Services передаются через конструкторы
Позволяет легко тестировать и заменять реализации
```

---

## 💾 Формат сохранения данных

### Сохранённая игра (game_save.txt)
```
PLAYER:PlayerName
SCORE:15000
LINES:45
LEVEL:5
TIME:2026-05-16 14:30:45
BOARD:
0000000000
0000000000
...
1111101111
```

### Таблица рекордов (leaderboard.txt)
```
Player1;50000;120;10;2026-05-16 10:15:30
Player2;45000;110;9;2026-05-15 18:20:15
Player3;40000;100;8;2026-05-14 20:45:00
...
```

---

## 🎯 Привязка данных WPF (Data Binding)

### Доступные свойства в GameViewModel:
```csharp
CurrentScore         // Текущий счёт (int)
LinesCleared         // Очищенные линии (int)
Level                // Уровень (int)
GameStatus           // Статус игры (string)
IsPaused             // На паузе ли (bool)
GridState            // Состояние поля (bool[,])
LeaderBoard          // Таблица рекордов (ObservableCollection)
```

### Доступные команды (Commands):
```csharp
StartGameCommand      // Начать новую игру
PauseGameCommand      // Пауза
ResumeGameCommand     // Продолжить
MoveLeftCommand       // Влево
MoveRightCommand      // Вправо
MoveDownCommand       // Вниз
RotateCommand         // Повернуть
```

---

## 🔧 Технологии и версии

- **C# 9.0+** - Язык программирования
- **.NET 6.0 (Windows)** - Framework
- **WPF** - Windows Presentation Foundation для UI
- **XAML** - Язык разметки для UI
- **async/await** - Асинхронные операции

---

## 📖 Соответствие пользовательским историям

| История | Требование | Реализация |
|---------|-----------|------------|
| 1 | Управлять падающими фигурами | `Game.MovePiece*()`, `Game.RotatePiece()` |
| 2 | Сохранять прогресс | `GamePersistenceService.SaveGameAsync()` |
| 3 | Загружать сохранённую игру | `GamePersistenceService.LoadGameAsync()` |
| 4 | Видеть таблицу рекордов | `LeaderBoardService`, `DataGrid` в View |
| 5 | Простая структура данных | Текстовые файлы (CSV-подобный формат) |

---

## 🚀 Быстрый старт

### 1. Создать WPF приложение
```bash
dotnet new wpf -n TetrisGame
cd TetrisGame
```

### 2. Скопировать классы из DomModel в проект

### 3. Использовать в App.xaml
```xaml
<Application StartupUri="Views/GameWindow.xaml">
    ...
</Application>
```

### 4. Запустить приложение
```csharp
public partial class App : Application
{
    // MainWindow будет загружена автоматически
}
```

---

## 📚 Дополнительная документация

- [DOMAIN_MODEL.md](DOMAIN_MODEL.md) - Подробное описание доменной модели
- [CLASS_DIAGRAM.txt](CLASS_DIAGRAM.txt) - ASCII диаграмма классов
- [USAGE_EXAMPLES.md](USAGE_EXAMPLES.md) - Примеры использования API
- [SETUP_INSTRUCTIONS.md](SETUP_INSTRUCTIONS.md) - Инструкции по конфигурации

---

## 🎓 Ключевые концепции для изучения

1. **Доменное моделирование** - как проектировать классы, отражающие реальные объекты
2. **Паттерны проектирования** - Observer, Command, MVVM, Aggregation
3. **WPF и Data Binding** - привязка данных между View и ViewModel
4. **Async/Await** - асинхронное сохранение и загрузка данных
5. **Тестируемость** - разделение ответственности между слоями

---

## 🔮 Возможные расширения

- 🎵 **Звуки и музыка** - добавить `IAudioService`
- 🎨 **Темы и скины** - конфигурируемые цвета и стили
- 🌐 **Мультиплеер** - сетевая игра через SignalR
- 📊 **Статистика** - детальная статистика игрока
- ⚙️ **Уровни сложности** - разные скорости падения
- 🤖 **AI противник** - компьютерный игрок
- 💾 **Облачное сохранение** - синхронизация прогресса

---

## 📝 Лицензия

Проект создан в образовательных целях.

---

## 👨‍💻 Автор

Создано как демонстрация применения паттернов проектирования и архитектуры приложений на C#/WPF.

---

## 📞 Помощь

Для вопросов и замечаний обратитесь к документации или примерам использования в файле [USAGE_EXAMPLES.md](USAGE_EXAMPLES.md).
