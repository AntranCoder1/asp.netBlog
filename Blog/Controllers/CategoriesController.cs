using Blog.ClassValue;
using Blog.Models;
using Blog.Repository;
using Blog.utils;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json;
using System.Text;

namespace Blog.Controllers
{
    [Route("api/categories")]
    [ApiController]
    public class CategoriesController : Controller
    {
        private readonly CategoriesRepo _categoriesRepo;
        private IConfiguration _configuration;

        public CategoriesController(CategoriesRepo categoriesRepo, IConfiguration configuration)
        {
            _categoriesRepo = categoriesRepo;
            _configuration = configuration;
        }

        [HttpPost("createCategory")]
        public async Task<IActionResult> CreateCategory()
        {
            try
            {
                string rawContent = string.Empty;
                using (var reader = new StreamReader(Request.Body, encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false))
                {
                    rawContent = await reader.ReadToEndAsync();
                }

                CategoryValue categoryValue = JsonConvert.DeserializeObject<CategoryValue>(rawContent);

                CategoryModel category = new CategoryModel
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Name = categoryValue.name,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                if (string.IsNullOrEmpty(categoryValue.parentId))
                {
                    category.Ancestors = new List<Ancestor>();
                }
                else
                {
                    // check parent
                    var checkParent = await _categoriesRepo.findParent(categoryValue.parentId);

                    if (checkParent == null)
                    {
                        return StatusCode(404, new { status = false, message = "Parent category not found" });
                    }

                    category.Parent = checkParent.Id;
                    category.Ancestors = checkParent.Ancestors ?? new List<Ancestor>();
                    category.Ancestors.Add(new Ancestor { Id = checkParent.Id, Name = checkParent.Name });
                }

                await _categoriesRepo.createCategory(category);

                return StatusCode(201, new { status = true, data = new { _id = category.Id } });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while create category {ex}");

                return StatusCode(500, new { status = false, message = $"An error occurred while create category {ex}" });
            }
        }

        [HttpGet("getCategories")]
        public async Task<ActionResult> GetCategories()
        {
            try
            {
                var getCategories = await _categoriesRepo.getCategories();

                if (getCategories.Count > 0)
                {
                    List<ItemCategory> categoryValues = new List<ItemCategory>();

                    foreach (var item in getCategories)
                    {
                        ItemCategory categoryValue = new ItemCategory
                        {
                            Id = item.Id,
                            Name = item.Name,
                            Parent = item.Parent,
                            CreatedAt = item.CreatedAt,
                            UpdatedAt = item.UpdatedAt,
                            Children = new List<ItemCategory>()
                        };

                        categoryValues.Add(categoryValue);
                    }

                    var convertData = Category.ConvertToHierarchy(categoryValues);

                    return StatusCode(200, new { status = true, data = convertData });

                }
                else
                {
                    return StatusCode(200, new { status = true, data = new int[] { } });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while get categories {ex}");

                return StatusCode(500, new { status = false, message = $"An error occurred while get categories {ex}" });
            }
        }


    }
}
