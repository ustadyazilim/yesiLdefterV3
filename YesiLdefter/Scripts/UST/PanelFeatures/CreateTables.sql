/*CreateTable*/

BEGIN TRANSACTION

-- ============================================================
-- PanelFeature Table
-- Stores feature definitions (students, lessons, exams, etc.)
-- ============================================================
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'PanelFeature' AND type = 'U')
    DROP TABLE [dbo].[PanelFeature]

CREATE TABLE [dbo].[PanelFeature] (
    Id                INT IDENTITY(1,1) NOT NULL,
    FeatureKey        VARCHAR(100) NOT NULL UNIQUE, -- 'students', 'lessons', 'exams'
    IsActive          BIT NOT NULL DEFAULT 1,
    DisplayOrder      INT NULL,
    IconName          VARCHAR(50) NULL, -- 'people', 'auto_stories', 'quiz'
    GroupType         VARCHAR(50) NULL, -- 'core', 'more', 'settings'
    CreatedAt         DATETIME2 DEFAULT GETDATE(),
    UpdatedAt         DATETIME2 DEFAULT GETDATE(),
    
    CONSTRAINT pk_PanelFeature PRIMARY KEY (Id)
)

-- ============================================================
-- PanelFeatureRoute Table
-- Stores all routes/slugs for each feature with enhanced fields
-- ============================================================
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'PanelFeatureRoute' AND type = 'U')
    DROP TABLE [dbo].[PanelFeatureRoute]

CREATE TABLE [dbo].[PanelFeatureRoute] (
    Id                INT IDENTITY(1,1) NOT NULL,
    FeatureId         INT NOT NULL,
    RouteKey          VARCHAR(100) NOT NULL, -- 'list', 'detail', 'all', 'conflicts'
    RoutePath         VARCHAR(500) NOT NULL, -- '/panel/students', '/panel/students/[id]'
    RouteType         VARCHAR(50) NOT NULL, -- 'page', 'dynamic', 'redirect'
    ParentRouteId     INT NULL,
    DisplayOrder      INT NULL,
    IsActive          BIT NOT NULL DEFAULT 1,
    ComponentPath     VARCHAR(500) NULL, -- 'panel/students/page', 'panel/students/[id]/page'
    RouteParams       NVARCHAR(MAX) NULL, -- JSON: {"id": {"type": "number", "required": true}}
    TabConfig         NVARCHAR(MAX) NULL, -- JSON: {"tabs": ["general", "activity-timeline"], "default": "general"}
    QueryParams       NVARCHAR(MAX) NULL, -- JSON: {"action": "new", "status": "all"}
    RedirectPath      VARCHAR(500) NULL, -- For redirect routes
    RequiresAuth      BIT NOT NULL DEFAULT 1,
    RequiresFirm      BIT NOT NULL DEFAULT 1,
    CreatedAt         DATETIME2 DEFAULT GETDATE(),
    UpdatedAt         DATETIME2 DEFAULT GETDATE(),
    
    CONSTRAINT pk_PanelFeatureRoute PRIMARY KEY (Id),
    CONSTRAINT fk_PanelFeatureRoute_Feature FOREIGN KEY (FeatureId) REFERENCES [dbo].[PanelFeature](Id) ON DELETE CASCADE,
    CONSTRAINT fk_PanelFeatureRoute_Parent FOREIGN KEY (ParentRouteId) REFERENCES [dbo].[PanelFeatureRoute](Id),
    CONSTRAINT uq_PanelFeatureRoute_FeatureKey UNIQUE (FeatureId, RouteKey)
)

-- ============================================================
-- PanelFeatureLocalization Table
-- Multilingual titles and descriptions
-- ============================================================
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'PanelFeatureLocalization' AND type = 'U')
    DROP TABLE [dbo].[PanelFeatureLocalization]

CREATE TABLE [dbo].[PanelFeatureLocalization] (
    Id                INT IDENTITY(1,1) NOT NULL,
    FeatureId         INT NULL,
    RouteId           INT NULL,
    LanguageCode      VARCHAR(10) NOT NULL, -- 'tr-TR', 'en-US'
    Title             NVARCHAR(200) NULL,
    Description       NVARCHAR(500) NULL,
    ShortDescription  NVARCHAR(200) NULL,
    CreatedAt         DATETIME2 DEFAULT GETDATE(),
    UpdatedAt         DATETIME2 DEFAULT GETDATE(),
    
    CONSTRAINT pk_PanelFeatureLocalization PRIMARY KEY (Id),
    CONSTRAINT fk_PanelFeatureLocalization_Feature FOREIGN KEY (FeatureId) REFERENCES [dbo].[PanelFeature](Id) ON DELETE CASCADE,
    CONSTRAINT fk_PanelFeatureLocalization_Route FOREIGN KEY (RouteId) REFERENCES [dbo].[PanelFeatureRoute](Id) ON DELETE CASCADE,
    CONSTRAINT chk_PanelFeatureLocalization_FeatureOrRoute CHECK ((FeatureId IS NOT NULL AND RouteId IS NULL) OR (FeatureId IS NULL AND RouteId IS NOT NULL))
)

