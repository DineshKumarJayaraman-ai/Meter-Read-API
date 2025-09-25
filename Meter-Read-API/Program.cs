
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Meter_Read_API.Data;
using Meter_Read_API.Middleware;
using Meter_Read_API.Repositories;
using Meter_Read_API.Repositories.Interfaces;
using Meter_Read_API.Services;
using Meter_Read_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;

namespace Meter_Read_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10, 
                        Window = TimeSpan.FromSeconds(30), 
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 2
                    });
                });

                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode = 429; 
                    await context.HttpContext.Response.WriteAsync("Rate limit exceeded. Try again later.", cancellationToken);
                };
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowStaticApp",
                    builder => builder
                        .WithOrigins("https://victorious-hill-0ec5ff210.2.azurestaticapps.net")
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });


            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            string keyVaultUrl = builder.Configuration["KeyVault:Url"];

            var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
            KeyVaultSecret secret = client.GetSecret("MeterDefaultConnection");

            //var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(secret.Value));

            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
            builder.Services.AddScoped<IMeterReadingRepository, MeterReadingRepository>();
            builder.Services.AddScoped<IMeterReadingService, MeterReadingService>();

            var app = builder.Build();

            app.UseMiddleware<FileValidationMiddleware>();

            using (var scope = app.Services.CreateScope())
            {
                var db= scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("AllowStaticApp");

            app.UseAuthorization();

            app.UseRateLimiter();

            app.MapControllers();

            app.Run();
        }
    }
}
