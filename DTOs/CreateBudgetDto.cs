public class CreateBudgetDto
{
    public DateTime Month { get; set; }
    public List<BudgetCategoryDto> Categories { get; set; } = new();
}
