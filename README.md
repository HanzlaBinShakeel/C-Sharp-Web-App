# Finance App - Complete Financial Accounting System

A full-featured web application built in ASP.NET Core with C# for small-to-mid-size finance departments. This system focuses on pure financial accounting with rock-solid bookkeeping logic and clean, auditable data flow.

## 🚀 Features Overview

### Core Accounting Features

#### 1. General Ledger Management
- **Double-entry accounting** with real-time posting
- **Account Group Management** - Organize accounts by type (Assets, Liabilities, Equity, Income, Expenses)
- **Ledger Account Management** - Create and manage individual ledger accounts
- **Opening Balance** handling (Debit/Credit)
- **Real-time balance calculations** and tracking

#### 2. Accounts Payable & Receivable
- **Vendor Master File** - Complete vendor management with:
  - Vendor code, name, contact information
  - Address, email, phone
  - Credit limits and opening balances
  - Linked ledger accounts
  - AP Aging reports
- **Customer Master File** - Complete customer management with:
  - Customer code, name, contact information
  - Address, email, phone
  - Credit limits and opening balances
  - Linked ledger accounts
  - AR Aging reports

#### 3. Voucher Entry System
All vouchers support double-entry bookkeeping with automatic ledger posting:

- **Receipt Voucher** - Record money received
- **Payment Voucher** - Record money paid out
- **Journal Voucher** - Manual journal entries
- **Contra Voucher** - Cash/Bank transfers

Features:
- Automatic voucher numbering system (configurable prefixes)
- Financial year-wise numbering
- Real-time ledger balance updates
- Complete audit trail

#### 4. Financial Reports

1. **Balance Sheet** - Assets, Liabilities, and Equity
2. **Trial Balance** - All account balances
3. **Profit & Loss Statement** - Income and Expenditure
4. **Schedule Reports** - Detailed account group schedules
5. **Double Column Cashbook** - Cash and bank transactions
6. **Receipt and Payment Report** - All receipts and payments
7. **Income and Expenditure Report** - Income vs expenses
8. **Bank Reconciliation Statement (BRS)** - Reconcile bank accounts
9. **Ledger Reconciliation** - Reconcile individual ledgers
10. **AP Aging Report** - Outstanding vendor balances
11. **AR Aging Report** - Outstanding customer balances

All reports are exportable to:
- **PDF** (using QuestPDF)
- **Excel** (using EPPlus)

#### 5. Schedule Management
- Create schedules for detailed account groupings
- Link multiple schedules to account groups
- Generate schedule-based reports

#### 6. Financial Year Management
- Create and manage financial years
- Set active financial year
- Financial year-wise voucher numbering
- Year-end closing support

### User Management & Security

#### Authentication & Authorization
- **ASP.NET Identity** integration
- **Login/Logout** functionality
- **Protected routes** - All pages require authentication
- **User profile management**
- **Password change** functionality
- **Activity logging** - Track user actions

#### Profile Management
- View and edit user profile
- Update email and phone number
- Change password
- View account status (email confirmation, 2FA, lockout)
- Recent activity log

### Settings & Configuration

#### Application Settings
- **Financial Year Settings** - Manage and set active financial year
- **Voucher Settings** - Configure voucher numbering:
  - Custom prefixes for each voucher type
  - Starting numbers
  - Format customization
- **Report Settings** - Configure report appearance:
  - Company name and address
  - Report footer text
- **System Settings** - System information and maintenance

### Audit & Tracking

- **Complete audit log** system
- Track all create, update, delete operations
- User activity tracking
- IP address logging
- Timestamp tracking
- Old and new values tracking

## 🛠️ Technology Stack

- **Framework**: ASP.NET Core 10.0
- **Language**: C#
- **Database**: SQL Server LocalDB / SQL Server Express
- **ORM**: Entity Framework Core
- **UI**: ASP.NET MVC with Razor Pages
- **Frontend**: 
  - Bootstrap 4
  - Font Awesome icons
  - Modern gradient design
  - Responsive layout
- **Authentication**: ASP.NET Identity
- **PDF Export**: QuestPDF
- **Excel Export**: EPPlus
- **Architecture**: MVC Pattern with Service Layer

## 📁 Project Structure

