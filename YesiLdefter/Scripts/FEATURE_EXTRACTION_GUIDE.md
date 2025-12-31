# Feature Extraction Guide - Quick Start

## Purpose

This guide helps quickly extract feature names from database scripts to create a feature matrix for tomorrow's presentation.

## Quick Extraction Method

### Step 1: Parse Table Names → Features

**Pattern**: Table name prefixes indicate feature categories

#### MTSK Module (`UST/Mtsk/`):
- `MtskAday*.txt` → **Candidate Management**
  - `MtskAday.txt` - Main candidate table
  - `MtskAdayAdres.txt` - Candidate addresses
  - `MtskAdayBelgeler.txt` - Candidate documents
  - `MtskAdaySertifika.txt` - Candidate certificates
  - `MtskAdayTakip.txt` - Candidate tracking
  - `MtskAdayTalep.txt` - Candidate requests
  - `MtskAdayUcret.txt` - Candidate fees

- `MtskDers*.txt` → **Lesson Management**
  - `MtskTeorikDers.txt` - Theoretical lessons
  - `MtskUygulamaliDers.txt` - Practical lessons
  - `MtskDerslik.txt` - Classrooms

- `MtskSinav*.txt` → **Exam Management**
  - `MtskSinavETeorik.txt` - Theoretical exams
  - `MtskSinavUygulama.txt` - Practical exams
  - `MtskSinavRandevu.txt` - Exam appointments
  - `MtskSinavTarihi.txt` - Exam dates

- `MtskSertifika*.txt` → **Certificate Management**
  - `MtskSertifikaSaatleri.txt` - Certificate hours
  - `MtskSertifikaSeriNo.txt` - Certificate serial numbers

- `MtskUcret*.txt` → **Fee Management**
  - `MtskUcretTeorik.txt` - Theoretical fees
  - `MtskUcretUygulama.txt` - Practical fees
  - `MtskUcretSinav.txt` - Exam fees

- `MtskSablon*.txt` → **Template Management**
  - `MtskSablonTeorik*.txt` - Theoretical templates
  - `MtskSablonUygulama*.txt` - Practical templates
  - `MtskSablonSMS.txt` - SMS templates
  - (!NEW!) `MtskSablonWhatsApp.txt` = WhatsApp templates ( needs to be done )

- `prc_Mtsk*.txt` → **Business Processes**
  - `prc_MtskAdayBorclandir.txt` - Candidate billing
  - `prc_MtskAdayTakip.txt` - Candidate tracking
  - `prc_MtskTeorikDersPlaniOlustur.txt` - Lesson plan creation
  - `prc_MtskUygulamaDersPlaniOlustur.txt` - Practical lesson plan creation

#### Accounting Module (`UST/OnMuhasebe/`):
- `OnmHesap*.txt` → **Account Management**
  - `OnmHesapCari.txt` - Customer accounts
  - `OnmHesapFinans.txt` - Financial accounts
  - `OnmHesapStok.txt` - Inventory accounts
  - `OnmHesapVezne.txt` - Cash register accounts

- `OnmBelge*.txt` → **Document Management**
  - `OnmBelgeDekont.txt` - Voucher documents
  - `OnmBelgeMaliyet.txt` - Cost documents
  - `OnmBelgeStok*.txt` - Inventory documents

- `OnmCari*.txt` → **Customer Management**
  - `OnmCariAdres.txt` - Customer addresses
  - `CariHesap*.txt` - Customer accounts
  - `CariIO.txt` - Customer I/O

- `OnmOdemePlani.txt` → **Payment Plan Management**

- `OnmMaas*.txt` → **Payroll Management**
  - `OnmMaasBilgisi.txt` - Salary information
  - `OnmMaasHesaplari.txt` - Salary calculations
  - `OnmMaasEkGelirler.txt` - Additional income
  - `OnmMaasEkKesintiler.txt` - Additional deductions

- `GIB_*.txt` → **Tax Authority Integration**
  - `GIB_FaturaCesitleri.txt` - Invoice types
  - `GIB_FaturaEkVergiler.txt` - Additional taxes
  - `GIB_ParaTipleri.txt` - Currency types
  - `GIB_Sehirler.txt` - Cities
  - `GIB_TevkifatMaddeleri.txt` - Withholding items
  - `GIB_Ulke.txt` - Countries

#### SRC Module (`UST/SRC/`):
- `SrcAday*.txt` → **SRC Candidate Management**
- `SrcSinav*.txt` → **SRC Exam Management**
- `SrcTeorikDers*.txt` → **SRC Theoretical Lessons**
- `SrcUcret*.txt` → **SRC Fee Management**

#### MS V3 Module (`UST/MsV3/`):
- `MsTables.txt` → **Dynamic Table System**rty
- `MsProjectTables.txt` → **Project-Specific Tables**
- `MsReports.txt` → **Report System**
- `MsWebNodes.txt`, `MsWebPages.txt` → **Web Structure Management**

