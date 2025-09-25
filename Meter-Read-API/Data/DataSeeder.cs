using Azure.Storage.Blobs;

namespace Meter_Read_API.Data
{
    public static class DataSeeder
    {
        public static void SeedCustomers(AppDbContext appDbContext,string blobConnectionString,string blobContainer, string blobName)
        {
            //if(!appDbContext.Customers.Any())
            //{
            //    var blobClient=new BlobClient(blobConnectionString, blobContainer, blobName);
            //    var stream = blobClient.OpenRead();

            //    using var reader = new StreamReader(stream);

            //    using var csv=new CsvReader
            //}
        }
    }
}
