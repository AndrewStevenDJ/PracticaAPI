[ApiController]
[Route("api/[controller]")]
public class BudgetsController : ControllerBase
{
    private readonly IBudgetService _service;

    public BudgetsController(IBudgetService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> CreateBudget([FromBody] CreateBudgetDto dto)
    {
        var result = await _service.CreateBudgetAsync(dto);
        return CreatedAtAction(nameof(GetBudget), new { id = result.Id }, result);
    }

    [HttpPost("{budgetId}/expenses")]
    public async Task<IActionResult> AddExpense(Guid budgetId, [FromBody] CreateExpenseDto dto)
    {
        try
        {
            var result = await _service.RegisterExpenseAsync(budgetId, dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET para obtener presupuesto si lo necesitas
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBudget(Guid id)
    {
        // opcional
        return Ok();
    }
}
