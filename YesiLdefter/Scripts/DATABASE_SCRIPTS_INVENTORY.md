# Database Scripts Inventory

This document provides a comprehensive inventory of all database scripts in the `Scripts/` directory, organized by category and purpose.

## Overview

Total script files: ~600+ SQL scripts organized into multiple categories for different modules and database systems.

## Script Categories

### 1. Core System Scripts

#### 1.1 2019VtMsSql/ (Legacy Core)
- **Purpose**: Legacy database structure from 2019
- **Files**: 28 scripts
- **Key Scripts**:
  - `SYS_TYPES_H.txt`, `SYS_TYPES_L.txt`, `SYS_TYPES_T.txt` - System type definitions
  - `SYS_VARIABLES.txt` - System variables
  - `MS_VARIABLES.txt` - Module variables
  - `HP_*.txt` - Core tables (HP_CARI, HP_FINANS, HP_USERS, etc.)
  - `prc_*.txt` - Stored procedures
  - `STK_*.txt` - Stock/inventory tables

#### 1.2 MPv3_vtMsSql/VT_MSSQL/ (MPv3 Core)
- **Purpose**: MPv3 module database structure for MS SQL Server
- **Files**: 50+ scripts
- **Key Scripts**:
  - `MS_TABLES.txt`, `MS_TABLES_IP.txt` - Core table definitions
  - `MS_FIELDS.txt`, `MS_FIELDS_IP.txt` - Field definitions
  - `MS_LAYOUT.txt`, `MS_LAYOUT_VIEW.txt` - Layout definitions
  - `MS_ITEMS.txt`, `MS_GROUPS.txt` - Menu/item structures
  - `MS_REPORTS.txt` - Report definitions
  - `SYS_*.txt` - System tables
  - `VW_*.txt` - Views
  - `trg_*.txt` - Triggers

#### 1.3 yesiL_vtMsSql/ (YesiL Core)
- **Purpose**: YesiL core database scripts
- **Files**: 63 scripts (both MS SQL and MySQL versions)
- **Subdirectories**:
  - `VT_MSSQL/` - MS SQL Server scripts
  - `VT_mySQL/` - MySQL scripts
- **Key Scripts**:
  - Core tables (HP_*, FN_*, AJ_*, STK_*)
  - Views (VW_*)
  - Triggers (trg_*)
  - Stored procedures (prc_*)

### 2. UST (Ustad) Module Scripts

#### 2.1 UST/Hub/ (Hub Module)
- **Purpose**: Hub-related tables and procedures
- **Files**: 7 scripts
- **Key Scripts**:
  - `HubBildirimSablonlari.txt` - Notification templates
  - `HubGecerlilikTarihi.txt` - Validity dates
  - `HubMtskSaat*.txt` - MTSK hour definitions
  - `HubMtskUcret*.txt` - MTSK fee definitions

#### 2.2 UST/MsV3/ (MS V3 Module)
- **Purpose**: MS V3 module database structure
- **Files**: 50 scripts
- **Key Scripts**:
  - `MsTables.txt`, `MsTablesIP.txt` - Table definitions
  - `MsProjectTables.txt` - Project tables
  - `MsReports.txt` - Reports
  - `MsWebNodes.txt`, `MsWebPages.txt` - Web structure
  - `prc_*.txt` - Stored procedures
  - `fnc_*.txt` - Functions

#### 2.3 UST/Mtsk/ (MTSK Module)
- **Purpose**: MTSK (Motorlu Taşıt Sürücü Kursu) module
- **Files**: 158 scripts
- **Key Scripts**:
  - `MtskAday*.txt` - Candidate tables
  - `MtskDers*.txt` - Lesson tables
  - `MtskSinav*.txt` - Exam tables
  - `MtskSertifika*.txt` - Certificate tables
  - `MtskUcret*.txt` - Fee tables
  - `prc_Mtsk*.txt` - Stored procedures
  - `fnc_*.txt` - Functions
  - `Lkp_*.txt` - Lookup tables

#### 2.4 UST/OnMuhasebe/ (Accounting Module)
- **Purpose**: Accounting/Finance module
- **Files**: 135 scripts
- **Subdirectories**:
  - `Duzenlenen/` - Updated/edited scripts
- **Key Scripts**:
  - `OnmHesap*.txt` - Account tables
  - `OnmBelge*.txt` - Document tables
  - `OnmCari*.txt` - Customer tables
  - `OnmOdemePlani.txt` - Payment plan
  - `OnmMaas*.txt` - Salary tables
  - `prc_Onm*.txt` - Stored procedures
  - `fnc_*.txt` - Functions
  - `Lkp_*.txt` - Lookup tables
  - `GIB_*.txt` - GIB (Tax Authority) related tables

#### 2.5 UST/SRC/ (SRC Module)
- **Purpose**: SRC module database structure
- **Files**: 58 scripts
- **Key Scripts**:
  - `SrcAday*.txt` - Candidate tables
  - `SrcSinav*.txt` - Exam tables
  - `SrcTeorikDers*.txt` - Theoretical lesson tables
  - `SrcUcret*.txt` - Fee tables
  - `prc_Src*.txt` - Stored procedures
  - `fnc_*.txt` - Functions
  - `Lkp_*.txt` - Lookup tables

