using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace software_estimator.Models;

public enum LineSourceType
{
    CommonPattern = 0,
    Custom = 1
}

public class FunctionalLineItem
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid EstimateId { get; set; }
    public Estimate? Estimate { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public LineSourceType SourceType { get; set; } = LineSourceType.Custom;

    // For CommonPattern lines, capture the pattern key and average sprint guidance
    public string? PatternKey { get; set; }
    public decimal? AverageSprints { get; set; }

    // How many sprints are estimated for this line
    public decimal Sprints { get; set; } = 1;

    // Persisted last-calculated cost for this functional line (captured at save time)
    public decimal Cost { get; set; } = 0m;

    // Optional alert flag when deviating from average
    public bool IsDeviationFlagged { get; set; }

    [MaxLength(10000)]
    public string? Outcome { get; set; }

    [MaxLength(200)]
    public string? Domain { get; set; }

    // Persisted list of resource assignments with utilization percent, stored as CSV of "key:percent" entries
    [MaxLength(200)]
    public string? AssignedResourceIds { get; set; }

    // UI-only: mapping of resource SourceKey -> utilization percent (0..100)
    [NotMapped]
    public Dictionary<string, decimal> AssignedResources
    {
        get
        {
            var dict = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(AssignedResourceIds)) return dict;
            var parts = AssignedResourceIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var p in parts)
            {
                var kv = p.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (kv.Length == 0) continue;
                var key = kv[0].Trim();
                if (string.IsNullOrWhiteSpace(key)) continue;
                decimal pct = 100m;
                if (kv.Length > 1 && decimal.TryParse(kv[1], out var parsed)) pct = Math.Clamp(parsed, 0m, 100m);
                dict[key] = pct;
            }
            return dict;
        }
        set
        {
            if (value is null || value.Count == 0)
            {
                AssignedResourceIds = null;
                return;
            }
            // Serialize as key:percent CSV
            var parts = value.Select(kvp => $"{kvp.Key.Trim()}:{Math.Round(Math.Clamp(kvp.Value, 0m, 100m), 2)}");
            AssignedResourceIds = string.Join(',', parts);
        }
    }
}
