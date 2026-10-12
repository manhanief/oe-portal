using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resto.Api.Auth;
using Resto.Api.Common;
using Resto.Api.Data;
using Resto.Api.Dtos;
using Resto.Api.Entities;

namespace Resto.Api.Controllers.Admin;

/// <summary>
/// Admin: manage menu categories.
/// Simple create/read/update/delete, so the controller talks to the DbContext directly;
/// business-heavy logic (ordering) lives in services instead.
/// </summary>
[ApiController]
[Route("api/admin/categories")]
[Authorize(Policy = AuthPolicies.Admin)]
public class AdminCategoriesController(RestoDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetAll(CancellationToken ct) =>
        Ok(await db.Categories
            .AsNoTracking()
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            .Select(c => new CategoryDto(c.CategoryId, c.Name, c.SortOrder, c.IsActive, c.MenuItems.Count))
            .ToListAsync(ct));

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(SaveCategoryRequest request, CancellationToken ct)
    {
        var name = request.Name.Trim();
        await EnsureNameIsFree(name, excludeId: null, ct);

        var category = new Category { Name = name, SortOrder = request.SortOrder, IsActive = request.IsActive };
        db.Categories.Add(category);
        await db.SaveChangesAsync(ct);

        return Ok(new CategoryDto(category.CategoryId, category.Name, category.SortOrder, category.IsActive, 0));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoryDto>> Update(int id, SaveCategoryRequest request, CancellationToken ct)
    {
        var category = await db.Categories.FindAsync([id], ct)
            ?? throw new NotFoundException($"Category {id} was not found.");

        var name = request.Name.Trim();
        await EnsureNameIsFree(name, excludeId: id, ct);

        category.Name = name;
        category.SortOrder = request.SortOrder;
        category.IsActive = request.IsActive;
        await db.SaveChangesAsync(ct);

        var itemCount = await db.MenuItems.CountAsync(m => m.CategoryId == id, ct);
        return Ok(new CategoryDto(category.CategoryId, category.Name, category.SortOrder, category.IsActive, itemCount));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var category = await db.Categories.FindAsync([id], ct)
            ?? throw new NotFoundException($"Category {id} was not found.");

        if (await db.MenuItems.AnyAsync(m => m.CategoryId == id, ct))
            throw new BusinessRuleException(
                $"'{category.Name}' still has dishes. Move or delete them first, or hide the category instead.");

        db.Categories.Remove(category);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    private async Task EnsureNameIsFree(string name, int? excludeId, CancellationToken ct)
    {
        var upper = name.ToUpper();
        var taken = await db.Categories.AnyAsync(
            c => c.Name.ToUpper() == upper && c.CategoryId != excludeId, ct);

        if (taken)
            throw new BusinessRuleException($"A category named '{name}' already exists.");
    }
}
