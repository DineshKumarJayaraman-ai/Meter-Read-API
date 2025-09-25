using Meter_Read_API.Dtos;

namespace Meter_Read_API.Services.Interfaces
{
    public interface IMeterReadingService
    {
        public Task<ProcessResultDto> ProcessReadings(IFormFile formFile,CancellationToken cancellationToken);
    }
}
