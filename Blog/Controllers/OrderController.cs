using Blog.Models;
using Blog.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Controllers
{
    [Route("api/order")]
    [ApiController]
    public class OrderController : Controller
    {
        private readonly OrderRepo _orderRepo;
        private readonly UserRepo _userRepo;
        private readonly PostRepo _postRepo;
        private readonly IConfiguration _configuration;

        public OrderController(UserRepo userRepo, PostRepo postRepo, IConfiguration configuration)
        {
            _userRepo = userRepo;
            _postRepo = postRepo;
            _configuration = configuration;
        }

        [HttpGet("findAll")]
        public async Task<ActionResult> FindOrder()
        {
            var orders = await _orderRepo.FindOrders();

            if (orders.Count > 0)
            {
                return StatusCode(200, new { status = true, data = orders });
            }
            else
            {
                return StatusCode(200, new { status = true, data = new List<OrderModel>() });
            }
        }

        [HttpGet("findById/{id:length(24)}")]
        public async Task<ActionResult> FindById(string id)
        {
            var order = await _orderRepo.FindOrderById(id);

            if (order != null)
            {
                return StatusCode(200, new { status = true, data = order });
            }
            else
            {
                return StatusCode(400, new { status = false });
            }
        }

        [HttpDelete("remove/{id:length(24)}")]
        public async Task<IActionResult> RemoveOrder(string id)
        {
            var order = await _orderRepo.FindOrderById(id);

            if (order != null)
            {
                await _orderRepo.RemoveOrder(id);

                return StatusCode(204, new { status = true });
            }
            else
            {
                return StatusCode(400, new { status = false });
            }
        }


    }
}
