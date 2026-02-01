using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace askMeWindows.Services;

/// <summary>
/// API連携サービス
/// 責務: 外部APIとの通信
/// </summary>
public class ApiService
{
  private static readonly HttpClient _httpClient = new HttpClient();
  private const string ApiUrl = "https://ask-me-orcin.vercel.app/api/update-location";

  public static async Task<bool> UpdateLocationAsync(string apiKey, string worldId, string? worldName, string? instanceId, string? displayName)
  {
    if (string.IsNullOrWhiteSpace(apiKey)) return false;

    try
    {
      var data = new
      {
        current_world_id = worldId,
        current_world_name = worldName,
        current_instance_id = instanceId,
        display_name = displayName
      };

      var json = JsonSerializer.Serialize(data);
      var content = new StringContent(json, Encoding.UTF8, "application/json");

      using var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
      request.Headers.Add("x-api-key", apiKey);
      request.Content = content;

      var response = await _httpClient.SendAsync(request);

      if (!response.IsSuccessStatusCode)
      {
        var error = await response.Content.ReadAsStringAsync();
        System.Diagnostics.Debug.WriteLine($"API update failed: {response.StatusCode} - {error}");
      }

      return response.IsSuccessStatusCode;
    }
    catch (Exception ex)
    {
      System.Diagnostics.Debug.WriteLine($"API update exception: {ex.Message}");
      return false;
    }
  }

  public static async Task<bool> ClearLocationAsync(string apiKey)
  {
    if (string.IsNullOrWhiteSpace(apiKey)) return false;

    try
    {
      var data = new
      {
        current_world_id = (string?)null,
        current_world_name = (string?)null,
        current_instance_id = (string?)null
      };

      var json = JsonSerializer.Serialize(data);
      var content = new StringContent(json, Encoding.UTF8, "application/json");

      using var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
      request.Headers.Add("x-api-key", apiKey);
      request.Content = content;

      var response = await _httpClient.SendAsync(request);
      return response.IsSuccessStatusCode;
    }
    catch (Exception ex)
    {
      System.Diagnostics.Debug.WriteLine($"API clear exception: {ex.Message}");
      return false;
    }
  }
}
