namespace Blog.ClassValue
{
    public class BookingValue
    {
        public string title { get; set; }
        public string description { get; set; }
        public List<string> img { get; set; }
        public string type { get; set; }
        public string thumbnail { get; set; }
        public float price { get; set; }
        public float price_cupon { get; set; }
        public string address { get; set; }
        public int quantity_open { get; set; }
        public int quantity_close { get; set; }
        public DateTime? createdAt { get; set; }
        public DateTime? updatedAt { get; set; }
    }
}
