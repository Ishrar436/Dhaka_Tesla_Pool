using BLL.DTOs;
using BLL.Interfaces;
using DAL.UnitOfWork;

namespace BLL.Services;

public class DriverService : IDriverService
{
    private readonly IUnitOfWork _uow;

    public DriverService(IUnitOfWork uow) => _uow = uow;

    public async Task GoOnlineAsync(Guid driverId)
    {
        var driver = await _uow.Drivers.GetByIdAsync(driverId)
            ?? throw new ArgumentException("Driver not found.");
        driver.IsOnline = true;
        _uow.Drivers.Update(driver);
        await _uow.SaveChangesAsync();
    }

    public async Task GoOfflineAsync(Guid driverId)
    {
        var driver = await _uow.Drivers.GetByIdAsync(driverId)
            ?? throw new ArgumentException("Driver not found.");
        driver.IsOnline = false;
        _uow.Drivers.Update(driver);
        await _uow.SaveChangesAsync();
    }

    public async Task<DriverDto?> GetProfileAsync(Guid driverId)
    {
        var driver = await _uow.Drivers.GetByIdAsync(driverId);
        if (driver == null) return null;

        var user = await _uow.Users.GetByIdAsync(driver.UserId);

        return new DriverDto
        {
            Id = driver.Id,
            UserId = driver.UserId,
            Name = user?.Name ?? "",
            LicenseNo = driver.LicenseNo,
            IsOnline = driver.IsOnline
        };
    }
}