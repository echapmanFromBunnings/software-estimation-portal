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
    // Professional color scheme using #005358
    private static readonly string PrimaryColorHex = "#005358";
    
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
                page.Size(PageSizes.A4);
                page.Margin(30);

                // Professional Header with #005358 background
                page.Header().Height(100).Background(PrimaryColorHex).Padding(20).Row(header =>
                {
                    header.RelativeItem().Column(col =>
                    {
                        col.Item().Text("SOFTWARE DEVELOPMENT ESTIMATE")
                            .FontSize(18).Bold().FontColor(Colors.White);
                        col.Item().PaddingTop(5).Text(e.Name)
                            .FontSize(14).SemiBold().FontColor(Colors.White);
                    });
                    
                    header.ConstantItem(120).AlignRight().Column(col =>
                    {
                        col.Item().Text($"Version {e.Version}")
                            .FontSize(11).FontColor(Colors.White).Bold();
                        col.Item().PaddingTop(2).Text(e.CreatedAtUtc.ToLocalTime().ToString("MMM dd, yyyy"))
                            .FontSize(10).FontColor(Colors.White);
                        col.Item().PaddingTop(2).Text($"#{e.Id.ToString().Substring(0, 8).ToUpper()}")
                            .FontSize(9).FontColor(Colors.White).Italic();
                    });
                });

                // Main Content
                page.Content().PaddingTop(20).Column(content =>
                {
                    // Project Information Section
                    content.Item().PaddingBottom(20).Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().PaddingBottom(8).Text("PROJECT INFORMATION")
                                .FontSize(12).Bold().FontColor(PrimaryColorHex);
                            col.Item().LineHorizontal(1).LineColor(PrimaryColorHex);
                            col.Item().PaddingTop(8).Background(Colors.Grey.Lighten4).Padding(15).Column(info =>
                            {
                                info.Item().Text($"Client: {e.Client}").FontSize(11).Bold();
                                if (!string.IsNullOrEmpty(e.TeamId))
                                {
                                    info.Item().PaddingTop(3).Text($"Team: {e.TeamId}").FontSize(10);
                                }
                                info.Item().PaddingTop(3).Text($"Sprint Length: {e.SprintLengthDays} days").FontSize(10);
                                info.Item().PaddingTop(3).Text($"Squad Rate: {squadPerSprint:C} per sprint").FontSize(10);
                            });
                        });

                        row.ConstantItem(20); // Spacer

                        row.RelativeItem().Column(col =>
                        {
                            col.Item().PaddingBottom(8).Text("COST BREAKDOWN")
                                .FontSize(12).Bold().FontColor(PrimaryColorHex);
                            col.Item().LineHorizontal(1).LineColor(PrimaryColorHex);
                            col.Item().PaddingTop(8).Background(Colors.Grey.Lighten4).Padding(15).Column(summary =>
                            {
                                summary.Item().Row(r =>
                                {
                                    r.RelativeItem().Text("Functional Work:");
                                    r.ConstantItem(80).AlignRight().Text(functionalSubtotal.ToString("C")).Bold();
                                });
                                summary.Item().PaddingTop(3).Row(r =>
                                {
                                    r.RelativeItem().Text("Non-Functional:");
                                    r.ConstantItem(80).AlignRight().Text(nonFunctionalSubtotal.ToString("C")).Bold();
                                });
                                
                                var contingencyAmount = total - functionalSubtotal - nonFunctionalSubtotal;
                                summary.Item().PaddingTop(3).Row(r =>
                                {
                                    r.RelativeItem().Text($"Contingency ({e.ContingencyPercent}%):");
                                    r.ConstantItem(80).AlignRight().Text(contingencyAmount.ToString("C")).Bold();
                                });
                                
                                summary.Item().PaddingTop(8).BorderTop(1).BorderColor(PrimaryColorHex);
                                summary.Item().PaddingTop(8).Row(r =>
                                {
                                    r.RelativeItem().Text("TOTAL ESTIMATE:").FontSize(12).Bold();
                                    r.ConstantItem(80).AlignRight().Text(total.ToString("C")).FontSize(14).Bold().FontColor(PrimaryColorHex);
                                });
                            });
                        });
                    });

                    // Resource Composition
                    AddResourceComposition(content, e);

                    // Functional Requirements Table
                    content.Item().PaddingTop(20).Column(col =>
                    {
                        col.Item().PaddingBottom(10).Text("FUNCTIONAL REQUIREMENTS")
                            .FontSize(14).Bold().FontColor(PrimaryColorHex);
                        col.Item().LineHorizontal(2).LineColor(PrimaryColorHex);

                        col.Item().PaddingTop(10).Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.ConstantColumn(30);
                                c.RelativeColumn(6);
                                c.ConstantColumn(60);
                                c.ConstantColumn(80);
                                c.ConstantColumn(80);
                            });
                            
                            // Professional table header
                            t.Header(h =>
                            {
                                h.Cell().Background(PrimaryColorHex).Padding(8).Text("#").FontColor(Colors.White).Bold().FontSize(9);
                                h.Cell().Background(PrimaryColorHex).Padding(8).Text("REQUIREMENT").FontColor(Colors.White).Bold().FontSize(9);
                                h.Cell().Background(PrimaryColorHex).Padding(8).AlignCenter().Text("SPRINTS").FontColor(Colors.White).Bold().FontSize(9);
                                h.Cell().Background(PrimaryColorHex).Padding(8).AlignCenter().Text("COST").FontColor(Colors.White).Bold().FontSize(9);
                                h.Cell().Background(PrimaryColorHex).Padding(8).AlignCenter().Text("STATUS").FontColor(Colors.White).Bold().FontSize(9);
                            });
                            
                            var idx = 1;
                            foreach (var f in e.FunctionalItems.OrderByDescending(x => x.Sprints))
                            {
                                var cost = f.Sprints * squadPerSprint;
                                var bgColor = idx % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;
                                
                                t.Cell().Background(bgColor).Padding(6).Text(idx.ToString()).FontSize(9);
                                t.Cell().Background(bgColor).Padding(6).Text(f.Title).FontSize(9);
                                t.Cell().Background(bgColor).Padding(6).AlignCenter().Text(f.Sprints.ToString("F1")).FontSize(9);
                                t.Cell().Background(bgColor).Padding(6).AlignCenter().Text(cost.ToString("C")).FontSize(9).Bold();
                                
                                if (f.IsDeviationFlagged)
                                {
                                    t.Cell().Background(bgColor).Padding(6).AlignCenter()
                                        .Text("⚠ REVIEW").FontSize(8).Bold().FontColor(Colors.Orange.Medium);
                                }
                                else
                                {
                                    t.Cell().Background(bgColor).Padding(6).AlignCenter()
                                        .Text("✓ APPROVED").FontSize(8).FontColor(Colors.Green.Medium);
                                }
                                
                                idx++;
                            }
                        });

                        // Warning for flagged items
                        if (e.FunctionalItems.Any(f => f.IsDeviationFlagged))
                        {
                            col.Item().PaddingTop(10).Background(Colors.Orange.Medium).Padding(10)
                                .Text("⚠ WARNING: Some requirements deviate significantly from estimates. Review recommended.")
                                .FontSize(9).FontColor(Colors.White).Bold();
                        }
                    });

                    // Non-Functional Requirements
                    if (e.NonFunctionalItems.Any())
                    {
                        content.Item().PaddingTop(25).Column(col =>
                        {
                            col.Item().PaddingBottom(10).Text("NON-FUNCTIONAL & SUPPORTING WORK")
                                .FontSize(14).Bold().FontColor(PrimaryColorHex);
                            col.Item().LineHorizontal(2).LineColor(PrimaryColorHex);

                            foreach (var n in e.NonFunctionalItems)
                            {
                                col.Item().PaddingTop(15).Column(nfCol =>
                                {
                                    nfCol.Item().PaddingBottom(5).Text(n.Title)
                                        .FontSize(12).Bold().FontColor("#007580");
                                    if (!string.IsNullOrEmpty(n.Description))
                                    {
                                        nfCol.Item().PaddingBottom(8).Text(n.Description)
                                            .FontSize(9).Italic();
                                    }

                                    nfCol.Item().Table(t =>
                                    {
                                        t.ColumnsDefinition(c =>
                                        {
                                            c.RelativeColumn(4);
                                            c.ConstantColumn(80);
                                            c.ConstantColumn(80);
                                            c.ConstantColumn(80);
                                        });
                                        
                                        t.Header(h =>
                                        {
                                            h.Cell().Background("#007580").Padding(6).Text("ROLE").FontColor(Colors.White).Bold().FontSize(9);
                                            h.Cell().Background("#007580").Padding(6).AlignCenter().Text("HOURS").FontColor(Colors.White).Bold().FontSize(9);
                                            h.Cell().Background("#007580").Padding(6).AlignCenter().Text("RATE").FontColor(Colors.White).Bold().FontSize(9);
                                            h.Cell().Background("#007580").Padding(6).AlignCenter().Text("COST").FontColor(Colors.White).Bold().FontSize(9);
                                        });

                                        var rowIdx = 1;
                                        foreach (var a in n.Allocations)
                                        {
                                            var role = a.Role;
                                            var map = e.RoleMappings.FirstOrDefault(m => m.SourceRole.Equals(role, StringComparison.OrdinalIgnoreCase));
                                            if (map is not null && !string.IsNullOrWhiteSpace(map.TargetRole)) 
                                                role = map.TargetRole;
                                                
                                            var rate = e.ResourceRates.FirstOrDefault(r => r.Role.Equals(role, StringComparison.OrdinalIgnoreCase));
                                            var hourly = rate is not null ? (rate.HourlyRate > 0 ? rate.HourlyRate : (rate.DailyRate > 0 ? rate.DailyRate / 8m : 0m)) : 0m;
                                            var costLine = a.Hours * hourly;
                                            
                                            var bgColor = rowIdx % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;
                                            
                                            t.Cell().Background(bgColor).Padding(6).Text(role).FontSize(9);
                                            t.Cell().Background(bgColor).Padding(6).AlignCenter().Text(a.Hours.ToString("F1")).FontSize(9);
                                            t.Cell().Background(bgColor).Padding(6).AlignCenter().Text(hourly.ToString("C")).FontSize(9);
                                            t.Cell().Background(bgColor).Padding(6).AlignCenter().Text(costLine.ToString("C")).FontSize(9).Bold();
                                            
                                            rowIdx++;
                                        }
                                    });
                                });
                            }
                        });
                    }
                });

                // Professional Footer
                page.Footer().Height(50).Background(PrimaryColorHex).Padding(15).Row(footer =>
                {
                    footer.RelativeItem().Text("Generated by Software Estimation Portal")
                        .FontSize(8).FontColor(Colors.White).Bold();
                    footer.ConstantItem(150).AlignRight().Text($"Generated: {now:MMM dd, yyyy HH:mm}")
                        .FontSize(8).FontColor(Colors.White);
                });
            });
        });

        return doc.GeneratePdf();
    }

    private void AddResourceComposition(QuestPDF.Fluent.ColumnDescriptor content, Estimate e)
    {
        var fteCount = e.ResourceRates.Count(r => r.Type == ResourceType.FTE);
        var contractorCount = e.ResourceRates.Count(r => r.Type == ResourceType.Contractor);
        
        if (fteCount > 0 || contractorCount > 0)
        {
            content.Item().PaddingTop(15).PaddingBottom(15).Column(col =>
            {
                col.Item().PaddingBottom(8).Text("TEAM COMPOSITION & RATES")
                    .FontSize(12).Bold().FontColor(PrimaryColorHex);
                col.Item().LineHorizontal(1).LineColor(PrimaryColorHex);

                col.Item().PaddingTop(8).Table(t =>
                {
                    t.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(3);
                        c.RelativeColumn(2);
                        c.RelativeColumn(3);
                    });

                    t.Header(h =>
                    {
                        h.Cell().Background("#007580").Padding(8).Text("ROLE").FontColor(Colors.White).Bold().FontSize(10);
                        h.Cell().Background("#007580").Padding(8).AlignCenter().Text("TYPE").FontColor(Colors.White).Bold().FontSize(10);
                        h.Cell().Background("#007580").Padding(8).AlignCenter().Text("RATES").FontColor(Colors.White).Bold().FontSize(10);
                    });

                    var idx = 1;
                    foreach (var resource in e.ResourceRates.OrderBy(r => r.Type).ThenBy(r => r.Role))
                    {
                        var bgColor = idx % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;
                        var rateDisplay = resource.HourlyRate > 0 
                            ? $"{resource.HourlyRate:C}/hr • {resource.HourlyRate * 8:C}/day"
                            : $"{resource.DailyRate:C}/day • {resource.DailyRate / 8:C}/hr";

                        t.Cell().Background(bgColor).Padding(8).Text(resource.Role).FontSize(9);
                        t.Cell().Background(bgColor).Padding(8).AlignCenter()
                            .Text(resource.Type == ResourceType.FTE ? "Full-Time" : "Contractor").FontSize(9);
                        t.Cell().Background(bgColor).Padding(8).AlignCenter().Text(rateDisplay).FontSize(9);
                        
                        idx++;
                    }
                });

                // Summary row
                col.Item().PaddingTop(8).Row(r =>
                {
                    r.RelativeItem().Text($"Total Team Size: {fteCount + contractorCount} resources").Bold().FontSize(10);
                    r.RelativeItem().AlignRight().Text($"FTE: {fteCount} • Contractors: {contractorCount}").FontSize(10);
                });
            });
        }
    }
}
