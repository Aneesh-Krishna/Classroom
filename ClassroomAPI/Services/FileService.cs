using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ClassroomAPI.Services
{
    public class FileService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public FileService(IConfiguration configuration)
        {
            _bucketName = configuration["AWS:S3BucketName"];
            _s3Client = new AmazonS3Client(configuration["AWS:AccessKey"], configuration["AWS:SecretKey"], Amazon.RegionEndpoint.GetBySystemName(configuration["AWS:Region"]));
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            var fileName = file.FileName;
            var contentType = file.ContentType;

            using (var fileStream = file.OpenReadStream())
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = fileName,
                    InputStream = fileStream,
                    ContentType = contentType
                };

                await _s3Client.PutObjectAsync(putRequest);
            }

            return $"https://{_bucketName}.s3.amazonaws.com/{fileName}";
        }

        public async Task<Stream> DownloadFileAsync(string fileName)
        {
            try
            {
                var getRequest = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = fileName
                };

                using (var response = await _s3Client.GetObjectAsync(getRequest))
                {
                    var memoryStream = new MemoryStream();
                    await response.ResponseStream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;
                    return memoryStream;
                }
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"Error encountered on server. Message:'{ex.Message}'");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unknown error encountered. Message:'{ex.Message}'");
                throw;
            }
        }
    }
}
