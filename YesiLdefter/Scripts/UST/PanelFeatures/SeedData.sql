/*CreateData*/

BEGIN TRANSACTION

-- ============================================================
-- Helper Variables
-- ============================================================
DECLARE @StudentsFeatureId INT
DECLARE @LessonsFeatureId INT
DECLARE @InboxFeatureId INT
DECLARE @CalendarFeatureId INT
DECLARE @BroadcastFeatureId INT
DECLARE @ExamsFeatureId INT
DECLARE @TheoryClassesFeatureId INT
DECLARE @ExamDocumentsFeatureId INT
DECLARE @PaymentsFeatureId INT
DECLARE @LedgerFeatureId INT
DECLARE @InvoicesFeatureId INT
DECLARE @InstructorsFeatureId INT
DECLARE @VehiclesFeatureId INT
DECLARE @MaintenanceFeatureId INT
DECLARE @SyncMebbisFeatureId INT
DECLARE @SyncEsrcFeatureId INT
DECLARE @SyncConflictsFeatureId INT
DECLARE @WhatsAppFeatureId INT
DECLARE @WhatsAppSettingsFeatureId INT
DECLARE @SettingsFeatureId INT
DECLARE @DocumentsFeatureId INT
DECLARE @ReportsFeatureId INT

-- Route IDs (will be set after inserts)
DECLARE @StudentsListRouteId INT
DECLARE @StudentsAllRouteId INT
DECLARE @StudentsDetailRouteId INT
DECLARE @LessonsLedgerRouteId INT
DECLARE @LessonsConflictsRouteId INT
DECLARE @LessonsConflictDetailRouteId INT
DECLARE @LessonsTheoreticalRouteId INT
DECLARE @LessonsPracticalRouteId INT
DECLARE @LessonsManualRouteId INT

-- ============================================================
-- 1. STUDENTS FEATURE
-- ============================================================
INSERT INTO [dbo].[PanelFeature] (FeatureKey, IsActive, DisplayOrder, IconName, GroupType)
VALUES ('students', 1, 1, 'people', 'core')

SET @StudentsFeatureId = SCOPE_IDENTITY()

-- Students Routes
INSERT INTO [dbo].[PanelFeatureRoute] (FeatureId, RouteKey, RoutePath, RouteType, ParentRouteId, DisplayOrder, IsActive, ComponentPath, RouteParams, TabConfig, RequiresAuth, RequiresFirm)
VALUES 
  (@StudentsFeatureId, 'list', '/panel/students', 'page', NULL, 1, 1, 'panel/students/page', NULL, NULL, 1, 1),
  (@StudentsFeatureId, 'all', '/panel/students/all', 'page', NULL, 2, 1, 'panel/students/all/page', NULL, NULL, 1, 1),
  (@StudentsFeatureId, 'detail', '/panel/students/[id]', 'dynamic', NULL, 3, 1, 'panel/students/[id]/page', 
   '{"id": {"type": "number", "required": true}}',
   '{"tabs": ["general", "activity-timeline", "document-vault", "exam-results", "education", "accounting"], "default": "general"}', 1, 1)

SELECT @StudentsListRouteId = Id FROM [dbo].[PanelFeatureRoute] WHERE FeatureId = @StudentsFeatureId AND RouteKey = 'list'
SELECT @StudentsAllRouteId = Id FROM [dbo].[PanelFeatureRoute] WHERE FeatureId = @StudentsFeatureId AND RouteKey = 'all'
SELECT @StudentsDetailRouteId = Id FROM [dbo].[PanelFeatureRoute] WHERE FeatureId = @StudentsFeatureId AND RouteKey = 'detail'

-- Update parent relationships
UPDATE [dbo].[PanelFeatureRoute] SET ParentRouteId = @StudentsListRouteId WHERE Id = @StudentsAllRouteId
UPDATE [dbo].[PanelFeatureRoute] SET ParentRouteId = @StudentsListRouteId WHERE Id = @StudentsDetailRouteId

-- Students Localization
INSERT INTO [dbo].[PanelFeatureLocalization] (FeatureId, RouteId, LanguageCode, Title, Description, ShortDescription)
VALUES
  (@StudentsFeatureId, NULL, 'tr-TR', 'Öğrenciler', 'Öğrenci listesi ve arama', 'Öğrenciler'),
  (@StudentsFeatureId, NULL, 'en-US', 'Students', 'Student list and search', 'Students'),
  (NULL, @StudentsListRouteId, 'tr-TR', 'Öğrenci Listesi', 'Tüm öğrencileri görüntüle ve arama yap', 'Liste'),
  (NULL, @StudentsListRouteId, 'en-US', 'Student List', 'View all students and search', 'List'),
  (NULL, @StudentsAllRouteId, 'tr-TR', 'Tüm Öğrenciler', 'Tüm öğrencilerin tam listesi', 'Tümü'),
  (NULL, @StudentsAllRouteId, 'en-US', 'All Students', 'Complete list of all students', 'All'),
  (NULL, @StudentsDetailRouteId, 'tr-TR', 'Öğrenci Detayı', 'Öğrenci bilgileri, belgeler ve geçmiş', 'Detay'),
  (NULL, @StudentsDetailRouteId, 'en-US', 'Student Detail', 'Student information, documents and history', 'Detail')

-- Students Badge Config
INSERT INTO [dbo].[PanelFeatureBadgeConfig] (FeatureId, BadgeType, ApiEndpoint, ApiField, DefaultValue, IsActive)
VALUES (@StudentsFeatureId, 'count', '/api/operations/data/students-count', 'count', 0, 1)

