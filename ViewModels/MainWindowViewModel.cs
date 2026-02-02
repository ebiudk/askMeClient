using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;
using askMeWindows.Models;
using askMeWindows.Services;

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace askMeWindows.ViewModels;

/// <summary>
/// メインウィンドウ用ViewModel
/// 責務: UI状態管理とサービスの連携
/// </summary>
public class MainWindowViewModel : INotifyPropertyChanged
{
  private readonly VRChatLogMonitorService _monitorService = new VRChatLogMonitorService();
  private CancellationTokenSource? _cancellationTokenSource;
  private bool _isMonitoring;
  private string _status = "待機中";
  private InstanceInfo? _currentInstance;
  private double _pollingInterval = 1.0;
  private string _apiKey = "";
  private bool _isLocationHidden;

  public event PropertyChangedEventHandler? PropertyChanged;

  public MainWindowViewModel()
  {
    _monitorService.InstanceChanged += OnInstanceChanged;
    _monitorService.LogFileUpdated += OnLogFileUpdated;
    _monitorService.ErrorOccurred += OnErrorOccurred;

    StartCommand = new RelayCommand(Start, () => !IsMonitoring);
    StopCommand = new RelayCommand(Stop, () => IsMonitoring);
    CopyUrlCommand = new RelayCommand(CopyUrl, () => CurrentInstance != null);
    OpenUrlCommand = new RelayCommand(OpenUrl, () => CurrentInstance != null);

    // 初期状態をチェック
    var logFile = LogFileFinder.FindLatestLogFile();
    if (string.IsNullOrEmpty(logFile))
    {
      Status = "⚠ VRChatログファイルが見つかりません";
    }
    else
    {
      Status = $"準備完了: {System.IO.Path.GetFileName(logFile)}";
      Start(); // アプリ起動時にポーリングを開始
    }
  }

  public bool IsMonitoring
  {
    get => _isMonitoring;
    set
    {
      if (_isMonitoring != value)
      {
        _isMonitoring = value;
        OnPropertyChanged();
        ((RelayCommand)StartCommand).RaiseCanExecuteChanged();
        ((RelayCommand)StopCommand).RaiseCanExecuteChanged();
      }
    }
  }

  public string Status
  {
    get => _status;
    set
    {
      if (_status != value)
      {
        _status = value;
        OnPropertyChanged();
      }
    }
  }

  public InstanceInfo? CurrentInstance
  {
    get => _currentInstance;
    set
    {
      if (_currentInstance != value)
      {
        _currentInstance = value;
        OnPropertyChanged();
        OnPropertyChanged(nameof(DisplayWorldName));
        OnPropertyChanged(nameof(DisplayInstanceId));
        OnPropertyChanged(nameof(DisplayLaunchUrl));
        ((RelayCommand)CopyUrlCommand).RaiseCanExecuteChanged();
        ((RelayCommand)OpenUrlCommand).RaiseCanExecuteChanged();
      }
    }
  }

  public string ApiKey
  {
    get => _apiKey;
    set
    {
      if (_apiKey != value)
      {
        _apiKey = value;
        OnPropertyChanged();
      }
    }
  }

  public bool IsLocationHidden
  {
    get => _isLocationHidden;
    set
    {
      if (_isLocationHidden != value)
      {
        _isLocationHidden = value;
        OnPropertyChanged();
        OnPropertyChanged(nameof(DisplayWorldName));
        OnPropertyChanged(nameof(DisplayInstanceId));
        OnPropertyChanged(nameof(DisplayLaunchUrl));

        // ステータス表示の更新
        if (CurrentInstance != null)
        {
          Status = $"[{CurrentInstance.DetectedAt:HH:mm:ss}] {CurrentInstance.WorldName}";

          // API通知
          _ = UpdateApiLocation(CurrentInstance);
        }
      }
    }
  }

  private async Task UpdateApiLocation(InstanceInfo instance)
  {
    if (!string.IsNullOrWhiteSpace(ApiKey))
    {
      string worldIdToSend = IsLocationHidden ? "private" : instance.WorldId;
      string? worldNameToSend = IsLocationHidden ? null : instance.WorldName;
      string? instanceIdToSend = IsLocationHidden ? null : instance.InstanceId;

      await ApiService.UpdateLocationAsync(ApiKey, worldIdToSend, worldNameToSend, instanceIdToSend, null);
    }
  }

