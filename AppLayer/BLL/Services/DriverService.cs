using BLL.DTOs;
using BLL.Exceptions;
using BLL.Interfaces;
using DAL.EF.Tables;
using DAL.UnitOfWork;

namespace BLL.Services
{
    public class DriverService : IDriverService
    {
        private readonly IUnitOfWork _uow;

        public DriverService(IUnitOfWork uow) => _uow = uow;

        public async Task GoOnlineAsync(Guid userId)
        {
            var driver = await GetDriverAsync(userId);

            if (await _uow.Vehicles.GetActiveVehicleForDriverAsync(driver.Id) == null)
                throw new InvalidOperationException("Register a vehicle before going online.");

            driver.IsOnline = true;
            _uow.Drivers.Update(driver);
            await _uow.SaveChangesAsync();
        }

        public async Task GoOfflineAsync(Guid userId)
        {
            var driver = await GetDriverAsync(userId);

            if ((await _uow.Pools.GetActivePoolsForDriverAsync(driver.Id)).Any())
                throw new InvalidOperationException("Finish your current pool before going offline.");

            driver.IsOnline = false;
            _uow.Drivers.Update(driver);
            await _uow.SaveChangesAsync();
        }

        public async Task<DriverDto> GetProfileAsync(Guid userId)
        {
            var driver = await GetDriverAsync(userId);
            var user = await _uow.Users.GetByIdAsync(userId);

            return new DriverDto
            {
                Id = driver.Id,
                UserId = driver.UserId,
                Name = user?.Name ?? "",
                LicenseNo = driver.LicenseNo,
                IsOnline = driver.IsOnline
            };
        }

        private async Task<Driver> GetDriverAsync(Guid userId) =>
            await _uow.Drivers.GetByUserIdAsync(userId)
                ?? throw new NotFoundException("Driver profile not found.");
    }
}

