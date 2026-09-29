using AppLayer.Extensions;
using BLL.DTOs;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppLayer.Controllers
{
    [ApiController]
    [Authorize(Roles = "Passenger")]
    [Route("api/rides")]
    public class RideRequestsController : ControllerBase
    {
        private readonly IRideRequestService _rides;

        public RideRequestsController(IRideRequestService rides) => _rides = rides;

        [HttpPost]
        public async Task<IActionResult> Create(CreateRideRequestDto dto)
        {
            var result = await _rides.CreateRideRequestAsync(User.GetUserId(), dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id) =>
            Ok(await _rides.GetStatusAsync(User.GetUserId(), id));

        [HttpGet("{id:guid}/timeline")]
        public async Task<IActionResult> GetTimeline(Guid id) =>
            Ok(await _rides.GetTimelineAsync(User.GetUserId(), id));

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory() =>
            Ok(await _rides.GetHistoryAsync(User.GetUserId()));

        [HttpPatch("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            await _rides.CancelAsync(User.GetUserId(), id);
            return NoContent();
        }
    }
}