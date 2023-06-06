using Blog.ClassValue;
using Blog.Models;
using Blog.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Blog.Controllers
{
    [Route("api/comment")]
    [ApiController]
    [Authorize]
    public class CommentController : Controller
    {
        private readonly PostRepo _postRepo;
        private readonly UserRepo _userRepo;
        private readonly CommentRepo _commentRepo;
        private IConfiguration _configuration;

        public CommentController(PostRepo postRepo, IConfiguration configuration, UserRepo userRepo, CommentRepo commentRepo)
        {
            _postRepo = postRepo;
            _configuration = configuration;
            _userRepo = userRepo;
            _commentRepo = commentRepo;
        }

        [HttpGet("getComments/{limit}/{page}")]
        public async Task<ActionResult> GetComments(string limit, string page)
        {
            var Limit = int.Parse(limit);
            var Page = int.Parse(page);

            var comments = await _commentRepo.getComments(Limit, Page);

            if (comments.Count > 0)
            {
                var convertedComments = comments.Select(p => new
                {
                    Id = p.Id.ToString(),
                    UserId = p.UserId.ToString(),
                    PostId = p.PostId.ToString(),
                    Comment = p.Comment,
                    ParentComment = p.ParentComment.ToString(),
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList();

                return Ok(new { status = "success", data = convertedComments });
            }
            else
            {
                return Ok(new { status = true, data = new int[] { } });
            }
        }

        [HttpPost("createComment")]
        public async Task<IActionResult> CreateComment([FromHeader(Name = "Authorization")] string token)
        {
            try
            {
                string rawContent = string.Empty;
                using (var reader = new StreamReader(Request.Body, encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false))
                {
                    rawContent = await reader.ReadToEndAsync();
                }

                CommentValue commentValue = JsonConvert.DeserializeObject<CommentValue>(rawContent);

                var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token.Replace("Bearer ", string.Empty));
                var userId = jwtToken.Claims.First(claim => claim.Type == "Id").Value;

                var checkParentCommentExists = await _commentRepo.findParentCommemt(commentValue.postId.ToString());

                if (checkParentCommentExists != null)
                {
                    //Guid myuuid = Guid.NewGuid();

                    //var parentComment = new[]
                    //{
                    //    new CommentParent { Id = myuuid.ToString(), CommentId = checkParentCommentExists.Id, UserId = new ObjectId(userId), Comment = commentValue.comment }
                    //};

                    if (checkParentCommentExists.ParentComment == null)
                    {
                        CommentModel commen = new CommentModel
                        {
                            UserId = new ObjectId(userId),
                            PostId = ObjectId.Parse(commentValue.postId),
                            Comment = commentValue.comment,
                            ParentComment = checkParentCommentExists.Id
                        };

                        //await _commentRepo.updateParentComment(checkParentCommentExists.Id.ToString(), commen);
                        await _commentRepo.createComment(commen);
                    }
                    else
                    {
                        //var parentComments = checkParentCommentExists.ParentComment.Concat(parentComment).ToArray();

                        //CommentModel commen = new CommentModel
                        //{
                        //    ParentComment = new ObjectId(commentValue.parentCommentId)
                        //};

                        //await _commentRepo.updateParentComment(checkParentCommentExists.Id.ToString(), commen);
                        CommentModel commen = new CommentModel
                        {
                            UserId = new ObjectId(userId),
                            PostId = ObjectId.Parse(commentValue.postId),
                            Comment = commentValue.comment,
                            ParentComment = checkParentCommentExists.Id
                        };

                        //await _commentRepo.updateParentComment(checkParentCommentExists.Id.ToString(), commen);
                        await _commentRepo.createComment(commen);
                    }

                }
                else
                {
                    var comment = new CommentModel
                    {
                        UserId = new ObjectId(userId),
                        PostId = ObjectId.Parse(commentValue.postId),
                        Comment = commentValue.comment,
                    };

                    await _commentRepo.createComment(comment);
                }

                return Ok(new { status = true, message = "Create comment success" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while creating the comment: {ex}");

                return StatusCode(500, new { status = false, message = "An error occurred while creating the comment." });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(string id, [FromHeader(Name = "Authorization")] string token)
        {
            try
            {
                string rawContent = string.Empty;
                using (var reader = new StreamReader(Request.Body, encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false))
                {
                    rawContent = await reader.ReadToEndAsync();
                }

                CommentValue commentValue = JsonConvert.DeserializeObject<CommentValue>(rawContent);

                var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token.Replace("Bearer ", string.Empty));
                var userId = jwtToken.Claims.First(claim => claim.Type == "Id").Value;

                var checkCommentUser = await _commentRepo.GetCommentWithUserId(userId.ToString(), commentValue.postId);

                if (checkCommentUser == null)
                {
                    return BadRequest(new { status = false, message = "This comment is not yours" });
                }
                else
                {
                    CommentModel updateComment = new CommentModel
                    {
                        UserId = new ObjectId(userId),
                        PostId = ObjectId.Parse(commentValue.postId),
                        Comment = commentValue.comment,
                    };

                    await _commentRepo.updateComment(id, updateComment);

                    return Ok(new { status = true, message = "update comment success" });

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while update the comment: {ex}");

                return StatusCode(500, new { status = false, message = "An error occurred while update the comment." });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(string id, [FromHeader(Name = "Authorization")] string token)
        {
            try
            {
                var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token.Replace("Bearer ", string.Empty));
                var userId = jwtToken.Claims.First(claim => claim.Type == "Id").Value;

                var checkCommentUser = await _commentRepo.GetCommentWithIdAndUserId(id, userId);

                if (checkCommentUser == null)
                {
                    return BadRequest(new { status = false, message = "This comment is not yours" });
                }
                else
                {
                    await _commentRepo.RemoveComment(id);

                    return Ok(new { status = true, message = "delete comment success" });

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while delete the comment: {ex}");

                return StatusCode(500, new { status = false, message = "An error occurred while delete the comment." });
            }
        }

        //[HttpPut("updateChilComment/{id}")]
        //public async Task<IActionResult> UpdateChilComment(string id, [FromHeader(Name = "Authorization")] string token)
        //{
        //    try
        //    {
        //        string rawContent = string.Empty;
        //        using (var reader = new StreamReader(Request.Body, encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false))
        //        {
        //            rawContent = await reader.ReadToEndAsync();
        //        }

        //        CommentValue commentValue = JsonConvert.DeserializeObject<CommentValue>(rawContent);

        //        var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token.Replace("Bearer ", string.Empty));
        //        var userId = jwtToken.Claims.First(claim => claim.Type == "Id").Value;

        //        var checkCommentChil = await _commentRepo.GetCommentChil(id, userId);

        //        if (checkCommentChil == null)
        //        {
        //            return StatusCode(400, new { status = false });
        //        }
        //        else
        //        {
        //            await _commentRepo.UpdateCommentChil(userId, id, commentValue.comment, checkCommentChil.Id.ToString());

        //            return StatusCode(200, new { status = true, message = "update comment success" });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"An error occurred while updating the comment: {ex}");

        //        return StatusCode(500, new { status = false, message = "An error occurred while updating the comment." });
        //    }
        //}

        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteCommentChil(string id, [FromHeader(Name = "Authorization")] string token)
        //{
        //    try
        //    {
        //        var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token.Replace("Bearer ", string.Empty));
        //        var userId = jwtToken.Claims.First(claim => claim.Type == "Id").Value;

        //        var checkCommentChil = await _commentRepo.GetCommentChil(id, userId);

        //        if (checkCommentChil == null)
        //        {
        //            return StatusCode(400, new { status = false });
        //        }
        //        else
        //        {
        //            await _commentRepo.DeleteCommentChil(id, userId);

        //            return StatusCode(200, new { status = true, message = "delete commentChil success" });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"An error occurred while delete the comment: {ex}");

        //        return StatusCode(500, new { status = false, message = "An error occurred while delete the comment." });
        //    }
        //}
    }
}
