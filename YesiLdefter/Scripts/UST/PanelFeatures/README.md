# Panel Features Database Setup

This directory contains SQL scripts for setting up the database-driven panel features system.

## Setup Order

1. **CreateTables.sql** - Creates all required tables
   ```sql
   -- Run this first to create the schema
   ```

2. **SeedData.sql** - Seeds all features, routes, and localizations
   ```sql
   -- Run this after CreateTables.sql to populate initial data
   ```

3. **VerifyAndFix.sql** - Verifies tables exist and shows data counts
   ```sql
   -- Run this to check if everything is set up correctly
   ```

## Tables Created

- `PanelFeature` - Main feature definitions (students, lessons, etc.)
- `PanelFeatureRoute` - Routes/slugs for each feature
- `PanelFeatureLocalization` - Multilingual titles and descriptions
- `PanelFeatureFirmConfig` - Firm-specific feature configuration
- `PanelFeatureBadgeConfig` - Badge configuration for features

## Features Seeded

The SeedData.sql script seeds 22+ features including:

- **Core Features**: students, lessons, inbox
- **More Features**: calendar, broadcast, exams, theory-classes, exam-documents, payments, ledger, invoices, instructors, vehicles, maintenance, sync-mebbis, sync-esrc, sync-conflicts, documents, reports
- **Settings Features**: whatsapp-settings, settings

Each feature includes:
- Main routes
- Sub-routes
- Dynamic routes (with [id], [conflictId], etc.)
- Tab configurations
- Multilingual localizations (TR/EN)
- Badge configurations

## Verification

After running the scripts, verify:

1. Tables exist: Run `VerifyAndFix.sql`
2. Features are seeded: Should see 22+ features
3. Routes are seeded: Should see 50+ routes
4. Localizations are seeded: Should see 100+ localizations

## Troubleshooting

If the Go API returns empty arrays:

1. **Check tables exist**: Run `VerifyAndFix.sql`
2. **Check data is seeded**: Look at row counts in `VerifyAndFix.sql` output
3. **Check database connection**: Verify Go API can connect to the database
4. **Check firmGUID**: Ensure JWT token includes firmGUID
5. **Restart Go API**: After creating tables, restart the Go API server

## Next Steps

After database setup:

1. Restart the Go API server to pick up new routes
2. Test endpoints: `npm run test:panel-features`
3. Check frontend: Features should load dynamically from database

