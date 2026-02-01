namespace askMeWindows.Models;

/// <summary>
/// VRChatインスタンス情報を表すデータクラス
/// 責務: VRChat の位置情報を表現
/// </summary>
public record InstanceInfo
{
  /// <summary>ワールド名</summary>
  public string WorldName { get; init; } = "Unknown";

  /// <summary>ワールドID（例：wrld_xxx）</summary>
  public string WorldId { get; init; } = "";

  /// <summary>インスタンスID（例：12345~hidden...）</summary>
  public string InstanceId { get; init; } = "";

  /// <summary>完全なインスタンスID（例：wrld_xxx:12345~...）</summary>
  public string FullInstanceId { get; init; } = "";

  /// <summary>VRChatランチURL</summary>
  public string LaunchUrl { get; init; } = "https://vrchat.com/";

  /// <summary>検出日時</summary>
  public DateTime DetectedAt { get; init; } = DateTime.Now;
}
