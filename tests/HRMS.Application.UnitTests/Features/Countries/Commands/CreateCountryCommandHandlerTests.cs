using HRMS.Application.Features.Countries.Commands.CreateCountry;

namespace HRMS.Application.UnitTests.Features.Countries.Commands;

public class CreateCountryCommandHandlerTests
{
    private readonly ICountryRepository _countryRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly ICurrentTenantService _currentTenantServiceMock;
    private readonly CreateCountryCommandHandler _handler;

    public CreateCountryCommandHandlerTests()
    {
        _countryRepositoryMock = Substitute.For<ICountryRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _currentTenantServiceMock = Substitute.For<ICurrentTenantService>();

        _handler = new CreateCountryCommandHandler(
            _countryRepositoryMock,
            _unitOfWorkMock,
            _currentTenantServiceMock);
    }

    [Fact]
    public async Task Handle_ShouldCreateCountry_WhenCommandIsValid()
    {
        // Arrange
        var command = new CreateCountryCommand("United States", "US", "+1");
        var tenantId = Guid.NewGuid();
        _currentTenantServiceMock.GetCurrentTenantId().Returns(tenantId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        await _countryRepositoryMock.Received(1).AddAsync(
            Arg.Is<Country>(c => c.Name == "United States" && c.CountryCode.Value == "US" && c.PhoneCode == "+1" && c.TenantId == tenantId),
            Arg.Any<CancellationToken>());

        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
