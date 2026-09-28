using BLL.DTOs;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppLayer.Controllers;

[ApiController]
[Authorize(Roles = "Driver")]
[Route("api/vehicles")]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleService _vehicleService;

    public VehiclesController(IVehicleService vehicleService) => _vehicleService = vehicleService;

    [HttpPost("{driverId}")]
    public async Task<IActionResult> Register(Guid driverId, CreateVehicleDto dto)
    {
        try
        {
            var result = await _vehicleService.RegisterVehicleAsync(driverId, dto);
            return Ok(result);
        }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpGet("driver/{driverId}")]
    public async Task<IActionResult> GetByDriver(Guid driverId)
    {
        var result = await _vehicleService.GetByDriverAsync(driverId);
        return Ok(result);
    }
}