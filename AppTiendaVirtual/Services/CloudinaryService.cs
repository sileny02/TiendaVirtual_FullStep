using CloudinaryDotNet;
using CloudinaryDotNet.Actions;


namespace AppTiendaVirtual.Services
    {
        public class CloudinaryService
        {
            private readonly Cloudinary _cloudinary;

            public CloudinaryService(IConfiguration config)
            {
                var account = new Account(
                    config["Cloudinary:CloudName"],
                    config["Cloudinary:ApiKey"],
                    config["Cloudinary:ApiSecret"]

                );
                _cloudinary = new Cloudinary(account);
                _cloudinary.Api.Secure = true;
            }

            public async Task<ImageUploadResult> SubirImagenAsync(IFormFile archivo)
            {
                using var stream = archivo.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(archivo.FileName, stream),
                    Folder = "calzado",
                    Transformation = new Transformation()
                        .Width(800).Height(800).Crop("limit")
                        .Quality("auto").FetchFormat("auto")
                };
                return await _cloudinary.UploadAsync(uploadParams);
            }

            public async Task EliminarImagenAsync(string publicId)
            {
                var deleteParams = new DeletionParams(publicId);
                await _cloudinary.DestroyAsync(deleteParams);
            }
        }
    }


