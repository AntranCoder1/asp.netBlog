namespace Blog.ClassValue
{
    public class ItemCategory
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Parent { get; set; }
        public List<ItemCategory> Children { get; set; }
        public List<ItemCategory> Ancestors { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
