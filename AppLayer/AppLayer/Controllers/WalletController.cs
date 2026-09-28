// AppLayer/Controllers/WalletController.cs
using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppLayer.Controllers;

[ApiController]
[Authorize]
[Route("api/wallet")]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletController(IWalletService walletService) => _walletService = walletService;

    [HttpGet]
    public async Task<IActionResult> GetBalance()
    {
        try
        {
            var result = await _walletService.GetBalanceAsync(GetUserId());
            return Ok(result);
        }
        catch (ArgumentException ex) { return NotFound(new { error = ex.Message }); }
    }

    [HttpPost("topup")]
    public async Task<IActionResult> TopUp([FromBody] long amountPaisa)
    {
        try
        {
            await _walletService.TopUpAsync(GetUserId(), amountPaisa);
            return NoContent();
        }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}