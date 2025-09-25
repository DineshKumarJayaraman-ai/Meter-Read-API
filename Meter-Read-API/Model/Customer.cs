using System.ComponentModel.DataAnnotations;

namespace Meter_Read_API.Model
{
    public class Customer
    {
        [Key]
        public int AccountId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}
