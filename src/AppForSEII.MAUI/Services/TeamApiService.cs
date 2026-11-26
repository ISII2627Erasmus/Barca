using System.Net.Http.Json;
using System.Text.Json;
using AppForSEII.MAUI.Models;

namespace AppForSEII.MAUI.Services;

public record PlayersResult(List<ApplicationUserForTeamDTO> Players, string? Error);
public record TeamsResult(List<TeamForGameDTO> Teams, string? Error);
public record CreateTeamResult(TeamDetailDTO? Team, List<string> Errors)
{
    public bool Success => Team != null;
}

public class TeamApiService
{
    private readonly HttpClient _http;

    public TeamApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<PlayersResult> GetUsersForTeamAsync(string? sportName, int? minAge, int? maxAge)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(sportName)) query.Add($"sportName={Uri.EscapeDataString(sportName)}");
        if (minAge != null) query.Add($"minAge={minAge}");
        if (maxAge != null) query.Add($"maxAge={maxAge}");

        var url = "api/ApplicationUsers/GetUsersForTeam";
        if (query.Count > 0) url += "?" + string.Join("&", query);

        try
        {
            var players = await _http.GetFromJsonAsync<List<ApplicationUserForTeamDTO>>(url) ?? new();
            // API rule: the captain cannot be invited to his own team
            players.RemoveAll(p => p.Id == ApiConfig.CaptainUserId);
            return new PlayersResult(players, null);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            System.Diagnostics.Debug.WriteLine($"[TeamApiService] {ex.Message}");
            return new PlayersResult(new(), "Could not load players. Is AppForSEII.API running?");
        }
    }

    public async Task<TeamsResult> GetTeamsAsync(string? captainName = null, string? sportName = null)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(captainName)) query.Add($"captainName={Uri.EscapeDataString(captainName)}");
        if (!string.IsNullOrWhiteSpace(sportName)) query.Add($"sportName={Uri.EscapeDataString(sportName)}");

        var url = "api/Teams/GetTeamsForGame";
        if (query.Count > 0) url += "?" + string.Join("&", query);

        try
        {
            var teams = await _http.GetFromJsonAsync<List<TeamForGameDTO>>(url) ?? new();
            return new TeamsResult(teams, null);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            System.Diagnostics.Debug.WriteLine($"[TeamApiService] {ex.Message}");
            return new TeamsResult(new(), "Could not load teams. Is AppForSEII.API running?");
        }
    }

    public async Task<CreateTeamResult> CreateTeamAsync(TeamForCreateDTO team)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/Teams/CreateTeam", team);

            if (response.IsSuccessStatusCode)
            {
                var created = await response.Content.ReadFromJsonAsync<TeamDetailDTO>();
                return new CreateTeamResult(created, new());
            }

            var body = await response.Content.ReadAsStringAsync();
            return new CreateTeamResult(null, ParseErrors(body, (int)response.StatusCode));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            System.Diagnostics.Debug.WriteLine($"[TeamApiService] {ex.Message}");
            return new CreateTeamResult(null, new() { "Could not reach the server. Is AppForSEII.API running?" });
        }
    }

    private static List<string> ParseErrors(string body, int statusCode)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("errors", out var errors))
            {
                return errors.EnumerateObject()
                    .SelectMany(field => field.Value.EnumerateArray())
                    .Select(msg => msg.GetString() ?? string.Empty)
                    .Where(msg => msg.Length > 0)
                    .ToList();
            }

            if (root.ValueKind == JsonValueKind.String)
                return new() { root.GetString()! };
        }
        catch (JsonException)
        {
            // Body is not JSON: fall back to the raw text below
        }

        // Conflict("...") is sent as plain text, so show the text itself.
        if (!string.IsNullOrWhiteSpace(body))
            return new() { body.Trim().Trim('"') };

        return new() { $"Error {statusCode}" };
    }
}