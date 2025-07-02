using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.BLL.Services
{
    public class ImageService
    {
        private static readonly Cloudinary cloudinary;
        static ImageService()
        {
            var CLOUDINARY_URL = "cloudinary://237471339167694:_QjVXu2VPxtxSoOZPKq3Hw8SvzU@dxeadj7wo";
            cloudinary = new Cloudinary(CLOUDINARY_URL);
            cloudinary.Api.Secure = true;
        }

        public static string UploadTemplateImage(IFormFile image, string? existingUrl = null)
        {
            return UploadImage(image, "Templates/", existingUrl);
        }
        public static string UploadAnswerImage(IFormFile image, string? existingUrl = null)
        {
            return UploadImage(image, "Answers/", existingUrl);
        }

        public static string UploadImage(IFormFile image,string imageclass , string? existingUrl = null)
        {
            string publicId;
            if (string.IsNullOrEmpty(existingUrl))
            {
                publicId = imageclass + Guid.NewGuid().ToString();
            }
            else
            {
                var urlParts = existingUrl.Split("/");
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(urlParts.Last());
                publicId = urlParts[urlParts.Length - 2] + "/" + fileNameWithoutExtension;
            }
            using (var imageStream = image.OpenReadStream())
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(image.FileName, imageStream),
                    PublicId = publicId,
                    UseFilename = false,
                    //UniqueFilename = true,
                    Overwrite = true
                };
                var uploadResult = cloudinary.Upload(uploadParams);
                if (uploadResult.Error != null) return string.Empty;
                return uploadResult.Url.ToString();
            }
        }
        public static bool DeleteImage(string imageUrl)
        {
            try
            {

                var uri = new Uri(imageUrl);
                var pathSegments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pathSegments.Last());
                var folder = pathSegments.Length >= 3 ? pathSegments[pathSegments.Length - 2] : string.Empty;
                var publicId = string.IsNullOrEmpty(folder) ? fileNameWithoutExtension : $"{folder}/{fileNameWithoutExtension}";

                var deleteParams = new DeletionParams(publicId)
                {
                    ResourceType = ResourceType.Image
                };

                var deleteResult = cloudinary.Destroy(deleteParams);
                return deleteResult.Result == "ok";
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
