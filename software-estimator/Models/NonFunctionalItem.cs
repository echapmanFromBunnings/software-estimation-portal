using System.ComponentModel.DataAnnotations;

namespace software_estimator.Models;

public class NonFunctionalItem
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

    public ICollection<ResourceAllocation> Allocations { get; set; } = new List<ResourceAllocation>();
}

public enum ResourceType
{
    FTE = 0,
    Contractor = 1
}

public class ResourceRate
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid EstimateId { get; set; }
    public Estimate? Estimate { get; set; }

    [Required]
    [MaxLength(100)]
    public string Role { get; set; } = string.Empty; // e.g., Developer, QA, BA

    public ResourceType Type { get; set; } = ResourceType.FTE;

    [Range(0, double.MaxValue)]
    public decimal DailyRate { get; set; } // Currency/day

    [Range(0, double.MaxValue)]
    public decimal HourlyRate { get; set; } // Optionally use either daily or hourly
}

public class ResourceAllocation
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid NonFunctionalItemId { get; set; }
    public NonFunctionalItem? NonFunctionalItem { get; set; }

    [Required]
    [MaxLength(100)]
    public string Role { get; set; } = string.Empty; // Must match a ResourceRate.Role

    [Range(0, double.MaxValue)]
    public decimal Hours { get; set; }
}
