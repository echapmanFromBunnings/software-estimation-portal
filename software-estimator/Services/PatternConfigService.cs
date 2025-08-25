using System.Text.Json;
using Microsoft.Extensions.Options;

namespace software_estimator.Services;

public class PatternConfigOptions
{
    public string Path { get; set; } = string.Empty;
}

public class CommonPattern
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal AverageSprints { get; set; }
    public string? Notes { get; set; }
}

public class PatternConfigRoot
{
    public int Version { get; set; }
    public List<CommonPattern> Patterns { get; set; } = new();
}

public interface IPatternConfigService
{
    Task<IReadOnlyList<CommonPattern>> GetPatternsAsync(CancellationToken ct = default);
    CommonPattern? FindByKey(string key);
}

public class PatternConfigService : IPatternConfigService, IDisposable
{
    private readonly string _path;
    private FileSystemWatcher? _watcher;
    private List<CommonPattern> _cache = new();
    private readonly object _lock = new();

    public PatternConfigService(IOptions<PatternConfigOptions> options)
    {
        _path = options.Value.Path;
        Load();
        TryWatch();
    }

    public Task<IReadOnlyList<CommonPattern>> GetPatternsAsync(CancellationToken ct = default)
    {
        lock (_lock)
        {
            return Task.FromResult((IReadOnlyList<CommonPattern>)_cache.ToList());
        }
    }

    public CommonPattern? FindByKey(string key)
    {
        lock (_lock)
        {
            return _cache.FirstOrDefault(p => string.Equals(p.Key, key, StringComparison.OrdinalIgnoreCase));
        }
    }

    private void Load()
    {
        if (!File.Exists(_path))
        {
            _cache = new();
            return;
        }
        var json = File.ReadAllText(_path);
        var root = JsonSerializer.Deserialize<PatternConfigRoot>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        _cache = root?.Patterns ?? new();
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
        catch
        {
            // ignore watcher failures
        }
    }

    public void Dispose()
    {
        _watcher?.Dispose();
    }
}

public static class OverheadSuggestions
{
    // Typical overhead ranges in percent of functional effort cost
    public const decimal QaPercent = 15m;
    public const decimal CodeReviewPercent = 5m;
    public const decimal IntegrationPercent = 10m;
    public const decimal DeploymentPercent = 5m;
}
