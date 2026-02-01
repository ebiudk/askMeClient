using askMeWindows.ViewModels;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace askMeWindows.Views;

public partial class ApiSettingsControl : UserControl
{
  public ApiSettingsControl()
  {
    InitializeComponent();
  }

  private void ApiKeyBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
  {
    if (DataContext is MainWindowViewModel viewModel && sender is PasswordBox passwordBox)
    {
      viewModel.ApiKey = passwordBox.Password;
    }
  }

  private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
  {
    Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
    e.Handled = true;
  }

  private void AskMeWebButton_Click(object sender, System.Windows.RoutedEventArgs e)
  {
    Process.Start(new ProcessStartInfo("https://ask-me.ebiudk.link/") { UseShellExecute = true });
  }
}