#### 2.6 UST/TabimMtsk/ (Tabim MTSK)
- **Purpose**: Tabim MTSK module
- **Files**: 7 scripts
- **Key Scripts**:
  - `KursiyerFatura.txt` - Student invoice
  - `prc_KursiyerSinavDurumu.txt` - Student exam status procedure
  - `trg_*.txt` - Triggers

#### 2.7 UST/Ustad/ (Ustad Core)
- **Purpose**: Ustad core tables
- **Files**: 8 scripts
- **Key Scripts**:
  - `HP_COMPS.txt`, `HP_FIRMS.txt`, `HP_USERS.txt` - Core tables
  - `MS_VARIABLES.txt` - Variables
  - `SYS_*.txt` - System tables

#### 2.8 UST/UstadCRM/ (CRM Module)
- **Purpose**: Customer Relationship Management
- **Files**: 22 scripts
- **Key Scripts**:
  - `AspNet*.txt` - ASP.NET Identity tables
  - `EduUsers.txt`, `EduUserTokens.txt` - Education users
  - `UstadUsers.txt`, `UstadFirms.txt` - User and firm tables
  - `prc_*.txt` - Stored procedures

#### 2.9 UST/Wentec/ (Wentec Module)
- **Purpose**: Wentec module
- **Files**: 1 script
- **Key Script**: `KURSIYER_KARTI.txt` - Student card

#### 2.10 UST/ResmiMuhasebe/ (Official Accounting)
- **Purpose**: Official accounting structure
- **Files**: 1 script
- **Key Script**: `HesapPlani.txt` - Chart of accounts

### 3. Design & Development Scripts

#### 3.1 UstadDesign/ (Design System)
- **Purpose**: Database design and development tools
- **Files**: 13 scripts
- **Key Scripts**:
  - `00_createDatabas.txt` - Database creation
  - `UstProjects.txt`, `UstModuls.txt` - Project/module definitions
  - `UstTables.txt`, `UstTablesIP.txt` - Table definitions
  - `UstFields.txt`, `UstFieldsIP.txt` - Field definitions
  - `sp_GenerateTableScript.txt` - Table script generator
  - `sp_GenerateLookupScript.txt` - Lookup script generator

### 4. Specialized Modules

#### 4.1 SEK/InfazKurumu/ (Prison Administration)
- **Purpose**: Prison administration module
- **Files**: 12 scripts
- **Key Scripts**:
  - `CihazHesap.txt`, `CihazLog*.txt` - Device account/logs
  - `prc_Cari*.txt` - Customer procedures
  - `prc_Cihaz*.txt` - Device procedures
  - `prc_Eylem*.txt` - Action procedures
  - `tablolar.txt` - Tables

### 5. Documentation

#### 5.1 KaynakBelgeler/ (Source Documents)
- **Purpose**: Reference documents
- **Files**: 1 PDF
- **File**: `vergidairelerilistesi.pdf` - Tax office list

## Script Naming Conventions

### Prefixes
- `HP_*` - Core tables (HP = Head/Base)
- `MS_*` - Module/System tables
- `SYS_*` - System tables
- `prc_*` - Stored procedures
- `fnc_*` - Functions
- `VW_*` - Views
- `trg_*` - Triggers
- `Lkp_*` - Lookup tables
- `Onm*` - Accounting module tables
- `Mtsk*` - MTSK module tables
- `Src*` - SRC module tables

### Suffixes
- `_IP` - IP (likely "Item Property" or similar)
- `_TS` - Timestamp related
- `_B` - Base/Master
- `_S` - Sub/Detail
- `_F` - Filter
- `_BA` - Backup/Archive

## Critical Scripts for Production

### Must-Have for Initial Setup:
1. **System Core**:
   - `2019VtMsSql/SYS_TYPES_*.txt` - System type definitions
   - `2019VtMsSql/SYS_VARIABLES.txt` - System variables
   - `UST/Ustad/HP_*.txt` - Core company/user tables

2. **Module Core**:
   - `MPv3_vtMsSql/VT_MSSQL/MS_TABLES.txt` - Core module tables
   - `MPv3_vtMsSql/VT_MSSQL/MS_FIELDS.txt` - Field definitions
   - `UST/MsV3/MsTables.txt` - MS V3 tables

3. **Module-Specific** (as needed):
   - MTSK: `UST/Mtsk/` scripts
   - Accounting: `UST/OnMuhasebe/` scripts
   - SRC: `UST/SRC/` scripts

### Execution Order Recommendations:
1. System types and variables first
2. Core tables (HP_*, MS_*)
3. Module-specific tables
4. Views and stored procedures
5. Triggers last

## Notes

- Scripts are provided in `.txt` format
- Some scripts have both MS SQL and MySQL versions (yesiL_vtMsSql)
- Scripts in `Duzenlenen/` subdirectories are updated/edited versions
- Some scripts may have dependencies - check for foreign key references
- Always backup database before executing scripts
- Test scripts in development environment first

## Validation Status

- ✅ Scripts inventoried and categorized
- ⚠️ Full SQL syntax validation pending (requires database connection)
- ⚠️ Dependency analysis pending
- ⚠️ Execution order verification pending

## Next Steps

1. Perform basic SQL syntax validation
2. Identify and document script dependencies
3. Create execution order guide
4. Test critical scripts in test environment
5. Document any known issues or warnings