-- ============================================================
-- 2. LESSONS FEATURE
-- ============================================================
INSERT INTO [dbo].[PanelFeature] (FeatureKey, IsActive, DisplayOrder, IconName, GroupType)
VALUES ('lessons', 1, 2, 'auto_stories', 'core')

SET @LessonsFeatureId = SCOPE_IDENTITY()

-- Lessons Routes
INSERT INTO [dbo].[PanelFeatureRoute] (FeatureId, RouteKey, RoutePath, RouteType, ParentRouteId, DisplayOrder, IsActive, ComponentPath, RouteParams, RequiresAuth, RequiresFirm)
VALUES 
  (@LessonsFeatureId, 'ledger', '/panel/lessons', 'page', NULL, 1, 1, 'panel/lessons/page', NULL, 1, 1),
  (@LessonsFeatureId, 'conflicts', '/panel/lessons/conflicts', 'page', NULL, 2, 1, 'panel/lessons/conflicts/page', NULL, 1, 1),
  (@LessonsFeatureId, 'conflict-detail', '/panel/lessons/[conflictId]', 'dynamic', NULL, 3, 1, 'panel/lessons/[conflictId]/page',
   '{"conflictId": {"type": "string", "required": true}}', 1, 1),
  (@LessonsFeatureId, 'theoretical', '/panel/lessons/theoretical', 'page', NULL, 4, 1, 'panel/lessons/theoretical/page', NULL, 1, 1),
  (@LessonsFeatureId, 'practical', '/panel/lessons/practical', 'page', NULL, 5, 1, 'panel/lessons/practical/page', NULL, 1, 1),
  (@LessonsFeatureId, 'manual', '/panel/lessons/manual', 'page', NULL, 6, 1, 'panel/lessons/manual/page', NULL, 1, 1)

SELECT @LessonsLedgerRouteId = Id FROM [dbo].[PanelFeatureRoute] WHERE FeatureId = @LessonsFeatureId AND RouteKey = 'ledger'
SELECT @LessonsConflictsRouteId = Id FROM [dbo].[PanelFeatureRoute] WHERE FeatureId = @LessonsFeatureId AND RouteKey = 'conflicts'
SELECT @LessonsConflictDetailRouteId = Id FROM [dbo].[PanelFeatureRoute] WHERE FeatureId = @LessonsFeatureId AND RouteKey = 'conflict-detail'
SELECT @LessonsTheoreticalRouteId = Id FROM [dbo].[PanelFeatureRoute] WHERE FeatureId = @LessonsFeatureId AND RouteKey = 'theoretical'
SELECT @LessonsPracticalRouteId = Id FROM [dbo].[PanelFeatureRoute] WHERE FeatureId = @LessonsFeatureId AND RouteKey = 'practical'
SELECT @LessonsManualRouteId = Id FROM [dbo].[PanelFeatureRoute] WHERE FeatureId = @LessonsFeatureId AND RouteKey = 'manual'

-- Update parent relationships
UPDATE [dbo].[PanelFeatureRoute] SET ParentRouteId = @LessonsLedgerRouteId WHERE Id = @LessonsConflictsRouteId
UPDATE [dbo].[PanelFeatureRoute] SET ParentRouteId = @LessonsConflictsRouteId WHERE Id = @LessonsConflictDetailRouteId
UPDATE [dbo].[PanelFeatureRoute] SET ParentRouteId = @LessonsLedgerRouteId WHERE Id = @LessonsTheoreticalRouteId
UPDATE [dbo].[PanelFeatureRoute] SET ParentRouteId = @LessonsLedgerRouteId WHERE Id = @LessonsPracticalRouteId
UPDATE [dbo].[PanelFeatureRoute] SET ParentRouteId = @LessonsLedgerRouteId WHERE Id = @LessonsManualRouteId

-- Lessons Localization
INSERT INTO [dbo].[PanelFeatureLocalization] (FeatureId, RouteId, LanguageCode, Title, Description, ShortDescription)
VALUES
  (@LessonsFeatureId, NULL, 'tr-TR', 'Ders Defteri', 'Pratik ders planlaması, eğitmen atamaları ve araç yönetimi', 'Ders Defteri'),
  (@LessonsFeatureId, NULL, 'en-US', 'Lessons Ledger', 'Practical lesson planning, instructor assignments and vehicle management', 'Lessons Ledger'),
  (NULL, @LessonsLedgerRouteId, 'tr-TR', 'Ders Defteri', 'Günlük ders planlaması ve takibi', 'Defter'),
  (NULL, @LessonsLedgerRouteId, 'en-US', 'Lessons Ledger', 'Daily lesson planning and tracking', 'Ledger'),
  (NULL, @LessonsConflictsRouteId, 'tr-TR', 'Ders Çakışmaları', 'Ders çakışmalarını görüntüle ve çöz', 'Çakışmalar'),
  (NULL, @LessonsConflictsRouteId, 'en-US', 'Lesson Conflicts', 'View and resolve lesson conflicts', 'Conflicts'),
  (NULL, @LessonsConflictDetailRouteId, 'tr-TR', 'Çakışma Çözümü', 'Ders çakışmasını çöz', 'Çözüm'),
  (NULL, @LessonsConflictDetailRouteId, 'en-US', 'Conflict Resolution', 'Resolve lesson conflict', 'Resolution'),
  (NULL, @LessonsTheoreticalRouteId, 'tr-TR', 'Teorik Ders Planı', 'Teorik ders planı hazırla', 'Teorik'),
  (NULL, @LessonsTheoreticalRouteId, 'en-US', 'Theoretical Lesson Plan', 'Prepare theoretical lesson plan', 'Theoretical'),
  (NULL, @LessonsPracticalRouteId, 'tr-TR', 'Pratik Ders Planı', 'Pratik ders planı hazırla', 'Pratik'),
  (NULL, @LessonsPracticalRouteId, 'en-US', 'Practical Lesson Plan', 'Prepare practical lesson plan', 'Practical'),
  (NULL, @LessonsManualRouteId, 'tr-TR', 'Manuel Ders Planı', 'Manuel ders planı hazırla', 'Manuel'),
  (NULL, @LessonsManualRouteId, 'en-US', 'Manual Lesson Plan', 'Prepare manual lesson plan', 'Manual')

