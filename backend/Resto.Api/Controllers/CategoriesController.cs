using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resto.Api.Data;
using Resto.Api.Dtos;

namespace Resto.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController(RestoDbContext db) : ControllerBase
{
    /// <summary>List all categories with how many menu items each has.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll(CancellationToken ct)
    {
        var categories = await db.Categories
            .AsNoTracking()
            .OrderBy(c => c.SortOrder)
            .Select(c => new CategoryDto(
                c.CategoryId,
                c.Name,
                c.SortOrder,
                c.IsActive,
                c.MenuItems.Count))
            .ToListAsync(ct);

        return Ok(categories);
    }
}
