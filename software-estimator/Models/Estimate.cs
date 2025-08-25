using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace software_estimator.Models;

public class Estimate
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Client { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? TeamId { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public int SprintLengthDays { get; set; } = 10; // 2 weeks

    public decimal ContingencyPercent { get; set; } = 0; // 0..100

    public ICollection<FunctionalLineItem> FunctionalItems { get; set; } = new List<FunctionalLineItem>();
    public ICollection<NonFunctionalItem> NonFunctionalItems { get; set; } = new List<NonFunctionalItem>();

    public ICollection<ResourceRate> ResourceRates { get; set; } = new List<ResourceRate>();

    public ICollection<RoleMapping> RoleMappings { get; set; } = new List<RoleMapping>();

    public int Version { get; set; } = 1;
    public Guid? ClonedFromEstimateId { get; set; }
}

public class RoleMapping
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EstimateId { get; set; }
    public Estimate? Estimate { get; set; }

    // Source role name as appears in allocations or configs
    public string SourceRole { get; set; } = string.Empty;
    // Target role name that matches a ResourceRate.Role
    public string TargetRole { get; set; } = string.Empty;
}
