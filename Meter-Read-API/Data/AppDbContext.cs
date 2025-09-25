using Meter_Read_API.Model;
using Microsoft.EntityFrameworkCore;

namespace Meter_Read_API.Data
{
    public class AppDbContext  : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<MeterReading> MeterReadings { get; set; } 
    }
}
