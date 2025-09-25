using CsvHelper;
using Meter_Read_API.Dtos;
using Meter_Read_API.Model;
using Meter_Read_API.Repositories.Interfaces;
using Meter_Read_API.Services.Interfaces;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Meter_Read_API.Services
{
    public class MeterReadingService : IMeterReadingService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IMeterReadingRepository _meterReadingRepository;
        public MeterReadingService(IMeterReadingRepository meterReadingRepository, ICustomerRepository customerRepository)
        {
            _meterReadingRepository = meterReadingRepository;
            _customerRepository = customerRepository;
        }

        public async Task<ProcessResultDto> ProcessReadings(IFormFile formFile, CancellationToken cancellationToken = default)
        {
            var accountIdsSet = new HashSet<int>();
            await foreach (var dto in ParseCsvFileStream(formFile, cancellationToken))
            {
                accountIdsSet.Add(dto.AccountId);
            }

            var accountIds = accountIdsSet.ToList();

            var existingAccountIds = await _customerRepository.GetExistingAccountIdsAsync(accountIds);
            var existingReadings = await _meterReadingRepository.GetExistingReadingsAsync(accountIds);
            var existingReadingsSet = new HashSet<string>(existingReadings.Select(r => $"{r.AccountId}_{r.ReadingDateTime:O}"));

            formFile.OpenReadStream().Position = 0;

            int failedCount = 0;
            int successCount = 0;
            var batchToInsert = new List<MeterReading>();
            const int batchSize = 1000;

            await foreach (var dto in ParseCsvFileStream(formFile, cancellationToken))
            {
                if (!IsValid(dto, existingAccountIds, existingReadingsSet))
                {
                    failedCount++;
                    continue;
                }

                batchToInsert.Add(new MeterReading
                {
                    AccountId = dto.AccountId,
                    ReadingDateTime = dto.MeterReadingDateTime,
                    MeterReadValue = dto.MeterReadValue
                });

                successCount++;

                if (batchToInsert.Count >= batchSize)
                {
                    await _meterReadingRepository.AddReadingsAsync(batchToInsert, cancellationToken);
                    batchToInsert.Clear();
                }
            }

            if (batchToInsert.Count > 0)
            {
                await _meterReadingRepository.AddReadingsAsync(batchToInsert, cancellationToken);
            }

            return new ProcessResultDto
            {
                SuccessCount = successCount,
                FailureCount = failedCount
            };
        }

        private bool IsValid(MeterReadingDto dto, List<int> existingAccountIds, HashSet<string> existingReadingsSet)
        {
            if (!existingAccountIds.Contains(dto.AccountId))
                return false;

            if (!Regex.IsMatch(dto.MeterReadValue, @"^\d{5}$"))
                return false;

            string key = $"{dto.AccountId}_{dto.MeterReadingDateTime:O}";
            if (existingReadingsSet.Contains(key))
                return false;

            return true;
        }

        private async IAsyncEnumerable<MeterReadingDto> ParseCsvFileStream(IFormFile formFile, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using var reader = new StreamReader(formFile.OpenReadStream());
            using var csv = new CsvReader(reader, new CultureInfo("en-GB"));

            await foreach (var record in csv.GetRecordsAsync<MeterReadingDto>().WithCancellation(cancellationToken))
            {
                yield return record;
            }
        }
    }
}
