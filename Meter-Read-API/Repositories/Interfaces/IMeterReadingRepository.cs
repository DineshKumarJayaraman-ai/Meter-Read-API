using Meter_Read_API.Model;

namespace Meter_Read_API.Repositories.Interfaces
{
    public interface IMeterReadingRepository
    {
        public Task<List<MeterReading>> GetExistingReadingsAsync(List<int> accountIds);

        public Task AddReadingsAsync(List<MeterReading> readings,CancellationToken cancellationToken);
    }
}
