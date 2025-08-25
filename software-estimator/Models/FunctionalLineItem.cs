using System.ComponentModel.DataAnnotations;

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

    // Optional alert flag when deviating from average
    public bool IsDeviationFlagged { get; set; }

    [MaxLength(500)]
    public string? Outcome { get; set; }
}