-- ============================================================
-- 3. INBOX FEATURE
-- ============================================================
INSERT INTO [dbo].[PanelFeature] (FeatureKey, IsActive, DisplayOrder, IconName, GroupType)
VALUES ('inbox', 1, 3, 'inbox', 'core')

SET @InboxFeatureId = SCOPE_IDENTITY()

INSERT INTO [dbo].[PanelFeatureRoute] (FeatureId, RouteKey, RoutePath, RouteType, ParentRouteId, DisplayOrder, IsActive, ComponentPath, QueryParams, RequiresAuth, RequiresFirm)
VALUES 
  (@InboxFeatureId, 'main', '/panel/inbox', 'page', NULL, 1, 1, 'panel/inbox/page', '{"view": "conversations"}', 1, 1)

INSERT INTO [dbo].[PanelFeatureLocalization] (FeatureId, RouteId, LanguageCode, Title, Description, ShortDescription)
VALUES
  (@InboxFeatureId, NULL, 'tr-TR', 'Gelen Kutusu', 'WhatsApp konuşmaları, lead pipeline ve şablonlar', 'Gelen Kutusu'),
  (@InboxFeatureId, NULL, 'en-US', 'Inbox', 'WhatsApp conversations, lead pipeline and templates', 'Inbox')

INSERT INTO [dbo].[PanelFeatureBadgeConfig] (FeatureId, BadgeType, ApiEndpoint, ApiField, DefaultValue, IsActive)
VALUES (@InboxFeatureId, 'count', '/api/operations/data/whatsapp-unread-count', 'count', 0, 1)

-- ============================================================
-- 4. CALENDAR FEATURE
-- ============================================================
INSERT INTO [dbo].[PanelFeature] (FeatureKey, IsActive, DisplayOrder, IconName, GroupType)
VALUES ('calendar', 1, 4, 'calendar_today', 'more')

SET @CalendarFeatureId = SCOPE_IDENTITY()

INSERT INTO [dbo].[PanelFeatureRoute] (FeatureId, RouteKey, RoutePath, RouteType, ParentRouteId, DisplayOrder, IsActive, ComponentPath, RequiresAuth, RequiresFirm)
VALUES 
  (@CalendarFeatureId, 'main', '/panel/calendar', 'page', NULL, 1, 1, 'panel/calendar/page', 1, 1)

INSERT INTO [dbo].[PanelFeatureLocalization] (FeatureId, RouteId, LanguageCode, Title, Description, ShortDescription)
VALUES
  (@CalendarFeatureId, NULL, 'tr-TR', 'Takvim', 'Ders planlama ve takvim', 'Takvim'),
  (@CalendarFeatureId, NULL, 'en-US', 'Calendar', 'Lesson planning and calendar', 'Calendar')

-- ============================================================
-- 5. BROADCAST FEATURE
-- ============================================================
INSERT INTO [dbo].[PanelFeature] (FeatureKey, IsActive, DisplayOrder, IconName, GroupType)
VALUES ('broadcast', 1, 5, 'campaign', 'more')

SET @BroadcastFeatureId = SCOPE_IDENTITY()

INSERT INTO [dbo].[PanelFeatureRoute] (FeatureId, RouteKey, RoutePath, RouteType, ParentRouteId, DisplayOrder, IsActive, ComponentPath, RequiresAuth, RequiresFirm)
VALUES 
  (@BroadcastFeatureId, 'main', '/panel/broadcast', 'page', NULL, 1, 1, 'panel/broadcast/page', 1, 1)

INSERT INTO [dbo].[PanelFeatureLocalization] (FeatureId, RouteId, LanguageCode, Title, Description, ShortDescription)
VALUES
  (@BroadcastFeatureId, NULL, 'tr-TR', 'Toplu Mesaj', 'Birden fazla öğrenciye toplu mesaj gönder', 'Toplu Mesaj'),
  (@BroadcastFeatureId, NULL, 'en-US', 'Broadcast', 'Send bulk messages to multiple students', 'Broadcast')

-- ============================================================
-- 6. EXAMS FEATURE
-- ============================================================
INSERT INTO [dbo].[PanelFeature] (FeatureKey, IsActive, DisplayOrder, IconName, GroupType)
VALUES ('exams', 1, 6, 'quiz', 'more')

SET @ExamsFeatureId = SCOPE_IDENTITY()

DECLARE @ExamsMainRouteId INT
DECLARE @ExamsEExamApplicationsRouteId INT
DECLARE @ExamsEExamEntryDocumentsRouteId INT

INSERT INTO [dbo].[PanelFeatureRoute] (FeatureId, RouteKey, RoutePath, RouteType, ParentRouteId, DisplayOrder, IsActive, ComponentPath, RequiresAuth, RequiresFirm)
VALUES 
  (@ExamsFeatureId, 'main', '/panel/exams', 'page', NULL, 1, 1, 'panel/exams/page', 1, 1),
  (@ExamsFeatureId, 'e-exam-applications', '/panel/exams/e-exam/applications', 'page', NULL, 2, 1, 'panel/exams/e-exam/applications/page', 1, 1),
  (@ExamsFeatureId, 'e-exam-entry-documents', '/panel/exams/e-exam/entry-documents', 'page', NULL, 3, 1, 'panel/exams/e-exam/entry-documents/page', 1, 1)

