using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PracticaAPI.Core.Entities;
using PracticaAPI.Data;
using PracticaAPI.DTOs;

namespace PracticaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoriesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Categories?budgetId=xxx&page=1&pageSize=10
    [HttpGet]
    public async Task<IActionResult> GetCategories([FromQuery] Guid budgetId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        var query = _context.BudgetCategories
            .Where(c => c.MonthlyBudgetId == budgetId);
        var total = await query.CountAsync();
        var categories = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new BudgetCategoryDto { Name = c.Name, Limit = c.Limit })
            .ToListAsync();
        return Ok(new { total, page, pageSize, categories });
    }

    // GET: api/Categories/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<BudgetCategoryDto>> GetCategory(Guid id)
    {
        var category = await _context.BudgetCategories
            .Include(c => c.Expenses)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return NotFound("Categoría no encontrada");
        }

        var categoryDto = new BudgetCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Limit = category.Limit,
            MonthlyBudgetId = category.MonthlyBudgetId,
            TotalSpent = category.Expenses.Sum(e => e.Amount)
        };

        return Ok(categoryDto);
    }

    // POST: api/Categories
    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        
        // Verificar que el presupuesto existe y pertenece al usuario
        var budget = await _context.MonthlyBudgets
            .Where(b => b.Id == dto.MonthlyBudgetId && b.UserId == userId)
            .FirstOrDefaultAsync();
            
        if (budget == null)
            return BadRequest("El presupuesto especificado no existe o no tienes permisos para acceder a él.");

        // Verificar que no existe una categoría con el mismo nombre en este presupuesto
        var exists = await _context.BudgetCategories
            .AnyAsync(c => c.MonthlyBudgetId == dto.MonthlyBudgetId && c.Name.ToLower() == dto.Name.ToLower());
            
        if (exists)
            return BadRequest("Ya existe una categoría con ese nombre en este presupuesto.");

        // Validar que el límite sea positivo
        if (dto.Limit <= 0)
            return BadRequest("El límite de la categoría debe ser mayor a 0.");

        var category = new BudgetCategory
        {
            Name = dto.Name,
            Limit = dto.Limit,
            MonthlyBudgetId = dto.MonthlyBudgetId
        };
        
        _context.BudgetCategories.Add(category);
        await _context.SaveChangesAsync();
        
        // Retornar la categoría creada con información completa
        var categoryDto = new BudgetCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Limit = category.Limit,
            MonthlyBudgetId = category.MonthlyBudgetId,
            TotalSpent = 0 // Nueva categoría, sin gastos
        };
        
        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, categoryDto);
    }

    // POST: api/Categories/comida
    [HttpPost("comida")]
    public async Task<IActionResult> CreateComidaCategory([FromBody] CreateComidaCategoryDto dto)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        
        // Verificar que el presupuesto existe y pertenece al usuario
        var budget = await _context.MonthlyBudgets
            .Where(b => b.Id == dto.MonthlyBudgetId && b.UserId == userId)
            .FirstOrDefaultAsync();
            
        if (budget == null)
            return BadRequest("El presupuesto especificado no existe o no tienes permisos para acceder a él.");

        // Verificar que no existe una categoría "Comida" en este presupuesto
        var exists = await _context.BudgetCategories
            .AnyAsync(c => c.MonthlyBudgetId == dto.MonthlyBudgetId && c.Name.ToLower() == "comida");
            
        if (exists)
            return BadRequest("Ya existe una categoría 'Comida' en este presupuesto.");

        // Validar que el límite sea positivo
        if (dto.Limit <= 0)
            return BadRequest("El límite de la categoría debe ser mayor a 0.");

        var category = new BudgetCategory
        {
            Name = "Comida",
            Limit = dto.Limit,
            MonthlyBudgetId = dto.MonthlyBudgetId
        };
        
        _context.BudgetCategories.Add(category);
        await _context.SaveChangesAsync();
        
        // Retornar la categoría creada con información completa
        var categoryDto = new BudgetCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Limit = category.Limit,
            MonthlyBudgetId = category.MonthlyBudgetId,
            TotalSpent = 0
        };
        
        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, categoryDto);
    }

    // PUT: api/Categories/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryDto dto)
    {
        var category = await _context.BudgetCategories.FindAsync(id);
        if (category == null)
            return NotFound();

        // Validar nombre único
        var exists = await _context.BudgetCategories.AnyAsync(c => c.MonthlyBudgetId == category.MonthlyBudgetId && c.Name.ToLower() == dto.Name.ToLower() && c.Id != id);
        if (exists)
            return BadRequest("Ya existe una categoría con ese nombre en este presupuesto.");

        category.Name = dto.Name;
        category.Limit = dto.Limit;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/Categories/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var category = await _context.BudgetCategories.Include(c => c.Expenses).FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
            return NotFound();
        if (category.Expenses.Any())
            return BadRequest("No se puede eliminar una categoría que tiene gastos registrados.");

        _context.BudgetCategories.Remove(category);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
