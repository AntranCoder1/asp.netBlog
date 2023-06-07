using Blog.ClassValue;

namespace Blog.utils
{
    public class Category
    {
        public static List<ItemCategory> ConvertToHierarchy(List<ItemCategory> data)
        {
            var result = new List<ItemCategory>();
            var map = new Dictionary<string, ItemCategory>();

            // Tạo một từ điển với khóa là ID và giá trị là một tham chiếu đến đối tượng trong danh sách
            foreach (var item in data)
            {
                item.Children = new List<ItemCategory>();
                map[item.Id] = item;
            }

            // Duyệt qua danh sách và xây dựng cây
            foreach (var item in data)
            {
                if (!string.IsNullOrEmpty(item.Parent))
                {
                    if (map.TryGetValue(item.Parent, out var parent))
                    {
                        parent.Children.Add(item);
                    }
                }
                else
                {
                    result.Add(item);
                }
            }

            return result;
        }
    }
}
