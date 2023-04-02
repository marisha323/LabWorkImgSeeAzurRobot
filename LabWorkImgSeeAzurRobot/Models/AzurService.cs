using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace LabWorkImgSeeAzurRobot.Models
{
    public class AzurService
    {
        private static string path1 = Environment.GetEnvironmentVariable("PATH_AZUR_KEY");

        private static BlobServiceClient blobServiceClient;


        public static async Task ImgPass(string fileName)
        {

          
        }

        public static async Task DownloadFile(string fileName)
        {
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("imgseerobot");
            BlobClient blobClient = containerClient.GetBlobClient(fileName);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                await blobClient.DownloadToAsync(memoryStream);
                byte[] bytes = memoryStream.ToArray();
                //await File.WriteAllBytesAsync($"wwwroot/img/{fileName}", bytes);
            }
        }

        public static async Task UploadFile(IFormFile file)
        {
            blobServiceClient = new BlobServiceClient(path1);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("imgseerobot");
            BlobClient blobClient = containerClient.GetBlobClient(file.FileName);

            using (Stream stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
            }

        }

        public static async Task<bool> TryCreateBlobContainer(string containerName)
        {
            try
            {
                blobServiceClient = new BlobServiceClient(path1);
                BlobContainerClient blobClient = await blobServiceClient.CreateBlobContainerAsync(containerName);
                return true;
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                return false;
            }
        }

    }
}
