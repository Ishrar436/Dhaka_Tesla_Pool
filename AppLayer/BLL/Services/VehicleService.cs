using BLL.DTOs;
using BLL.Exceptions;
using BLL.Interfaces;
using DAL.EF.Tables;
using DAL.UnitOfWork;

namespace BLL.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IUnitOfWork _uow;

        public VehicleService(IUnitOfWork uow) => _uow = uow;

        public async Task<VehicleDto> RegisterVehicleAsync(Guid userId, CreateVehicleDto dto)
        {
            if (dto.Capacity <= 0)
                throw new ArgumentException("Capacity must be greater than zero.");

            var driver = await _uow.Drivers.GetByUserIdAsync(userId)
                ?? throw new NotFoundException("Driver profile not found.");

            // Assumption: one vehicle per driver in this MVP.
            if ((await _uow.Vehicles.GetByDriverIdAsync(driver.Id)).Any())
                throw new InvalidOperationException("This driver already has a registered vehicle.");

            var vehicle = new Vehicle
            {
                Id = Guid.NewGuid(),
                DriverId = driver.Id,
                Name = dto.Name,
                Capacity = dto.Capacity,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _uow.Vehicles.AddAsync(vehicle);
            await _uow.SaveChangesAsync();
            return MapToDto(vehicle);
        }

        public async Task<IEnumerable<VehicleDto>> GetMineAsync(Guid userId)
        {
            var driver = await _uow.Drivers.GetByUserIdAsync(userId)
                ?? throw new NotFoundException("Driver profile not found.");

            var vehicles = await _uow.Vehicles.GetByDriverIdAsync(driver.Id);
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
}