  public string DisplayWorldName => CurrentInstance?.WorldName ?? "未検出";
  public string DisplayInstanceId => CurrentInstance?.InstanceId ?? "未検出";
  public string DisplayLaunchUrl => CurrentInstance?.LaunchUrl ?? "未検出";

  public double PollingInterval
  {
    get => _pollingInterval;
    set
    {
      if (_pollingInterval != value)
      {
        _pollingInterval = value;
        _monitorService.PollingInterval = value;
        OnPropertyChanged();
      }
    }
  }

  public ObservableCollection<InstanceInfo> InstanceHistory { get; } =
      new ObservableCollection<InstanceInfo>();

  public ICommand StartCommand { get; }
  public ICommand StopCommand { get; }
  public ICommand CopyUrlCommand { get; }
  public ICommand OpenUrlCommand { get; }

  private async void Start()
  {
    if (IsMonitoring) return;
    try
    {
      IsMonitoring = true;
      Status = "監視中...";
      _cancellationTokenSource = new CancellationTokenSource();

      await _monitorService.StartAsync(_cancellationTokenSource.Token);
    }
    catch (OperationCanceledException)
    {
      // 正常なキャンセル
    }
    catch (Exception ex)
    {
      Status = $"エラー: {ex.Message}";
    }
    finally
    {
      IsMonitoring = false;
      if (Status == "監視中...") Status = "停止しました";
    }
  }

  private async void Stop()
  {
    if (!IsMonitoring) return;

    _cancellationTokenSource?.Cancel();
    IsMonitoring = false;
    Status = "停止中...";

    try
    {
      if (!string.IsNullOrWhiteSpace(ApiKey))
      {
        await ApiService.ClearLocationAsync(ApiKey);
      }
    }
    finally
    {
      Status = "停止しました";
    }
  }

  public async Task OnClosing()
  {
    if (!string.IsNullOrWhiteSpace(ApiKey))
    {
      await ApiService.ClearLocationAsync(ApiKey);
    }
  }

  private void CopyUrl()
  {
    if (CurrentInstance?.LaunchUrl != null)
    {
      try
      {
        System.Windows.Clipboard.SetText(CurrentInstance.LaunchUrl);
        Status = "URLをコピーしました";
      }
      catch
      {
        Status = "コピーに失敗しました";
      }
    }
  }

  private void OpenUrl()
  {
    if (CurrentInstance?.LaunchUrl != null)
    {
      try
      {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
          FileName = CurrentInstance.LaunchUrl,
          UseShellExecute = true
        });
      }
      catch
      {
        Status = "URLを開けません";
      }
    }
  }

  private async void OnInstanceChanged(object? sender, InstanceInfo instance)
  {
    CurrentInstance = instance;

    // オフライン時は履歴に追加しない
    if (instance.WorldId != "offline")
    {
      InstanceHistory.Insert(0, instance);

      // 履歴は最大100件
      if (InstanceHistory.Count > 100)
        InstanceHistory.RemoveAt(100);
    }

    Status = $"[{instance.DetectedAt:HH:mm:ss}] {instance.WorldName}";

    // API連携
    await UpdateApiLocation(instance);
  }

  private void OnLogFileUpdated(object? sender, string logFileName)
  {
    Status = $"監視中: {logFileName}";
  }

  private void OnErrorOccurred(object? sender, string message)
  {
    Status = $"警告: {message}";
  }

  protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
  {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
  }
}

/// <summary>
/// ICommand実装のヘルパークラス
/// </summary>
public class RelayCommand : ICommand
{
  private readonly Action _execute;
  private readonly Func<bool>? _canExecute;

  public event EventHandler? CanExecuteChanged
  {
    add { CommandManager.RequerySuggested += value; }
    remove { CommandManager.RequerySuggested -= value; }
  }

  public RelayCommand(Action execute, Func<bool>? canExecute = null)
  {
    _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    _canExecute = canExecute;
  }

  public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

  public void Execute(object? parameter) => _execute();

  public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
}
