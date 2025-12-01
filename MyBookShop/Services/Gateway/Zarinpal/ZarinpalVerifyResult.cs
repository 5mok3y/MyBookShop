namespace MyBookShop.Services.Gateway.Zarinpal
{
    public class ZarinpalVerifyResult
    {
        public Data? data { get; set; }
        public object? errors { get; set; }

        public class Data
        {
            public int code { get; set; }
            public string message { get; set; } = null!;
            public int ref_id { get; set; }
            public string fee_type { get; set; } = null!;
            public int fee { get; set; }
            public string card_pan { get; set; } = null!;
        }
    }
}
