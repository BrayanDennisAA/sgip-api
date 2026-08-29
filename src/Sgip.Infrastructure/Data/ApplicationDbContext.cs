using Microsoft.EntityFrameworkCore;
using Sgip.Domain.Entities;

namespace Sgip.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<PaymentSchedule> PaymentSchedules => Set<PaymentSchedule>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- Loan ---
        modelBuilder.Entity<Loan>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.UserId).IsRequired().HasMaxLength(100);
            entity.Property(l => l.Amount).HasColumnType("decimal(18,2)");
            entity.Property(l => l.InterestRate).HasColumnType("decimal(9,6)");
            entity.Property(l => l.MonthlyPayment).HasColumnType("decimal(18,2)");
            entity.Property(l => l.LoanType).HasConversion<string>().HasMaxLength(20);
            entity.Property(l => l.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(l => l.CreatedAt).HasColumnType("timestamp with time zone");
            entity.Property(l => l.UpdatedAt).HasColumnType("timestamp with time zone");
            entity.HasIndex(l => l.UserId);

            entity.HasMany(l => l.PaymentSchedules)
                  .WithOne(p => p.Loan!)
                  .HasForeignKey(p => p.LoanId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(l => l.Transactions)
                  .WithOne(t => t.Loan)
                  .HasForeignKey(t => t.LoanId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // --- PaymentSchedule ---
        modelBuilder.Entity<PaymentSchedule>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.TotalPayment).HasColumnType("decimal(18,2)");
            entity.Property(p => p.Principal).HasColumnType("decimal(18,2)");
            entity.Property(p => p.Interest).HasColumnType("decimal(18,2)");
            entity.Property(p => p.RemainingBalance).HasColumnType("decimal(18,2)");
            entity.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(p => p.DueDate).HasColumnType("timestamp with time zone");

            entity.HasIndex(p => new { p.LoanId, p.PaymentNumber }).IsUnique();
        });

        // --- Transaction ---
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.IdempotencyKey).IsRequired().HasMaxLength(200);
            entity.Property(t => t.Amount).HasColumnType("decimal(18,2)");
            entity.Property(t => t.Type).HasConversion<string>().HasMaxLength(20);
            entity.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(t => t.Description).HasMaxLength(500);
            entity.Property(t => t.CreatedAt).HasColumnType("timestamp with time zone");

            entity.HasIndex(t => t.IdempotencyKey).IsUnique();
        });
    }

}