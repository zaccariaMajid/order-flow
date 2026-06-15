using OrderService.Application.Orders.Create;

namespace OrderService.UnitTests.Orders;

public class CreateOrderCommandValidatorTests
{
    private readonly CreateOrderCommandValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        CreateOrderCommand command = ValidCommand();

        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Customer_id_is_required()
    {
        CreateOrderCommand command = ValidCommand(customerId: Guid.Empty);

        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateOrderCommand.CustomerId));
    }

    [Fact]
    public void At_least_one_item_is_required()
    {
        CreateOrderCommand command = new(Guid.NewGuid(), []);

        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateOrderCommand.Items));
    }

    [Theory]
    [InlineData("productId")]
    [InlineData("productName")]
    [InlineData("quantity")]
    [InlineData("unitPrice")]
    public void Item_fields_are_validated(string invalidField)
    {
        CreateOrderItem item = invalidField switch
        {
            "productId" => new CreateOrderItem(Guid.Empty, "Product", 1, 10m),
            "productName" => new CreateOrderItem(Guid.NewGuid(), "", 1, 10m),
            "quantity" => new CreateOrderItem(Guid.NewGuid(), "Product", 0, 10m),
            "unitPrice" => new CreateOrderItem(Guid.NewGuid(), "Product", 1, 0m),
            _ => throw new ArgumentOutOfRangeException(nameof(invalidField))
        };
        CreateOrderCommand command = new(Guid.NewGuid(), [item]);

        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    private static CreateOrderCommand ValidCommand(Guid? customerId = null) =>
        new(
            customerId ?? Guid.NewGuid(),
            [
                new CreateOrderItem(
                    Guid.NewGuid(),
                    "Product",
                    1,
                    10m)
            ]);
}
