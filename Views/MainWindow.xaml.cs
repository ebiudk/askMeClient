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

  protected override void OnStateChanged(System.EventArgs e)
  {
    if (WindowState == WindowState.Minimized)
    {
      this.Hide();
    }
    base.OnStateChanged(e);
  }

  protected override async void OnClosing(System.ComponentModel.CancelEventArgs e)
  {
    // Appが終了処理中であれば、そのまま閉じる
    if (Application.Current is App app && app.IsExiting)
    {
      if (DataContext is MainWindowViewModel viewModel)
      {
        await viewModel.OnClosing();
      }
      base.OnClosing(e);
      return;
    }

    // 終了処理中でなければ、ウィンドウを非表示にして終了をキャンセルする
    e.Cancel = true;
    this.Hide();
  }
}
