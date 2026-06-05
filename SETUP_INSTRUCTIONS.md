# App.xaml
```xaml
<Application x:Class="DomModel.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             StartupUri="Views/GameWindow.xaml">
    <Application.Resources>
    </Application.Resources>
</Application>
```

# App.xaml.cs
```csharp
using System.Windows;

namespace DomModel
{
    public partial class App : Application
    {
    }
}
```

# Файл проекта (.csproj)
```xml
<Project Sdk="Microsoft.NET.Sdk.WindowsDesktop">

  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net6.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <LangVersion>latest</LangVersion>
  </PropertyGroup>

</Project>
```

# Пример App.config для использования файлов
```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="SaveGameDirectory" value="./Saves"/>
    <add key="LeaderBoardFilePath" value="./Data/leaderboard.txt"/>
  </appSettings>
</configuration>
```
