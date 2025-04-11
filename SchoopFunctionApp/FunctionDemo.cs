using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SchoopFunctionApp.Models;
using System.Collections.Generic;
using SchoopFunctionApp.Services;
using SchoopFunctionApp.Entities;
using System.Text.RegularExpressions;
using System.Threading;
using System.Linq;
using System.Text;
using static Azure.Core.HttpHeader;
using SchoopFunctionApp.Extensions;
using Microsoft.IdentityModel.Protocols;
using Azure.Storage.Blobs;
using System.IO.Compression;
using Azure;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Blobs.Models;
using Newtonsoft.Json.Linq;
using System.Net.Http.Json;
using System.Text.Json;

namespace SchoopFunctionApp
{
    

    public static class FunctionDemo
    {
        private static string _containerName = "schoopdata";
        private static string _blobName = "LatLongData.json";

        [FunctionName("StoreLatLong")]
        public static async Task<IActionResult> StoreLatLong(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string id = req.Query["id"];
            string lat = req.Query["lat"];
            string longitude = req.Query["long"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            id = id ?? data?.id;
            lat = lat ?? data?.lat;
            longitude = longitude ?? data?.longitude;

            var device = new DemoDevice { ID=id, Latitude = lat, Longitude = longitude };

            await DownloadAndRewriteJsonToBlobAsync(device);

            return new OkObjectResult("Data have been posted to blob successfully.");
        }

        [FunctionName("LoadJsonFile")]
        public static async Task<IActionResult> LoadJsonFile(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                // 使用Azure Functions的环境变量
                var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorageLatLong");

                var blobServiceClient = new BlobServiceClient(connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient(_blobName);

                var stream = await blobClient.DownloadStreamingAsync();

                using (StreamReader reader = new StreamReader(stream.Value.Content))
                {
                    string jsonContent = await reader.ReadToEndAsync();
                    dynamic configData = JsonConvert.DeserializeObject(jsonContent);

                    return new OkObjectResult(configData);
                }
            }
            catch (Exception ex)
            {
                log.LogError($"Error reading JSON blob: {ex.Message}");
                return new BadRequestObjectResult("Error processing the request");
            }
        }

        [FunctionName("DeleteLatLongById")]
        public static async Task<IActionResult> DeleteLatLongById(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string id = req.Query["id"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            id = id ?? data?.id;

            if (string.IsNullOrEmpty(id))
            {
                return new OkObjectResult("ID is null, nothing to be deleted.");
            }

            await DeleteByIdAndRewriteJsonToBlobAsync(id);

            return new OkObjectResult("Device ID - " + id + " has been deleted from blob successfully.");
        }

        public static async Task AppendJsonToBlobAsync(DemoDevice device)
        {
            // 使用Azure Functions的环境变量
            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorageLatLong");

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            //var blobClient = containerClient.GetBlobClient(_blobName);

            // 序列化对象为JSON
            string jsonContent = JsonConvert.SerializeObject(device, Formatting.Indented);

            // 创建或获取Append Blob
            var appendBlobClient = containerClient.GetAppendBlobClient(_blobName);
            if (!await appendBlobClient.ExistsAsync())
            {
                await appendBlobClient.CreateAsync();
            }

            // 追加内容（自动换行）
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent + Environment.NewLine)))
            {
                await appendBlobClient.AppendBlockAsync(stream);
            }
        }

        public static async Task AppendJsonObjectWithConcurrencyControlAsync(DemoDevice device)
        {
            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorageLatLong");

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(_blobName);
            BlobDownloadResult downloadResult = null;
            JToken jsonData;

            // max retry 3 times
            int retryCount = 0;
            const int maxRetries = 3;

            while (retryCount < maxRetries)
            {
                try
                {
                    if (await blobClient.ExistsAsync())
                    {
                        downloadResult = await blobClient.DownloadContentAsync();
                        using (var reader = new StreamReader(downloadResult.Content.ToStream()))
                        {
                            jsonData = JToken.Parse(await reader.ReadToEndAsync());
                        }
                    }
                    else
                    {
                        jsonData = new JArray();
                    }

                    // 2. add new data to json
                    if (jsonData is JArray array)
                    {
                        array.Add(JObject.FromObject(device));
                    }

                    // 3. upload back to blob
                    var conditions = downloadResult != null ? new BlobRequestConditions { IfMatch = downloadResult.Details.ETag } : null;

                    using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonData.ToString(Formatting.Indented))))
                    {
                        await blobClient.UploadAsync(stream, conditions: conditions);
                        return; // success break
                    }
                }
                catch (RequestFailedException ex)  // ETag missmatch
                {
                        retryCount++;
                        if (retryCount >= maxRetries)
                        {
                            throw new Exception("Concurrent modification conflict, operation failed!" + ex.Message);
                        }
                        await Task.Delay(100 * retryCount); // exponential backoff
                }
            }
        }

        public static async Task WriteJsonToBlobAsync(DemoDevice device)
        {
            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorageLatLong");

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(_blobName);

            // 序列化对象为JSON
            string jsonContent = JsonConvert.SerializeObject(device, Formatting.Indented);

            // 上传到Blob
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent)))
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }
        }

        public static async Task DownloadAndRewriteJsonToBlobAsync(DemoDevice device)
        {
            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorageLatLong");

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(_blobName);

            var stream = await blobClient.DownloadStreamingAsync();
            string jsonContent = "";

            using (StreamReader reader = new StreamReader(stream.Value.Content))
            {
                jsonContent = await reader.ReadToEndAsync();

                var jsonData = JToken.Parse(jsonContent);

                // 2. add new data to json
                if (jsonData is JArray array)
                {
                    array.Add(JObject.FromObject(device));
                    
                    jsonContent = JsonConvert.SerializeObject(array, Formatting.Indented);
                }
            }

            if(!string.IsNullOrEmpty(jsonContent))
            {
                // 上传到Blob
                using (var newstream = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent)))
                {
                    await blobClient.UploadAsync(newstream, overwrite: true);
                }
            }
            
        }

        public static async Task DeleteByIdAndRewriteJsonToBlobAsync(string deviceId)
        {
            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorageLatLong");

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(_blobName);

            var stream = await blobClient.DownloadStreamingAsync();
            string jsonContent = "";

            using (StreamReader reader = new StreamReader(stream.Value.Content))
            {
                jsonContent = await reader.ReadToEndAsync();

                // 反序列化JSON数组
                var devices = JsonConvert.DeserializeObject<List<DemoDevice>>(jsonContent);

                // remove the objects with ID
                devices.RemoveAll(p => p.ID == deviceId);

                // 重新序列化为 JSON
                jsonContent = JsonConvert.SerializeObject(devices, Formatting.Indented);

            }

            if (!string.IsNullOrEmpty(jsonContent))
            {
                // 上传到Blob
                using (var newstream = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent)))
                {
                    await blobClient.UploadAsync(newstream, overwrite: true);
                }
            }

        }

        public class DemoDevice
        {
            public string ID { get; set; }
            public string Latitude { get; set; }
            public string Longitude { get; set; }

        }
    }
}
