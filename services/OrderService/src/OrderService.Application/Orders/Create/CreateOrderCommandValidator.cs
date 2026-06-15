using FluentValidation;

namespace OrderService.Application.Orders.Create;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(command => command.CustomerId)
            .NotEqual(Guid.Empty);

        RuleFor(command => command.Items)
            .NotNull()
            .NotEmpty();

        RuleForEach(command => command.Items)
            .SetValidator(new CreateOrderItemValidator());
    }
}

public sealed class CreateOrderItemValidator : AbstractValidator<CreateOrderItem>
{
    public CreateOrderItemValidator()
    {
        RuleFor(item => item.ProductId)
            .NotEqual(Guid.Empty);

        RuleFor(item => item.ProductName)
            .NotEmpty();

        RuleFor(item => item.Quantity)
            .GreaterThan(0);

        RuleFor(item => item.UnitPrice)
            .GreaterThan(0);
    }
}
