using Microsoft.EntityFrameworkCore;
using FinanceApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace FinanceApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<AccountGroup> AccountGroups { get; set; }
        public DbSet<Ledger> Ledgers { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<ScheduleItem> ScheduleItems { get; set; }
        public DbSet<JournalEntry> JournalEntries { get; set; }
        public DbSet<JournalEntryLine> JournalEntryLines { get; set; }
        public DbSet<LedgerBalance> LedgerBalances { get; set; }
        public DbSet<BankReconciliation> BankReconciliations { get; set; }
        public DbSet<BankReconciliationItem> BankReconciliationItems { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<FinancialYear> FinancialYears { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<VoucherNumber> VoucherNumbers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure AccountGroup
            modelBuilder.Entity<AccountGroup>()
                .HasIndex(g => g.GroupCode)
                .IsUnique();

            // Configure Ledger
            modelBuilder.Entity<Ledger>()
                .HasIndex(l => l.LedgerCode)
                .IsUnique();

            // Configure Schedule
            modelBuilder.Entity<Schedule>()
                .HasIndex(s => s.ScheduleCode)
                .IsUnique();

            // Configure JournalEntry
            modelBuilder.Entity<JournalEntry>()
                .HasIndex(e => e.EntryNumber)
                .IsUnique();

            // Configure Vendor
            modelBuilder.Entity<Vendor>()
                .HasIndex(v => v.VendorCode)
                .IsUnique();

            // Configure Customer
            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.CustomerCode)
                .IsUnique();

            // Configure VoucherNumber - unique on VoucherType + FinancialYearID
            modelBuilder.Entity<VoucherNumber>()
                .HasIndex(v => new { v.VoucherType, v.FinancialYearID })
                .IsUnique();

            // Configure relationships
            modelBuilder.Entity<AccountGroup>()
                .HasMany(g => g.SubGroups)
                .WithOne(g => g.ParentGroup)
                .HasForeignKey(g => g.ParentGroupID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AccountGroup>()
                .HasMany(g => g.Ledgers)
                .WithOne(l => l.Group)
                .HasForeignKey(l => l.GroupID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Schedule>()
                .HasMany(s => s.ScheduleItems)
                .WithOne(i => i.Schedule)
                .HasForeignKey(i => i.ScheduleID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<JournalEntry>()
                .HasMany(e => e.JournalEntryLines)
                .WithOne(l => l.JournalEntry)
                .HasForeignKey(l => l.EntryID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Ledger>()
                .HasMany(l => l.JournalEntryLines)
                .WithOne(j => j.Ledger)
                .HasForeignKey(j => j.LedgerID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BankReconciliation>()
                .HasMany(r => r.ReconciliationItems)
                .WithOne(i => i.BankReconciliation)
                .HasForeignKey(i => i.ReconciliationID)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Vendor relationships
            modelBuilder.Entity<Vendor>()
                .HasOne(v => v.Ledger)
                .WithMany()
                .HasForeignKey(v => v.LedgerID)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure Customer relationships
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.Ledger)
                .WithMany()
                .HasForeignKey(c => c.LedgerID)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure FinancialYear relationships
            modelBuilder.Entity<FinancialYear>()
                .HasMany(f => f.JournalEntries)
                .WithOne(j => j.FinancialYear)
                .HasForeignKey(j => j.FinancialYearID)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure JournalEntry relationships with Vendor/Customer
            modelBuilder.Entity<JournalEntry>()
                .HasOne(j => j.Vendor)
                .WithMany(v => v.PurchaseEntries)
                .HasForeignKey(j => j.VendorID)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<JournalEntry>()
                .HasOne(j => j.Customer)
                .WithMany(c => c.SalesEntries)
                .HasForeignKey(j => j.CustomerID)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}