SELECT @ExamsMainRouteId = Id FROM [dbo].[PanelFeatureRoute] WHERE FeatureId = @ExamsFeatureId AND RouteKey = 'main'
SELECT @ExamsEExamApplicationsRouteId = Id FROM [dbo].[PanelFeatureRoute] WHERE FeatureId = @ExamsFeatureId AND RouteKey = 'e-exam-applications'
SELECT @ExamsEExamEntryDocumentsRouteId = Id FROM [dbo].[PanelFeatureRoute] WHERE FeatureId = @ExamsFeatureId AND RouteKey = 'e-exam-entry-documents'

UPDATE [dbo].[PanelFeatureRoute] SET ParentRouteId = @ExamsMainRouteId WHERE Id = @ExamsEExamApplicationsRouteId
UPDATE [dbo].[PanelFeatureRoute] SET ParentRouteId = @ExamsMainRouteId WHERE Id = @ExamsEExamEntryDocumentsRouteId

INSERT INTO [dbo].[PanelFeatureLocalization] (FeatureId, RouteId, LanguageCode, Title, Description, ShortDescription)
VALUES
  (@ExamsFeatureId, NULL, 'tr-TR', 'Sınavlar', 'Sınav planlama ve sonuçlar', 'Sınavlar'),
  (@ExamsFeatureId, NULL, 'en-US', 'Exams', 'Exam planning and results', 'Exams'),
  (NULL, @ExamsMainRouteId, 'tr-TR', 'Sınavlar', 'Sınav listesi ve yönetimi', 'Sınavlar'),
  (NULL, @ExamsMainRouteId, 'en-US', 'Exams', 'Exam list and management', 'Exams'),
  (NULL, @ExamsEExamApplicationsRouteId, 'tr-TR', 'E-Sınav Başvuruları', 'E-sınav başvurularını yönet', 'Başvurular'),
  (NULL, @ExamsEExamApplicationsRouteId, 'en-US', 'E-Exam Applications', 'Manage e-exam applications', 'Applications'),
  (NULL, @ExamsEExamEntryDocumentsRouteId, 'tr-TR', 'E-Sınav Giriş Belgeleri', 'E-sınav giriş belgelerini yönet', 'Giriş Belgeleri'),
  (NULL, @ExamsEExamEntryDocumentsRouteId, 'en-US', 'E-Exam Entry Documents', 'Manage e-exam entry documents', 'Entry Documents')

-- ============================================================
-- 7. THEORY CLASSES FEATURE
-- ============================================================
INSERT INTO [dbo].[PanelFeature] (FeatureKey, IsActive, DisplayOrder, IconName, GroupType)
VALUES ('theory-classes', 1, 7, 'class', 'more')

SET @TheoryClassesFeatureId = SCOPE_IDENTITY()

INSERT INTO [dbo].[PanelFeatureRoute] (FeatureId, RouteKey, RoutePath, RouteType, ParentRouteId, DisplayOrder, IsActive, ComponentPath, RequiresAuth, RequiresFirm)
VALUES 
  (@TheoryClassesFeatureId, 'main', '/panel/theory-classes', 'page', NULL, 1, 1, 'panel/theory-classes/page', 1, 1)

INSERT INTO [dbo].[PanelFeatureLocalization] (FeatureId, RouteId, LanguageCode, Title, Description, ShortDescription)
VALUES
  (@TheoryClassesFeatureId, NULL, 'tr-TR', 'Teorik Dersler', 'Sınıf grupları yönetimi', 'Teorik Dersler'),
  (@TheoryClassesFeatureId, NULL, 'en-US', 'Theory Classes', 'Classroom group management', 'Theory Classes')

-- ============================================================
-- 8. EXAM DOCUMENTS FEATURE
-- ============================================================
INSERT INTO [dbo].[PanelFeature] (FeatureKey, IsActive, DisplayOrder, IconName, GroupType)
VALUES ('exam-documents', 1, 8, 'description', 'more')

SET @ExamDocumentsFeatureId = SCOPE_IDENTITY()

INSERT INTO [dbo].[PanelFeatureRoute] (FeatureId, RouteKey, RoutePath, RouteType, ParentRouteId, DisplayOrder, IsActive, ComponentPath, RedirectPath, RequiresAuth, RequiresFirm)
VALUES 
  (@ExamDocumentsFeatureId, 'main', '/panel/exam-documents', 'page', NULL, 1, 1, 'panel/exam-documents/page', NULL, 1, 1),
  (@ExamDocumentsFeatureId, 'redirect-from-exams', '/panel/exams/e-exam/entry-documents', 'redirect', NULL, 2, 1, NULL, '/panel/exam-documents', 1, 1)

INSERT INTO [dbo].[PanelFeatureLocalization] (FeatureId, RouteId, LanguageCode, Title, Description, ShortDescription)
VALUES
  (@ExamDocumentsFeatureId, NULL, 'tr-TR', 'E-Sınav Giriş Belgeleri', 'Sınav giriş belgelerini WhatsApp ile gönder', 'E-Sınav Belgeleri'),
  (@ExamDocumentsFeatureId, NULL, 'en-US', 'E-Exam Entry Documents', 'Send exam entry documents via WhatsApp', 'E-Exam Documents')

-- ============================================================
-- 9. PAYMENTS FEATURE
-- ============================================================
INSERT INTO [dbo].[PanelFeature] (FeatureKey, IsActive, DisplayOrder, IconName, GroupType)
VALUES ('payments', 1, 9, 'payments', 'more')

SET @PaymentsFeatureId = SCOPE_IDENTITY()

