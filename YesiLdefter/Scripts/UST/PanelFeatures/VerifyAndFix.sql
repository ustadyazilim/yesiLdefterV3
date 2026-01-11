/*VerifyAndFix*/

-- ============================================================
-- Verify Panel Features Tables and Data
-- ============================================================

-- Check if tables exist
SELECT 
    'PanelFeature' AS TableName,
    CASE WHEN EXISTS (SELECT * FROM sys.tables WHERE name = 'PanelFeature') 
         THEN 'EXISTS' 
         ELSE 'MISSING' 
    END AS Status,
    (SELECT COUNT(*) FROM PanelFeature) AS RowCount
UNION ALL
SELECT 
    'PanelFeatureRoute' AS TableName,
    CASE WHEN EXISTS (SELECT * FROM sys.tables WHERE name = 'PanelFeatureRoute') 
         THEN 'EXISTS' 
         ELSE 'MISSING' 
    END AS Status,
    (SELECT COUNT(*) FROM PanelFeatureRoute) AS RowCount
UNION ALL
SELECT 
    'PanelFeatureLocalization' AS TableName,
    CASE WHEN EXISTS (SELECT * FROM sys.tables WHERE name = 'PanelFeatureLocalization') 
         THEN 'EXISTS' 
         ELSE 'MISSING' 
    END AS Status,
    (SELECT COUNT(*) FROM PanelFeatureLocalization) AS RowCount
UNION ALL
SELECT 
    'PanelFeatureFirmConfig' AS TableName,
    CASE WHEN EXISTS (SELECT * FROM sys.tables WHERE name = 'PanelFeatureFirmConfig') 
         THEN 'EXISTS' 
         ELSE 'MISSING' 
    END AS Status,
    (SELECT COUNT(*) FROM PanelFeatureFirmConfig) AS RowCount
UNION ALL
SELECT 
    'PanelFeatureBadgeConfig' AS TableName,
    CASE WHEN EXISTS (SELECT * FROM sys.tables WHERE name = 'PanelFeatureBadgeConfig') 
         THEN 'EXISTS' 
         ELSE 'MISSING' 
    END AS Status,
    (SELECT COUNT(*) FROM PanelFeatureBadgeConfig) AS RowCount;

-- Show feature count
SELECT 
    'Features' AS Type,
    COUNT(*) AS Count
FROM PanelFeature
WHERE IsActive = 1;

-- Show routes count
SELECT 
    'Routes' AS Type,
    COUNT(*) AS Count
FROM PanelFeatureRoute
WHERE IsActive = 1;

-- Show localizations count
SELECT 
    'Localizations' AS Type,
    COUNT(*) AS Count
FROM PanelFeatureLocalization;

-- Show sample features
SELECT TOP 5
    Id,
    FeatureKey,
    IsActive,
    DisplayOrder,
    IconName,
    GroupType
FROM PanelFeature
ORDER BY DisplayOrder;

-- Show sample routes
SELECT TOP 10
    r.Id,
    r.RouteKey,
    r.RoutePath,
    r.RouteType,
    f.FeatureKey
FROM PanelFeatureRoute r
INNER JOIN PanelFeature f ON r.FeatureId = f.Id
WHERE r.IsActive = 1
ORDER BY f.DisplayOrder, r.DisplayOrder;

-- Show sample localizations
SELECT TOP 10
    l.Id,
    l.LanguageCode,
    l.Title,
    f.FeatureKey,
    r.RouteKey
FROM PanelFeatureLocalization l
LEFT JOIN PanelFeature f ON l.FeatureId = f.Id
LEFT JOIN PanelFeatureRoute r ON l.RouteId = r.Id
ORDER BY l.Id;

/*VerifyAndFixEnd*/

