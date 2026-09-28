using AppLayer.Extensions;
using BLL.DTOs;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppLayer.Controllers
{
    [ApiController]
    [Authorize(Roles = "Driver")]
    [Route("api/vehicles")]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleService _vehicles;

        public VehiclesController(IVehicleService vehicles) => _vehicles = vehicles;

        [HttpPost]
        public async Task<IActionResult> Register(CreateVehicleDto dto) =>
            Ok(await _vehicles.RegisterVehicleAsync(User.GetUserId(), dto));

        [HttpGet]
        public async Task<IActionResult> GetMine() =>
            Ok(await _vehicles.GetMineAsync(User.GetUserId()));
    }
}