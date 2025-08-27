using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using software_estimator.Models;
using System.Collections.Generic;

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
        QuestPDF.Settings.EnableDebugging = true;
    }

    public byte[] Generate(Estimate e, decimal squadPerSprint, decimal functionalSubtotal, decimal nonFunctionalSubtotal, decimal total)
    {
        var now = DateTime.Now;
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);

                // Minimal Header - just one line to avoid layout constraints
                page.Header().Height(60).Background(PrimaryColorHex).Padding(15).Row(header =>
                {
                    header.RelativeItem().Text("SOFTWARE DEVELOPMENT ESTIMATE")
                        .FontSize(16).Bold().FontColor(Colors.White);
                    header.ConstantItem(200).AlignRight().Text($"{e.Name ?? "Untitled Estimate"} v{e.Version}")
                        .FontSize(12).FontColor(Colors.White);
                });

                page.Content().Padding(20).Column(content =>
                {
                    // Basic project information
                    content.Item().PaddingBottom(15).Column(section =>
                    {
                        section.Item().Text("Project Overview")
                            .FontSize(16).Bold().FontColor(PrimaryColorHex);
                        
                        section.Item().PaddingTop(10).Text($"Client: {e.Client ?? "Not specified"}")
                            .FontSize(12);
                        section.Item().Text($"Version: {e.Version}")
                            .FontSize(12);
                        section.Item().Text($"Created: {e.CreatedAtUtc.ToLocalTime():MMMM dd, yyyy}")
                            .FontSize(12);
                        
                        if (!string.IsNullOrWhiteSpace(e.PreparedBy))
                        {
                            section.Item().Text($"Prepared by: {e.PreparedBy}")
                                .FontSize(12);
                        }
                    });

                    // Problem Statement (if provided)
                    if (!string.IsNullOrWhiteSpace(e.ProblemStatement))
                    {
                        content.Item().PaddingBottom(15).Column(section =>
                        {
                            section.Item().Text("Problem Statement")
                                .FontSize(16).Bold().FontColor(PrimaryColorHex);
                            section.Item().PaddingTop(8).Text(e.ProblemStatement!)
                                .FontSize(11);
                        });
                    }

                    // Jira Links (if provided)
                    if (!string.IsNullOrWhiteSpace(e.JiraIdeaUrl) || !string.IsNullOrWhiteSpace(e.JiraInitiativeUrl))
                    {
                        content.Item().PaddingBottom(15).Column(section =>
                        {
                            section.Item().Text("Project References")
                                .FontSize(16).Bold().FontColor(PrimaryColorHex);
                            
                            if (!string.IsNullOrWhiteSpace(e.JiraIdeaUrl))
                            {
                                section.Item().PaddingTop(5).Text($"Jira Idea: {e.JiraIdeaUrl}")
                                    .FontSize(10);
                            }
                            
                            if (!string.IsNullOrWhiteSpace(e.JiraInitiativeUrl))
                            {
                                section.Item().PaddingTop(5).Text($"Jira Initiative: {e.JiraInitiativeUrl}")
                                    .FontSize(10);
                            }
                        });
                    }

                    // Financial Summary
                    content.Item().PaddingBottom(15).Column(section =>
                    {
                        section.Item().Text("Financial Summary")
                            .FontSize(16).Bold().FontColor(PrimaryColorHex);

                        section.Item().PaddingTop(10).Column(summary =>
                        {
                            summary.Item().Text($"Functional Subtotal: {functionalSubtotal:C}")
                                .FontSize(12);
                            summary.Item().Text($"Non-Functional Subtotal: {nonFunctionalSubtotal:C}")
                                .FontSize(12);
                            
                            var contingencyAmount = total - functionalSubtotal - nonFunctionalSubtotal;
                            summary.Item().Text($"Contingency ({e.ContingencyPercent}%): {contingencyAmount:C}")
                                .FontSize(12);
                            
                            summary.Item().PaddingTop(8).Text($"TOTAL ESTIMATE: {total:C}")
                                .FontSize(14).Bold().FontColor(PrimaryColorHex);
                        });
                    });

                    // Functional Requirements (simplified)
                    if (e.FunctionalItems?.Any() == true)
                    {
                        content.Item().PaddingBottom(15).Column(section =>
                        {
                            section.Item().Text("Functional Requirements")
                                .FontSize(16).Bold().FontColor(PrimaryColorHex);

                            section.Item().PaddingTop(10).Column(items =>
                            {
                                foreach (var item in e.FunctionalItems.Take(10)) // Limit to avoid overflow
                                {
                                    if (item != null)
                                    {
                                        items.Item().PaddingBottom(5).Column(itemCol =>
                                        {
                                            itemCol.Item().Text($"• {item.Title ?? "Untitled"}")
                                                .FontSize(11).Bold();
                                            itemCol.Item().Text($"  Sprints: {item.Sprints:0.##} | Cost: {item.Cost:C}")
                                                .FontSize(10);
                                        });
                                    }
                                }
                                
                                if (e.FunctionalItems.Count > 10)
                                {
                                    items.Item().Text($"... and {e.FunctionalItems.Count - 10} more items")
                                        .FontSize(10).Italic();
                                }
                            });
                        });
                    }

                    // Non-Functional Requirements (simplified)  
                    if (e.NonFunctionalItems?.Any() == true)
                    {
                        content.Item().PaddingBottom(15).Column(section =>
                        {
                            section.Item().Text("Non-Functional & Supporting Activities")
                                .FontSize(16).Bold().FontColor(PrimaryColorHex);

                            section.Item().PaddingTop(10).Column(items =>
                            {
                                foreach (var item in e.NonFunctionalItems.Take(10))
                                {
                                    if (item != null)
                                    {
                                        items.Item().PaddingBottom(5).Column(itemCol =>
                                        {
                                            itemCol.Item().Text($"• {item.Title ?? "Untitled"}")
                                                .FontSize(11).Bold();
                                            itemCol.Item().Text($"  Cost: {item.Cost:C}")
                                                .FontSize(10);
                                        });
                                    }
                                }
                                
                                if (e.NonFunctionalItems.Count > 10)
                                {
                                    items.Item().Text($"... and {e.NonFunctionalItems.Count - 10} more items")
                                        .FontSize(10).Italic();
                                }
                            });
                        });
                    }

                    // Team Resources (simplified)
                    if (e.ResourceRates?.Any() == true)
                    {
                        content.Item().PaddingBottom(15).Column(section =>
                        {
                            section.Item().Text("Team Resources")
                                .FontSize(16).Bold().FontColor(PrimaryColorHex);

                            section.Item().PaddingTop(10).Column(resources =>
                            {
                                var fteCount = e.ResourceRates.Count(r => r?.Type == ResourceType.FTE);
                                var contractorCount = e.ResourceRates.Count(r => r?.Type == ResourceType.Contractor);
                                
                                resources.Item().Text($"Total Team Size: {fteCount + contractorCount} resources")
                                    .FontSize(12);
                                resources.Item().Text($"FTE: {fteCount} | Contractors: {contractorCount}")
                                    .FontSize(12);
                            });
                        });
                    }
                });

                // Simple Footer
                page.Footer().Height(30).Background(PrimaryColorHex).Padding(10).Row(footer =>
                {
                    footer.RelativeItem().Text("Generated by Software Estimation Portal")
                        .FontSize(8).FontColor(Colors.White);
                    footer.ConstantItem(120).AlignRight().Text($"Generated: {now:MMM dd, yyyy}")
                        .FontSize(8).FontColor(Colors.White);
                });
            });
        });

        return doc.GeneratePdf();
    }
}