```
FinanceApp/
├── Controllers/          # MVC Controllers
│   ├── AccountGroupController.cs
│   ├── LedgerController.cs
│   ├── VendorController.cs
│   ├── CustomerController.cs
│   ├── JournalController.cs
│   ├── ReceiptController.cs
│   ├── PaymentController.cs
│   ├── ContraController.cs
│   ├── BalanceSheetController.cs
│   ├── TrialBalanceController.cs
│   ├── ProfitLossController.cs
│   ├── ScheduleController.cs
│   ├── BRSController.cs
│   ├── LedgerReconciliationController.cs
│   ├── ProfileController.cs
│   ├── SettingsController.cs
│   └── ... (other report controllers)
├── Models/              # Entity Models
│   ├── AccountGroup.cs
│   ├── Ledger.cs
│   ├── Vendor.cs
│   ├── Customer.cs
│   ├── JournalEntry.cs
│   ├── JournalEntryLine.cs
│   ├── LedgerBalance.cs
│   ├── Schedule.cs
│   ├── FinancialYear.cs
│   ├── VoucherNumber.cs
│   ├── AuditLog.cs
│   └── ViewModels/
├── Services/            # Business Logic Services
│   ├── IAccountGroupService.cs / AccountGroupService.cs
│   ├── ILedgerService.cs / LedgerService.cs
│   ├── IVendorService.cs / VendorService.cs
│   ├── ICustomerService.cs / CustomerService.cs
│   ├── IVoucherService.cs / VoucherService.cs
│   ├── IReportService.cs / ReportService.cs
│   ├── IFinancialYearService.cs / FinancialYearService.cs
│   ├── IVoucherNumberService.cs / VoucherNumberService.cs
│   └── IAuditLogService.cs / AuditLogService.cs
├── Data/                # Data Access Layer
│   ├── ApplicationDbContext.cs
│   └── DbInitializer.cs
├── Views/               # Razor Views
│   ├── Home/
│   ├── AccountGroup/
│   ├── Ledger/
│   ├── Vendor/
│   ├── Customer/
│   ├── Journal/
│   ├── Receipt/
│   ├── Payment/
│   ├── Contra/
│   ├── BalanceSheet/
│   ├── TrialBalance/
│   ├── Profile/
│   ├── Settings/
│   └── Shared/
├── Areas/Identity/      # Identity Pages
│   └── Pages/Account/
│       ├── Login.cshtml
│       └── Register.cshtml
└── wwwroot/            # Static Files
    └── css/
        └── modern-style.css
```

## 🗄️ Database Schema

### Core Tables

- **AccountGroups** - Account group master (Assets, Liabilities, etc.)
- **Ledgers** - Individual ledger accounts
- **Vendors** - Vendor master data
- **Customers** - Customer master data
- **JournalEntries** - Voucher headers (Receipt, Payment, Journal, Contra)
- **JournalEntryLines** - Voucher line items (double-entry)
- **LedgerBalances** - Current balances for each ledger
- **Schedules** - Schedule definitions
- **ScheduleItems** - Schedule line items
- **FinancialYears** - Financial year master
- **VoucherNumbers** - Voucher numbering configuration
- **AuditLogs** - Audit trail
- **AspNetUsers** - User accounts (Identity)
- **AspNetRoles** - User roles (Identity)

### Key Relationships

- Ledger → AccountGroup (Many-to-One)
- JournalEntryLine → Ledger (Many-to-One)
- JournalEntryLine → JournalEntry (Many-to-One)
- Vendor → Ledger (One-to-One, optional)
- Customer → Ledger (One-to-One, optional)
- VoucherNumber → FinancialYear (Many-to-One)

## 🚀 Getting Started

### Prerequisites

- .NET 10.0 SDK
- SQL Server LocalDB or SQL Server Express
- Visual Studio 2022 or VS Code (recommended)

### Installation Steps

1. **Clone the repository** or extract the project files

2. **Configure Database Connection**

   Edit `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=FinanceApp;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
     }
   }
   ```

   For SQL Server Express:
   ```json
   "DefaultConnection": "Server=.\\SQLEXPRESS;Database=FinanceApp;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
   ```

3. **Restore Packages**
   ```bash
   dotnet restore
   ```

4. **Build the Project**
   ```bash
   dotnet build
   ```

5. **Run the Application**
   ```bash
   dotnet run
   ```

6. **Access the Application**
   - Open browser: `https://localhost:5001` or `http://localhost:5000`
   - Login page will appear automatically

### Initial Setup

On first run, the database is automatically created and seeded with:

- **Default Account Groups**: Assets, Liabilities, Equity, Revenue, Expenses
- **Sample Ledgers**: Cash, Bank, Accounts Receivable, Accounts Payable, Capital, Sales, Purchase, Salaries
- **Admin User**: 
  - Email: `admin@finance.com`
  - Password: `admin123`

## 📋 Usage Guide

### 1. Setting Up Financial Year

1. Navigate to **Settings** → **Financial Year Settings**
2. Create a new financial year
3. Set it as active

### 2. Creating Ledgers

1. Go to **Masters** → **Account Groups** → Create groups if needed
2. Go to **Masters** → **Ledgers** → Create ledger accounts
3. Assign each ledger to an appropriate group
4. Set opening balances if required

### 3. Creating Vendors/Customers

1. Go to **Masters** → **Vendors** or **Customers**
2. Create vendor/customer records
3. Link to ledger accounts (optional)
4. Set credit limits and opening balances

### 4. Entering Transactions

