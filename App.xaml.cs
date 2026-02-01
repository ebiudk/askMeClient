using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace askMeWindows;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
  public App()
  {
    AppDomain.CurrentDomain.UnhandledException += (s, e) =>
    {
      var exception = e.ExceptionObject as Exception;
      var message = $"予期しないエラーが発生しました:\n{exception?.Message}\n\n{exception?.StackTrace}";
      MessageBox.Show(message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
    };

    DispatcherUnhandledException += (s, e) =>
    {
      var message = $"UIエラーが発生しました:\n{e.Exception.Message}\n\n{e.Exception.StackTrace}";
      MessageBox.Show(message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
      e.Handled = true;
    };
  }
}
