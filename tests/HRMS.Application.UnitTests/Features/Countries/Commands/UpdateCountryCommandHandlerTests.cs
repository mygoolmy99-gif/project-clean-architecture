using HRMS.Application.Features.Countries.Commands.UpdateCountry;

namespace HRMS.Application.UnitTests.Features.Countries.Commands;

public class UpdateCountryCommandHandlerTests
{
    private readonly ICountryRepository _countryRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly UpdateCountryCommandHandler _handler;

    public UpdateCountryCommandHandlerTests()
    {
        _countryRepositoryMock = Substitute.For<ICountryRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();

        _handler = new UpdateCountryCommandHandler(
            _countryRepositoryMock,
            _unitOfWorkMock);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenCountryDoesNotExist()
    {
        // Arrange
        var command = new UpdateCountryCommand(Guid.NewGuid(), "United States", "US", "+1", []);
        _countryRepositoryMock.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns((Country?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Severity.Should().Be(ErrorSeverity.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnConcurrencyConflict_WhenRowVersionDoesNotMatch()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var country = Country.Create(tenantId, "Old Name", "UK", "+44");
        // Simulate a past update to change RowVersion
        country.Update("Old Name 2", "UK", "+44"); 

        var staleRowVersion = new byte[] { 0, 0, 0, 1 }; // Different from current RowVersion
        var command = new UpdateCountryCommand(country.Id, "United Kingdom", "UK", "+44", staleRowVersion);
        
        _countryRepositoryMock.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(country);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("CONCURRENCY_CONFLICT");
    }

    [Fact]
    public async Task Handle_ShouldUpdateCountry_WhenValidAndRowVersionMatches()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var country = Country.Create(tenantId, "Old Name", "UK", "+44");
        
        var command = new UpdateCountryCommand(country.Id, "United Kingdom", "UK", "+44", country.RowVersion);
        
        _countryRepositoryMock.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(country);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        _countryRepositoryMock.Received(1).Update(country);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        
        country.Name.Should().Be("United Kingdom");
    }
}
