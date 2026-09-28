using DAL.EF.Tables;
using DAL.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Seeding
{
    public class DbSeeder
    {
        private const string DemoPassword = "Pass@1234";
        private readonly IUnitOfWork _uow;

        public DbSeeder(IUnitOfWork uow) => _uow = uow;

        public async Task SeedAsync()
        {
            if (!(await _uow.Zones.GetAllAsync()).Any())
            {
                // Approximate coordinates, good enough for the simple distance model.
                var zones = new (string Name, decimal Lat, decimal Lng)[]
                {
                ("Banani", 23.7937m, 90.4066m),
                ("Gulshan 1", 23.7808m, 90.4167m),
                ("Mohakhali", 23.7781m, 90.3986m),
                ("Dhanmondi", 23.7461m, 90.3742m),
                ("Mirpur", 23.8069m, 90.3687m),
                ("Uttara", 23.8759m, 90.3795m),
                ("Farmgate", 23.7561m, 90.3872m),
                ("Bashundhara", 23.8190m, 90.4526m)
                };

                foreach (var z in zones)
                    await _uow.Zones.AddAsync(new Zone
                    {
                        Id = Guid.NewGuid(),
                        Name = z.Name,
                        Latitude = z.Lat,
                        Longitude = z.Lng
                    });
            }

            if (!(await _uow.Users.GetAllAsync()).Any())
            {
                var jashim = await AddUserAsync("Jashim", "01700000001", "Driver", 0);
                var driver = new Driver
                {
                    Id = Guid.NewGuid(),
                    UserId = jashim.Id,
                    LicenseNo = "DHK-1001",
                    IsOnline = true,
                    CreatedAt = DateTime.UtcNow
                };
                await _uow.Drivers.AddAsync(driver);
                await _uow.Vehicles.AddAsync(new Vehicle
                {
                    Id = Guid.NewGuid(),
                    DriverId = driver.Id,
                    Name = "Bullet",
                    Capacity = 3,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });

                await AddUserAsync("Nusrat", "01700000002", "Passenger", 50000);
                await AddUserAsync("Rafiq", "01700000003", "Passenger", 50000);
                await AddUserAsync("Shirin", "01700000004", "Passenger", 1000);
            }

            await _uow.SaveChangesAsync();
        }

        private async Task<User> AddUserAsync(string name, string phone, string role, long walletPaisa)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = name,
                Phone = phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(DemoPassword),
                Role = role,
                CreatedAt = DateTime.UtcNow
            };
            await _uow.Users.AddAsync(user);
            await _uow.Wallets.AddAsync(new Wallet
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                BalancePaisa = walletPaisa
            });
            return user;
        }
    }
}
