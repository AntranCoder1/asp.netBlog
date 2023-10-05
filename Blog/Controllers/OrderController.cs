using Blog.ClassValue;
using Blog.Models;
using Blog.Repository;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Net.Mail;
using System.Net;
using Blog.utils;

namespace Blog.Controllers
{
    [Route("api/order")]
    [ApiController]
    public class OrderController : Controller
    {
        private readonly OrderRepo _orderRepo;
        private readonly UserRepo _userRepo;
        public OrderController(OrderRepo orderRepo, UserRepo userRepo)
        {
            _orderRepo = orderRepo;
            _userRepo = userRepo;
        }

        [HttpGet("getOrders/{limit}/{offset}")]
        public async Task<ActionResult> GetOrders(int limit, int offset)
        {
            if (Request.Cookies.TryGetValue("AuthToken", out string AuthToken))
            {
                string rawContent = string.Empty;
                using (var reader = new StreamReader(Request.Body,
                              encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false))
                {
                    rawContent = await reader.ReadToEndAsync();
                }

                UserValue user = JsonConvert.DeserializeObject<UserValue>(rawContent);



                var orders = await _orderRepo.FindOrders(limit, offset);

                if (orders.Count > 0)
                {
                    return Ok(new { status = "success", data = orders });
                }
                else
                {
                    return Ok(new { status = true, data = new int[] { } });
                }
            }
            else
            {
                // Cookie không tồn tại trong yêu cầu, xử lý logic phù hợp
                return BadRequest(new { status = false, message = "Unauthorized access" });
            }
        }

        [HttpPost("createOrder")]
        public async Task<IActionResult> createNewOrder([FromHeader(Name = "Authorization")] string token)
        {
            try
            {
                var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token.Replace("Bearer ", string.Empty));
                var userId = jwtToken.Claims.First(claim => claim.Type == "Id").Value;

                string rawContent = string.Empty;
                using (var reader = new StreamReader(Request.Body,
                              encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false))
                {
                    rawContent = await reader.ReadToEndAsync();
                }

                OrderValue order = JsonConvert.DeserializeObject<OrderValue>(rawContent);

                OrderModel orderModel = new OrderModel();

                if (order != null)
                {
                    if (!string.IsNullOrEmpty(userId))
                    {
                        orderModel.UserId = ObjectId.Parse(userId);
                    }

                    if (!string.IsNullOrEmpty(order.BookingId))
                    {
                        orderModel.BookingId = ObjectId.Parse(order.BookingId);
                    }

                    orderModel.noted = order.noted;
                }

                await _orderRepo.createOrder(orderModel);

                // findUserById
                var user = await _userRepo.GetUser(userId);

                // Set the email sender, recipient, subject, and body
                string senderEmail = "thanhantran21@gmail.com";
                string recipientEmail = user.email;
                string subject = "Verify Code";
                string body = $"Hi {user.username}, you have successfully booked your room at {DateTime.Now} with veryfy code is {GenerateCode.VerifyCode()}";

                // Create a MailMessage object
                MailMessage mail = new MailMessage(senderEmail, recipientEmail, subject, body);

                // Set the SMTP server details
                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
                smtpClient.Credentials = new NetworkCredential("thanhantran21@gmail.com", "okifhhhxhjasrioe");
                smtpClient.EnableSsl = true;

                // Send the email
                smtpClient.Send(mail);

                return StatusCode(200, new { status = true, message = "order has been create successfully" });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine($"An error occurred while creating the order: {ex}");

                // Return an error response
                return StatusCode(500, new { status = false, message = "An error occurred while creating the order." });
            }
        }

        [HttpGet("getOrder/{id}")]
        public async Task<ActionResult> GetOrderById(string id)
        {
            try
            {
                var order = await _orderRepo.FindOrderById(id);

                if (order == null)
                {
                    return StatusCode(404, new { status = false, message = "order not found" });
                }
                else
                {
                    return StatusCode(200, new { status = true, data = order });
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine($"An error occurred while getting the order: {ex}");

                // Return an error response
                return StatusCode(500, new { status = false, message = "An error occurred while getting the order." });
            }
        }

        [HttpGet("getOrderByUserId/{id}/{limit}/{offset}")]
        public async Task<ActionResult> GetOrderByUserId(string id, int limit, int offset)
        {
            try
            {
                var order = await _orderRepo.FindOrderByUserId(id, limit, offset);

                if (order == null)
                {
                    return StatusCode(404, new { status = false, message = "order not found" });
                }
                else
                {
                    return StatusCode(200, new { status = true, data = order });
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine($"An error occurred while getting the order: {ex}");

                // Return an error response
                return StatusCode(500, new { status = false, message = "An error occurred while getting the order." });
            }
        }

        [HttpPut("updateOrder/{id}")]
        public async Task<IActionResult> UpdateOrder([FromHeader(Name = "Authorization")] string token, string id)
        {
            try
            {
                var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token.Replace("Bearer ", string.Empty));
                var userId = jwtToken.Claims.First(claim => claim.Type == "Id").Value;

                string rawContent = string.Empty;
                using (var reader = new StreamReader(Request.Body,
                              encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false))
                {
                    rawContent = await reader.ReadToEndAsync();
                }

                OrderValue order = JsonConvert.DeserializeObject<OrderValue>(rawContent);

                var findOrderUserId = await _orderRepo.findOrderWithUserIdAndOrderId(id, userId);

                if (findOrderUserId == null)
                {
                    return StatusCode(400, new { status = false, message = "You not allow update" });
                }
                else
                {
                    OrderModel orderModel = new OrderModel();

                    if (!string.IsNullOrEmpty(order.BookingId))
                    {
                        orderModel.BookingId = ObjectId.Parse(order.BookingId);
                    }
                    orderModel.noted = order.noted;
                    orderModel.updatedAt = new DateTime();

                    var updateOrder = await _orderRepo.UpdateOrder(id, orderModel);

                    if (updateOrder != null)
                    {
                        return StatusCode(200, new { status = true, message = "Update order has been successfully" });
                    }
                    else
                    {
                        return StatusCode(400, new { status = false, message = "Update faild" });
                    }

                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine($"An error occurred while update the order: {ex}");

                // Return an error response
                return StatusCode(500, new { status = false, message = "An error occurred while update the order." });
            }
        }

        [HttpDelete("deleteOrder/{id}")]
        public async Task<IActionResult> DeleteOrder([FromHeader(Name = "Authorization")] string token, string id)
        {
            try
            {
                var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token.Replace("Bearer ", string.Empty));
                var userId = jwtToken.Claims.First(claim => claim.Type == "Id").Value;

                var findOrder = await _orderRepo.findOrderWithUserIdAndOrderId(id, userId);

                if (findOrder != null)
                {
                    await _orderRepo.RemoveOrder(id, userId);

                    return StatusCode(200, new { status = true, message = "delete order has been successfully" });
                }
                else
                {
                    return StatusCode(404, new { status = false, message = "order not found" });
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine($"An error occurred while delete the order: {ex}");

                // Return an error response
                return StatusCode(500, new { status = false, message = "An error occurred while delete the order." });
            }
        }
    }
}
