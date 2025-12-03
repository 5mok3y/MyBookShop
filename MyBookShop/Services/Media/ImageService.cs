using MyBookShop.Models.Common;
using MyBookShop.Models.Media;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace MyBookShop.Services.Media
{
    public class ImageService(IConfiguration _configuration) : IImageService
    {
        private readonly long _maxSize = _configuration.GetValue<long>("ImageUploadOptions:MaxImageSize");
        private readonly string[] _allowedExtensions = _configuration.GetSection("ImageUploadOptions:AllowedImageExtensions").Get<string[]>()!;
        private readonly string[] _allowedMimes = _configuration.GetSection("ImageUploadOptions:AllowedImageMimes").Get<string[]>()!;
        private readonly int _imageWidth = _configuration.GetValue<int>("ImageUploadOptions:ResizedImageWidth");
        private readonly int _imageHeight = _configuration.GetValue<int>("ImageUploadOptions:ResizedImageHeight");
        private readonly string _rootPath = Directory.GetCurrentDirectory();
        private readonly string _imageFolder = _configuration["ImageUploadOptions:ImageUploadPath"]!;
        private readonly string _responsePath = _configuration["ImageUploadOptions:ImageResponsePath"]!;

        public ServiceResult<bool> DeleteImage(string relativeImagePath)
        {
            if (string.IsNullOrWhiteSpace(relativeImagePath))
            {
                return ServiceResult<bool>.Failed(new() { "Invalid image path" });
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativeImagePath.TrimStart('/'));

            if (!File.Exists(filePath))
            {
                return ServiceResult<bool>.Failed(new() { "Image not found" });
            }

            File.Delete(filePath);

            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<ImageUploadResultDto>> UploadImageAsync(IFormFile file)
        {
            if (file is null || file.Length == 0)
            {
                return ServiceResult<ImageUploadResultDto>.Failed(new() { "Image is required" });
            }

            if (file.Length > _maxSize)
            {
                return ServiceResult<ImageUploadResultDto>.Failed(new() { "Image size cannot exceed 2MB" });
            }

            if (!_allowedMimes.Contains(file.ContentType))
            {
                return ServiceResult<ImageUploadResultDto>.Failed(new() { "Invalid image MIME type" });
            }

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!_allowedExtensions.Contains(extension))
            {
                return ServiceResult<ImageUploadResultDto>.Failed(new() { "Invalid image extension" });
            }

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                var bytes = memoryStream.ToArray();

                if (!IsValidImageSignature(bytes, extension))
                {
                    return ServiceResult<ImageUploadResultDto>.Failed(new() { "Invalid or corrupted image file" });
                }
                memoryStream.Position = 0;

                string savePath = Path.Combine(_rootPath, _imageFolder);
                Directory.CreateDirectory(savePath);

                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(savePath, fileName);

                using (var image = await Image.LoadAsync(memoryStream))
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(_imageWidth,_imageHeight),
                        Mode = ResizeMode.Max
                    }));

                    await image.SaveAsync(filePath);
                }

                var response = new ImageUploadResultDto
                {
                    ImagePath = $"{_responsePath}/{fileName}"
                };

                return ServiceResult<ImageUploadResultDto>.Ok(response);
            }
        }

        // Check file signature
        private bool IsValidImageSignature(byte[] bytes, string extension)
        {
            extension = extension.ToLower();

            // JPEG
            // FF D8 FF
            if (extension is ".jpg" or ".jpeg")
            {
                return bytes.Length > 3 &&
                       bytes[0] == 0xFF &&
                       bytes[1] == 0xD8 &&
                       bytes[2] == 0xFF;
            }

            // PNG
            // 89 50 4E 47 0D 0A 1A 0A
            if (extension == ".png")
            {
                return bytes.Length > 8 &&
                       bytes[0] == 0x89 &&
                       bytes[1] == 0x50 &&
                       bytes[2] == 0x4E &&
                       bytes[3] == 0x47 &&
                       bytes[4] == 0x0D &&
                       bytes[5] == 0x0A &&
                       bytes[6] == 0x1A &&
                       bytes[7] == 0x0A;
            }

            // WEBP
            // RIFF....WEBP
            if (extension == ".webp")
            {
                return bytes.Length > 16 &&
                       bytes[0] == 0x52 && // R
                       bytes[1] == 0x49 && // I
                       bytes[2] == 0x46 && // F
                       bytes[3] == 0x46 && // F
                       bytes[8] == 0x57 && // W
                       bytes[9] == 0x45 && // E
                       bytes[10] == 0x42 && // B
                       bytes[11] == 0x50;  // P
            }

            return false;
        }
    }
}
