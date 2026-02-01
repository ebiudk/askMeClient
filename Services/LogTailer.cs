namespace askMeWindows.Services;

using System.IO;

/// <summary>
/// ログファイルをテーリング（最新部分を監視）するクラス
/// 責務: ログファイルの読み込みと行単位での監視
/// </summary>
public class LogTailer : IDisposable
{
  private readonly string _logFilePath;
  private FileStream? _fileStream;
  private StreamReader? _reader;
  private long _lastPosition;

  public LogTailer(string logFilePath)
  {
    _logFilePath = logFilePath ?? throw new ArgumentNullException(nameof(logFilePath));
    _lastPosition = 0;
  }

  /// <summary>
  /// ログファイルを開く
  /// </summary>
  /// <returns>ファイルが正常に開けたかどうか</returns>
  public bool Open()
  {
    try
    {
      if (!File.Exists(_logFilePath))
        return false;

      Close();

      _fileStream = new FileStream(
          _logFilePath,
          FileMode.Open,
          FileAccess.Read,
          FileShare.ReadWrite,
          4096,
          FileOptions.SequentialScan
      );

      _reader = new StreamReader(_fileStream, System.Text.Encoding.UTF8, true);

      // ファイルの最後に移動
      _fileStream.Seek(0, SeekOrigin.End);
      _lastPosition = _fileStream.Position;

      return true;
    }
    catch (Exception ex)
    {
      System.Diagnostics.Debug.WriteLine($"ログファイルを開けません: {ex.Message}");
      Close();
      return false;
    }
  }

  /// <summary>
  /// 新しい行を取得する
  /// </summary>
  /// <returns>新しく追加された行のコレクション</returns>
  public IEnumerable<string> GetNewLines()
  {
    if (_reader == null || _fileStream == null)
      return Enumerable.Empty<string>();

    try
    {
      var lines = new List<string>();
      _fileStream.Seek(_lastPosition, SeekOrigin.Begin);

      string? line;
      while ((line = _reader.ReadLine()) != null)
      {
        lines.Add(line);
      }

      _lastPosition = _fileStream.Position;
      return lines;
    }
    catch (Exception ex)
    {
      System.Diagnostics.Debug.WriteLine($"ログ読み込みエラー: {ex.Message}");
      return Enumerable.Empty<string>();
    }
  }

  /// <summary>
  /// ログファイルを閉じる
  /// </summary>
  public void Close()
  {
    _reader?.Dispose();
    _fileStream?.Dispose();
    _reader = null;
    _fileStream = null;
  }

  public void Dispose()
  {
    Close();
    GC.SuppressFinalize(this);
  }
}
