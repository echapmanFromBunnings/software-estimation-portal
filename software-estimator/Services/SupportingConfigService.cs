using System.Text.Json;
using Microsoft.Extensions.Options;

namespace software_estimator.Services;

public class SupportingConfigOptions
{
    public string Path { get; set; } = string.Empty;
}

public class SupportingActivity
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal? SuggestedPercentOfFunctional { get; set; }
    public List<SupportAllocation> DefaultAllocations { get; set; } = new();
    public string? Notes { get; set; }
    public bool Baseline { get; set; } = false;
}

public class SupportAllocation
{
    public string Role { get; set; } = string.Empty;
    public decimal Hours { get; set; }
}

public class SupportingConfigRoot
{
    public int Version { get; set; }
    public List<SupportingActivity> Activities { get; set; } = new();
}

public interface ISupportingConfigService
{
    Task<IReadOnlyList<SupportingActivity>> GetActivitiesAsync(CancellationToken ct = default);
    SupportingActivity? FindByKey(string key);
}

public class SupportingConfigService : ISupportingConfigService, IDisposable
{
    private readonly string _path;
    private FileSystemWatcher? _watcher;
    private List<SupportingActivity> _cache = new();
    private readonly object _lock = new();

    public SupportingConfigService(IOptions<SupportingConfigOptions> options)
    {
        _path = options.Value.Path;
        Load();
        TryWatch();
    }

    public Task<IReadOnlyList<SupportingActivity>> GetActivitiesAsync(CancellationToken ct = default)
    {
        lock (_lock)
        {
            return Task.FromResult((IReadOnlyList<SupportingActivity>)_cache.ToList());
        }
    }

    public SupportingActivity? FindByKey(string key)
    {
        lock (_lock)
        {
            return _cache.FirstOrDefault(a => string.Equals(a.Key, key, StringComparison.OrdinalIgnoreCase));
        }
    }

    private void Load()
    {
        if (!File.Exists(_path)) { _cache = new(); return; }
        var json = File.ReadAllText(_path);
        var root = JsonSerializer.Deserialize<SupportingConfigRoot>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        _cache = root?.Activities ?? new();
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
        catch { }
    }

    public void Dispose() => _watcher?.Dispose();
}
