using software_estimator.Models;
using System.Collections.Generic;
using System.Linq;

namespace software_estimator.Services;

public class AssignmentMigrationService
{
    // Convert legacy CSV entries like "1,2,3" or "1:100,2:50" where numeric items represent static numbers
    // into the new key:percent format using a mapping from static number to SourceKey.
    // mapping: Dictionary<string,int> where key is SourceKey and value is StaticNumber
    public static void MigrateAssignments(IEnumerable<Estimate> estimates, Dictionary<string,int> resourceNumberMap)
    {
        if (estimates == null) return;
        // Build reverse map: static number -> sourceKey
        var rev = resourceNumberMap.ToDictionary(kv => kv.Value, kv => kv.Key);

        foreach (var est in estimates)
        {
            foreach (var f in est.FunctionalItems)
            {
                if (string.IsNullOrWhiteSpace(f.AssignedResourceIds)) continue;
                // Split by comma and normalize
                var parts = f.AssignedResourceIds.Split(',', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);
                var dict = new Dictionary<string, decimal>(System.StringComparer.OrdinalIgnoreCase);
                foreach (var p in parts)
                {
                    var kv = p.Split(':', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);
                    if (kv.Length == 0) continue;
                    if (int.TryParse(kv[0], out var idx))
                    {
                        if (rev.TryGetValue(idx, out var src))
                        {
                            decimal pct = 100m;
                            if (kv.Length > 1 && decimal.TryParse(kv[1], out var parsed)) pct = System.Math.Clamp(parsed, 0m, 100m);
                            dict[src] = pct;
                        }
                        else
                        {
                            // leave numeric entries we cannot map (skip)
                        }
                    }
                    else
                    {
                        // assume already a key or key:percent
                        var key = kv[0].Trim();
                        decimal pct = 100m;
                        if (kv.Length > 1 && decimal.TryParse(kv[1], out var parsed)) pct = System.Math.Clamp(parsed, 0m, 100m);
                        dict[key] = pct;
                    }
                }
                f.AssignedResources = dict;
            }
        }
    }
}
