namespace MyBookShop.Services.Gateway.Zarinpal
{
    public class ZarinpalSettings
    {
        public string MerchantId { get; set; } = null!;
        public string Currency { get; set; } = null!;
        public bool IsSandbox { get; set; }
    }
}
