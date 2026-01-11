# Panel Features Setup Instructions

## Current Status

The panel features API is implemented but needs database setup and Go API restart.

## Required Steps

### 1. Create Database Tables

Run the SQL scripts in order:

```sql
-- Step 1: Create tables
-- Execute: CreateTables.sql
-- This creates all 5 tables needed for panel features

-- Step 2: Seed data  
-- Execute: SeedData.sql
-- This seeds 22+ features with all routes, localizations, and badges

-- Step 3: Verify
-- Execute: VerifyAndFix.sql
-- This shows table status and data counts
```

### 2. Restart Go API Server

After creating tables, **restart the Go API server** to:
- Pick up new routes
- Load new database schema
- Register panel feature endpoints

### 3. Test the API

```bash
cd C:\UstadWeb\ustad-web\apps\ustad-web-yesildefter
npm run test:panel-features
```

## Expected Results After Setup

### Database Tables Should Have:

- **PanelFeature**: ~22 rows (one per feature)
- **PanelFeatureRoute**: ~50+ rows (routes for all features)
- **PanelFeatureLocalization**: ~100+ rows (TR/EN for features and routes)
- **PanelFeatureBadgeConfig**: ~2-3 rows (badges for students, inbox)
- **PanelFeatureFirmConfig**: 0 rows initially (firm-specific configs created on demand)

### API Endpoints Should Return:

- `GET /api/panel/features` - Array of all features with routes
- `GET /api/panel/features/students` - Students feature with complete route tree
- `GET /api/panel/routes/resolve?path=/panel/students` - Route configuration
- `GET /api/panel/routes/validate?path=/panel/students` - Access validation

## Troubleshooting

### If API returns 404:

1. **Check Go API is running**: `http://localhost:8080`
2. **Check routes are registered**: Look for `/api/panel/*` in Go API logs
3. **Restart Go API** after creating tables

### If API returns empty arrays:

1. **Check tables exist**: Run `VerifyAndFix.sql`
2. **Check data is seeded**: Look at row counts
3. **Check database connection**: Verify Go API can connect
4. **Check firmGUID**: Ensure JWT includes firmGUID

### If API returns errors:

1. **Check database connection string** in Go API `.env`
2. **Check UstadFirms table exists** in tenant database
3. **Check PanelFeature tables exist** in tenant database
4. **Check Go API logs** for detailed error messages

## Database Location

The panel features tables should be created in the **same database** as other MTSK tables (e.g., `Mtsk00000011` or the tenant-specific database).

The Go API uses tenant resolution to connect to the correct database based on `firmGUID`.

## Next Steps After Setup

1. ✅ Tables created and seeded
2. ✅ Go API restarted
3. ✅ API endpoints tested
4. ✅ Frontend loads features dynamically
5. ✅ Features configurable per firm

