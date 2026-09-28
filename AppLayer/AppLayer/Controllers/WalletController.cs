// AppLayer/Controllers/WalletController.cs
using AppLayer.Extensions;
using BLL.DTOs;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppLayer.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/wallet")]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _wallet;

        public WalletController(IWalletService wallet) => _wallet = wallet;

        [HttpGet]
        public async Task<IActionResult> GetBalance() =>
            Ok(await _wallet.GetBalanceAsync(User.GetUserId()));

        [HttpPost("topup")]
        public async Task<IActionResult> TopUp(TopUpDto dto)
        {
            await _wallet.TopUpAsync(User.GetUserId(), dto.AmountPaisa);
            return NoContent();
        }
    }
}