using Blog.ClassValue;
using Blog.Models;
using Blog.Repository;
using Blog.utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;

namespace Blog.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly UserRepo _userRepo;
        private readonly LikeRepo _likeRepo;
        private readonly CommentRepo _commentRepo;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public UserController(UserRepo userRepo, LikeRepo likeRepo, CommentRepo commentRepo, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _userRepo = userRepo;
            _likeRepo = likeRepo;
            _commentRepo = commentRepo;
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("getUsers")]
        public async Task<ActionResult> GetUsers()
        {
            var users = await _userRepo.GetUsers();

            if (users.Count > 0)
            {
                return Ok(new { status = "success", data = users });
            }
            else
            {
                return Ok(new { status = true, data = new int[] { } });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register()
        {
            string rawContent = string.Empty;
            using (var reader = new StreamReader(Request.Body,
                          encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false))
            {
                rawContent = await reader.ReadToEndAsync();
            }

            UserValue user = JsonConvert.DeserializeObject<UserValue>(rawContent);

            UserModel userModel = new UserModel
            {
                username = user.username,
                email = user.email,
                password = BCrypt.Net.BCrypt.HashPassword(user.password),
                image = user.image
            };

            await _userRepo.createUser(userModel);

            return Ok(new { status = "success" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login()
        {
            string rawContent = string.Empty;
            using (var reader = new StreamReader(Request.Body,
                          encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false))
            {
                rawContent = await reader.ReadToEndAsync();
            }

            UserValue user = JsonConvert.DeserializeObject<UserValue>(rawContent);

            if (user != null && user.email != null && user.password != null)
            {
                var findUser = await _userRepo.findUserWithEmail(user.email);

                if (findUser is null)
                {
                    return NotFound(new { status = false, message = "Email Not Found" });
                }

                if (BCrypt.Net.BCrypt.Verify(user.password, findUser.password))
                {
                    var claims = new[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                        new Claim("Id", findUser.Id.ToString()),
                    };

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                    var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                    var token = new JwtSecurityToken(_configuration["Jwt:Issuer"], _configuration["Jwt:Audience"], claims, expires: DateTime.UtcNow.AddDays(1), signingCredentials: signIn);

                    return Ok(new { status = true, data = new JwtSecurityTokenHandler().WriteToken(token) });
                }
                else
                {
                    return BadRequest(new { status = false, message = "Invalid crendentials" });
                }
            }
            else
            {
                return BadRequest(new { status = false });
            }
        }

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult> GetUser(string id)
        {
            var findUser = await _userRepo.GetUser(id);

            if (findUser != null)
            {
                return Ok(new { status = true, data = findUser });
            }
            else
            {
                return NotFound(new { status = false, message = "User Not Found" });
            }
        }

        [HttpPut("{id:length(24)}")]
        [Authorize]
        public async Task<IActionResult> updateUser(string id, [FromHeader(Name = "Authorization")] string token)
        {
            string rawContent = string.Empty;
            using (var reader = new StreamReader(Request.Body,
                          encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false))
            {
                rawContent = await reader.ReadToEndAsync();
            }

            UserValue user = JsonConvert.DeserializeObject<UserValue>(rawContent);

            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token.Replace("Bearer ", string.Empty));
            var userId = jwtToken.Claims.First(claim => claim.Type == "Id").Value;

            var findUser = await _userRepo.GetUser(id);

            var chechGovermentUser = await _userRepo.getGovermentUser(userId);

            if (chechGovermentUser != null)
            {
                if (findUser is null)
                {
                    return NotFound();
                }

                UserModel userModel = new UserModel
                {
                    username = user.username,
                    email = user.email,
                    password = BCrypt.Net.BCrypt.HashPassword(user.password),
                    image = user.image
                };

                await _userRepo.updateUser(id, userModel);

                return Ok(new { status = true });
            }
            else
            {
                return StatusCode(400, new { status = false });
            }

        }

        [HttpDelete("{id:length(24)}")]
        [Authorize]
        public async Task<IActionResult> deleteUser(string id, [FromHeader(Name = "Authorization")] string token)
        {
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token.Replace("Bearer ", string.Empty));
            var userId = jwtToken.Claims.First(claim => claim.Type == "Id").Value;

            var findUser = await _userRepo.GetUser(id);

            var chechGovermentUser = await _userRepo.getGovermentUser(userId);

            if (chechGovermentUser is null)
            {
                return StatusCode(400, new { status = false });
            }
            else
            {
                if (findUser == null)
                {
                    return NotFound();
                }


                await _userRepo.RemoveUser(id);

                return Ok(new { status = true });

            }

        }

        [HttpPost("uploadImage")]
        public async Task<IActionResult> uploadImage(IFormFile image)
        {
            var imageExtension = new ImageExtension();

            if (!imageExtension.IsImageExtension(image.FileName))
            {
                return BadRequest(new { status = "failed", message = "Invalid image type" });
            }

            string uploadsFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "Upload/User");
            Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);

            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            string imageUrl = Url.Content("~/Upload/User/" + uniqueFileName);

            return Ok(new { status = "success", data = imageUrl });
        }

        [HttpGet("images/{filename}")]
        public async Task<ActionResult> getImage(string fileName)
        {
            var path = Path.Combine(_webHostEnvironment.ContentRootPath, "Upload/User", fileName);
            var image = System.IO.File.OpenRead(path);
            return File(image, "image/jpeg");
        }

        [HttpPost("verifyCode")]
        public async Task<IActionResult> VerifyCode()
        {
            try
            {
                string rawContent = string.Empty;
                using (var reader = new StreamReader(Request.Body, encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false))
                {
                    rawContent = await reader.ReadToEndAsync();
                }

                UserValue userValue = JsonConvert.DeserializeObject<UserValue>(rawContent);

                string tokenRP = await Common.EncryptPassword("resetPassword");

                string token = tokenRP.Substring(21).Replace("/", "");

                UserModel user = new UserModel
                {
                    verify_code = int.Parse(GenerateCode.VerifyCode()),
                    verify_token = token,
                };

                var checkEmailExists = await _userRepo.getUserWithEmail(userValue.email);

                if (checkEmailExists == null)
                {
                    return StatusCode(404, new { status = false, message = "user not found" });
                }
                else
                {
                    await _userRepo.updateVerify(userValue.email, user);

                    // Set the email sender, recipient, subject, and body
                    string senderEmail = "thanhantran21@gmail.com";
                    string recipientEmail = userValue.email;
                    string subject = "Verify Code";
                    string body = $"lease enter this code to change your password: {GenerateCode.VerifyCode()}";

                    // Create a MailMessage object
                    MailMessage mail = new MailMessage(senderEmail, recipientEmail, subject, body);

                    // Set the SMTP server details
                    SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
                    smtpClient.Credentials = new NetworkCredential("thanhantran21@gmail.com", "okifhhhxhjasrioe");
                    smtpClient.EnableSsl = true;

                    // Send the email
                    smtpClient.Send(mail);

                    Console.WriteLine("Email sent successfully.");

                    return StatusCode(200, new { status = true });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while verifying the user: {ex}");

                return StatusCode(500, new { status = false, message = "An error occurred while verifying the user." });
            }
        }

        [HttpPost("resetPass")]
        public async Task<IActionResult> ResetPassword()
        {
            try
            {
                string rawContent = string.Empty;
                using (var reader = new StreamReader(Request.Body, encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false))
                {
                    rawContent = await reader.ReadToEndAsync();
                }

                UserValue userValue = JsonConvert.DeserializeObject<UserValue>(rawContent);

                var checkVerify = await _userRepo.getUserWithVerifyCodeAndToken(userValue.verify, userValue.verify_token);

                if (checkVerify == null)
                {
                    return StatusCode(404, new { status = false, message = "user not found" });
                }

                if (userValue.comfirmPassword != userValue.newPassword)
                {
                    return StatusCode(400, new { status = false, message = "comfirm password and new password doesn't match" });
                }

                UserModel updateUser = new UserModel
                {
                    username = checkVerify.username,
                    email = checkVerify.email,
                    password = userValue.newPassword,
                    image = checkVerify.image,
                    verify_code = 0,
                    verify_token = ""
                };

                await _userRepo.updateUser(checkVerify.Id.ToString(), updateUser);

                await _userRepo.updateVerify(checkVerify.email, updateUser);

                return StatusCode(200, new { status = true });

            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while reset password: {ex}");

                return StatusCode(500, new { status = false, message = "An error occurred while rest password." });
            }
        }

        [HttpGet("FindUserLike")]
        [Authorize]
        public async Task<ActionResult> FindUserLike([FromHeader(Name = "Authorization")] string token)
        {
            try
            {
                var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token.Replace("Bearer ", string.Empty));
                var userId = jwtToken.Claims.First(claim => claim.Type == "Id").Value;

                var findUserLikes = await _likeRepo.GetLikedPosts(userId);

                if (findUserLikes == null)
                {
                    return StatusCode(200, new { status = true, data = new int[] { } });
                }
                else
                {
                    return StatusCode(200, new { status = true, data = findUserLikes });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while find user like: {ex}");

                return StatusCode(500, new { status = false, message = "An error occurred while find user like" });
            }
        }

        [HttpGet("FindUserComment")]
        [Authorize]
        public async Task<ActionResult> FindUserComment([FromHeader(Name = "Authorization")] string token)
        {
            try
            {
                var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token.Replace("Bearer ", string.Empty));
                var userId = jwtToken.Claims.First(claim => claim.Type == "Id").Value;

                var findUserComments = await _commentRepo.FindCommentUser(userId);

                if (findUserComments == null)
                {
                    return StatusCode(200, new { status = true, data = new int[] { } });
                }
                else
                {
                    return StatusCode(200, new { status = true, data = findUserComments });
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while find user comment: {ex}");

                return StatusCode(500, new { status = false, message = "An error occurred while find user comment" });
            }
        }
    }
}
