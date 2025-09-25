using Moq;
using Microsoft.AspNetCore.Http;
using Meter_Read_API.Model;
using Meter_Read_API.Repositories.Interfaces;
using Meter_Read_API.Services;
using System.Text;

public class MeterReadingServiceTests
{
    private IFormFile CreateTestCsvFile(string csvContent)
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
        return new FormFile(stream, 0, stream.Length, "file", "test.csv");
    }

    [Fact]
    public async Task AllValidReadingsInserted()
    {
        // Arrange
        var csvContent = "AccountId,MeterReadingDateTime,MeterReadValue\n" +
                         "1001,2023-01-01T10:00:00,12345\n" +
                         "1002,2023-01-01T11:00:00,67890";

        var file = CreateTestCsvFile(csvContent);

        var mockRepo = new Mock<IMeterReadingRepository>();
        mockRepo.Setup(r => r.GetExistingAccountIdsAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(new List<int> { 1001, 1002 });

        mockRepo.Setup(r => r.GetExistingReadingsAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(new List<MeterReading>());

        mockRepo.Setup(r => r.AddReadingsAsync(It.IsAny<List<MeterReading>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

        var service = new MeterReadingService(mockRepo.Object);

        // Act
        var result = await service.ProcessReadings(file, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.SuccessCount);
        Assert.Equal(0, result.FailureCount);
        mockRepo.Verify(r => r.AddReadingsAsync(It.IsAny<List<MeterReading>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InvalidAccountIdsShouldSkip()
    {
        // Arrange
        var csvContent = "AccountId,MeterReadingDateTime,MeterReadValue\n" +
                         "9999,2023-01-01T10:00:00,12345";

        var file = CreateTestCsvFile(csvContent);

        var mockRepo = new Mock<IMeterReadingRepository>();
        mockRepo.Setup(r => r.GetExistingAccountIdsAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(new List<int>()); // No valid accounts

        mockRepo.Setup(r => r.GetExistingReadingsAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(new List<MeterReading>());

        var service = new MeterReadingService(mockRepo.Object);

        // Act
        var result = await service.ProcessReadings(file, CancellationToken.None);

        // Assert
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(1, result.FailureCount);
        mockRepo.Verify(r => r.AddReadingsAsync(It.IsAny<List<MeterReading>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task InvalidMeterReadValueShouldSkip()
    {
        // Arrange
        var csvContent = "AccountId,MeterReadingDateTime,MeterReadValue\n" +
                         "1001,2023-01-01T10:00:00,12AB3";

        var file = CreateTestCsvFile(csvContent);

        var mockRepo = new Mock<IMeterReadingRepository>();
        mockRepo.Setup(r => r.GetExistingAccountIdsAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(new List<int> { 1001 });

        mockRepo.Setup(r => r.GetExistingReadingsAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(new List<MeterReading>());

        var service = new MeterReadingService(mockRepo.Object);

        // Act
        var result = await service.ProcessReadings(file, CancellationToken.None);

        // Assert
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(1, result.FailureCount);
        mockRepo.Verify(r => r.AddReadingsAsync(It.IsAny<List<MeterReading>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DuplicateReadingShouldSkip()
    {
        // Arrange
        var csvContent = "AccountId,MeterReadingDateTime,MeterReadValue\n" +
                         "1001,2023-01-01T10:00:00,12345";

        var file = CreateTestCsvFile(csvContent);

        var mockRepo = new Mock<IMeterReadingRepository>();
        mockRepo.Setup(r => r.GetExistingAccountIdsAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(new List<int> { 1001 });

        mockRepo.Setup(r => r.GetExistingReadingsAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(new List<MeterReading>
                {
                    new MeterReading { AccountId = 1001, ReadingDateTime = DateTime.Parse("2023-01-01T10:00:00") }
                });

        var service = new MeterReadingService(mockRepo.Object);

        // Act
        var result = await service.ProcessReadings(file, CancellationToken.None);

        // Assert
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(1, result.FailureCount);
        mockRepo.Verify(r => r.AddReadingsAsync(It.IsAny<List<MeterReading>>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
