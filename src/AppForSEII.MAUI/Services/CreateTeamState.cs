using AppForSEII.MAUI.Models;

namespace AppForSEII.MAUI.Services;

public class CreateTeamState
{
    public int CurrentStep { get; private set; } = 1;

    public TeamForCreateDTO Team { get; private set; } = NewTeam();

    public List<ApplicationUserForTeamDTO> SelectedPlayers { get; } = new();

    // Key = UserId. Kept here so messages survive going back with Edit.
    public Dictionary<string, string> InvitationMessages { get; } = new();
    
    // Filled from GET api/Teams/GetTeamsForGame so a taken name is shown while typing.
    public HashSet<string> ExistingTeamNames { get; } = new(StringComparer.OrdinalIgnoreCase);

    public bool IsNameTaken => ExistingTeamNames.Contains(Team.Name.Trim());

    public event Action? OnChange;

    private static TeamForCreateDTO NewTeam() => new()
    {
        MinAge = 18,
        MaxAge = 30,
        MaxMembers = 12,
        CaptainUserName = ApiConfig.CaptainUserName
    };

    // The captain also counts as a member (API rule: MaxMembers >= invitations + 1).
    public int FreeSpots => Team.MaxMembers - 1 - SelectedPlayers.Count;

    public bool IsFull => FreeSpots <= 0;

    public bool IsSelected(string userId) => SelectedPlayers.Any(p => p.Id == userId);

    public void GoToStep(int step)
    {
        CurrentStep = Math.Clamp(step, 1, 3);
        NotifyStateChanged();
    }

    public void AddPlayer(ApplicationUserForTeamDTO player)
    {
        if (IsFull || IsSelected(player.Id))
            return;

        SelectedPlayers.Add(player);
        NotifyStateChanged();
    }

    public void RemovePlayer(string userId)
    {
        SelectedPlayers.RemoveAll(p => p.Id == userId);
        InvitationMessages.Remove(userId);
        NotifyStateChanged();
    }

    public List<string> ValidateDetails()
    {
        var errors = new List<string>();
        var name = Team.Name?.Trim() ?? string.Empty;

        if (name.Length < 4 || name.Length > 9)
            errors.Add("Team name must be between 4 and 9 characters long.");
        else if (IsNameTaken)
            errors.Add($"A team named '{name}' already exists. Choose another name.");
        if (string.IsNullOrWhiteSpace(Team.SportName))
            errors.Add("Select a sport.");
        if (Team.MinAge < 3)
            errors.Add("Minimum age must be at least 3.");
        if (Team.MaxAge < Team.MinAge)
            errors.Add("Maximum age must be greater than or equal to minimum age.");
        if (Team.MaxMembers < 2)
            errors.Add("Max. players must be at least 2 (you and one teammate).");
        else if (Team.MaxMembers < SelectedPlayers.Count + 1)
            errors.Add($"Max. players must be at least {SelectedPlayers.Count + 1}: you already selected {SelectedPlayers.Count} teammates.");

        return errors;
    }

    // After Edit the user may change sport or age range: players that no longer fit are removed
    // and their names are returned so the page can tell the user (nothing disappears silently).
    public List<string> RemovePlayersThatNoLongerFit()
    {
        var removed = SelectedPlayers
            .Where(p => p.Age < Team.MinAge
                     || p.Age > Team.MaxAge
                     || p.Sports.All(s => s.SportName != Team.SportName))
            .ToList();

        foreach (var player in removed)
        {
            SelectedPlayers.Remove(player);
            InvitationMessages.Remove(player.Id);
        }

        if (removed.Count > 0)
            NotifyStateChanged();

        return removed.Select(p => p.FullName).ToList();
    }

    public string GetMessage(string userId) =>
        InvitationMessages.TryGetValue(userId, out var message)
            ? message
            : $"Join my team {Team.Name}!";

    public void SetMessage(string userId, string message) => InvitationMessages[userId] = message;

    public List<string> ValidateMessages() =>
        SelectedPlayers
            .Where(p => GetMessage(p.Id).Trim().Length is < 3 or > 255)
            .Select(p => $"Message for {p.FullName} must be between 3 and 255 characters.")
            .ToList();

    // Builds the DTO that is sent to POST api/Teams/CreateTeam.
    public TeamForCreateDTO ToCreateDTO()
    {
        Team.Name = Team.Name.Trim();
        Team.TeamInvitations = SelectedPlayers
            .Select(p => new TeamInvitationDTO
            {
                UserId = p.Id,
                Name = p.Name,
                Surname = p.Surname,
                Age = p.Age,
                InvitationMessage = GetMessage(p.Id).Trim()
            })
            .ToList();
        return Team;
    }

    public void Reset()
    {
        Team = NewTeam();
        SelectedPlayers.Clear();
        InvitationMessages.Clear();
        CurrentStep = 1;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}