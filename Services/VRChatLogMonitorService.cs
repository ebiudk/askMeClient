using System.IO;
using askMeWindows.Models;

namespace askMeWindows.Services;

/// <summary>
/// VRChatログを監視するサービス
/// 責務: ログファイル監視と複数行ログ統合
/// </summary>
public class VRChatLogMonitorService
{
  private LogTailer? _tailer;
  private string? _currentLogFile;
  private string? _pendingWorldName;
  private string? _lastInstanceId;

  public event EventHandler<InstanceInfo>? InstanceChanged;
  public event EventHandler<string>? LogFileUpdated;
  public event EventHandler<string>? ErrorOccurred;

  public double PollingInterval { get; set; } = 1.0;

  /// <summary>
  /// ログファイルを探して必要に応じて切り替える
  /// </summary>
  /// <returns>ログファイルの準備ができているかどうか</returns>
  private bool FindAndSwitchLogFile()
  {
    var latestLog = LogFileFinder.FindLatestLogFile();

    if (string.IsNullOrEmpty(latestLog))
    {
      if (string.IsNullOrEmpty(_currentLogFile))
      {
        ErrorOccurred?.Invoke(this, "VRChat ログファイルが見つかりません。");
      }
      return false;
    }

    // 新しいログファイルに切り替え
    if (_currentLogFile != latestLog)
    {
      _tailer?.Close();

      _currentLogFile = latestLog;
      _tailer = new LogTailer(latestLog);

      if (_tailer.Open())
      {
        LogFileUpdated?.Invoke(this, System.IO.Path.GetFileName(latestLog));
        return true;
      }
      else
      {
        ErrorOccurred?.Invoke(this, $"ログファイルを開けません: {latestLog}");
        _tailer = null;
        return false;
      }
    }

    return _tailer != null;
  }

  /// <summary>
  /// ログファイルから新しい行を処理
  /// </summary>
  private void ProcessLogLines()
  {
    if (_tailer == null)
      return;

    foreach (var line in _tailer.GetNewLines())
    {
      // ワールド名を抽出（[Behaviour] Entering Room: xxx）
      var worldName = InstanceParser.ParseEnteringRoom(line);
      if (worldName != null)
      {
        _pendingWorldName = worldName;
        continue;
      }

      // インスタンスIDを抽出（Joining wrld_xxx）
      var fullInstanceId = InstanceParser.ParseInstanceId(line);
      if (fullInstanceId != null)
      {
        // 最後に検出されたワールド名と組み合わせる
        var world = _pendingWorldName ?? "Unknown";
        var launchUrl = InstanceParser.CreateLaunchUrl(fullInstanceId);
        var (worldId, instanceIdPart) = InstanceParser.SplitInstanceId(fullInstanceId);

        var instance = new InstanceInfo
        {
          WorldName = world,
          WorldId = worldId,
          InstanceId = instanceIdPart,
          FullInstanceId = fullInstanceId,
          LaunchUrl = launchUrl,
          DetectedAt = DateTime.Now
        };

        // 前回と異なるインスタンスの場合のみ通知
        if (_lastInstanceId != fullInstanceId)
        {
          InstanceChanged?.Invoke(this, instance);
          _lastInstanceId = fullInstanceId;
        }

        // 次のワールド検出に備えてリセット
        _pendingWorldName = null;
        continue;
      }
    }
  }

  /// <summary>
  /// 監視を開始（別スレッドで実行）
  /// </summary>
  /// <param name="cancellationToken">キャンセルトークン</param>
  public async Task StartAsync(CancellationToken cancellationToken)
  {
    while (!cancellationToken.IsCancellationRequested)
    {
      try
      {
        // ログファイルの存在を確認し、必要に応じて切り替え
        if (!FindAndSwitchLogFile())
        {
          await Task.Delay(TimeSpan.FromSeconds(PollingInterval), cancellationToken);
          continue;
        }

        // ログファイルの新しい行を処理
        ProcessLogLines();

        await Task.Delay(TimeSpan.FromSeconds(PollingInterval), cancellationToken);
      }
      catch (OperationCanceledException)
      {
        break;
      }
      catch (Exception ex)
      {
        ErrorOccurred?.Invoke(this, $"エラー: {ex.Message}");
        await Task.Delay(TimeSpan.FromSeconds(PollingInterval), cancellationToken);
      }
    }
  }

  /// <summary>
  /// リソースをクリーンアップ
  /// </summary>
  public void Dispose()
  {
    _tailer?.Close();
    _tailer?.Dispose();
  }
}