INSERT INTO [dbo].[PanelFeatureRoute] (FeatureId, RouteKey, RoutePath, RouteType, ParentRouteId, DisplayOrder, IsActive, ComponentPath, RequiresAuth, RequiresFirm)
VALUES 
  (@PaymentsFeatureId, 'main', '/panel/payments', 'page', NULL, 1, 1, 'panel/payments/page', 1, 1)

INSERT INTO [dbo].[PanelFeatureLocalization] (FeatureId, RouteId, LanguageCode, Title, Description, ShortDescription)
VALUES
  (@PaymentsFeatureId, NULL, 'tr-TR', 'Ödemeler', 'Ödeme takibi ve muhasebe', 'Ödemeler'),
  (@PaymentsFeatureId, NULL, 'en-US', 'Payments', 'Payment tracking and accounting', 'Payments')

-- ============================================================
-- 10. LEDGER FEATURE
-- ============================================================
INSERT INTO [dbo].[PanelFeature] (FeatureKey, IsActive, DisplayOrder, IconName, GroupType)
VALUES ('ledger', 1, 10, 'account_balance', 'more')

SET @LedgerFeatureId = SCOPE_IDENTITY()

DECLARE @LedgerMainRouteId INT
DECLARE @LedgerRedirectRouteId INT

INSERT INTO [dbo].[PanelFeatureRoute] (FeatureId, RouteKey, RoutePath, RouteType, ParentRouteId, DisplayOrder, IsActive, ComponentPath, RedirectPath, RequiresAuth, RequiresFirm)
VALUES 
  (@LedgerFeatureId, 'main', '/panel/ledger', 'page', NULL, 1, 1, 'panel/ledger/page', NULL, 1, 1),
  (@LedgerFeatureId, 'redirect-from-finance', '/panel/finance/ledger', 'redirect', NULL, 2, 1, NULL, '/panel/ledger', 1, 1)

SELECT @LedgerMainRouteId = Id FROM [dbo].[PanelFeatureRoute] WHERE FeatureId = @LedgerFeatureId AND RouteKey = 'main'
SELECT @LedgerRedirectRouteId = Id FROM [dbo].[PanelFeatureRoute] WHERE FeatureId = @LedgerFeatureId AND RouteKey = 'redirect-from-finance'

INSERT INTO [dbo].[PanelFeatureLocalization] (FeatureId, RouteId, LanguageCode, Title, Description, ShortDescription)
VALUES
  (@LedgerFeatureId, NULL, 'tr-TR', 'Muhasebe Defteri', 'Gelir ve gider takibi', 'Muhasebe Defteri'),
  (@LedgerFeatureId, NULL, 'en-US', 'Finance Ledger', 'Income and expense tracking', 'Finance Ledger')

-- ============================================================
-- 11. INVOICES FEATURE
-- ============================================================
INSERT INTO [dbo].[PanelFeature] (FeatureKey, IsActive, DisplayOrder, IconName, GroupType)
VALUES ('invoices', 1, 11, 'receipt', 'more')

SET @InvoicesFeatureId = SCOPE_IDENTITY()

DECLARE @InvoicesMainRouteId INT
DECLARE @InvoicesRedirectRouteId INT

INSERT INTO [dbo].[PanelFeatureRoute] (FeatureId, RouteKey, RoutePath, RouteType, ParentRouteId, DisplayOrder, IsActive, ComponentPath, RedirectPath, RequiresAuth, RequiresFirm)
VALUES 
  (@InvoicesFeatureId, 'main', '/panel/invoices', 'page', NULL, 1, 1, 'panel/invoices/page', NULL, 1, 1),
  (@InvoicesFeatureId, 'redirect-from-finance', '/panel/finance/invoices', 'redirect', NULL, 2, 1, NULL, '/panel/invoices', 1, 1)

SELECT @InvoicesMainRouteId = Id FROM [dbo].[PanelFeatureRoute] WHERE FeatureId = @InvoicesFeatureId AND RouteKey = 'main'
SELECT @InvoicesRedirectRouteId = Id FROM [dbo].[PanelFeatureRoute] WHERE FeatureId = @InvoicesFeatureId AND RouteKey = 'redirect-from-finance'

INSERT INTO [dbo].[PanelFeatureLocalization] (FeatureId, RouteId, LanguageCode, Title, Description, ShortDescription)
VALUES
  (@InvoicesFeatureId, NULL, 'tr-TR', 'Faturalar', 'Dijital fatura ve makbuz oluşturma', 'Faturalar'),
  (@InvoicesFeatureId, NULL, 'en-US', 'Invoices', 'Digital invoice and receipt generation', 'Invoices')

-- ============================================================
-- 12. INSTRUCTORS FEATURE
-- ============================================================
INSERT INTO [dbo].[PanelFeature] (FeatureKey, IsActive, DisplayOrder, IconName, GroupType)
VALUES ('instructors', 1, 12, 'school', 'more')

SET @InstructorsFeatureId = SCOPE_IDENTITY()

DECLARE @InstructorsMainRouteId INT
DECLARE @InstructorsResourcesRouteId INT

INSERT INTO [dbo].[PanelFeatureRoute] (FeatureId, RouteKey, RoutePath, RouteType, ParentRouteId, DisplayOrder, IsActive, ComponentPath, RedirectPath, RequiresAuth, RequiresFirm)
VALUES 
  (@InstructorsFeatureId, 'main', '/panel/instructors', 'page', NULL, 1, 1, 'panel/instructors/page', NULL, 1, 1),
  (@InstructorsFeatureId, 'resources', '/panel/resources/instructors', 'page', NULL, 2, 1, 'panel/resources/instructors/page', NULL, 1, 1)

