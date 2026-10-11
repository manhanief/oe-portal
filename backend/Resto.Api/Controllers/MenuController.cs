using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resto.Api.Data;
using Resto.Api.Dtos;

namespace Resto.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuController(RestoDbContext db) : ControllerBase
{
    /// <summary>
    /// Customer menu: active categories with their items.
    /// Unavailable items are included with isAvailable = false, so the UI can show "Sold out".
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MenuCategoryDto>>> Get(CancellationToken ct)
    {
        var menu = await db.Categories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .Select(c => new MenuCategoryDto(
                c.CategoryId,
                c.Name,
                c.MenuItems
                    .OrderBy(i => i.Name)
                    .Select(i => new MenuItemDto(
                        i.ItemId,
                        i.Name,
                        i.Description,
                        i.Price,
                        i.ImageUrl,
                        i.IsAvailable))
                    .ToList()))
            .ToListAsync(ct);

        return Ok(menu);
    }
}
