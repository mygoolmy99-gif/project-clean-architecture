using HRMS.Domain.Entities;
using HRMS.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Persistence.Repositories;

public sealed class CountryRepository(ApplicationDbContext dbContext) : ICountryRepository
{
    public async Task<Country?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Countries.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Country>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Countries.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Country country, CancellationToken cancellationToken = default)
    {
        await dbContext.Countries.AddAsync(country, cancellationToken);
    }

    public void Update(Country country)
    {
        dbContext.Countries.Update(country);
    }

    public void Delete(Country country)
    {
        dbContext.Countries.Remove(country);
    }
}
