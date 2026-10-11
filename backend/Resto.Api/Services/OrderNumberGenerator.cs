using Microsoft.EntityFrameworkCore;
using Resto.Api.Data;

namespace Resto.Api.Services;

/// <summary>
/// Creates short daily order numbers: A001, A002, ... restarting each day.
/// "Today" is decided by the database clock (SYSDATE), so the API and DB always agree.
/// </summary>
public class OrderNumberGenerator(RestoDbContext db)
{
    public async Task<string> NextAsync(CancellationToken ct)
    {
        var next = await db.Database
            .SqlQuery<int>($"""
                SELECT NVL(MAX(TO_NUMBER(SUBSTR(order_no, 2))), 0) + 1 AS "Value"
                FROM orders
                WHERE created_at >= TRUNC(SYSDATE)
                """)
            .SingleAsync(ct);

        return $"A{next:D3}";
    }
}