SELECT @InstructorsMainRouteId = Id FROM [dbo].[PanelFeatureRoute] WHERE FeatureId = @InstructorsFeatureId AND RouteKey = 'main'
SELECT @InstructorsResourcesRouteId = Id FROM [dbo].[PanelFeatureRoute] WHERE FeatureId = @InstructorsFeatureId AND RouteKey = 'resources'

INSERT INTO [dbo].[PanelFeatureLocalization] (FeatureId, RouteId, LanguageCode, Title, Description, ShortDescription)
VALUES
  (@InstructorsFeatureId, NULL, 'tr-TR', 'Eğitmenler', 'Eğitmen yönetimi', 'Eğitmenler'),
  (@InstructorsFeatureId, NULL, 'en-US', 'Instructors', 'Instructor management', 'Instructors')

-- ============================================================
-- 13. VEHICLES FEATURE
-- ============================================================
INSERT INTO [dbo].[PanelFeature] (FeatureKey, IsActive, DisplayOrder, IconName, GroupType)
VALUES ('vehicles', 1, 13, 'directions_car', 'more')

SET @VehiclesFeatureId = SCOPE_IDENTITY()

DECLARE @VehiclesMainRouteId INT
DECLARE @VehiclesResourcesRouteId INT

INSERT INTO [dbo].[PanelFeatureRoute] (FeatureId, RouteKey, RoutePath, RouteType, ParentRouteId, DisplayOrder, IsActive, ComponentPath, RequiresAuth, RequiresFirm)
VALUES 
  (@VehiclesFeatureId, 'main', '/panel/vehicles', 'page', NULL, 1, 1, 'panel/vehicles/page', 1, 1),
  (@VehiclesFeatureId, 'resources', '/panel/resources/vehicles', 'page', NULL, 2, 1, 'panel/resources/vehicles/page', 1, 1)

SELECT @VehiclesMainRouteId = Id FROM [dbo].[PanelFeatureRoute] WHERE FeatureId = @VehiclesFeatureId AND RouteKey = 'main'
SELECT @VehiclesResourcesRouteId = Id FROM [dbo].[PanelFeatureRoute] WHERE FeatureId = @VehiclesFeatureId AND RouteKey = 'resources'

INSERT INTO [dbo].[PanelFeatureLocalization] (FeatureId, RouteId, LanguageCode, Title, Description, ShortDescription)
VALUES
  (@VehiclesFeatureId, NULL, 'tr-TR', 'Araçlar', 'Araç yönetimi', 'Araçlar'),
  (@VehiclesFeatureId, NULL, 'en-US', 'Vehicles', 'Vehicle management', 'Vehicles')

-- ============================================================
-- 14. MAINTENANCE FEATURE
-- ============================================================
INSERT INTO [dbo].[PanelFeature] (FeatureKey, IsActive, DisplayOrder, IconName, GroupType)
VALUES ('maintenance', 1, 14, 'build', 'more')

SET @MaintenanceFeatureId = SCOPE_IDENTITY()

DECLARE @MaintenanceMainRouteId INT
DECLARE @MaintenanceResourcesRouteId INT

INSERT INTO [dbo].[PanelFeatureRoute] (FeatureId, RouteKey, RoutePath, RouteType, ParentRouteId, DisplayOrder, IsActive, ComponentPath, RedirectPath, RequiresAuth, RequiresFirm)
VALUES 
  (@MaintenanceFeatureId, 'main', '/panel/maintenance', 'page', NULL, 1, 1, 'panel/maintenance/page', NULL, 1, 1),
  (@MaintenanceFeatureId, 'redirect-from-resources', '/panel/resources/maintenance', 'redirect', NULL, 2, 1, NULL, '/panel/maintenance', 1, 1)

SELECT @MaintenanceMainRouteId = Id FROM [dbo].[PanelFeatureRoute] WHERE FeatureId = @MaintenanceFeatureId AND RouteKey = 'main'
SELECT @MaintenanceResourcesRouteId = Id FROM [dbo].[PanelFeatureRoute] WHERE FeatureId = @MaintenanceFeatureId AND RouteKey = 'redirect-from-resources'

INSERT INTO [dbo].[PanelFeatureLocalization] (FeatureId, RouteId, LanguageCode, Title, Description, ShortDescription)
VALUES
  (@MaintenanceFeatureId, NULL, 'tr-TR', 'Araç Bakımı', 'Araç bakım kayıtları ve takip', 'Araç Bakımı'),
  (@MaintenanceFeatureId, NULL, 'en-US', 'Vehicle Maintenance', 'Vehicle maintenance logs and tracking', 'Vehicle Maintenance')

-- ============================================================
-- 15. SYNC MEBBIS FEATURE
-- ============================================================
INSERT INTO [dbo].[PanelFeature] (FeatureKey, IsActive, DisplayOrder, IconName, GroupType)
VALUES ('sync-mebbis', 1, 15, 'sync', 'more')

SET @SyncMebbisFeatureId = SCOPE_IDENTITY()

INSERT INTO [dbo].[PanelFeatureRoute] (FeatureId, RouteKey, RoutePath, RouteType, ParentRouteId, DisplayOrder, IsActive, ComponentPath, RequiresAuth, RequiresFirm)
VALUES 
  (@SyncMebbisFeatureId, 'main', '/panel/sync/mebbis', 'page', NULL, 1, 1, 'panel/sync/mebbis/page', 1, 1)

INSERT INTO [dbo].[PanelFeatureLocalization] (FeatureId, RouteId, LanguageCode, Title, Description, ShortDescription)
VALUES
  (@SyncMebbisFeatureId, NULL, 'tr-TR', 'MEBBIS Senkron', 'Devlet sistemleri ile senkronizasyon', 'MEBBIS Senkron'),
  (@SyncMebbisFeatureId, NULL, 'en-US', 'MEBBIS Sync', 'Government system synchronization', 'MEBBIS Sync')

