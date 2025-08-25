using software_estimator.Components;
using Microsoft.EntityFrameworkCore;
using software_estimator.Data;
using software_estimator.Services;
using software_estimator.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// SQLite database configuration (local file under app data)
var dbPath = Path.Combine(AppContext.BaseDirectory, "App_Data");
Directory.CreateDirectory(dbPath);
var dbFile = Path.Combine(dbPath, "estimator.db");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbFile}")
          .EnableSensitiveDataLogging(false));

// Config-driven patterns
var patternsPath = Path.Combine(AppContext.BaseDirectory, "config", "common_patterns.json");
builder.Services.Configure<PatternConfigOptions>(o => o.Path = patternsPath);
builder.Services.AddSingleton<IPatternConfigService, PatternConfigService>();

// Cost calculator
builder.Services.AddSingleton<ICostCalculator, CostCalculator>();
builder.Services.AddSingleton<IEstimatePdfService, EstimatePdfService>();

// Config-driven supporting activities
var supportingPath = Path.Combine(AppContext.BaseDirectory, "config", "supporting_activities.json");
builder.Services.Configure<SupportingConfigOptions>(o => o.Path = supportingPath);
builder.Services.AddSingleton<ISupportingConfigService, SupportingConfigService>();

// Config-driven teams
var teamsPath = Path.Combine(AppContext.BaseDirectory, "config", "teams.json");
builder.Services.Configure<TeamConfigOptions>(o => o.Path = teamsPath);
builder.Services.AddSingleton<ITeamConfigService, TeamConfigService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Ensure database is migrated
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

// Minimal API: list and create estimates
app.MapGet("/api/estimates", (AppDbContext db) =>
{
    var items = db.Estimates
        .OrderByDescending(e => e.CreatedAtUtc)
        .Select(e => new { e.Id, e.Name, e.Client, e.CreatedAtUtc, e.Version })
        .ToList();
    return Results.Ok(items);
});

app.MapPost("/api/estimates", async (AppDbContext db, ISupportingConfigService supporting, software_estimator.Contracts.EstimateCreateDto dto) =>
{
    var est = new Estimate
    {
        Name = string.IsNullOrWhiteSpace(dto.Name) ? "New Estimate" : dto.Name!,
        Client = dto.Client ?? string.Empty
    };
    // Add baseline supporting activities as zero-hour placeholders
    var acts = await supporting.GetActivitiesAsync();
    foreach (var a in acts.Where(a => a.Baseline))
    {
        var nf = new NonFunctionalItem { Title = a.Title };
        foreach (var da in a.DefaultAllocations)
        {
            nf.Allocations.Add(new ResourceAllocation { Role = da.Role, Hours = 0 });
        }
        est.NonFunctionalItems.Add(nf);
    }
    db.Estimates.Add(est);
    await db.SaveChangesAsync();
    return Results.Created($"/api/estimates/{est.Id}", new { est.Id });
});

// Offline backup/export: all estimates
app.MapGet("/api/backup/export", async (AppDbContext db) =>
{
    var list = db.Estimates.ToList();
    foreach (var e in list)
    {
        await db.Entry(e).Collection(x => x.FunctionalItems).LoadAsync();
        await db.Entry(e).Collection(x => x.NonFunctionalItems).LoadAsync();
        foreach (var n in e.NonFunctionalItems)
            await db.Entry(n).Collection(x => x.Allocations).LoadAsync();
        await db.Entry(e).Collection(x => x.ResourceRates).LoadAsync();
        await db.Entry(e).Collection(x => x.RoleMappings).LoadAsync();
    }
    return Results.Json(list);
});

// Offline backup/import: accepts an array of estimates
app.MapPost("/api/backup/import", async (AppDbContext db, IEnumerable<Estimate> estimates) =>
{
    int added = 0, skipped = 0;
    foreach (var e in estimates)
    {
        var exists = await db.Estimates.AnyAsync(x => x.Id == e.Id);
        if (exists)
        {
            skipped++;
            continue;
        }
        // ensure child FK set
        foreach (var f in e.FunctionalItems) f.EstimateId = e.Id;
        foreach (var n in e.NonFunctionalItems)
        {
            n.EstimateId = e.Id;
            foreach (var a in n.Allocations) a.NonFunctionalItemId = n.Id;
        }
        foreach (var r in e.ResourceRates) r.EstimateId = e.Id;
        foreach (var m in e.RoleMappings) m.EstimateId = e.Id;
        db.Estimates.Add(e);
        added++;
    }
    await db.SaveChangesAsync();
    return Results.Ok(new { added, skipped });
});
// Minimal API for JSON backup export of an estimate
app.MapGet("/api/estimates/{id:guid}/json", async (Guid id, AppDbContext db) =>
{
    var e = await db.Estimates.FindAsync(id);
    if (e is null) return Results.NotFound();
    await db.Entry(e).Collection(x => x.FunctionalItems).LoadAsync();
    await db.Entry(e).Collection(x => x.NonFunctionalItems).LoadAsync();
    foreach (var n in e.NonFunctionalItems)
        await db.Entry(n).Collection(x => x.Allocations).LoadAsync();
    await db.Entry(e).Collection(x => x.ResourceRates).LoadAsync();
    await db.Entry(e).Collection(x => x.RoleMappings).LoadAsync();
    return Results.Json(e);
});

