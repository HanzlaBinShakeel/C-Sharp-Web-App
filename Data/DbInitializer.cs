using FinanceApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            // Check if database exists and has all required tables
            bool needsRecreate = false;
            
            try
            {
                // Check if database exists
                if (await context.Database.CanConnectAsync())
                {
                    // Try to query new tables to see if they exist
                    try
                    {
                        // Try a simple count query on each new table - if it fails, table doesn't exist
                        _ = await context.Vendors.CountAsync();
                        _ = await context.Customers.CountAsync();
                        _ = await context.FinancialYears.CountAsync();
                    }
                    catch
                    {
                        // New tables don't exist - need to recreate database
                        needsRecreate = true;
                    }
                }
            }
            catch
            {
                // Database doesn't exist or can't connect - will be created below
            }

            if (needsRecreate)
            {
                // Delete existing database to recreate with new schema
                // Note: This will delete all existing data
                await context.Database.EnsureDeletedAsync();
            }

            // Ensure database is created with all tables
            await context.Database.EnsureCreatedAsync();

            // Check if data already exists
            if (context.AccountGroups.Any())
            {
                return; // Database has been seeded
            }

            // Create Admin User
            var adminUser = new IdentityUser
            {
                UserName = "admin@finance.com",
                Email = "admin@finance.com",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(adminUser, "admin123");

            // Create Account Groups
            var groups = new[]
            {
                new AccountGroup { GroupCode = "CA", GroupName = "Current Assets", GroupType = "Asset", IsActive = true },
                new AccountGroup { GroupCode = "FA", GroupName = "Fixed Assets", GroupType = "Asset", IsActive = true },
                new AccountGroup { GroupCode = "CL", GroupName = "Current Liabilities", GroupType = "Liability", IsActive = true },
                new AccountGroup { GroupCode = "LTL", GroupName = "Long-term Liabilities", GroupType = "Liability", IsActive = true },
                new AccountGroup { GroupCode = "EQ", GroupName = "Equity", GroupType = "Equity", IsActive = true },
                new AccountGroup { GroupCode = "REV", GroupName = "Revenue", GroupType = "Income", IsActive = true },
                new AccountGroup { GroupCode = "EXP", GroupName = "Expenses", GroupType = "Expense", IsActive = true }
            };

            context.AccountGroups.AddRange(groups);
            await context.SaveChangesAsync();

            // Create Sample Ledgers
            var currentAssetsGroup = groups.First(g => g.GroupCode == "CA");
            var fixedAssetsGroup = groups.First(g => g.GroupCode == "FA");
            var currentLiabilitiesGroup = groups.First(g => g.GroupCode == "CL");
            var equityGroup = groups.First(g => g.GroupCode == "EQ");
            var revenueGroup = groups.First(g => g.GroupCode == "REV");
            var expenseGroup = groups.First(g => g.GroupCode == "EXP");

            var ledgers = new[]
            {
                new Ledger { LedgerCode = "CA001", LedgerName = "Cash Account", GroupID = currentAssetsGroup.GroupID, OpeningBalance = 100000, OpeningBalanceType = "Debit", IsActive = true },
                new Ledger { LedgerCode = "CA002", LedgerName = "Bank Account", GroupID = currentAssetsGroup.GroupID, OpeningBalance = 500000, OpeningBalanceType = "Debit", IsActive = true },
                new Ledger { LedgerCode = "CA003", LedgerName = "Accounts Receivable", GroupID = currentAssetsGroup.GroupID, OpeningBalance = 0, OpeningBalanceType = "Debit", IsActive = true },
                new Ledger { LedgerCode = "FA001", LedgerName = "Fixed Assets", GroupID = fixedAssetsGroup.GroupID, OpeningBalance = 1000000, OpeningBalanceType = "Debit", IsActive = true },
                new Ledger { LedgerCode = "CL001", LedgerName = "Accounts Payable", GroupID = currentLiabilitiesGroup.GroupID, OpeningBalance = 0, OpeningBalanceType = "Credit", IsActive = true },
                new Ledger { LedgerCode = "EQ001", LedgerName = "Capital", GroupID = equityGroup.GroupID, OpeningBalance = 1500000, OpeningBalanceType = "Credit", IsActive = true },
                new Ledger { LedgerCode = "REV001", LedgerName = "Sales", GroupID = revenueGroup.GroupID, OpeningBalance = 0, OpeningBalanceType = "Credit", IsActive = true },
                new Ledger { LedgerCode = "EXP001", LedgerName = "Purchase", GroupID = expenseGroup.GroupID, OpeningBalance = 0, OpeningBalanceType = "Debit", IsActive = true },
                new Ledger { LedgerCode = "EXP002", LedgerName = "Salaries", GroupID = expenseGroup.GroupID, OpeningBalance = 0, OpeningBalanceType = "Debit", IsActive = true }
            };

            context.Ledgers.AddRange(ledgers);
            await context.SaveChangesAsync();

            // Initialize Ledger Balances
            foreach (var ledger in ledgers)
            {
                var balance = new LedgerBalance
                {
                    LedgerID = ledger.LedgerID,
                    BalanceDate = DateTime.Now.Date,
                    DebitBalance = ledger.OpeningBalanceType == "Debit" ? ledger.OpeningBalance : 0,
                    CreditBalance = ledger.OpeningBalanceType == "Credit" ? ledger.OpeningBalance : 0,
                    NetBalance = ledger.OpeningBalanceType == "Debit" ? ledger.OpeningBalance : -ledger.OpeningBalance,
                    BalanceType = ledger.OpeningBalanceType
                };
                context.LedgerBalances.Add(balance);
            }

            await context.SaveChangesAsync();
        }
    }
}