-- ============================================================
-- 16. SYNC ESRC FEATURE
-- ============================================================
INSERT INTO [dbo].[PanelFeature] (FeatureKey, IsActive, DisplayOrder, IconName, GroupType)
VALUES ('sync-esrc', 1, 16, 'sync_alt', 'more')

SET @SyncEsrcFeatureId = SCOPE_IDENTITY()

INSERT INTO [dbo].[PanelFeatureRoute] (FeatureId, RouteKey, RoutePath, RouteType, ParentRouteId, DisplayOrder, IsActive, ComponentPath, RequiresAuth, RequiresFirm)
VALUES 
  (@SyncEsrcFeatureId, 'main', '/panel/sync/esrc-external-data', 'page', NULL, 1, 1, 'panel/sync/esrc-external-data/page', 1, 1)

INSERT INTO [dbo].[PanelFeatureLocalization] (FeatureId, RouteId, LanguageCode, Title, Description, ShortDescription)
VALUES
  (@SyncEsrcFeatureId, NULL, 'tr-TR', 'e-src.net Sync', 'e-src.net dış veri senkronizasyonu', 'e-src.net Sync'),
  (@SyncEsrcFeatureId, NULL, 'en-US', 'e-src.net Sync', 'e-src.net external data synchronization', 'e-src.net Sync')

-- ============================================================
-- 17. SYNC CONFLICTS FEATURE
-- ============================================================
INSERT INTO [dbo].[PanelFeature] (FeatureKey, IsActive, DisplayOrder, IconName, GroupType)
VALUES ('sync-conflicts', 1, 17, 'warning', 'more')

SET @SyncConflictsFeatureId = SCOPE_IDENTITY()

INSERT INTO [dbo].[PanelFeatureRoute] (FeatureId, RouteKey, RoutePath, RouteType, ParentRouteId, DisplayOrder, IsActive, ComponentPath, RequiresAuth, RequiresFirm)
VALUES 
  (@SyncConflictsFeatureId, 'main', '/panel/sync/conflicts', 'page', NULL, 1, 1, 'panel/sync/conflicts/page', 1, 1)

INSERT INTO [dbo].[PanelFeatureLocalization] (FeatureId, RouteId, LanguageCode, Title, Description, ShortDescription)
VALUES
  (@SyncConflictsFeatureId, NULL, 'tr-TR', 'Senkronizasyon Çakışmaları', 'Sistemler arası veri uyumsuzluklarını çöz', 'Senkronizasyon Çakışmaları'),
  (@SyncConflictsFeatureId, NULL, 'en-US', 'Sync Conflicts', 'Resolve data mismatches between systems', 'Sync Conflicts')

-- ============================================================
-- 18. WHATSAPP FEATURE
-- ============================================================
INSERT INTO [dbo].[PanelFeature] (FeatureKey, IsActive, DisplayOrder, IconName, GroupType)
VALUES ('whatsapp', 1, 18, 'inbox', 'more')

SET @WhatsAppFeatureId = SCOPE_IDENTITY()

DECLARE @WhatsAppMainRouteId INT
DECLARE @WhatsAppConversationRouteId INT

INSERT INTO [dbo].[PanelFeatureRoute] (FeatureId, RouteKey, RoutePath, RouteType, ParentRouteId, DisplayOrder, IsActive, ComponentPath, RouteParams, QueryParams, RequiresAuth, RequiresFirm)
VALUES 
  (@WhatsAppFeatureId, 'main', '/panel/whatsapp', 'page', NULL, 1, 1, 'panel/whatsapp/page', NULL, '{"compose": "true"}', 1, 1),
  (@WhatsAppFeatureId, 'conversation', '/panel/whatsapp/[conversationId]', 'dynamic', NULL, 2, 1, 'panel/whatsapp/[conversationId]/page',
   '{"conversationId": {"type": "string", "required": true}}', NULL, 1, 1)

SELECT @WhatsAppMainRouteId = Id FROM [dbo].[PanelFeatureRoute] WHERE FeatureId = @WhatsAppFeatureId AND RouteKey = 'main'
SELECT @WhatsAppConversationRouteId = Id FROM [dbo].[PanelFeatureRoute] WHERE FeatureId = @WhatsAppFeatureId AND RouteKey = 'conversation'

UPDATE [dbo].[PanelFeatureRoute] SET ParentRouteId = @WhatsAppMainRouteId WHERE Id = @WhatsAppConversationRouteId

INSERT INTO [dbo].[PanelFeatureLocalization] (FeatureId, RouteId, LanguageCode, Title, Description, ShortDescription)
VALUES
  (@WhatsAppFeatureId, NULL, 'tr-TR', 'WhatsApp', 'WhatsApp mesajlaşma', 'WhatsApp'),
  (@WhatsAppFeatureId, NULL, 'en-US', 'WhatsApp', 'WhatsApp messaging', 'WhatsApp'),
  (NULL, @WhatsAppConversationRouteId, 'tr-TR', 'WhatsApp Konuşması', 'WhatsApp konuşma detayı', 'Konuşma'),
  (NULL, @WhatsAppConversationRouteId, 'en-US', 'WhatsApp Conversation', 'WhatsApp conversation detail', 'Conversation')

-- ============================================================
-- 19. WHATSAPP SETTINGS FEATURE
-- ============================================================
INSERT INTO [dbo].[PanelFeature] (FeatureKey, IsActive, DisplayOrder, IconName, GroupType)
VALUES ('whatsapp-settings', 1, 19, 'settings', 'settings')

SET @WhatsAppSettingsFeatureId = SCOPE_IDENTITY()