#### Receipt Voucher
1. Go to **Accounting** → **Receipt**
2. Select "Received From" and "Received In" ledgers
3. Enter amount and narration
4. Save - automatically creates double-entry

#### Payment Voucher
1. Go to **Accounting** → **Payment**
2. Select "Paid To" and "Paid From" ledgers
3. Enter amount and narration
4. Save - automatically creates double-entry

#### Journal Voucher
1. Go to **Accounting** → **Journal**
2. Add multiple debit and credit lines
3. Ensure total debits = total credits
4. Save - validates double-entry before posting

#### Contra Voucher
1. Go to **Accounting** → **Contra**
2. Select source and destination cash/bank accounts
3. Enter amount
4. Save - automatically creates double-entry

### 5. Generating Reports

1. Navigate to **Reports** menu
2. Select desired report (Balance Sheet, P&L, etc.)
3. Select date range and filters
4. Generate report
5. Export to PDF or Excel using action buttons

### 6. Bank Reconciliation

1. Go to **Reports** → **Bank Reconciliation**
2. Select bank ledger
3. Enter statement balance
4. Match transactions
5. Generate BRS report

## 🎨 Design Features

- **Modern gradient design** with attractive color schemes
- **Responsive layout** - works on desktop, tablet, and mobile
- **Font Awesome icons** throughout the interface
- **Smooth animations** and transitions
- **Modern card-based UI** with shadows and gradients
- **Intuitive navigation** with sidebar and top navbar
- **Active menu highlighting** - shows current page
- **User-friendly forms** with validation
- **Success/Error message alerts**

## 🔒 Security Features

- **Authentication required** for all pages
- **Secure password handling** with ASP.NET Identity
- **CSRF protection** on all forms
- **XSS protection** with input validation
- **SQL injection protection** via Entity Framework
- **Audit logging** for all critical operations
- **Session management** with configurable timeout

## 📊 Key Accounting Principles Implemented

1. **Double-Entry Bookkeeping**
   - Every transaction has equal debits and credits
   - Validated before posting
   - Automatic balance calculations

2. **Real-time Posting**
   - Ledger balances update immediately
   - No batch processing required
   - Instant reflection in reports

3. **Chart of Accounts**
   - Hierarchical account structure
   - Group-based organization
   - Flexible account creation

4. **Financial Year Management**
   - Separate data by financial year
   - Year-end closing support
   - Historical data preservation

## 🔧 Configuration

### Voucher Numbering

Configure in **Settings** → **Voucher Settings**:
- Set prefix for each voucher type (Receipt, Payment, Journal, Contra)
- Set starting number
- Format: `{Prefix}-{Number}` (e.g., RCP-000001)

### Report Configuration

Configure in **Settings** → **Report Settings**:
- Company name (appears on report headers)
- Company address
- Report footer text

## 🐛 Troubleshooting

### Database Connection Issues

**Problem**: Cannot connect to LocalDB
- **Solution**: Ensure SQL Server LocalDB is installed and running
- Check connection string in `appsettings.json`
- Verify service is running: `sqllocaldb info MSSQLLocalDB`

**Problem**: Cannot connect to SQL Server Express
- **Solution**: Ensure SQL Server Express service is running
- Check connection string uses `.\SQLEXPRESS`
- Verify Windows Authentication is enabled

### Build Errors

**Problem**: Circular reference errors
- **Solution**: Already fixed - JSON serialization configured to ignore cycles

**Problem**: Missing packages
- **Solution**: Run `dotnet restore`

## 📝 Development Notes

### Adding New Reports

1. Create controller in `Controllers/`
2. Add service method in `Services/ReportService.cs`
3. Create view in `Views/{ReportName}/`
4. Add route in navigation

### Adding New Voucher Types

1. Add voucher type to `JournalEntry.VoucherType` enum
2. Update `VoucherService` if needed
3. Create controller and views
4. Add to voucher settings

## 🎯 Acceptance Test Checklist

- ✅ Create account groups
- ✅ Create ledger accounts
- ✅ Create vendors and customers
- ✅ Enter sample vouchers (Receipt, Payment, Journal, Contra)
- ✅ Verify ledger posting (check balances)
- ✅ Generate Balance Sheet
- ✅ Generate P&L Statement
- ✅ Export report to PDF
- ✅ Export report to Excel
- ✅ Test login/logout
- ✅ Test profile management
- ✅ Test settings configuration

## 📞 Support

For issues or questions:
1. Check this README first
2. Review error messages in application logs
3. Check database connection settings
4. Verify all prerequisites are installed

## 📄 License

This project is provided as-is for financial accounting purposes.

## 🙏 Acknowledgments

- Built with ASP.NET Core
- UI components from Bootstrap
- Icons from Font Awesome
- PDF generation with QuestPDF
- Excel generation with EPPlus

---

**Version**: 1.0.0  
**Last Updated**: December 2025  
**Framework**: .NET 10.0  
**Database**: SQL Server LocalDB/Express

