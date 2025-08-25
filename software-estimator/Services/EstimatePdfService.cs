using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using software_estimator.Models;

namespace software_estimator.Services;

public interface IEstimatePdfService
{
    byte[] Generate(Estimate e, decimal squadPerSprint, decimal functionalSubtotal, decimal nonFunctionalSubtotal, decimal total);
}

public class EstimatePdfService : IEstimatePdfService
{
    static EstimatePdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Generate(Estimate e, decimal squadPerSprint, decimal functionalSubtotal, decimal nonFunctionalSubtotal, decimal total)
    {
        var now = DateTime.Now;
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Header().Text($"Estimate: {e.Name}").SemiBold().FontSize(18);
                page.Content().Column(col =>
                {
                    col.Item().Text($"Client: {e.Client}");
                    col.Item().Text($"Created: {e.CreatedAtUtc.ToLocalTime():g}  |  Version: {e.Version}");
                    col.Item().Text($"Sprint length: {e.SprintLengthDays} days  |  Squad per sprint: {squadPerSprint:C}");
                    
                    // Add team and resource composition
                    AddResourceComposition(col, e);

                    col.Item().Text("Functional Work").SemiBold();
                    col.Item().Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(8);
                            c.RelativeColumn(6);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                        });
                        t.Header(h =>
                        {
                            h.Cell().Text("#");
                            h.Cell().Text("Title");
                            h.Cell().Text("Sprints");
                            h.Cell().Text("Cost");
                        });
                        var idx = 1;
                        foreach (var f in e.FunctionalItems)
                        {
                            t.Cell().Text(idx++.ToString());
                            var title = f.IsDeviationFlagged ? $"{f.Title} ⚠" : f.Title;
                            t.Cell().Text(title);
                            t.Cell().Text(f.Sprints.ToString());
                            t.Cell().Text((f.Sprints * squadPerSprint).ToString("C"));
                        }
                    });
                    col.Item().Text($"Functional subtotal: {functionalSubtotal:C}");
                    if (e.FunctionalItems.Any(f => f.IsDeviationFlagged))
                    {
                        col.Item().Text("⚠ Some items deviate significantly from average. Review estimates.").FontColor(Colors.Orange.Medium);
                    }

                    col.Item().Text("Non-Functional / Supporting").SemiBold();
                    foreach (var n in e.NonFunctionalItems)
                    {
                        col.Item().Text(n.Title).Bold();
                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(4);
                                c.RelativeColumn(2);
                                c.RelativeColumn(2);
                            });
                            t.Header(h =>
                            {
                                h.Cell().Text("Role");
                                h.Cell().Text("Hours");
                                h.Cell().Text("Cost");
                            });
                            foreach (var a in n.Allocations)
                            {
                                var role = a.Role;
                                var map = e.RoleMappings.FirstOrDefault(m => m.SourceRole.Equals(role, StringComparison.OrdinalIgnoreCase));
                                if (map is not null && !string.IsNullOrWhiteSpace(map.TargetRole)) role = map.TargetRole;
                                var rate = e.ResourceRates.FirstOrDefault(r => r.Role.Equals(role, StringComparison.OrdinalIgnoreCase));
                                var hourly = rate is not null ? (rate.HourlyRate > 0 ? rate.HourlyRate : (rate.DailyRate > 0 ? rate.DailyRate / 8m : 0m)) : 0m;
                                var costLine = a.Hours * hourly;
                                t.Cell().Text(role);
                                t.Cell().Text(a.Hours.ToString());
                                t.Cell().Text(costLine.ToString("C"));
                            }
                        });
                    }
                    col.Item().Text($"Non-functional subtotal: {nonFunctionalSubtotal:C}");

                    col.Item().Text($"Contingency: {e.ContingencyPercent}%");
                    col.Item().Text($"Total: {total:C}").SemiBold();
                });

                page.Footer().AlignRight().Text($"Generated {now:g}");
            });
        });

        return doc.GeneratePdf();
    }

    private void AddResourceComposition(QuestPDF.Fluent.ColumnDescriptor col, Estimate e)
    {
        // Show team assignment if available
        if (!string.IsNullOrEmpty(e.TeamId))
        {
            col.Item().Text($"Assigned Team: {e.TeamId}");
        }

        // Show resource composition based on resource rates
        var fteCount = e.ResourceRates.Count(r => r.Type == ResourceType.FTE);
        var contractorCount = e.ResourceRates.Count(r => r.Type == ResourceType.Contractor);
        
        if (fteCount > 0 || contractorCount > 0)
        {
            col.Item().Text("Resource Composition").SemiBold();
            
            if (fteCount > 0)
            {
                var fteRoles = e.ResourceRates.Where(r => r.Type == ResourceType.FTE).ToList();
                col.Item().Text($"Full-Time Employees: {fteCount}");
                foreach (var fte in fteRoles)
                {
                    col.Item().Text($"  • {fte.Role}: {(fte.HourlyRate > 0 ? fte.HourlyRate.ToString("C") + "/hr" : fte.DailyRate.ToString("C") + "/day")}");
                }
            }
            
            if (contractorCount > 0)
            {
                var contractorRoles = e.ResourceRates.Where(r => r.Type == ResourceType.Contractor).ToList();
                col.Item().Text($"Contractors: {contractorCount}");
                foreach (var contractor in contractorRoles)
                {
                    col.Item().Text($"  • {contractor.Role}: {(contractor.HourlyRate > 0 ? contractor.HourlyRate.ToString("C") + "/hr" : contractor.DailyRate.ToString("C") + "/day")}");
                }
            }
            
            col.Item().Text($"Total Resources: {fteCount + contractorCount}");
            col.Item().Text(" "); // Add spacing
        }
    }
}
