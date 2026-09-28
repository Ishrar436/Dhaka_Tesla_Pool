using BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AppLayer.Controllers;

[ApiController]
[Route("api/zones")]
public class ZonesController : ControllerBase
{
    private readonly IZoneService _zoneService;

    public ZonesController(IZoneService zoneService) => _zoneService = zoneService;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _zoneService.GetAllAsync();
        return Ok(result);
    }
}