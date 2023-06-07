using Blog.ClassValue;
using Blog.Models;
using Blog.Repository;
using Blog.utils;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Blog.Controllers
{
    [Route("api/post")]
    [ApiController]
    //[Authorize]
    public class PostController : Controller
    {
        private readonly PostRepo _postRepo;
        private readonly UserRepo _userRepo;
        private readonly LikeRepo _likeRepo;
        private IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PostController(PostRepo postRepo, IConfiguration configuration, UserRepo userRepo, IWebHostEnvironment webHostEnvironment, LikeRepo likeRepo)
        {
            _postRepo = postRepo;
            _configuration = configuration;
            _userRepo = userRepo;
            _webHostEnvironment = webHostEnvironment;
            _likeRepo = likeRepo;
        }

        [HttpPost("createPost")]
        public async Task<IActionResult> createNewPost([FromHeader(Name = "Authorization")] string token)
        {
            try
            {
                string rawContent = string.Empty;
                using (var reader = new StreamReader(Request.Body,
                              encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false))
                {
                    rawContent = await reader.ReadToEndAsync();
                }

                PostValue post = JsonConvert.DeserializeObject<PostValue>(rawContent);

                var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token.Replace("Bearer ", string.Empty));

                var userId = jwtToken.Claims.First(claim => claim.Type == "Id").Value;

                PostModel postModel = new PostModel
                {
                    Title = post.Title,
                    Description = post.Description,
                    View = post.View ?? 0,
                    Like = post.Like ?? 0,
                    Image = post.Image,
                    UserId = ObjectId.Parse(userId)
                };

                await _postRepo.createPost(postModel);

                return StatusCode(201, new { status = true });

            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine($"An error occurred while creating the post: {ex}");

                // Return an error response
                return StatusCode(500, new { status = false, message = "An error occurred while creating the post." });
            }
        }

        [HttpGet("getPosts/{limit}/{page}")]
        public async Task<ActionResult> GetPosts(string limit, string page)
        {
            if (Request.Cookies.TryGetValue("AuthToken", out string AuthToken))
            {

                var Limit = int.Parse(limit);
                var Page = int.Parse(page);

                var posts = await _postRepo.GetPostsPagination(Limit, Page);

                if (posts.Count > 0)
                {
                    var convertedPosts = posts.Select(p => new
                    {
                        Id = p.Id.ToString(),
                        title = p.Title,
                        description = p.Description,
                        view = p.View,
                        like = p.Like,
                        image = p.Image,
                        UserId = p.UserId.ToString(),
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt
                    }).ToList();

                    return Ok(new { status = "success", data = convertedPosts });
                }
                else
                {
                    return Ok(new { status = true, data = new int[] { } });
                }
            }
            else
            {
                return BadRequest(new { status = false, message = "Unauthorized access" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPost(string id)
        {
            var post = await _postRepo.GetPost(id);

            if (post == null)
            {
                return NotFound(new { status = false, message = "post not found" });
            }
            else
            {
                var convertedPost = new
                {
                    Id = post.Id.ToString(),
                    title = post.Title,
                    description = post.Description,
                    view = post.View,
                    like = post.Like,
                    image = post.Image,
                    UserId = post.UserId.ToString(),
                    CreatedAt = post.CreatedAt,
                    UpdatedAt = post.UpdatedAt
                };

                return Ok(new { status = true, data = convertedPost });
            };
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(string id, [FromHeader(Name = "Authorization")] string token)
        {
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token.Replace("Bearer ", string.Empty));

            var userId = jwtToken.Claims.First(claim => claim.Type == "Id").Value;

            var findPost = await _postRepo.GetPost(id);

            if (findPost is null)
            {
                return NotFound();
            }

            if (String.Compare(id, findPost.UserId.ToString(), false) != 1)
            {
                return BadRequest(new { status = "failed", message = "you are not the owner of the article" });
            }

            await _postRepo.RemovePost(id);

            return Ok(new { status = true });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(string id)
        {
            string rawContent = string.Empty;
            using (var reader = new StreamReader(Request.Body,
                          encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false))
            {
                rawContent = await reader.ReadToEndAsync();
            }

            PostValue post = JsonConvert.DeserializeObject<PostValue>(rawContent);

            var findPost = await _postRepo.GetPost(id);

            if (findPost is null)
            {
                return NotFound(new { status = false, message = "post not found" });
            }

            if (String.Compare(id, findPost.UserId.ToString(), false) != 1)
            {
                return BadRequest(new { status = "failed", message = "you are not the owner of the article" });
            }

            PostModel postModel = new PostModel
            {
                Title = post.Title,
                Description = post.Description,
                View = post.View,
                Like = post.Like,
                Image = post.Image,
            };

            await _postRepo.updatePost(id, postModel);

            return Ok(new { status = true });
        }

        [HttpGet("getPostWithUserId")]
        public async Task<IActionResult> GetPostWIthUserId()
        {
            var users = await _userRepo.GetUsers();
            var posts = await _postRepo.GetPosts();

            var usersWithPosts = users.GroupJoin(
                posts,
                user => user.Id,
                post => post.UserId,
                (user, userPosts) => new
                {
                    UserId = user.Id.ToString(),
                    UserName = user.username,
                    Posts = userPosts.Select(post => new
                    {
                        PostId = post.Id.ToString(),
                        Title = post.Title,
                        Description = post.Description,
                        Like = post.Like,
                        View = post.View,
                        Image = post.Image,
                    }).ToList()
                }
            );

            return Ok(new { status = "success", data = usersWithPosts });
        }

        [HttpPost("uploadImage")]
        public async Task<IActionResult> UploadImage(IFormFile image)
        {
            var imageExtension = new ImageExtension();

            if (!imageExtension.IsImageExtension(image.FileName))
            {
                return BadRequest(new { status = "failed", message = "Invalid image type" });
            }

            string uploadsFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "Upload/Post");
            Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);

            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            string imageUrl = Url.Content("~/Upload/Post/" + uniqueFileName);

            if (imageUrl == null)
            {
                return BadRequest(new { status = "failed" });
            }

            return Ok(new { status = "success", data = imageUrl });
        }

        [HttpGet("images/{filename}")]
        public async Task<ActionResult> GetImage(string fileName)
        {
            var path = Path.Combine(_webHostEnvironment.ContentRootPath, "Upload/Post", fileName);
            var image = System.IO.File.OpenRead(path);

            if (image == null)
            {
                return NotFound(new { status = "false", message = "Image not found" });
            }
            return File(image, "image/jpeg");
        }

        [HttpPut("like/{id}")]
        public async Task<IActionResult> LikePost(string id, [FromHeader(Name = "Authorization")] string token)
        {
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token.Replace("Bearer ", string.Empty));

            var userId = jwtToken.Claims.First(claim => claim.Type == "Id").Value;

            var findLikePost = await _likeRepo.findLikeWithUsrIdAndPostId(userId, id);

            if (findLikePost == null)
            {
                await _postRepo.LikePost(id);

                LikeModel like = new LikeModel
                {
                    UserId = new ObjectId(userId),
                    PostId = new ObjectId(id),
                    Type = Enum.Parse<LikeType>("Like", true)
                };

                await _likeRepo.createLike(like);
            }
            else
            {
                if (findLikePost.Type.ToString() == "Dislike")
                {
                    await _postRepo.LikePost(id);

                    LikeModel like = new LikeModel
                    {
                        UserId = new ObjectId(userId),
                        PostId = new ObjectId(id),
                        Type = Enum.Parse<LikeType>("Like", true)
                    };

                    await _likeRepo.createLike(like);

                    await _likeRepo.RemoveLike(findLikePost.Id.ToString());
                }
                else
                {
                    await _postRepo.DislikePost(id);

                    LikeModel like = new LikeModel
                    {
                        UserId = new ObjectId(userId),
                        PostId = new ObjectId(id),
                        Type = Enum.Parse<LikeType>("Dislike", true)
                    };

                    await _likeRepo.createLike(like);

                    await _likeRepo.RemoveLike(findLikePost.Id.ToString());
                }
            }

            return Ok(new { status = true, message = "Post liked successfully." });
        }
    }
}
