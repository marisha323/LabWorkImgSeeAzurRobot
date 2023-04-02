using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using LabWorkImgSeeAzurRobot.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace LabWorkImgSeeAzurRobot.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            await AzurService.TryCreateBlobContainer("imgseerobot");
            return View();
        }
     

        BlobServiceClient blobServiceClient;
        private static string path1 = Environment.GetEnvironmentVariable("PATH_AZUR_KEY");
        [HttpGet]
        public async Task<IActionResult> TestBotContent()
        {

            //var account = new CloudStorageAccount(new StorageCredentials(accountName, accountKey), true);
            //var cloudBlobClient = account.CreateCloudBlobClient();
            //var container = cloudBlobClient.GetContainerReference("container-name");
            //var blob = container.GetBlockBlobReference("image.png");
            //blob.UploadFromFile("File Path ....");//Upload file....

            //var blobUrl = blob.Uri.AbsoluteUri;     
            BlobServiceClient blobServiceClient = new BlobServiceClient(path1);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("imgseerobot");
            // Отримуємо останній Blob з контейнера
            BlobItem blobItem = containerClient.GetBlobs()
            .OrderByDescending(b => b.Properties.LastModified)
            .FirstOrDefault();

            // Отримуємо URL останньої доданої картинки
            string lastBlobUrl = containerClient.Uri + "/" + blobItem.Name;

            ViewBag.Path=lastBlobUrl;

           
            BlobClient blobClient = containerClient.GetBlobClient(lastBlobUrl);

            string endPoint = "https://seefunction.cognitiveservices.azure.com/";
            string key = "de38f3b6aa5341c28fea587309e7f24a";
            ComputerVisionClient ComputerVisionClient = new ComputerVisionClient(new ApiKeyServiceClientCredentials(key)) { Endpoint = endPoint };
            string pash_img = blobClient.Uri.ToString();
            List<VisualFeatureTypes?> featureTypes = Enum.GetValues(typeof(VisualFeatureTypes)).OfType<VisualFeatureTypes?>().ToList();
            //ImageAnalysis imageAnalysis = await ComputerVisionClient.AnalyzeImageAsync(pash_img, featureTypes);
            ImageAnalysis imageAnalysis = await ComputerVisionClient.AnalyzeImageAsync(lastBlobUrl, featureTypes);

            
            ViewBag.Analiz1 = $"A {imageAnalysis.Adult.IsAdultContent} {imageAnalysis.Adult.AdultScore}";
           
            foreach (var item in imageAnalysis.Objects)
            {
                ViewBag.Analiz2 = $"{item.ObjectProperty} : {item.Confidence} : X:{item.Rectangle.X} : Y:{item.Rectangle.Y} : W:{item.Rectangle.W} : H:{item.Rectangle.H}";
               
            }
           // await AzurService.ImgPass(lastBlobUrl);
            return View();
        }
        public async Task<IActionResult> UploadImageAsync(IFormFile? image)
        {
            string extension = Path.GetExtension(image?.FileName);

            string[] exp = { ".jpg", ".bmp", ".jpeg", ".jfif", ".webp" };

            if (Array.IndexOf(exp, extension) == -1)
            {
                return View("Index");
            }

           

            await AzurService.UploadFile(image!);

           
            await AzurService.DownloadFile(image!.FileName);

            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}