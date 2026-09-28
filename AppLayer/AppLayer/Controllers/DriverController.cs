using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppLayer.Controllers;

[ApiController]
[Authorize(Roles = "Driver")]
[Route("api/driver")]
public class DriverController : ControllerBase
{
    private readonly IDriverService _driverService;

    public DriverController(IDriverService driverService) => _driverService = driverService;

    [HttpPost("{driverId}/online")]
    public async Task<IActionResult> GoOnline(Guid driverId)
    {
        try { await _driverService.GoOnlineAsync(driverId); return NoContent(); }
        catch (ArgumentException ex) { return NotFound(new { error = ex.Message }); }
    }

    [HttpPost("{driverId}/offline")]
    public async Task<IActionResult> GoOffline(Guid driverId)
    {
        try { await _driverService.GoOfflineAsync(driverId); return NoContent(); }
        catch (ArgumentException ex) { return NotFound(new { error = ex.Message }); }
    }

    [HttpGet("{driverId}")]
    public async Task<IActionResult> GetProfile(Guid driverId)
    {
        var result = await _driverService.GetProfileAsync(driverId);
        return result == null ? NotFound() : Ok(result);
    }
}