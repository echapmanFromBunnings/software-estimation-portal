# Software Estimator

A Blazor (.NET 9) app for creating software build estimates with config-driven patterns and supporting activities.

## Config

- `config/common_patterns.json`
  - Defines functional patterns with `key`, `title`, and `averageSprints`
- `config/supporting_activities.json`
  - Defines supporting activities with `key`, `title`, `suggestedPercentOfFunctional`, `defaultAllocations`, and `notes`
  - `defaultAllocations` items: `{ "role": "RoleName", "hours": number }`
    - The `hours` number is used as a weight when splitting the activity’s target cost across roles.

Both files are copied to the output and hot-reloaded by the app services when changed.

## Supporting activities, rate mapping, and hour calculation

1. Functional subtotal is computed using `sprints × squadCostPerSprint` per line, summing all lines.
2. When adding a supporting activity with a `suggestedPercentOfFunctional`:
   - Target cost = `functionalSubtotal × percent / 100`
   - That target cost is split across roles listed in `defaultAllocations` by weight (use `hours` as weight; if all are 0 or missing, split equally)
   - For each role, the app looks up a matching `ResourceRate.Role` in the estimate and uses:
     - `HourlyRate` if provided, else `DailyRate / 8`
     - If no rate is found for that role, it falls back to the average hourly rate across all `ResourceRates`
   - Hours are computed as `shareCost / hourlyRate` and rounded to 0.1h; stored under Non-Functional allocations

Baseline activities:
- Some activities in `supporting_activities.json` have `"baseline": true`. These are auto-added (with zero hours) when creating a new estimate (via UI or API). You can remove or edit them later.

Role-rate mapping:
- If a supporting activity references a role that doesn't exist in `ResourceRates`, the editor shows an Unmapped Roles warning with a dropdown to map that source role to an existing rate role. The mapping is stored per-estimate and used everywhere (editor totals, CSV/PDF exports).

Tips:
- Ensure `ResourceRates` Role names match those used in `supporting_activities.json` so the specific rates are applied.
- You can adjust `suggestedPercentOfFunctional` and allocations at any time; they refresh on next add.

## Endpoints

- `GET /api/estimates` and `POST /api/estimates`
- `GET /api/estimates/{id}/json` (single estimate JSON export)
- `GET /api/estimates/{id}/csv` (CSV export)
- `GET /api/estimates/{id}/pdf` (PDF export)
- `GET /api/backup/export` (all estimates JSON)
- `POST /api/backup/import` (import list of estimates)

Note: `POST /api/estimates` will auto-add baseline supporting activities using the current config.

## PDF export details

The PDF includes:
- Metadata (client, created date, version, sprint length, squad per sprint)
- Functional items (title with deviation indicator, sprints, cost), with functional subtotal
- Non-functional breakdown for each item: role, hours, and cost using mapped or direct rates, with non-functional subtotal
- Contingency and total

## Database migrations

This project uses EF Core Migrations (SQLite). The database is migrated on startup.

Common commands:

```powershell
# Add a new migration
dotnet ef migrations add <MigrationName> -p .\software-estimator\software-estimator.csproj -s .\software-estimator\software-estimator.csproj

# Update database
dotnet ef database update -p .\software-estimator\software-estimator.csproj -s .\software-estimator\software-estimator.csproj
```

If you previously ran the app before migrations existed, you may need to delete the dev database file at `bin\Debug\net9.0\App_Data\estimator.db` and then run the update command.

## Validation

The editor enforces basic validations:
- Estimate name is required
- Sprint length must be at least 1
- Contingency must be between 0 and 100
- Rates and hours cannot be negative

## Run

```powershell
# From repository root
dotnet build .\software-estimator\software-estimator.csproj -c Debug

# Run
dotnet run --project .\software-estimator\software-estimator.csproj
```

Open `http://localhost:5000`, go to `Estimates`, and create/edit an estimate.
