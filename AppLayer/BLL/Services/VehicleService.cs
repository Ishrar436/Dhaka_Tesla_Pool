using BLL.DTOs;
using BLL.Interfaces;
using DAL.EF.Tables;
using DAL.UnitOfWork;

namespace BLL.Services;

public class VehicleService : IVehicleService
{
    private readonly IUnitOfWork _uow;

    public VehicleService(IUnitOfWork uow) => _uow = uow;

    public async Task<VehicleDto> RegisterVehicleAsync(Guid driverId, CreateVehicleDto dto)
    {
        if (dto.Capacity <= 0)
            throw new ArgumentException("Capacity must be greater than zero.");

        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            DriverId = driverId,
            Name = dto.Name,
            Capacity = dto.Capacity,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _uow.Vehicles.AddAsync(vehicle);
        await _uow.SaveChangesAsync();

        return MapToDto(vehicle);
    }

    public async Task<IEnumerable<VehicleDto>> GetByDriverAsync(Guid driverId)
    {
        var vehicles = await _uow.Vehicles.GetByDriverIdAsync(driverId);
        return vehicles.Select(MapToDto);
    }

    private static VehicleDto MapToDto(Vehicle v) => new()
    {
        Id = v.Id,
        DriverId = v.DriverId,
        Name = v.Name,
        Capacity = v.Capacity,
        IsActive = v.IsActive
    };
}