namespace MyBookShop.Services.Gateway.Zarinpal
{
    public class ZarinpalRequestResult
    {
        public Data? data { get; set; }
        public object? errors { get; set; }

        public class Data
        {
            public int code { get; set; }
            public string message { get; set; } = null!;
            public string authority { get; set; } = null!;
            public string fee_type { get; set; } = null!;
            public int fee { get; set; }
        }
    }
}
