using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;

namespace askMeWindows;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
  [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
  private static extern bool DestroyIcon(IntPtr handle);

  private NotifyIcon? _notifyIcon;
  private Icon? _icon;
  private IntPtr _iconHandle = IntPtr.Zero;
  private bool _isExiting = false;

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

  protected override void OnStartup(StartupEventArgs e)
  {
    base.OnStartup(e);
    SetupNotifyIcon();
  }

  private void SetupNotifyIcon()
  {
    _notifyIcon = new NotifyIcon();

    // アイコンの設定 (logo.pngがあれば使用、なければ標準)
    try
    {
      string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.png");
      if (File.Exists(logoPath))
      {
        using var stream = File.OpenRead(logoPath);
        using var bitmap = new Bitmap(stream);
        _iconHandle = bitmap.GetHicon();
        _icon = Icon.FromHandle(_iconHandle);
        _notifyIcon.Icon = _icon;
      }
      else
      {
        _notifyIcon.Icon = SystemIcons.Application;
      }
    }
    catch
    {
      _notifyIcon.Icon = SystemIcons.Application;
    }

    _notifyIcon.Visible = true;
    _notifyIcon.Text = "AskMe! VRChat Log Monitor";

    var contextMenu = new ContextMenuStrip();
    contextMenu.Items.Add("開く", null, (s, e) => ShowMainWindow());
    contextMenu.Items.Add("-");
    contextMenu.Items.Add("終了", null, (s, e) => ExitApplication());

    _notifyIcon.ContextMenuStrip = contextMenu;
    _notifyIcon.DoubleClick += (s, e) => ShowMainWindow();
  }

  private void ShowMainWindow()
  {
    var mainWindow = Application.Current.MainWindow;
    if (mainWindow != null)
    {
      mainWindow.Show();
      mainWindow.Activate();
      if (mainWindow.WindowState == WindowState.Minimized)
      {
        mainWindow.WindowState = WindowState.Normal;
      }
    }
  }

  public void ExitApplication()
  {
    _isExiting = true;
    _notifyIcon?.Dispose();

    if (_icon != null)
    {
      _icon.Dispose();
      _icon = null;
    }

    if (_iconHandle != IntPtr.Zero)
    {
      DestroyIcon(_iconHandle);
      _iconHandle = IntPtr.Zero;
    }

    Application.Current.Shutdown();
  }

  public bool IsExiting => _isExiting;

  protected override void OnExit(ExitEventArgs e)
  {
    _notifyIcon?.Dispose();

    if (_icon != null)
    {
      _icon.Dispose();
      _icon = null;
    }

    if (_iconHandle != IntPtr.Zero)
    {
      DestroyIcon(_iconHandle);
      _iconHandle = IntPtr.Zero;
    }

    base.OnExit(e);
  }
}
