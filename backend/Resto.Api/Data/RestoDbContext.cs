using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Resto.Api.Entities;

namespace Resto.Api.Data;

public class RestoDbContext(DbContextOptions<RestoDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<RestaurantTable> RestaurantTables => Set<RestaurantTable>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // The tables already exist (created by db/01_schema.sql).
        // This is "database-first": we only describe them to EF Core,
        // we never let EF Core create or change them.

        b.Entity<Category>(e =>
        {
            e.ToTable("CATEGORIES");
            e.HasKey(x => x.CategoryId);
            e.Property(x => x.Name).HasMaxLength(50);
        });

        b.Entity<MenuItem>(e =>
        {
            e.ToTable("MENU_ITEMS");
            e.HasKey(x => x.ItemId);
            e.Property(x => x.Name).HasMaxLength(100);
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.Price).HasPrecision(8, 2);
            e.Property(x => x.ImageUrl).HasMaxLength(500);

            e.HasOne(x => x.Category)
             .WithMany(c => c.MenuItems)
             .HasForeignKey(x => x.CategoryId);
        });

        b.Entity<RestaurantTable>(e =>
        {
            e.ToTable("RESTAURANT_TABLES");
            e.HasKey(x => x.TableId);
            e.Property(x => x.TableNo).HasMaxLength(10);
        });

        b.Entity<Order>(e =>
        {
            e.ToTable("ORDERS");
            e.HasKey(x => x.OrderId);
            e.Property(x => x.OrderNo).HasMaxLength(20);
            e.Property(x => x.CustomerName).HasMaxLength(100);
            e.Property(x => x.Subtotal).HasPrecision(10, 2);
            e.Property(x => x.Tax).HasPrecision(10, 2);
            e.Property(x => x.Total).HasPrecision(10, 2);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("SYSTIMESTAMP");

            e.HasOne(x => x.Table)
             .WithMany(t => t.Orders)
             .HasForeignKey(x => x.TableId);
        });

        b.Entity<OrderItem>(e =>
        {
            e.ToTable("ORDER_ITEMS");
            e.HasKey(x => x.OrderItemId);
            e.Property(x => x.UnitPrice).HasPrecision(8, 2);
            e.Property(x => x.Notes).HasMaxLength(200);

            e.HasOne(x => x.Order)
             .WithMany(o => o.Items)
             .HasForeignKey(x => x.OrderId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.MenuItem)
             .WithMany()
             .HasForeignKey(x => x.ItemId);
        });

        b.Entity<User>(e =>
        {
            e.ToTable("USERS");
            e.HasKey(x => x.UserId);
            e.Property(x => x.Username).HasMaxLength(50);
            e.Property(x => x.PasswordHash).HasMaxLength(200);
        });

        ApplyOracleConventions(b);
    }

    /// <summary>
    /// Rules applied to every property so we don't repeat them per column:
    /// 1. Column names: PascalCase -> UPPER_SNAKE (CategoryId -> CATEGORY_ID)
    /// 2. bool -> NUMBER(1) holding 0 or 1
    /// 3. enum -> UPPER_SNAKE text ('PENDING', 'DINE_IN', ...)
    /// </summary>
    private static void ApplyOracleConventions(ModelBuilder b)
    {
        foreach (var entity in b.Model.GetEntityTypes())
        {
            foreach (var prop in entity.GetProperties())
            {
                prop.SetColumnName(NamingHelper.ToUpperSnake(prop.Name));

                var type = Nullable.GetUnderlyingType(prop.ClrType) ?? prop.ClrType;

                if (type == typeof(bool))
                {
                    prop.SetValueConverter(new BoolToZeroOneConverter<int>());
                    prop.SetColumnType("NUMBER(1)");
                }
                else if (type.IsEnum)
                {
                    var converterType = typeof(UpperSnakeEnumConverter<>).MakeGenericType(type);
                    prop.SetValueConverter((ValueConverter)Activator.CreateInstance(converterType)!);
                }
            }
        }
    }
}
