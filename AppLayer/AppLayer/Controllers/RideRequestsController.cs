using BLL.DTOs;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppLayer.Controllers;

[ApiController]
[Authorize]
[Route("api/rides")]
public class RideRequestsController : ControllerBase
{
    private readonly IRideRequestService _rideRequestService;

    public RideRequestsController(IRideRequestService rideRequestService) =>
        _rideRequestService = rideRequestService;

    [HttpPost]
    public async Task<IActionResult> Create(CreateRideRequestDto dto)
    {
        try
        {
            var result = await _rideRequestService.CreateRideRequestAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { error = ex.Message }); }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _rideRequestService.GetStatusAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var passengerId = GetUserId();
        var result = await _rideRequestService.GetHistoryAsync(passengerId);
        return Ok(result);
    }

    [HttpPatch("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        try
        {
            await _rideRequestService.CancelAsync(id);
            return NoContent();
        }
        catch (ArgumentException ex) { return NotFound(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { error = ex.Message }); }
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}