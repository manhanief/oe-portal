using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resto.Api.Auth;
using Resto.Api.Data;
using Resto.Api.Dtos;
using Resto.Api.Entities;

namespace Resto.Api.Controllers.Admin;

/// <summary>Admin: simple sales summary for one day.</summary>
[ApiController]
[Route("api/admin/reports")]
[Authorize(Policy = AuthPolicies.Admin)]
public class ReportsController(RestoDbContext db) : ControllerBase
{
    /// <summary>
    /// Sales for a day (default: today). Cancelled orders are counted separately
    /// and are not included in revenue.
    /// Example: /api/admin/reports/daily?date=2026-10-12
    /// </summary>
    [HttpGet("daily")]
    public async Task<ActionResult<DailySalesDto>> Daily([FromQuery] DateOnly? date, CancellationToken ct)
    {
        var day = date ?? DateOnly.FromDateTime(DateTime.Now);
        var start = day.ToDateTime(TimeOnly.MinValue);
        var end = start.AddDays(1);

        var orders = await db.Orders
            .AsNoTracking()
            .Where(o => o.CreatedAt >= start && o.CreatedAt < end)
            .Select(o => new { o.Status, o.Total })
            .ToListAsync(ct);

        var paid = orders.Where(o => o.Status != OrderStatus.Cancelled).ToList();
        var revenue = paid.Sum(o => o.Total);

        // A day's order lines are a small list, so we group them in C#
        var lines = await db.OrderItems
            .AsNoTracking()
            .Where(i => i.Order.CreatedAt >= start
                        && i.Order.CreatedAt < end
                        && i.Order.Status != OrderStatus.Cancelled)
            .Select(i => new { i.MenuItem.Name, i.Quantity, i.UnitPrice })
            .ToListAsync(ct);

        var topItems = lines
            .GroupBy(l => l.Name)
            .Select(g => new TopItemDto(g.Key, g.Sum(l => l.Quantity), g.Sum(l => l.UnitPrice * l.Quantity)))
            .OrderByDescending(t => t.Quantity)
            .ThenByDescending(t => t.Revenue)
            .Take(5)
            .ToList();

        return Ok(new DailySalesDto(
            day,
            paid.Count,
            orders.Count - paid.Count,
            revenue,
            paid.Count == 0 ? 0 : Math.Round(revenue / paid.Count, 2),
            topItems));
    }
}
