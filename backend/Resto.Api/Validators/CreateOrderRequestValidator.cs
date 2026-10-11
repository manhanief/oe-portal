using FluentValidation;
using Resto.Api.Dtos;
using Resto.Api.Entities;

namespace Resto.Api.Validators;

/// <summary>
/// Checks the SHAPE of the request (like a Laravel Form Request).
/// Rules that need the database (does the item exist? is it available?)
/// are checked in OrderService instead.
/// </summary>
public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.OrderType).IsInEnum();

        // Dine-in needs a table; takeaway must not have one (same rule as the DB CHECK constraint)
        RuleFor(x => x.TableId)
            .NotNull().WithMessage("Please choose a table for dine-in orders.")
            .When(x => x.OrderType == OrderType.DineIn);

        RuleFor(x => x.TableId)
            .Null().WithMessage("Takeaway orders must not have a table.")
            .When(x => x.OrderType == OrderType.Takeaway);

        // Takeaway needs a name so staff can call the customer
        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Please enter a name for takeaway orders.")
            .When(x => x.OrderType == OrderType.Takeaway);

        RuleFor(x => x.CustomerName).MaximumLength(100);

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Your order has no items.");

        RuleFor(x => x.Items.Count)
            .LessThanOrEqualTo(50).WithMessage("An order can have at most 50 lines.")
            .When(x => x.Items is not null);

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ItemId).GreaterThan(0);
            item.RuleFor(i => i.Quantity)
                .InclusiveBetween(1, 99).WithMessage("Quantity must be between 1 and 99.");
            item.RuleFor(i => i.Notes).MaximumLength(200);
        });
    }
}
