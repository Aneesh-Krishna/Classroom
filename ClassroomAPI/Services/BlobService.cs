using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

namespace ClassroomAPI.Services
{
    public class BlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _containerClient;
        private readonly string _containerName;

        public BlobService(IConfiguration configuration)
        {
            var connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
            _containerName = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONTAINER_NAME");

            _blobServiceClient = new BlobServiceClient(connectionString);
        }


        // Method to get a reference to the blob container
        public BlobContainerClient GetBlobContainerClient()
        {
            return _blobServiceClient.GetBlobContainerClient(_containerName);
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
        {
            var blobClient = _containerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(fileStream, true);
            return blobClient.Uri.ToString(); // Returns the URL of the uploaded file
        }

    }
}