### Step 2: Parse Procedure Names → Workflows

**Pattern**: `prc_` prefix indicates business processes

#### Key Procedures to Document:
- `prc_MtskAdayBorclandir*.txt` - Billing workflows
- `prc_MtskAdayTakip*.txt` - Tracking workflows
- `prc_MtskTeorikDersPlaniOlustur*.txt` - Planning workflows
- `prc_OnmOdemePlaniOlustur.txt` - Payment plan creation
- `prc_OnmMaasHesaplari.txt` - Payroll calculation
- `prc_OnmBelgeDekont*.txt` - Document processing

### Step 3: Create Feature Matrix

**Template Structure**:

```markdown
| Feature Name | Module | Category | Status | Script Files | Description | Demo Ready |
|--------------|--------|----------|--------|--------------|-------------|------------|
| Candidate Management | MTSK | Core | Implemented | MtskAday*.txt (15 files) | Manage driving school candidates | Yes |
| Lesson Planning | MTSK | Core | Implemented | MtskTeorikDers.txt, prc_MtskTeorikDersPlaniOlustur.txt | Create and manage lesson plans | Yes |
| Exam Management | MTSK | Core | Implemented | MtskSinav*.txt (8 files) | Schedule and manage exams | Yes |
| Certificate Management | MTSK | Core | Implemented | MtskSertifika*.txt (3 files) | Issue and track certificates | Partial |
| Fee Management | MTSK | Financial | Implemented | MtskUcret*.txt (4 files) | Calculate and track fees | Yes |
| Customer Management | Accounting | Core | Implemented | OnmCari*.txt, CariHesap*.txt | Manage customers | Yes |
| Financial Accounts | Accounting | Financial | Implemented | OnmHesapFinans.txt | Manage financial accounts | Yes |
| Document Management | Accounting | Core | Implemented | OnmBelge*.txt (5 files) | Create and manage documents | Yes |
| Payment Plans | Accounting | Financial | Implemented | OnmOdemePlani.txt | Create payment plans | Yes |
| Payroll | Accounting | HR | Implemented | OnmMaas*.txt (5 files) | Calculate salaries | Yes |
| Tax Integration | Accounting | Integration | Implemented | GIB_*.txt (7 files) | GIB tax authority integration | Partial |
| Dynamic Tables | MS V3 | System | Implemented | MsTables.txt, MsTablesIP.txt | Create tables dynamically | Yes |
| Report System | MS V3 | System | Implemented | MsReports.txt | Generate reports | Yes |
| Web Structure | MS V3 | System | Implemented | MsWebNodes.txt, MsWebPages.txt | Manage web structure | Yes |
```

### Step 4: Categorize by Presentation Value

**High Value (Show in Demo)**:
- Candidate Management (MTSK)
- Lesson Planning (MTSK)
- Exam Management (MTSK)
- Customer Management (Accounting)
- Document Management (Accounting)
- Dynamic Tables (MS V3)

**Medium Value (Mention)**:
- Certificate Management
- Fee Management
- Payment Plans
- Payroll

**Low Value (List Only)**:
- Lookup tables (Lkp_*)
- System tables (SYS_*)
- Helper procedures

## Automation Script (Optional)

Create a PowerShell script to auto-extract:

```powershell
# Extract table names from scripts
Get-ChildItem -Path "YesiLdefter\Scripts\UST\Mtsk\*.txt" | 
    ForEach-Object { 
        $content = Get-Content $_.FullName -Raw
        if ($content -match "create table (\w+)") {
            "$($_.Name) -> $($matches[1])"
        }
    }
```

## Quick Feature List for Presentation

### MTSK Module (Driving School):
1. ✅ Candidate Registration & Management
2. ✅ Document Management (ID, Health, etc.)
3. ✅ Lesson Planning (Theoretical & Practical)
4. ✅ Exam Scheduling & Management
5. ✅ Certificate Issuance
6. ✅ Fee Calculation & Billing
7. ✅ Progress Tracking

### Accounting Module:
1. ✅ Customer/Account Management
2. ✅ Financial Account Management
3. ✅ Document Processing (Invoices, Vouchers)
4. ✅ Payment Plan Management
5. ✅ Payroll Calculation
6. ✅ Tax Authority Integration (GIB)

### System Features:
1. ✅ Dynamic Table Creation
2. ✅ Report Generation
3. ✅ Web Structure Management
4. ✅ Multi-database Support (MS SQL, MySQL)

## Next Steps

1. Run through MTSK scripts and extract all table names
2. Run through Accounting scripts and extract all table names
3. Create feature matrix with descriptions
4. Identify which features have UI (can demo) vs. backend only (can promise)
5. Prepare demo flow for presentation

