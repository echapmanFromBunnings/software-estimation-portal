using System.Text.Json;
using Microsoft.Extensions.Options;

namespace software_estimator.Services;

public class TeamConfigOptions
{
    public string Path { get; set; } = string.Empty;
}

public class TeamRole
{
    public string Role { get; set; } = string.Empty;
    public int Count { get; set; }
    public string Seniority { get; set; } = string.Empty;
    public string EmploymentType { get; set; } = string.Empty; // "FullTime" or "Contractor"
}

public class Team
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = new();
    public List<string> Capabilities { get; set; } = new();
    public List<TeamRole> Roles { get; set; } = new();
    public bool Active { get; set; } = true;
}

public class TeamConfigRoot
{
    public int Version { get; set; }
    public List<Team> Teams { get; set; } = new();
}

public class ResourceValidationResult
{
    public bool IsValid { get; set; }
    public List<ResourceValidationIssue> Issues { get; set; } = new();
}

public class ResourceValidationIssue
{
    public string Role { get; set; } = string.Empty;
    public int RequiredCount { get; set; }
    public int AvailableCount { get; set; }
    public string Severity { get; set; } = "Warning"; // Warning, Error
    public string Message { get; set; } = string.Empty;
}

public interface ITeamConfigService
{
    Task<IReadOnlyList<Team>> GetTeamsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Team>> GetActiveTeamsAsync(CancellationToken ct = default);
    Team? FindById(string id);
    ResourceValidationResult ValidateResourceAllocation(string teamId, Dictionary<string, int> roleRequirements);
}

public class TeamConfigService : ITeamConfigService, IDisposable
{
    private readonly string _path;
    private FileSystemWatcher? _watcher;
    private List<Team> _cache = new();
    private readonly object _lock = new();

    public TeamConfigService(IOptions<TeamConfigOptions> options)
    {
        _path = options.Value.Path;
        Load();
        TryWatch();
    }

    public Task<IReadOnlyList<Team>> GetTeamsAsync(CancellationToken ct = default)
    {
        lock (_lock)
        {
            return Task.FromResult((IReadOnlyList<Team>)_cache.ToList());
        }
    }

    public Task<IReadOnlyList<Team>> GetActiveTeamsAsync(CancellationToken ct = default)
    {
        lock (_lock)
        {
            return Task.FromResult((IReadOnlyList<Team>)_cache.Where(t => t.Active).ToList());
        }
    }

    public Team? FindById(string id)
    {
        lock (_lock)
        {
            return _cache.FirstOrDefault(t => string.Equals(t.Id, id, StringComparison.OrdinalIgnoreCase));
        }
    }

    public ResourceValidationResult ValidateResourceAllocation(string teamId, Dictionary<string, int> roleRequirements)
    {
        var result = new ResourceValidationResult { IsValid = true };
        
        var team = FindById(teamId);
        if (team == null)
        {
            result.IsValid = false;
            result.Issues.Add(new ResourceValidationIssue
            {
                Role = "Team",
                Message = "Selected team not found",
                Severity = "Error"
            });
            return result;
        }

        foreach (var requirement in roleRequirements)
        {
            var role = requirement.Key;
            var requiredCount = requirement.Value;
            
            var teamRole = team.Roles.FirstOrDefault(r => 
                string.Equals(r.Role, role, StringComparison.OrdinalIgnoreCase));
            
            if (teamRole == null)
            {
                result.IsValid = false;
                result.Issues.Add(new ResourceValidationIssue
                {
                    Role = role,
                    RequiredCount = requiredCount,
                    AvailableCount = 0,
                    Severity = "Error",
                    Message = $"Team '{team.Name}' does not have any {role} resources"
                });
                continue;
            }

            if (requiredCount > teamRole.Count)
            {
                result.IsValid = false;
                result.Issues.Add(new ResourceValidationIssue
                {
                    Role = role,
                    RequiredCount = requiredCount,
                    AvailableCount = teamRole.Count,
                    Severity = requiredCount > teamRole.Count * 1.5 ? "Error" : "Warning",
                    Message = $"Requires {requiredCount} {role}(s) but team '{team.Name}' only has {teamRole.Count}"
                });
            }
        }

        return result;
    }

    private void Load()
    {
        if (!File.Exists(_path)) 
        { 
            _cache = new(); 
            return; 
        }
        
        try
        {
            var json = File.ReadAllText(_path);
            var root = JsonSerializer.Deserialize<TeamConfigRoot>(json, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
            _cache = root?.Teams ?? new();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading teams config: {ex.Message}");
            _cache = new();
        }
    }

    private void TryWatch()
    {
        try
        {
            var dir = Path.GetDirectoryName(_path);
            var file = Path.GetFileName(_path);
            if (dir is null) return;
            
            _watcher = new FileSystemWatcher(dir, file)
            {
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime
            };
            _watcher.Changed += (_, __) => Load();
            _watcher.Created += (_, __) => Load();
            _watcher.Renamed += (_, __) => Load();
        }
        catch { /* Ignore watcher setup failures */ }
    }

    public void Dispose() => _watcher?.Dispose();
}