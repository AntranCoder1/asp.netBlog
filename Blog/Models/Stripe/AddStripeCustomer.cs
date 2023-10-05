namespace Blog.Models.Stripe
{
    public record AddStripeCustomer(
        string Email,
        string Name,
        AddStripeCard CreditCard);
}
