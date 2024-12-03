using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;

namespace ClassroomAPI.Services
{
    public class FileService
    {
        private readonly BlobContainerClient _blobContainerClient;

        public FileService(IConfiguration configuration)
        {
            var connectionString = configuration["AZURE_STORAGE_CONNECTION_STRING"];
            var containerName = configuration["AZURE_STORAGE_CONTAINER_NAME"];
            _blobContainerClient = new BlobContainerClient(connectionString, containerName);
            _blobContainerClient.CreateIfNotExists(PublicAccessType.Blob);
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            var blobClient = _blobContainerClient.GetBlobClient(file.FileName);
            await using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
            return blobClient.Uri.ToString();
        }
    }
}
