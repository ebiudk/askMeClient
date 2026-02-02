using System.Text.RegularExpressions;

namespace askMeWindows.Services;

/// <summary>
/// VRChatログからインスタンス情報を抽出する機能を専門とするクラス
/// 責務: ログ行からインスタンス情報のパースと抽出
/// </summary>
public static class InstanceParser
{
  // VRChatログの典型的なパターン
  // [Behaviour] Entering Room: [ワールド名]
  private static readonly Regex EnteringRoomPattern = new(
      @"\[Behaviour\]\s+Entering Room:\s+(.+?)(?:\s*\(.*\))?$",
      RegexOptions.Compiled
  );

  // Joining wrld_xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx:xxxxx~region(xx)~friends(xxx)~...
  // 最後のスペースまでを全て取得
  private static readonly Regex JoiningPattern = new(
      @"Joining\s+(wrld_[^\s]+)",
      RegexOptions.Compiled
  );

  // Joined location: wrld_xxxxxxxx:xxxxxxxx~private(usr_xxxxxxxx)~hidden(usr_xxxxxxxx)~canRequestInvite~nonce(xxxxxxxx)
  private static readonly Regex LocationPattern = new(
      @"Joined location:\s+(wrld_[a-zA-Z0-9]+(?::[~\w\(\)]+)*)",
      RegexOptions.Compiled
  );

  // VRCApplication: HandleApplicationQuit
  private static readonly Regex QuitPattern = new(
      @"VRCApplication: HandleApplicationQuit",
      RegexOptions.Compiled
  );

  /// <summary>
  /// ワールド名を抽出
  /// </summary>
  /// <param name="logLine">ログの1行</param>
  /// <returns>抽出されたワールド名、見つからない場合はnull</returns>
  public static string? ParseEnteringRoom(string logLine)
  {
    var match = EnteringRoomPattern.Match(logLine);
    return match.Success ? match.Groups[1].Value.Trim() : null;
  }

  /// <summary>
  /// 終了行かどうかを判定
  /// </summary>
  /// <param name="logLine">ログの1行</param>
  /// <returns>終了行であればtrue</returns>
  public static bool IsQuitLine(string logLine)
  {
    return QuitPattern.IsMatch(logLine);
  }

  /// <summary>
  /// インスタンスIDを抽出
  /// </summary>
  /// <param name="logLine">ログの1行</param>
  /// <returns>抽出されたインスタンスID、見つからない場合はnull</returns>
  public static string? ParseInstanceId(string logLine)
  {
    // Joining パターン
    var match = JoiningPattern.Match(logLine);
    if (match.Success)
      return match.Groups[1].Value;

    // Location パターン
    match = LocationPattern.Match(logLine);
    if (match.Success)
      return match.Groups[1].Value;

    return null;
  }

  /// <summary>
  /// インスタンスIDからVRChat公式ランチURLを生成
  /// </summary>
  /// <param name="instanceId">インスタンスID（例：wrld_xxxx:yyyy~...）</param>
  /// <returns>VRChat ランチURL</returns>
  public static string CreateLaunchUrl(string instanceId)
  {
    if (string.IsNullOrEmpty(instanceId) ||
        instanceId == "Unknown" ||
        !instanceId.Contains("wrld_"))
    {
      return "https://vrchat.com/";
    }

    // instance_id から world_id と instance_part を分割
    // 形式：wrld_xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx:xxxxx~...
    // または：wrld_xxxxxxxx:xxxxx~...
    var parts = instanceId.Split(':', 2);
    var worldId = parts[0];
    var instancePart = parts.Length > 1 ? parts[1] : "";

    // URLを構築
    var url = $"https://vrchat.com/home/launch?worldId={worldId}";

    // instanceId パラメータには数字とメタデータのみを使用（wrld_ は不要）
    if (!string.IsNullOrEmpty(instancePart))
    {
      url += $"&instanceId={instancePart}";
    }

    return url;
  }

  /// <summary>
  /// 完全なインスタンスIDをワールドIDとインスタンス部分に分割
  /// </summary>
  /// <param name="fullId">wrld_xxx:yyy~...</param>
  /// <returns>(WorldId, InstanceId)</returns>
  public static (string worldId, string instanceId) SplitInstanceId(string fullId)
  {
    if (string.IsNullOrEmpty(fullId)) return ("", "");

    var parts = fullId.Split(':', 2);
    var worldId = parts[0];
    var instanceId = parts.Length > 1 ? parts[1] : "";

    return (worldId, instanceId);
  }
}
