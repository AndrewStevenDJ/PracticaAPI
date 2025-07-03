using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PracticaAPI.Data;
using PracticaAPI.Core.Entities;
using Microsoft.AspNetCore.Authorization;

namespace PracticaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExpensesController : ControllerBase
{
    private readonly AppDbContext _context;
    public ExpensesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Expenses?categoryId=xxx&page=1&pageSize=10
    [HttpGet]
    public async Task<IActionResult> GetExpenses([FromQuery] Guid categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        var query = _context.Expenses.Where(e => e.CategoryId == categoryId);
        var total = await query.CountAsync();
        var expenses = await query
            .OrderByDescending(e => e.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return Ok(new { total, page, pageSize, expenses });
    }
}
