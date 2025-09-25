namespace Meter_Read_API.Model
{
    public class MeterReading
    {
        public int Id { get; set; }

        public int AccountId { get; set; }

        public DateTime ReadingDateTime { get; set; }

        public string MeterReadValue{ get; set; }

    }
}
