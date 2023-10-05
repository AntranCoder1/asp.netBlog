using Blog.ClassValue;
using Blog.Contracts;
using Blog.Models.Stripe;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;


namespace Blog.Controllers
{
    [Route("api/stripe")]
    public class StripeController : Controller
    {
        private readonly IStripeAppService _stripeService;

        public StripeController(IStripeAppService stripeAppService)
        {
            _stripeService = stripeAppService;
        }

        [HttpPost("customer/add")]
        public async Task<ActionResult<StripeCustomer>> AddStripeCustomer([FromBody] AddStripeCustomer customer, CancellationToken ct)
        {
            StripeCustomer createdCustomer = await _stripeService.AddStripeCustomerAsync(customer, ct);

            return StatusCode(200, new { status = true, data = createdCustomer });
        }

        [HttpPost("payment/add")]
        public async Task<ActionResult<StripePayment>> AddStripePayment([FromBody] AddStripePayment payment, CancellationToken ct)
        {
            StripePayment createdPayment = await _stripeService.AddStripePaymentAsync(payment, ct);

            return StatusCode(200, new { status = true, data = createdPayment });
        }
    }
}
