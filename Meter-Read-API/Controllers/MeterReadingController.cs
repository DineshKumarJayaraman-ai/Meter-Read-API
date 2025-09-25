using Meter_Read_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Meter_Read_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeterReadingController : ControllerBase
    {
        private readonly IMeterReadingService _meterReadingService;
        private readonly ILogger<MeterReadingController> _logger;

        public MeterReadingController(IMeterReadingService meterReadingService, ILogger<MeterReadingController> logger)
        {
            _meterReadingService = meterReadingService;
            _logger = logger;
        }

        [HttpPost("meter-reading-uploads")]
        public async Task<IActionResult> Upload(IFormFile formFile, CancellationToken cancellationToken)
        {
            try
            {
                if (formFile == null || formFile.Length == 0)
                    return BadRequest("No File Uploaded");

                if (!Path.GetExtension(formFile.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
                    return BadRequest("Only CSV files are allowed.");

                if (formFile.Length > 5 * 1024 * 1024)
                    return BadRequest("File size exceeds 5MB limit.");

                var result = await _meterReadingService.ProcessReadings(formFile, cancellationToken);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Reading File");
                return BadRequest("Invalid File");
            }
        }

    }
}
