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
        // The menu is small (a few categories, a few dozen items), so we load it
        // in one simple query and do the filtering and shaping in C#.
        var categories = await db.Categories
            .AsNoTracking()
            .Include(c => c.MenuItems)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(ct);

        var menu = categories
            .Where(c => c.IsActive)
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
            .ToList();

        return Ok(menu);
    }
}
