namespace Meter_Read_API.Services.Interfaces
{
    public interface ICustomerRepository
    {
        Task<List<int>> GetExistingAccountIdsAsync(List<int> accountIds);
    }
}
