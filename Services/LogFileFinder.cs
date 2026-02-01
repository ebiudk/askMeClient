namespace askMeWindows.Services;

using System.IO;

/// <summary>
/// VRChatログファイルの検出機能を専門とするクラス
/// 責務: VRChatログファイルの場所特定と最新ファイルの検出
/// </summary>
public static class LogFileFinder
{
  private static readonly string[] WindowsLogPaths =
  {
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "..",
            "LocalLow",
            "VRChat",
            "VRChat"
        ),
    };

  private const string LogFilePrefix = "output_log_";
  private const string LogFileSuffix = ".txt";

  /// <summary>
  /// 実行環境に応じた検索パスのリストを返す
  /// </summary>
  /// <returns>検索対象のパスリスト</returns>
  private static List<string> GetSearchPaths()
  {
    var paths = new List<string>();

    // ネイティブWindows環境
    foreach (var basePath in WindowsLogPaths)
    {
      if (Directory.Exists(basePath))
        paths.Add(basePath);

      var crashesPath = Path.Combine(basePath, "Crashes");
      if (Directory.Exists(crashesPath))
        paths.Add(crashesPath);
    }

    // WSL 環境対応
    const string wslUsersPath = @"/mnt/c/Users";
    if (Directory.Exists(wslUsersPath))
    {
      try
      {
        var usersDir = new DirectoryInfo(wslUsersPath);
        foreach (var userDir in usersDir.GetDirectories())
        {
          if (userDir.Name.StartsWith("."))
            continue;

          var logPath = Path.Combine(
              userDir.FullName,
              "AppData",
              "LocalLow",
              "VRChat",
              "VRChat"
          );

          if (Directory.Exists(logPath))
            paths.Add(logPath);

          var crashesPath = Path.Combine(logPath, "Crashes");
          if (Directory.Exists(crashesPath))
            paths.Add(crashesPath);
        }
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error scanning WSL Windows users: {ex.Message}");
      }
    }

    return paths;
  }

  /// <summary>
  /// VRChatログディレクトリを検出
  /// </summary>
  /// <returns>見つかったログディレクトリ、見つからない場合はnull</returns>
  public static string? FindLogDirectory()
  {
    var searchPaths = GetSearchPaths();

    foreach (var path in searchPaths)
    {
      if (Directory.Exists(path))
        return path;
    }

    return null;
  }

  /// <summary>
  /// 最新のVRChatログファイルを検出
  /// </summary>
  /// <returns>見つかった最新のログファイル、見つからない場合はnull</returns>
  public static string? FindLatestLogFile()
  {
    var logDir = FindLogDirectory();
    if (string.IsNullOrEmpty(logDir))
      return null;

    try
    {
      var dirInfo = new DirectoryInfo(logDir);
      var logFiles = dirInfo
          .GetFiles($"{LogFilePrefix}*{LogFileSuffix}")
          .OrderByDescending(f => f.LastWriteTime)
          .FirstOrDefault();

      return logFiles?.FullName;
    }
    catch (Exception ex)
    {
      System.Diagnostics.Debug.WriteLine($"Error finding log files: {ex.Message}");
      return null;
    }
  }

  /// <summary>
  /// すべてのVRChatログファイルを取得（古い順）
  /// </summary>
  /// <returns>見つかったログファイルのリスト（古い順）</returns>
  public static List<string> GetAllLogFiles()
  {
    var logDir = FindLogDirectory();
    if (string.IsNullOrEmpty(logDir) || !Directory.Exists(logDir))
      return new List<string>();

    try
    {
      var dirInfo = new DirectoryInfo(logDir);
      return dirInfo
          .GetFiles($"{LogFilePrefix}*{LogFileSuffix}")
          .OrderBy(f => f.LastWriteTime)
          .Select(f => f.FullName)
          .ToList();
    }
    catch (Exception ex)
    {
      System.Diagnostics.Debug.WriteLine($"Error getting all log files: {ex.Message}");
      return new List<string>();
    }
  }
}
