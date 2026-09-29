using AppLayer.Extensions;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppLayer.Controllers
{
    [ApiController]
    [Authorize(Roles = "Driver")]
    [Route("api/pools")]
    public class PoolsController : ControllerBase
    {
        private readonly IPoolLifecycleService _pools;

        public PoolsController(IPoolLifecycleService pools) => _pools = pools;

        [HttpGet("mine")]
        public async Task<IActionResult> GetMine() =>
            Ok(await _pools.GetMyPoolsAsync(User.GetUserId()));

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory() =>
            Ok(await _pools.GetHistoryAsync(User.GetUserId()));

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id) =>
            Ok(await _pools.GetPoolAsync(User.GetUserId(), id));

        [HttpPatch("{id:guid}/accept")]
        public async Task<IActionResult> Accept(Guid id)
        {
            await _pools.AcceptAsync(User.GetUserId(), id);
            return NoContent();
        }

        [HttpPatch("{id:guid}/decline")]
        public async Task<IActionResult> Decline(Guid id)
        {
            await _pools.DeclineAsync(User.GetUserId(), id);
            return NoContent();
        }

        [HttpPatch("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            await _pools.CancelAsync(User.GetUserId(), id);
            return NoContent();
        }

        [HttpPatch("{id:guid}/arrive")]
        public async Task<IActionResult> Arrive(Guid id)
        {
            await _pools.ArriveAsync(User.GetUserId(), id);
            return NoContent();
        }

        [HttpPatch("{id:guid}/start")]
        public async Task<IActionResult> Start(Guid id)
        {
            await _pools.StartAsync(User.GetUserId(), id);
            return NoContent();
        }

        [HttpPatch("{id:guid}/complete")]
        public async Task<IActionResult> Complete(Guid id)
        {
            await _pools.CompleteAsync(User.GetUserId(), id);
            return NoContent();
        }
    }
}
