public interface IBudgetService
{
    Task<MonthlyBudget> CreateBudgetAsync(CreateBudgetDto dto);
    Task<Expense> RegisterExpenseAsync(Guid budgetId, CreateExpenseDto dto);
    // Más métodos para las categorías (CRUD)
}
