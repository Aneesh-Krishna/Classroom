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
            // Retrieve the connection string and container name from configuration
            var connectionString = configuration["AzureBlobStorage:ConnectionString"];
            _containerName = configuration["AzureBlobStorage:ContainerName"];

            // Initialize BlobServiceClient with the connection string
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
