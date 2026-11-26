using System.Text.Json;
using AppForSEII.MAUI.Models;

namespace AppForSEII.MAUI.Services;

public class RecentTeammatesStore
{
    private const string PreferencesKey = "recent_teammates";
    private const int MaxRecent = 6;

    private readonly IPreferences _preferences;

    public RecentTeammatesStore(IPreferences preferences)
    {
        _preferences = preferences;
    }

    public List<ApplicationUserForTeamDTO> GetAll()
    {
        var json = _preferences.Get(PreferencesKey, string.Empty);
        if (string.IsNullOrEmpty(json))
            return new();

        try
        {
            return JsonSerializer.Deserialize<List<ApplicationUserForTeamDTO>>(json) ?? new();
        }
        catch (JsonException)
        {
            // Corrupted value: start again with an empty list.
            _preferences.Remove(PreferencesKey);
            return new();
        }
    }

    // Newest first, no duplicates, max 6.
    public void SaveTeammates(IEnumerable<ApplicationUserForTeamDTO> teammates)
    {
        var newest = teammates.Reverse().ToList();
        var merged = newest
            .Concat(GetAll().Where(old => newest.All(n => n.Id != old.Id)))
            .Take(MaxRecent)
            .ToList();

        _preferences.Set(PreferencesKey, JsonSerializer.Serialize(merged));
    }

    // Only players inside the chosen age range who are not selected yet.
    public List<ApplicationUserForTeamDTO> GetSuggestions(int minAge, int maxAge, IEnumerable<string> selectedIds)
    {
        var selected = selectedIds.ToHashSet();
        return GetAll()
            .Where(p => p.Age >= minAge && p.Age <= maxAge && !selected.Contains(p.Id))
            .ToList();
    }
}