using Blog.ClassValue;
using Blog.Models;
using Blog.Repository;
using Blog.utils;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace Blog.Controllers
{
    [Route("api/booking")]
    [ApiController]
    public class BookingController : Controller
    {
        private IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly BookingRepo _bookingRepo;
        public BookingController(IConfiguration configuration, IWebHostEnvironment webHostEnvironment, BookingRepo bookingRepo)
        {
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
            _bookingRepo = bookingRepo;
        }

        [HttpPost("create")]
        public async Task<IActionResult> createNewBooking()
        {
            try
            {
                string rawContent = string.Empty;
                using (var reader = new StreamReader(Request.Body,
                              encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false))
                {
                    rawContent = await reader.ReadToEndAsync();
                }

                BookingValue booking = JsonConvert.DeserializeObject<BookingValue>(rawContent);

                BookingModel bookingModel = new BookingModel
                {
                    title = booking.title,
                    description = booking.description,
                    img = booking.img,
                    type = booking.type,
                    thumbnail = booking.thumbnail,
                    price = booking.price,
                    price_cupon = booking.price_cupon,
                    address = booking.address,
                };

                await _bookingRepo.createBooking(bookingModel);

                return Ok(new { status = "success" });

            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine($"An error occurred while creating the post: {ex}");

                // Return an error response
                return StatusCode(500, new { status = false, message = "An error occurred while creating the booking." });
            }
        }

        [HttpGet("findAll")]
        public async Task<ActionResult> findAllBooking()
        {

            string rawContent = string.Empty;
            using (var reader = new StreamReader(Request.Body,
                            encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false))
            {
                rawContent = await reader.ReadToEndAsync();
            }

            BookingValue bookingValue = JsonConvert.DeserializeObject<BookingValue>(rawContent);

            var bookings = await _bookingRepo.findAllBooking(bookingValue);

            if (bookings.Count > 0)
            {
                return StatusCode(200, new { status = true, booking = bookings });
            }
            else
            {
                return StatusCode(500, new { status = false, message = "Internal server error" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> findBooking(string id)
        {
            var booking = await _bookingRepo.findBookingWithId(id);

            if (booking == null)
            {
                return StatusCode(500, new { status = false, message = "Internal server error" });
            }
            else
            {
                return StatusCode(200, new { status = true, booking = booking });
            }
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> updateBooking(string id)
        {
            try
            {
                string rawContent = string.Empty;
                using (var reader = new StreamReader(Request.Body,
                              encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false))
                {
                    rawContent = await reader.ReadToEndAsync();
                }
                BookingValue booking = JsonConvert.DeserializeObject<BookingValue>(rawContent);

                var findBooking = await _bookingRepo.findBookingWithId(id);

                if (findBooking == null)
                {
                    return StatusCode(500, new { status = false, message = "booking not found" });
                }
                else
                {
                    BookingModel bookingModel = new BookingModel
                    {
                        title = booking.title != null ? booking.title : findBooking.title,
                        description = booking.description != null ? booking.description : findBooking.description,
                        img = booking.img != null ? booking.img : findBooking.img,
                        type = booking.type != null ? booking.type : findBooking.type,
                        thumbnail = booking.thumbnail != null ? booking.thumbnail : findBooking.thumbnail,
                        price = booking.price != 0 ? booking.price : findBooking.price,
                        price_cupon = booking.price_cupon != 0 ? booking.price_cupon : findBooking.price_cupon,
                        address = booking.address != null ? booking.address : findBooking.address,
                        quantity_open = booking.quantity_open != 0 ? booking.quantity_open : findBooking.quantity_open,
                        quantity_closed = booking.quantity_close != 0 ? booking.quantity_close : findBooking.quantity_closed,
                    };

                    await _bookingRepo.updateBooking(id, bookingModel);

                    return StatusCode(200, new { status = true });
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine($"An error occurred while update the booking: {ex}");

                // Return an error response
                return StatusCode(500, new { status = false, message = "An error occurred while cupdate the booking." });
            }
        }

        [HttpPost("uploadImage")]
        public async Task<IActionResult> UploadImage(IFormFile image)
        {
            var imageExtension = new ImageExtension();

            if (!imageExtension.IsImageExtension(image.FileName))
            {
                return BadRequest(new { status = "failed", message = "Invalid image type" });
            }

            string uploadsFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "Upload/Booking");
            Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);

            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            string imageUrl = Url.Content("~/Upload/Booking/" + uniqueFileName);

            if (imageUrl == null)
            {
                return BadRequest(new { status = "failed" });
            }

            return Ok(new { status = "success", data = imageUrl });
        }

        [HttpGet("images/{filename}")]
        public async Task<ActionResult> GetImage(string fileName)
        {
            var path = Path.Combine(_webHostEnvironment.ContentRootPath, "Upload/Booking", fileName);
            var image = System.IO.File.OpenRead(path);

            if (image == null)
            {
                return NotFound(new { status = "false", message = "Image not found" });
            }
            return File(image, "image/jpeg");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(string id)
        {
            var removeBooking = _bookingRepo.findBookingWithId(id);

            if (removeBooking == null)
            {
                return StatusCode(400, new { status = false, message = "booking not found" });
            }
            else
            {
                await _bookingRepo.RemoveBooking(id);
                return StatusCode(200, new { status = true, message = "booking has been deleted success" });
            }
        }

    }
}
