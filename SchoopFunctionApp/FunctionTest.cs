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

namespace SchoopFunctionApp
{
    public static class FunctionTest
    {
        private static DataServices _dataServices;

        [FunctionName("FunctionTest")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }

        [FunctionName("GetSchool")]
        public static async Task<object> GetSchool(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                string schoopID = req.Query["schoopID"];

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                schoopID = schoopID ?? data?.schoopID;

                int schId = 0;
                var strJSON = new List<School> { };
                if (int.TryParse(schoopID, out schId))
                {
                    if (schId > 0)
                    {
                        if (_dataServices == null)
                        {
                            _dataServices = new DataServices();
                        }
                        var school = _dataServices.GetSchoolByID(schId);
                        strJSON.Add(school);
                    }
                    return strJSON.ToArray();
                }
            }
            catch (Exception ex) {
                log.LogError(ex.Message);
            }

            return setErrorCode(0);
        }


        [FunctionName("GetSchoolV2")]
        public static async Task<object> GetSchoolV2(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string schoopID = req.Query["schoopID"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            schoopID = schoopID ?? data?.schoopID;

            int schId = 0;
            var strJSON = new List<School> { };
            if (int.TryParse(schoopID, out schId))
            {
                if (schId > 0)
                {
                    if (_dataServices == null)
                    {
                        _dataServices = new DataServices();
                    }
                    var school = _dataServices.GetSchoolByID(schId);
                    strJSON.Add(school);
                }
                return strJSON.ToArray();
            }

            return setErrorCode(0);
        }


        #region public functions

        public static object setErrorCode(int errID)
        {
            List<ErrResult> strResult = new List<ErrResult>();

            switch (errID)
            {
                case 0:
                    // Schoop ID not found
                    strResult.Add(new ErrResult("0"));
                    return strResult.ToArray();
                case 1:
                    // General exception
                    strResult.Add(new ErrResult("0"));
                    return strResult.ToArray();
                case 2:
                    // NaN
                    strResult.Add(new ErrResult("fucked on 255"));
                    return strResult.ToArray();
                case 3:
                    // Success
                    strResult.Add(new ErrResult("1"));
                    return strResult.ToArray();
                case 5:
                    // Fucked
                    strResult.Add(new ErrResult("5"));
                    return strResult.ToArray();
                case 6:
                    // No new news
                    strResult.Add(new ErrResult("6"));
                    return strResult.ToArray();
                case 7:
                    // if there is an error or the alert no longer exists.if there are no alerts > alert_id posted
                    strResult.Add(new ErrResult("7"));
                    return strResult.ToArray();
                default:
                    strResult.Add(new ErrResult("0"));
                    return strResult.ToArray();
            }
        }

        //=========================================
        //Return the device ID as a JSON object
        //=========================================

        public static Object returnGUID(string dID)
        {
            var deviceIDs = new List<clsDeviceID>();
            deviceIDs.Add(new clsDeviceID("1", dID));
            return deviceIDs.ToArray();
        }

        #endregion
    }
}
