using MyBookShop.Models.Common;
using MyBookShop.Models.Media;

namespace MyBookShop.Services.Media
{
    public interface IImageService
    {
        public Task<ServiceResult<ImageUploadResultDto>> UploadImageAsync(IFormFile file);
        public ServiceResult<bool> DeleteImage(string relativeImagePath);
    }
}
