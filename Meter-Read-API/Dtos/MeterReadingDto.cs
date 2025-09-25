namespace Meter_Read_API.Dtos
{
    public class MeterReadingDto
    {
        public int AccountId { get; set; }

        public DateTime MeterReadingDateTime { get; set; }

        public string  MeterReadValue { get; set; }
    }
}
