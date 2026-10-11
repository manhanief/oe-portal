using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resto.Api.Data;
using Resto.Api.Dtos;

namespace Resto.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TablesController(RestoDbContext db) : ControllerBase
{
    /// <summary>All restaurant tables, for the dine-in table picker.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TableDto>>> GetAll(CancellationToken ct)
    {
        var tables = await db.RestaurantTables
            .AsNoTracking()
            .OrderBy(t => t.TableNo)
            .Select(t => new TableDto(t.TableId, t.TableNo, t.Seats))
            .ToListAsync(ct);

        return Ok(tables);
    }
}
