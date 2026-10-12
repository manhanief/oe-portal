using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resto.Api.Auth;
using Resto.Api.Common;
using Resto.Api.Data;
using Resto.Api.Dtos;
using Resto.Api.Entities;

namespace Resto.Api.Controllers.Admin;

/// <summary>Admin: manage restaurant tables. (The public list is TablesController.)</summary>
[ApiController]
[Route("api/admin/tables")]
[Authorize(Policy = AuthPolicies.Admin)]
public class AdminTablesController(RestoDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TableDto>> Create(SaveTableRequest request, CancellationToken ct)
    {
        var tableNo = request.TableNo.Trim().ToUpperInvariant();
        await EnsureTableNoIsFree(tableNo, excludeId: null, ct);

        var table = new RestaurantTable { TableNo = tableNo, Seats = request.Seats };
        db.RestaurantTables.Add(table);
        await db.SaveChangesAsync(ct);

        return Ok(new TableDto(table.TableId, table.TableNo, table.Seats));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TableDto>> Update(int id, SaveTableRequest request, CancellationToken ct)
    {
        var table = await db.RestaurantTables.FindAsync([id], ct)
            ?? throw new NotFoundException($"Table {id} was not found.");

        var tableNo = request.TableNo.Trim().ToUpperInvariant();
        await EnsureTableNoIsFree(tableNo, excludeId: id, ct);

        table.TableNo = tableNo;
        table.Seats = request.Seats;
        await db.SaveChangesAsync(ct);

        return Ok(new TableDto(table.TableId, table.TableNo, table.Seats));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var table = await db.RestaurantTables.FindAsync([id], ct)
            ?? throw new NotFoundException($"Table {id} was not found.");

        if (await db.Orders.AnyAsync(o => o.TableId == id, ct))
            throw new BusinessRuleException(
                $"Table {table.TableNo} appears in past orders, so it can't be deleted.");

        db.RestaurantTables.Remove(table);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    private async Task EnsureTableNoIsFree(string tableNo, int? excludeId, CancellationToken ct)
    {
        if (await db.RestaurantTables.AnyAsync(t => t.TableNo == tableNo && t.TableId != excludeId, ct))
            throw new BusinessRuleException($"Table {tableNo} already exists.");
    }
}
