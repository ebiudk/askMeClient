using askMeWindows.ViewModels;
using System.Windows;

namespace askMeWindows;

/// <summary>
/// メインウィンドウのコードビハインド
/// </summary>
public partial class MainWindow : Window
{
  public MainWindow()
  {
    InitializeComponent();
    DataContext = new MainWindowViewModel();
  }

  protected override async void OnClosing(System.ComponentModel.CancelEventArgs e)
  {
    if (DataContext is MainWindowViewModel viewModel)
    {
      await viewModel.OnClosing();
    }
    base.OnClosing(e);
  }
}