app.MapGet("/api/estimates/{id:guid}/csv", async (Guid id, AppDbContext db, ICostCalculator cost) =>
{
    var e = await db.Estimates.FindAsync(id);
    if (e is null) return Results.NotFound();
    await db.Entry(e).Collection(x => x.FunctionalItems).LoadAsync();
    await db.Entry(e).Collection(x => x.NonFunctionalItems).LoadAsync();
    foreach (var n in e.NonFunctionalItems)
        await db.Entry(n).Collection(x => x.Allocations).LoadAsync();
    await db.Entry(e).Collection(x => x.ResourceRates).LoadAsync();
    await db.Entry(e).Collection(x => x.RoleMappings).LoadAsync();

    var sprintDays = e.SprintLengthDays;
    var fte = e.ResourceRates.Where(r => r.Type == ResourceType.FTE).ToList();
    var contractors = e.ResourceRates.Where(r => r.Type == ResourceType.Contractor).ToList();
    var comp = new SquadComposition(
        fte.Count,
        contractors.Count,
        fte.DefaultIfEmpty(new ResourceRate { DailyRate = 0 }).Average(r => r.DailyRate),
        contractors.DefaultIfEmpty(new ResourceRate { DailyRate = 0 }).Average(r => r.DailyRate),
        sprintDays
    );
    var squadPerSprint = cost.CalcSquadCostPerSprint(comp);

    var sb = new System.Text.StringBuilder();
    sb.AppendLine("Section,Title,Detail,Sprints/Hours,Cost");
    foreach (var f in e.FunctionalItems)
        sb.AppendLine($"Functional,{EscapeCsv(f.Title)},, {f.Sprints}, {f.Sprints * squadPerSprint}");
    foreach (var n in e.NonFunctionalItems)
    {
        foreach (var a in n.Allocations)
        {
            var role = a.Role;
            var map = e.RoleMappings.FirstOrDefault(m => m.SourceRole.Equals(role, StringComparison.OrdinalIgnoreCase));
            if (map is not null && !string.IsNullOrWhiteSpace(map.TargetRole)) role = map.TargetRole;
            var rate = e.ResourceRates.FirstOrDefault(r => r.Role.Equals(role, StringComparison.OrdinalIgnoreCase));
            var hourly = rate is not null ? (rate.HourlyRate > 0 ? rate.HourlyRate : rate.DailyRate/8m) : 0m;
            var costLine = a.Hours * hourly;
            sb.AppendLine($"NonFunctional,{EscapeCsv(n.Title)},{role},{a.Hours},{costLine}");
        }
    }
    static string EscapeCsv(string s) => '"' + (s?.Replace("\"", "\"\"") ?? string.Empty) + '"';
    return Results.Text(sb.ToString(), "text/csv");
});

app.MapGet("/api/estimates/{id:guid}/pdf", async (Guid id, AppDbContext db, ICostCalculator cost, IEstimatePdfService pdf) =>
{
    var e = await db.Estimates.FindAsync(id);
    if (e is null) return Results.NotFound();
    await db.Entry(e).Collection(x => x.FunctionalItems).LoadAsync();
    await db.Entry(e).Collection(x => x.NonFunctionalItems).LoadAsync();
    foreach (var n in e.NonFunctionalItems)
        await db.Entry(n).Collection(x => x.Allocations).LoadAsync();
    await db.Entry(e).Collection(x => x.ResourceRates).LoadAsync();

    var sprintDays = e.SprintLengthDays;
    var fte = e.ResourceRates.Where(r => r.Type == ResourceType.FTE).ToList();
    var contractors = e.ResourceRates.Where(r => r.Type == ResourceType.Contractor).ToList();
    var comp = new SquadComposition(
        fte.Count,
        contractors.Count,
        fte.DefaultIfEmpty(new ResourceRate { DailyRate = 0 }).Average(r => r.DailyRate),
        contractors.DefaultIfEmpty(new ResourceRate { DailyRate = 0 }).Average(r => r.DailyRate),
        sprintDays
    );
    var squadPerSprint = cost.CalcSquadCostPerSprint(comp);
    var functionalSubtotal = e.FunctionalItems.Sum(f => cost.CalcFunctionalLineCost(f.Sprints, squadPerSprint));
    var nonFunctionalSubtotal = e.NonFunctionalItems.Sum(n => cost.CalcNonFunctionalCost(n, e.ResourceRates, e.RoleMappings));
    var total = cost.ApplyContingency(functionalSubtotal + nonFunctionalSubtotal, e.ContingencyPercent);

    var bytes = pdf.Generate(e, squadPerSprint, functionalSubtotal, nonFunctionalSubtotal, total);
    return Results.File(bytes, "application/pdf", $"estimate-{e.Id}.pdf");
});

app.Run();
