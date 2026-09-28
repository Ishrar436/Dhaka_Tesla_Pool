using AppLayer.Extensions;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppLayer.Controllers
{
    [ApiController]
    [Authorize(Roles = "Driver")]
    [Route("api/driver")]
    public class DriverController : ControllerBase
    {
        private readonly IDriverService _drivers;

        public DriverController(IDriverService drivers) => _drivers = drivers;

        [HttpPost("online")]
        public async Task<IActionResult> GoOnline()
        {
            await _drivers.GoOnlineAsync(User.GetUserId());
            return NoContent();
        }

        [HttpPost("offline")]
        public async Task<IActionResult> GoOffline()
        {
            await _drivers.GoOfflineAsync(User.GetUserId());
            return NoContent();
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetProfile() =>
            Ok(await _drivers.GetProfileAsync(User.GetUserId()));
    }
}