using software_estimator.Models;
using System;
using System.Linq;

namespace software_estimator.Services;

public static class AssignmentCostHelper
{
    // Calculate cost of a functional line based on assigned resources and utilisation.
    // Mirrors logic used previously in the EstimateEditor component.
    public static decimal CalculateFunctionalLineCost(FunctionalLineItem f, Estimate estimate)
    {
        if (f is null) return 0m;
        if (estimate is null) return 0m;
        var lineDays = f.Sprints * estimate.SprintLengthDays;
        decimal total = 0m;
        var dict = f.AssignedResources ?? new System.Collections.Generic.Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        foreach (var kv in dict)
        {
            var key = kv.Key;
            var pct = Math.Clamp(kv.Value, 0m, 100m) / 100m;
            var rate = estimate.ResourceRates.FirstOrDefault(r => string.Equals(r.SourceKey, key, StringComparison.OrdinalIgnoreCase));
            if (rate is null) continue;
            var daily = rate.DailyRate;
            total += daily * lineDays * pct;
        }
        return Decimal.Round(total, 2);
    }
}