INSERT INTO [dbo].[PanelFeatureRoute] (FeatureId, RouteKey, RoutePath, RouteType, ParentRouteId, DisplayOrder, IsActive, ComponentPath, RequiresAuth, RequiresFirm)
VALUES 
  (@WhatsAppSettingsFeatureId, 'main', '/panel/whatsapp-settings', 'page', NULL, 1, 1, 'panel/whatsapp-settings/page', 1, 1)

INSERT INTO [dbo].[PanelFeatureLocalization] (FeatureId, RouteId, LanguageCode, Title, Description, ShortDescription)
VALUES
  (@WhatsAppSettingsFeatureId, NULL, 'tr-TR', 'WhatsApp Ayarları', 'WhatsApp entegrasyon ayarları', 'WhatsApp Ayarları'),
  (@WhatsAppSettingsFeatureId, NULL, 'en-US', 'WhatsApp Settings', 'WhatsApp integration settings', 'WhatsApp Settings')

-- ============================================================
-- 20. SETTINGS FEATURE
-- ============================================================
INSERT INTO [dbo].[PanelFeature] (FeatureKey, IsActive, DisplayOrder, IconName, GroupType)
VALUES ('settings', 1, 20, 'settings', 'settings')

SET @SettingsFeatureId = SCOPE_IDENTITY()

INSERT INTO [dbo].[PanelFeatureRoute] (FeatureId, RouteKey, RoutePath, RouteType, ParentRouteId, DisplayOrder, IsActive, ComponentPath, RequiresAuth, RequiresFirm)
VALUES 
  (@SettingsFeatureId, 'main', '/panel/settings', 'page', NULL, 1, 1, 'panel/settings/page', 1, 1)

INSERT INTO [dbo].[PanelFeatureLocalization] (FeatureId, RouteId, LanguageCode, Title, Description, ShortDescription)
VALUES
  (@SettingsFeatureId, NULL, 'tr-TR', 'Ayarlar', 'Profil ve sistem ayarları', 'Ayarlar'),
  (@SettingsFeatureId, NULL, 'en-US', 'Settings', 'Profile and system settings', 'Settings')

-- ============================================================
-- 21. DOCUMENTS FEATURE
-- ============================================================
INSERT INTO [dbo].[PanelFeature] (FeatureKey, IsActive, DisplayOrder, IconName, GroupType)
VALUES ('documents', 1, 21, 'description', 'more')

SET @DocumentsFeatureId = SCOPE_IDENTITY()

DECLARE @DocumentsMissingRouteId INT
DECLARE @DocumentsEntryRouteId INT

INSERT INTO [dbo].[PanelFeatureRoute] (FeatureId, RouteKey, RoutePath, RouteType, ParentRouteId, DisplayOrder, IsActive, ComponentPath, QueryParams, RequiresAuth, RequiresFirm)
VALUES 
  (@DocumentsFeatureId, 'missing', '/panel/documents/missing', 'page', NULL, 1, 1, 'panel/documents/missing/page', NULL, 1, 1),
  (@DocumentsFeatureId, 'entry', '/panel/documents/entry', 'page', NULL, 2, 1, 'panel/documents/entry/page', '{"studentId": ""}', 1, 1)

SELECT @DocumentsMissingRouteId = Id FROM [dbo].[PanelFeatureRoute] WHERE FeatureId = @DocumentsFeatureId AND RouteKey = 'missing'
SELECT @DocumentsEntryRouteId = Id FROM [dbo].[PanelFeatureRoute] WHERE FeatureId = @DocumentsFeatureId AND RouteKey = 'entry'

INSERT INTO [dbo].[PanelFeatureLocalization] (FeatureId, RouteId, LanguageCode, Title, Description, ShortDescription)
VALUES
  (@DocumentsFeatureId, NULL, 'tr-TR', 'Belgeler', 'Belge yönetimi', 'Belgeler'),
  (@DocumentsFeatureId, NULL, 'en-US', 'Documents', 'Document management', 'Documents'),
  (NULL, @DocumentsMissingRouteId, 'tr-TR', 'Eksik Belgeler', 'Eksik belgeleri takip et', 'Eksik Belgeler'),
  (NULL, @DocumentsMissingRouteId, 'en-US', 'Missing Documents', 'Track missing documents', 'Missing Documents'),
  (NULL, @DocumentsEntryRouteId, 'tr-TR', 'Belge Girişi', 'Yeni belge ekle', 'Belge Girişi'),
  (NULL, @DocumentsEntryRouteId, 'en-US', 'Document Entry', 'Add new document', 'Document Entry')

-- ============================================================
-- 22. REPORTS FEATURE
-- ============================================================
INSERT INTO [dbo].[PanelFeature] (FeatureKey, IsActive, DisplayOrder, IconName, GroupType)
VALUES ('reports', 1, 22, 'chart_line', 'more')

SET @ReportsFeatureId = SCOPE_IDENTITY()

INSERT INTO [dbo].[PanelFeatureRoute] (FeatureId, RouteKey, RoutePath, RouteType, ParentRouteId, DisplayOrder, IsActive, ComponentPath, RequiresAuth, RequiresFirm)
VALUES 
  (@ReportsFeatureId, 'main', '/panel/reports', 'page', NULL, 1, 1, 'panel/reports/page', 1, 1)

INSERT INTO [dbo].[PanelFeatureLocalization] (FeatureId, RouteId, LanguageCode, Title, Description, ShortDescription)
VALUES
  (@ReportsFeatureId, NULL, 'tr-TR', 'Raporlar', 'Sistem raporları', 'Raporlar'),
  (@ReportsFeatureId, NULL, 'en-US', 'Reports', 'System reports', 'Reports')

COMMIT TRANSACTION

/*CreateDataEnd*/

