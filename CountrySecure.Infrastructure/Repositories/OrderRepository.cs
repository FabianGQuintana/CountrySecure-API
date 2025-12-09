using CountrySecure.Domain.Entities;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using CountrySecure.Domain.Enums;

namespace CountrySecure.Infrastructure.Repositories;

public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    private readonly CountrySecureDbContext _dbContext;

    public OrderRepository(CountrySecureDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await _dbContext.Orders.ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetByStatusAsync(bool activo)
    {
        String status;
        if (activo == true)
        {
            status = "Active";
        }
        else
        {
            status = "Inactive";
        }

        return await _dbContext.Orders
            .Where(o => o.Status == status)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetByTypeAsync(OrderStatus tipoServicio)
    {
        return await _dbContext.Orders
            .Where(o => o.OrderType == tipoServicio)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetBySupplierAsync(string nombreProveedor)
    {
        return await _dbContext.Orders
            .Where(o => o.SupplierName == nombreProveedor)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetMostRequestedAsync()
    {
        return await _dbContext.Orders
            .OrderByDescending(o => o.Requests.Count)
            .Take(10)
            .ToListAsync();
    }
}
