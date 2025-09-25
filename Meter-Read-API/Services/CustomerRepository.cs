using Meter_Read_API.Data;
using Meter_Read_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Meter_Read_API.Services
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _appDbContext;

        public CustomerRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<List<int>> GetExistingAccountIdsAsync(List<int> accountIds)
        {
            return await _appDbContext.Customers
                .Where(c => accountIds.Contains(c.AccountId))
                .Select(c => c.AccountId)
                .ToListAsync();
        }
    }

}
