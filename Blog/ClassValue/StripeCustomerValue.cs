namespace Blog.ClassValue
{
    public class StripeCustomerValue
    {
        public string Email { get; set; }
        public string Name { get; set; }
        StripeCardValue StripeCard { get; set; }
    }
}
