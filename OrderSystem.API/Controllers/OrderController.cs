using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderSystem.Application.DTOs;
using OrderSystem.Application.Services;

namespace OrderSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? searchTerm = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _orderService.GetAllOrdersAsync(searchTerm, page, pageSize);
            return Ok(new { result.Items, result.TotalCount, page, pageSize });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null) return NotFound();
            return Ok(order);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OrderCreateDto dto)
        {
            var username = User.Identity?.Name;
            var order = await _orderService.CreateOrderAsync(dto, username ?? "Unknown");
            return Ok(order);
        }

        [HttpPost("approve")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> Approve([FromBody] ApprovalDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            
            var (success, message) = await _orderService.ApproveOrderAsync(dto, userId!, userRole!);
            
            if (!success) return BadRequest(new { message });
            return Ok(new { message });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] OrderUpdateDto dto)
        {
            var updatedOrder = await _orderService.UpdateOrderAsync(id, dto);
            if (updatedOrder == null) return NotFound();
            return Ok(updatedOrder);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _orderService.DeleteOrderAsync(id);
            if (!success) return NotFound();
            return Ok(new { message = "Order deleted successfully" });
        }
    }
}
