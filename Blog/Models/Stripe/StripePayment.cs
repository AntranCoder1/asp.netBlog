namespace Blog.Models.Stripe
{
    public record StripePayment(
        string CustomerId,
        string ReCeiptEmail,
        string Description,
        string Currency,
        long Amount,
        string PaymentId);
}
