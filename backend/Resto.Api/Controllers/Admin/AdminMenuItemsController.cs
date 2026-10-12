using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resto.Api.Auth;
using Resto.Api.Common;
using Resto.Api.Data;
using Resto.Api.Dtos;
using Resto.Api.Entities;

namespace Resto.Api.Controllers.Admin;

/// <summary>Admin: manage dishes (name, price, category, photo URL, sold out).</summary>
[ApiController]
[Route("api/admin/menu-items")]
[Authorize(Policy = AuthPolicies.Admin)]
public class AdminMenuItemsController(RestoDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminMenuItemDto>>> GetAll(CancellationToken ct)
    {
        var items = await db.MenuItems
            .AsNoTracking()
            .Include(m => m.Category)
            .OrderBy(m => m.Category.SortOrder).ThenBy(m => m.Name)
            .ToListAsync(ct);

        return Ok(items.Select(ToDto).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<AdminMenuItemDto>> Create(SaveMenuItemRequest request, CancellationToken ct)
    {
        var item = new MenuItem();
        await ApplyAsync(item, request, excludeId: null, ct);

        db.MenuItems.Add(item);
        await db.SaveChangesAsync(ct);

        return Ok(await GetDtoAsync(item.ItemId, ct));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AdminMenuItemDto>> Update(int id, SaveMenuItemRequest request, CancellationToken ct)
    {
        var item = await db.MenuItems.FindAsync([id], ct)
            ?? throw new NotFoundException($"Dish {id} was not found.");

        await ApplyAsync(item, request, excludeId: id, ct);
        await db.SaveChangesAsync(ct);

        return Ok(await GetDtoAsync(id, ct));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var item = await db.MenuItems.FindAsync([id], ct)
            ?? throw new NotFoundException($"Dish {id} was not found.");

        // Old orders point to this dish (foreign key), so it must stay for the receipts
        if (await db.OrderItems.AnyAsync(oi => oi.ItemId == id, ct))
            throw new BusinessRuleException(
                $"'{item.Name}' appears in past orders, so it can't be deleted. Mark it as sold out instead.");

        db.MenuItems.Remove(item);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    // ---------------------------------------------------------------

    private async Task ApplyAsync(MenuItem item, SaveMenuItemRequest request, int? excludeId, CancellationToken ct)
    {
        if (!await db.Categories.AnyAsync(c => c.CategoryId == request.CategoryId, ct))
            throw new BusinessRuleException($"Category {request.CategoryId} does not exist.");

        var name = request.Name.Trim();
        var upper = name.ToUpper();
        if (await db.MenuItems.AnyAsync(m => m.Name.ToUpper() == upper && m.ItemId != excludeId, ct))
            throw new BusinessRuleException($"A dish named '{name}' already exists.");

        item.CategoryId = request.CategoryId;
        item.Name = name;
        item.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        item.Price = request.Price;
        item.ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim();
        item.IsAvailable = request.IsAvailable;
    }

    private async Task<AdminMenuItemDto> GetDtoAsync(int id, CancellationToken ct)
    {
        var item = await db.MenuItems.AsNoTracking().Include(m => m.Category).FirstAsync(m => m.ItemId == id, ct);
        return ToDto(item);
    }

    private static AdminMenuItemDto ToDto(MenuItem m) => new(
        m.ItemId, m.CategoryId, m.Category.Name, m.Name, m.Description, m.Price, m.ImageUrl, m.IsAvailable);
}
