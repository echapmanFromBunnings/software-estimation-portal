using software_estimator.Models;

namespace software_estimator.Services;

public record SquadComposition(int FteCount, int ContractorCount, decimal FteDailyRate, decimal ContractorDailyRate, int SprintLengthDays = 10);

public interface ICostCalculator
{
    decimal CalcSquadCostPerSprint(SquadComposition squad);
    decimal CalcFunctionalLineCost(decimal sprints, decimal squadCostPerSprint);
    decimal CalcNonFunctionalCost(NonFunctionalItem item, IEnumerable<ResourceRate> rates, IEnumerable<RoleMapping>? mappings = null);
    decimal ApplyContingency(decimal subtotal, decimal contingencyPercent);
    bool IsDeviation(decimal? averageSprints, decimal actualSprints, decimal thresholdPercent = 25m);
}

public class CostCalculator : ICostCalculator
{
    public decimal CalcSquadCostPerSprint(SquadComposition squad)
    {
        var fteCost = squad.FteCount * squad.FteDailyRate * squad.SprintLengthDays;
        var contractorCost = squad.ContractorCount * squad.ContractorDailyRate * squad.SprintLengthDays;
        return Decimal.Round(fteCost + contractorCost, 2);
    }

    public decimal CalcFunctionalLineCost(decimal sprints, decimal squadCostPerSprint)
    {
        return Decimal.Round(sprints * squadCostPerSprint, 2);
    }

    public decimal CalcNonFunctionalCost(NonFunctionalItem item, IEnumerable<ResourceRate> rates, IEnumerable<RoleMapping>? mappings = null)
    {
        decimal total = 0m;
        foreach (var a in item.Allocations)
        {
            var role = a.Role;
            if (mappings is not null)
            {
                var map = mappings.FirstOrDefault(m => string.Equals(m.SourceRole, role, StringComparison.OrdinalIgnoreCase));
                if (map is not null && !string.IsNullOrWhiteSpace(map.TargetRole))
                    role = map.TargetRole;
            }
            var rate = rates.FirstOrDefault(r => string.Equals(r.Role, role, StringComparison.OrdinalIgnoreCase));
            if (rate is null) continue;
            var hourly = rate.HourlyRate > 0 ? rate.HourlyRate : rate.DailyRate / 8m;
            total += a.Hours * hourly;
        }
        return Decimal.Round(total, 2);
    }

    public decimal ApplyContingency(decimal subtotal, decimal contingencyPercent)
    {
        if (contingencyPercent <= 0) return subtotal;
        var extra = subtotal * (contingencyPercent / 100m);
        return Decimal.Round(subtotal + extra, 2);
    }

    public bool IsDeviation(decimal? averageSprints, decimal actualSprints, decimal thresholdPercent = 25m)
    {
        if (averageSprints is null || averageSprints == 0) return false;
        var avg = averageSprints.Value;
        var delta = Math.Abs(actualSprints - avg);
        var pct = (delta / avg) * 100m;
        return pct >= thresholdPercent;
    }
}
