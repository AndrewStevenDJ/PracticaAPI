public class MonthlyBudget
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Month { get; set; }
    public List<BudgetCategory> Categories { get; set; } = new();
}
