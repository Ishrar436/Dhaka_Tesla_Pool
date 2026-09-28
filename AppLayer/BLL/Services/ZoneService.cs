using BLL.DTOs;
using BLL.Interfaces;
using DAL.UnitOfWork;

namespace BLL.Services;

public class ZoneService : IZoneService
{
    private readonly IUnitOfWork _uow;

    public ZoneService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<ZoneDto>> GetAllAsync()
    {
        var zones = await _uow.Zones.GetAllAsync();
        return zones.Select(z => new ZoneDto
        {
            Id = z.Id,
            Name = z.Name,
            Latitude = z.Latitude,
            Longitude = z.Longitude
        });
    }
}