-- ============================================================
-- PanelFeatureFirmConfig Table
-- Firm-specific feature configuration
-- ============================================================
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'PanelFeatureFirmConfig' AND type = 'U')
    DROP TABLE [dbo].[PanelFeatureFirmConfig]

CREATE TABLE [dbo].[PanelFeatureFirmConfig] (
    Id                INT IDENTITY(1,1) NOT NULL,
    FirmId            INT NOT NULL,
    FeatureId         INT NOT NULL,
    RouteId           INT NULL,
    IsEnabled         BIT NOT NULL DEFAULT 1,
    IsVisible         BIT NOT NULL DEFAULT 1,
    DisplayOrder      INT NULL,
    BadgeConfig       NVARCHAR(MAX) NULL, -- JSON: {"type": "count", "endpoint": "/api/students/count"}
    CustomIcon        VARCHAR(50) NULL,
    CustomTitle       NVARCHAR(200) NULL,
    CustomDescription NVARCHAR(500) NULL,
    Metadata          NVARCHAR(MAX) NULL, -- JSON for additional config
    CreatedAt         DATETIME2 DEFAULT GETDATE(),
    UpdatedAt         DATETIME2 DEFAULT GETDATE(),
    
    CONSTRAINT pk_PanelFeatureFirmConfig PRIMARY KEY (Id),
    CONSTRAINT fk_PanelFeatureFirmConfig_Feature FOREIGN KEY (FeatureId) REFERENCES [dbo].[PanelFeature](Id) ON DELETE CASCADE,
    CONSTRAINT fk_PanelFeatureFirmConfig_Route FOREIGN KEY (RouteId) REFERENCES [dbo].[PanelFeatureRoute](Id) ON DELETE CASCADE,
    CONSTRAINT uq_PanelFeatureFirmConfig_FirmFeatureRoute UNIQUE (FirmId, FeatureId, RouteId)
)

-- ============================================================
-- PanelFeatureBadgeConfig Table
-- Badge configuration for features (unread counts, etc.)
-- ============================================================
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'PanelFeatureBadgeConfig' AND type = 'U')
    DROP TABLE [dbo].[PanelFeatureBadgeConfig]

CREATE TABLE [dbo].[PanelFeatureBadgeConfig] (
    Id                INT IDENTITY(1,1) NOT NULL,
    FeatureId         INT NOT NULL,
    BadgeType         VARCHAR(50) NOT NULL, -- 'count', 'status', 'custom'
    ApiEndpoint       VARCHAR(500) NULL,
    ApiField          VARCHAR(100) NULL, -- Field name in API response
    DefaultValue      INT DEFAULT 0,
    IsActive          BIT NOT NULL DEFAULT 1,
    CreatedAt         DATETIME2 DEFAULT GETDATE(),
    
    CONSTRAINT pk_PanelFeatureBadgeConfig PRIMARY KEY (Id),
    CONSTRAINT fk_PanelFeatureBadgeConfig_Feature FOREIGN KEY (FeatureId) REFERENCES [dbo].[PanelFeature](Id) ON DELETE CASCADE
)

-- Create indexes for performance
CREATE INDEX idx_PanelFeature_FeatureKey ON [dbo].[PanelFeature](FeatureKey)
CREATE INDEX idx_PanelFeatureRoute_FeatureId ON [dbo].[PanelFeatureRoute](FeatureId)
CREATE INDEX idx_PanelFeatureRoute_ParentRouteId ON [dbo].[PanelFeatureRoute](ParentRouteId)
CREATE INDEX idx_PanelFeatureRoute_RoutePath ON [dbo].[PanelFeatureRoute](RoutePath)
CREATE INDEX idx_PanelFeatureLocalization_FeatureId ON [dbo].[PanelFeatureLocalization](FeatureId)
CREATE INDEX idx_PanelFeatureLocalization_RouteId ON [dbo].[PanelFeatureLocalization](RouteId)
CREATE INDEX idx_PanelFeatureLocalization_LanguageCode ON [dbo].[PanelFeatureLocalization](LanguageCode)
CREATE INDEX idx_PanelFeatureFirmConfig_FirmId ON [dbo].[PanelFeatureFirmConfig](FirmId)
CREATE INDEX idx_PanelFeatureFirmConfig_FeatureId ON [dbo].[PanelFeatureFirmConfig](FeatureId)
CREATE INDEX idx_PanelFeatureBadgeConfig_FeatureId ON [dbo].[PanelFeatureBadgeConfig](FeatureId)

GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[PanelFeature] TO public
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[PanelFeatureRoute] TO public
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[PanelFeatureLocalization] TO public
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[PanelFeatureFirmConfig] TO public
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[PanelFeatureBadgeConfig] TO public

COMMIT TRANSACTION

/*CreateTableEnd*/

