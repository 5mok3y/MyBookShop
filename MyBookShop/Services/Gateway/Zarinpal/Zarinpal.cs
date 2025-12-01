using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace MyBookShop.Services.Gateway.Zarinpal
{
    public class Zarinpal
    {

        private readonly HttpClient _httpClient;
        private readonly string _merchantId;
        private readonly bool _isSandbox;
        private readonly string _currency;

        public Zarinpal(IOptions<ZarinpalSettings> options)
        {
            var settings = options.Value;
            _httpClient = new HttpClient();
            _merchantId = settings.MerchantId;
            _currency = settings.Currency;
            _isSandbox = settings.IsSandbox;
        }

        private string BaseUrl =>
            _isSandbox ? "https://sandbox.zarinpal.com/pg/v4/payment" : "https://payment.zarinpal.com/pg/v4/payment";

        public async Task<ZarinpalRequestResult?> RequestAsync(int amount, string callbackUrl, string description, string? email, string? mobile)
        {
            var payload = new
            {
                merchant_id = _merchantId,
                amount,
                currency = _currency,
                description,
                callback_url = callbackUrl,
                metadata = new { email, mobile }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{BaseUrl}/request.json", content);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return null;

            var result = JsonSerializer.Deserialize<ZarinpalRequestResult>(json);
            return result;
        }

        public async Task<ZarinpalVerifyResult?> VerifyAsync(string authority, int amount)
        {
            var payload = new
            {
                merchant_id = _merchantId,
                amount,
                authority
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{BaseUrl}/verify.json", content);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return null;

            var result = JsonSerializer.Deserialize<ZarinpalVerifyResult>(json);
            return result;
        }

        public string GetPaymentUrl(string authority)
        {
            var baseUrl = _isSandbox
                ? "https://sandbox.zarinpal.com/pg/StartPay/"
                : "https://payment.zarinpal.com/pg/StartPay/";

            return $"{baseUrl}{authority}";
        }
    }
}
