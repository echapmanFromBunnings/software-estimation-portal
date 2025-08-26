using System.Text.Json;
using Microsoft.Extensions.Options;

namespace software_estimator.Services;

public class RoleRatesOptions
{
    public string Path { get; set; } = string.Empty;
}

public class RoleRateEntry
{
    public string Role { get; set; } = string.Empty;
    public string EmploymentType { get; set; } = "FullTime"; // FullTime | Contractor
    public decimal DailyRate { get; set; }
    public decimal HourlyRate { get; set; }
}

public class RoleRatesRoot
{
    public int Version { get; set; }
    public List<RoleRateEntry> Rates { get; set; } = new();
}

public interface IRoleRatesService
{
    Task<IReadOnlyList<RoleRateEntry>> GetDefaultRatesAsync(CancellationToken ct = default);
}

public class RoleRatesService : IRoleRatesService, IDisposable
{
    private readonly string _path;
    private List<RoleRateEntry> _cache = new();
    private FileSystemWatcher? _watcher;
    private readonly object _lock = new();

    public RoleRatesService(IOptions<RoleRatesOptions> options)
    {
        _path = options.Value.Path;
        Load();
        TryWatch();
    }

    public Task<IReadOnlyList<RoleRateEntry>> GetDefaultRatesAsync(CancellationToken ct = default)
    {
        lock (_lock)
        {
            return Task.FromResult((IReadOnlyList<RoleRateEntry>)_cache.ToList());
        }
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
            var root = JsonSerializer.Deserialize<RoleRatesRoot>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            _cache = root?.Rates ?? new();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading role rates config: {ex.Message}");
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
        catch { /* ignore */ }
    }

    public void Dispose() => _watcher?.Dispose();
}
