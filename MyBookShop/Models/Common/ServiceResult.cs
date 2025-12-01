namespace MyBookShop.Models.Common
{
    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public T? Response { get; set; }
        public List<string>? Errors { get; set; }


        public static ServiceResult<T> Ok(T response)
            => new() { Success = true, Response = response };

        public static ServiceResult<T> Failed(List<string> errors)
            => new() { Success = false, Errors = errors };
    }
}
