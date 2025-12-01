using CountrySecure.Domain.Entities;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using CountrySecure.Domain.Enums;

namespace CountrySecure.Infrastructure.Repositories;

public class PropertyRepository : GenericRepository<Property>, IPropertyRepository
{
	private readonly CountrySecureDbContext _dbContext;

	public PropertyRepository(CountrySecureDbContext dbContext) : base(dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task<Property?> GetPropertyByAdressAsync(string street, int PropertyNumber)
	{
		return await _dbContext.Properties
			.FirstOrDefaultAsync(p => p.Street == street && p.PropertyNumber == PropertyNumber);
	}

	public async Task<IEnumerable<Property>> GetPropertyByUserIdAsync(Guid UserId)
	{
		return await _dbContext.Properties
			.Where(p => p.UserId == UserId)
			.ToListAsync();
	}

	public async Task<IEnumerable<Property>> GetPropertyByLotIdAsync(Guid LotId)
	{
		return await _dbContext.Properties
			.Where(p => p.LotId == LotId)
			.ToListAsync();
	}


	public async Task<IEnumerable<Property>> GetPropertiesByStatusAsync(PropertyStatus status, int numberPage, int pageSize)
	{
		return await _dbContext.Properties
			.Where(p => p.Status == status)
			.OrderBy(p => p.Id)
			.Skip((numberPage - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync();
	}


}
