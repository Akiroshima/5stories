using System;
using System.Diagnostics;
using System.Windows;

namespace DomModel
{
    public partial class App : Application
    {
        public App()
        {
            // Обработчик необработанных исключений
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                string msg = $"❌ КРИТИЧЕСКАЯ ОШИБКА:\n{ex?.Message}\n\n{ex?.StackTrace}";
                Debug.WriteLine(msg);
                MessageBox.Show(msg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            };

            // Обработчик необработанных исключений в потоках Dispatcher
            this.DispatcherUnhandledException += (s, e) =>
            {
                string msg = $"❌ ОШИБКА В ПОТОКЕ UI:\n{e.Exception.Message}\n\n{e.Exception.StackTrace}";
                Debug.WriteLine(msg);
                MessageBox.Show(msg, "Ошибка UI", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = false; // Позволить приложению завершиться
            };
        }
    }
}
