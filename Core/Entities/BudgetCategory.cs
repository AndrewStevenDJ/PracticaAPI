public class BudgetCategory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public decimal Limit { get; set; }
    public List<Expense> Expenses { get; set; } = new();

    public decimal TotalSpent => Expenses.Sum(e => e.Amount);
}
