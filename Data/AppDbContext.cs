using Microsoft.EntityFrameworkCore;
using PracticaAPI.Core.Entities;

namespace PracticaAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<MonthlyBudget> MonthlyBudgets { get; set; }
    public DbSet<BudgetCategory> BudgetCategories { get; set; }
    public DbSet<Expense> Expenses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BudgetCategory>()
            .HasIndex(c => new { c.Name, MonthlyBudgetId = c.MonthlyBudget.Id })
            .IsUnique(); // prevenir nombres repetidos dentro de un mismo presupuesto
    }
}