using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Resto.Api.Data;

/// <summary>
/// Saves an enum as UPPER_SNAKE text (OrderStatus.Pending -> 'PENDING',
/// OrderType.DineIn -> 'DINE_IN') to match the CHECK constraints in Oracle,
/// and reads it back into the enum.
/// </summary>
public class UpperSnakeEnumConverter<TEnum> : ValueConverter<TEnum, string>
    where TEnum : struct, Enum
{
    public UpperSnakeEnumConverter()
        : base(
            v => NamingHelper.ToUpperSnake(v.ToString()),
            v => Enum.Parse<TEnum>(v.Replace("_", ""), true))
    {
    }
}
