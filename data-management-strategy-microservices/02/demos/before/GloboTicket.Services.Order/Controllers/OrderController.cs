using GloboTicket.Services.Ordering.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GloboTicket.Services.Ordering.Controllers
{
    [ApiController]
    [Route("api/order")]
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;

        public OrderController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> List(Guid userId)
        {
            var orders = await _orderRepository.GetOrdersForUser(userId);
            return Ok(orders);
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> Get(Guid orderId)
        {
            var order = await _orderRepository.GetOrderById(orderId);
            return Ok(order);
        }

    }
}
