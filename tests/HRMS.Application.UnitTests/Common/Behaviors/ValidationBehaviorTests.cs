using FluentValidation;
using FluentValidation.Results;
using HRMS.Application.Common.Behaviors;
using MediatR;

namespace HRMS.Application.UnitTests.Common.Behaviors;

public class ValidationBehaviorTests
{
    public sealed record TestRequest : IRequest<Result<string>>;
    
    [Fact]
    public async Task Handle_ShouldCallNext_WhenNoValidators()
    {
        // Arrange
        var behavior = new ValidationBehavior<TestRequest, Result<string>>([]);
        var request = new TestRequest();
        var nextMock = Substitute.For<RequestHandlerDelegate<Result<string>>>();
        nextMock.Invoke().Returns(Result<string>.Success("ok"));

        // Act
        var result = await behavior.Handle(request, nextMock, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("ok");
        await nextMock.Received(1).Invoke();
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationError_WhenValidationFails()
    {
        // Arrange
        var validatorMock = Substitute.For<IValidator<TestRequest>>();
        var validationFailure = new ValidationFailure("Prop", "Error") { ErrorCode = "ERR1" };
        validatorMock.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult([validationFailure]));

        var behavior = new ValidationBehavior<TestRequest, Result<string>>([validatorMock]);
        var request = new TestRequest();
        var nextMock = Substitute.For<RequestHandlerDelegate<Result<string>>>();

        // Act
        var result = await behavior.Handle(request, nextMock, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Severity.Should().Be(ErrorSeverity.Validation);
        result.Error.Code.Should().Be("ERR1");
        
        await nextMock.DidNotReceive().Invoke();
    }
}
