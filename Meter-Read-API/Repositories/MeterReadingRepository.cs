using Meter_Read_API.Data;
using Meter_Read_API.Model;
using Meter_Read_API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Meter_Read_API.Repositories
{
    public class MeterReadingRepository : IMeterReadingRepository
    {
        private readonly AppDbContext _appDbContext;

        public MeterReadingRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<List<MeterReading>> GetExistingReadingsAsync(List<int> accountIds)
        {
            return await _appDbContext.MeterReadings
                .Where(r => accountIds.Contains(r.AccountId))
                .ToListAsync();
        }

        public async Task AddReadingsAsync(List<MeterReading> readings, CancellationToken cancellationToken)
        {
            _appDbContext.MeterReadings.AddRange(readings);
            await _appDbContext.SaveChangesAsync();
        }
    }
}
