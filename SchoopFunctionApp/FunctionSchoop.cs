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

namespace SchoopFunctionApp
{
    public static class FunctionSchoop
    {
        private static DataServices _dataServices;
        private const int _defaultLanguageId = 1;

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
                        var school = _dataServices.GetSchoolByIDV2(schId);
                        strJSON.Add(school);
                    }
                    return strJSON.ToArray();
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }

            return setErrorCode(0);
        }


        [FunctionName("DeviceIDRegister")]
        public static async Task<object> DeviceIDRegister(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                string deviceToken = req.Query["deviceToken"];
                string deviceTypeID = req.Query["deviceTypeID"];
                string deviceOSVersion = req.Query["deviceOSVersion"];
                string languageID = req.Query["languageID"];


                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                deviceToken = deviceToken ?? data?.deviceToken;
                deviceTypeID = deviceTypeID ?? data?.deviceTypeID;
                deviceOSVersion = deviceOSVersion ?? data?.deviceOSVersion;
                languageID = languageID ?? data?.languageID;

                if (string.IsNullOrEmpty(deviceToken)) {
                    return setErrorCode(0);
                }
                string deviceIDTrimmed = deviceToken.Replace("'", "").Replace("\"", ""); //remove unsafe characters like ' or ", add on 20161026

                int typeId = 0;
                if (!int.TryParse(deviceTypeID, out typeId))
                {
                    return setErrorCode(0);
                }
                int langId = 0;
                if (!int.TryParse(languageID, out langId))
                {
                    return setErrorCode(0);
                }
                string devicePlatform = GetDevicePlatform(typeId);
                if (_dataServices == null)
                {
                    _dataServices = new DataServices();
                }

                if (deviceToken == "1111155555ppppp00000AAAAABBBBBcccccXXXXX33333nnnnnSSSSSWWWWWQQQQQ88888]PPPPPDEVICE" || deviceToken == "1111155555ppppp00000AAAAABBBBBcccccXXXXX33333nnnnnSSSSSWWWWWQQQQQ88888PPPPPDEVICE") //faked device token
                {
                    var newDeviceId = _dataServices.InsertDevice(null, typeId, deviceOSVersion, langId);

                    var activeYearsByDevice = new tbl_active_years_by_device { device_id = newDeviceId, schoop_id = 2820, active_years = "|0|" };
                    _dataServices.InsertActiveYearsByDevice(activeYearsByDevice);
                    //subscribe to group
                    var groups = new List<int>();
                    if (typeId == 1 || typeId == 5) //IOS
                    {
                        groups.Add(2573);
                    }
                    else if (typeId == 2) //Android
                    {
                        groups.Add(2574);
                    }
                    else if (typeId == 3) //Email
                    {
                        groups.Add(2575);
                    }
                    _dataServices.SetActiveGroups(newDeviceId, 2820, groups);
                    return new
                    {
                        res = 1,
                        deviceID = newDeviceId.ToString()
                    };
                }
                var objAlphaNumericPattern = new Regex("^[0-9a-zA-Z-_.,:-]{1,4096}$");

                var device = _dataServices.GetDeviceByToken(deviceIDTrimmed);
                if (null == device)
                {
                    if (objAlphaNumericPattern.IsMatch(deviceIDTrimmed.ToString()))
                    {
                        var newDeviceId = _dataServices.InsertDevice(deviceIDTrimmed, typeId, deviceOSVersion, langId);

                        var activeYearsByDevice = new tbl_active_years_by_device { device_id = newDeviceId, schoop_id = 2820, active_years = "|0|" };
                        _dataServices.InsertActiveYearsByDevice(activeYearsByDevice);
                        //subscribe to group
                        var groups = new List<int>();
                        if (typeId == 1 || typeId == 5) //IOS
                        {
                            devicePlatform = "apns";
                            groups.Add(2573);
                        }
                        else if (typeId == 2) //Android
                        {
                            devicePlatform = "gcm";
                            groups.Add(2574);
                        }
                        else if (typeId == 3) //Email
                        {
                            groups.Add(2575);
                        }
                        _dataServices.SetActiveGroups(newDeviceId, 2820, groups);


                        return new
                        {
                            res = 1,
                            deviceID = newDeviceId.ToString()
                        };
                    }
                    else
                    {
                        return setErrorCode(5);

                    }
                }
                else
                {
                    //Update tbl_devices
                    if (_dataServices.UpdateDeviceByToken(deviceToken, typeId, deviceOSVersion, langId))
                    {

                        if (typeId == 1 || typeId == 2) //only for IOS or Android
                        {
                            //RegistNHByDeviceID(device.deviceID);
                            return new
                            {
                                res = 45,
                                deviceID = device.deviceID,
                                mySchools = _dataServices.GetActiveYearByDeviceID(device.deviceID)
                            };
                        }

                    }

                    return setErrorCode(2);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }

            return setErrorCode(0);
        }

        [FunctionName("DeviceTokenUpdate")]
        public static async Task<object> DeviceTokenUpdate(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string deviceToken = req.Query["deviceToken"];
            string dId = req.Query["deviceID"];


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            deviceToken = deviceToken ?? data?.deviceToken;
            dId = dId ?? data?.deviceID;

            int deviceID = 0;
            if (!int.TryParse(dId, out deviceID)) //null device ID
            {
                return null;
            }
            try
            {
                string deviceIDTrimmed = null;
                //deviceIDTrimmed = Regex.Replace(deviceToken, "[^a-zA-Z0-9]", "").ToString();
                deviceIDTrimmed = deviceToken;
                //var objAlphaNumericPattern = new Regex("^[0-9a-zA-Z-_.,:-]{1,4096}$");
                if (_dataServices == null)
                {
                    _dataServices = new DataServices();
                }

                var device = _dataServices.GetDeviceByDeviceId(deviceID);
                if (device != null)
                {
                    //Update tbl_devices
                    if (_dataServices.UpdateDeviceTokenByDeviceID(deviceIDTrimmed, deviceID))
                    {
                        //var deviceTypeID = device.deviceTypeID;
                        if (device.deviceTypeID == 1 || device.deviceTypeID == 2) //only for IOS or Android
                        {
                            //UpdateNHRegistration(device);
                            return new
                            {
                                res = 45,
                                deviceID = device.deviceID,
                                mySchools = _dataServices.GetActiveYearByDeviceID(device.deviceID)
                            };
                        }
                        return new
                        {
                            res = 1
                        };
                    }
                }

                return setErrorCode(0);
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                if (ex.InnerException != null)
                {
                    message += "-Inner exception: " + ex.InnerException.Message;
                }
                return message;
            }
        }

        [FunctionName("DeviceTypeIDUpdate")]
        public static async Task<object> DeviceTypeIDUpdate(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        //public object DeviceTypeIDUpdate(int deviceTypeID, int? deviceID)
        {
            string deviceTypeID = req.Query["deviceTypeID"];
            string deviceID = req.Query["deviceID"];


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            deviceTypeID = deviceTypeID ?? data?.deviceTypeID;
            deviceID = deviceID ?? data?.deviceID;

            int typeId = 0;
            if (!int.TryParse(deviceTypeID, out typeId)) //null device ID
            {
                return null;
            }
            int dId = 0;
            if (!int.TryParse(deviceID, out dId)) //null device ID
            {
                return null;
            }
            try
            {
                if (_dataServices == null)
                {
                    _dataServices = new DataServices();
                }
                var device = _dataServices.GetDeviceByDeviceId(dId);
                if (device != null)
                {
                    //Update tbl_devices
                    if (_dataServices.UpdateDeviceTypeIdByDeviceID(typeId, dId))
                    {
                        return new
                        {
                            res = 1
                        };
                    }
                }

                return setErrorCode(0);
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                if (ex.InnerException != null)
                {
                    message += "-Inner exception: " + ex.InnerException.Message;
                }
                return message;
            }
        }

        [FunctionName("SetActiveYears")]
        public static async Task<object> SetActiveYears(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        //public object SetActiveYears(int? deviceID, int schoopID, string activeyears)
        {
            string deviceID = req.Query["deviceID"];
            string schoopID = req.Query["schoopID"];
            string activeyears = req.Query["activeyears"];


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            deviceID = deviceID ?? data?.deviceID;
            schoopID = schoopID ?? data?.schoopID;
            activeyears = activeyears ?? data?.activeyears;

            int dId = 0;
            if (!int.TryParse(deviceID, out dId)) //null device ID
            {
                return null;
            }
            int schId = 0;
            if (!int.TryParse(schoopID, out schId)) //null device ID
            {
                return setErrorCode(0);
            }
            try
            {
                if (_dataServices == null)
                {
                    _dataServices = new DataServices();
                }

                var device = _dataServices.GetDeviceByDeviceId(dId);
                if (device == null)
                {
                    return setErrorCode(0);
                }

                _dataServices.SetActiveYears(dId, schId, activeyears);
                var deviceTypeID = device.deviceTypeID;
                if (deviceTypeID == 1 || deviceTypeID == 2) //only for IOS or Android
                {
                    //UpdateNHRegistration(device);
                }

                if (schId == 9999)
                {
                    //send alert back to device
                    var alert = new tbl_school_alerts()
                    {
                        Activeyears = activeyears,
                        Alert_sent_by = 0,
                        Alert_text = "Schoop Engineer. Your Device ID is: " + deviceID,
                        Alert_urgent = false,
                        Schoop_id = schId,
                        Language_id = 1,
                        Alert_date_time = DateTime.Now,
                        ActiveGroups = "0",
                        ToSendTime = DateTime.Now
                    };
                    _dataServices.InsertAlert(alert);
                    
                }


                //send private alert for contact form, added on 20161018
                if (device.EmailIsAuthed == false && device.PhoneIsAuthed == false)
                {
                    var alert = new tbl_school_alerts()
                    {
                        Activeyears = "0",
                        Alert_sent_by = 0,
                        Alert_text = "Welcome to Schoop. Please provide your up to date contact details. Please click on the pencil icon to complete the form.",
                        Alert_urgent = false,
                        Schoop_id = 2820,
                        IsContactFormAlert = true,
                        Language_id = 1,
                        Alert_date_time = DateTime.Now,
                        SendByDeviceIds = true,
                        DeviceIds = deviceID.ToString(),
                        ActiveGroups = "0",
                        ToSendTime = DateTime.Now
                    };
                    _dataServices.InsertAlert(alert);
                }

                Thread.Sleep(4000);
                return setErrorCode(3);
            }

            catch (Exception ex)
            {
                if (schId == 9999)
                {
                    var alert = new tbl_school_alerts()
                    {
                        Activeyears = activeyears,
                        Alert_sent_by = 0,
                        Alert_text = ex.Message.Length > 200 ? ex.Message.Substring(0, 200) : ex.Message,
                        Alert_urgent = false,
                        Schoop_id = schId,
                        Language_id = 1,
                        Alert_date_time = DateTime.Now,
                        ToSendTime = DateTime.Now
                    };
                    _dataServices.InsertAlert(alert);
                    //SendAlertToDevice(deviceID, ex.Message.Length > 200 ? ex.Message.Substring(0, 200) : ex.Message);
                }
                // Schoop.Domain.Utils.Helper.EmailHelper.SendErrorEmail("Set Active Years or Groups", ex.Message + "<br>" + ex.StackTrace + " - deviceID: " + deviceID + " - schoopID: " + schoopID + " - activeyears: " + activeyears);
                return setErrorCode(0);
            }
        }


        [FunctionName("DeleteActiveYearsByDevice")]
        public static async Task<object> DeleteActiveYearsByDevice(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        //public object DeleteActiveYearsByDevice(int? deviceID, int schoopID)
        {
            string deviceID = req.Query["deviceID"];
            string schoopID = req.Query["schoopID"];


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            deviceID = deviceID ?? data?.deviceID;
            schoopID = schoopID ?? data?.schoopID;

            int dId = 0;
            if (!int.TryParse(deviceID, out dId)) //null device ID
            {
                return null;
            }
            int schId = 0;
            if (!int.TryParse(schoopID, out schId)) //null device ID
            {
                return setErrorCode(0);
            }
            try
            {
                if (_dataServices == null)
                {
                    _dataServices = new DataServices();
                }
                _dataServices.DeleteActiveYears(dId, schId);

                //delete the groups associated with that device
                _dataServices.DeleteActiveGroups(dId, schId);

                return new
                {
                    res = 1
                };
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                if (ex.InnerException != null)
                {
                    message += "-Inner exception: " + ex.InnerException.Message;
                }
                return setErrorCode(0);
            }
        }

        [FunctionName("GetSchoolsByTownId")]
        public static async Task<object> GetSchoolsByTownId(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        //public object GetSchoolsByTownId(int townId)
        {
            string townId = req.Query["townId"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            townId = townId ?? data?.townId;
            int tId = 0;
            if (!int.TryParse(townId, out tId)) //null device ID
            {
                return null;
            }

            try
            {
                if(_dataServices == null)
                {
                    _dataServices = new DataServices();
                }
                var schools = _dataServices.GetSchoolsByTownId(tId);
                var schoolData = new List<schoolsFromTownID> { };

                foreach (var item in schools)
                {
                    schoolData.Add(new schoolsFromTownID(item.EstablishmentName.ToString(), item.SchoopID));
                }
                return new
                {
                    res = 1,
                    data = schoolData.ToArray()
                };
            }
            catch (Exception)
            {
                return setErrorCode(0);
            }
        }

        [FunctionName("GetNews")]
        public static async Task<object> GetNews(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        //public Object GetNews(int? deviceID, int schoopID, int school_news_id, int languageID)
        {
            string deviceID = req.Query["deviceID"];
            string schoopID = req.Query["schoopID"];
            string school_news_id = req.Query["school_news_id"];
            string languageID = req.Query["languageID"];


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            deviceID = deviceID ?? data?.deviceID;
            schoopID = schoopID ?? data?.schoopID;
            school_news_id = school_news_id ?? data?.school_news_id;
            languageID = languageID ?? data?.languageID;

            int dId = 0;
            if (!int.TryParse(deviceID, out dId)) //null device ID
            {
                return setErrorCode(0);
            }
            int schId = 0;
            if (!int.TryParse(schoopID, out schId)) //null device ID
            {
                return setErrorCode(0);
            }
            int newsId = 0;
            if (!int.TryParse(school_news_id, out newsId)) //null device ID
            {
                return setErrorCode(0);
            }
            int langId = 0;
            if (!int.TryParse(languageID, out langId)) //null device ID
            {
                return setErrorCode(0);
            }
            try
            {
                if (_dataServices == null)
                {
                    _dataServices = new DataServices();
                }
                var validNews = new List<SchoolNews>();

                IList<tbl_school_news> news = null;
                //var activeYears = _activeYearsByDeviceService.GetActiveYearBySchoopIDAndDeviceID(schoopID, deviceID);
                news = _dataServices.GetNewerNews(newsId, schId, dId, langId);
                if (news.Count == 0)
                {
                    news = _dataServices.GetNewerNews(newsId, schId, dId, _defaultLanguageId);
                }
                var index = 0;
                news = news.Where(n => n.Published == true).OrderByDescending(n => n.school_news_date).Take(100).ToList();
                var dateFormat = "dd/MM/yyyy";
                
                var customFormat = _dataServices.GetSchoolDateFormat(schId);
                if (!string.IsNullOrEmpty(customFormat))
                {
                    dateFormat = customFormat;
                }
                foreach (var item in news)
                {
                    if (index++ >= 100) break; //get 100 news max 
                    validNews.Add(new SchoolNews
                    {
                        School_news_ID = item.school_news_id,
                        School_news_date = item.school_news_date.ToString(dateFormat),
                        School_news_headline = item.school_news_headline,
                        School_news_active_years = item.school_news_active_years
                    });
                }

                if (validNews.Count > 0)
                {
                    return new
                    {
                        res = 1,
                        data = validNews.ToArray()
                    };
                }
                else
                {
                    return new
                    {
                        res = 7
                    };
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                if (ex.InnerException != null)
                {
                    message += "-Inner exception: " + ex.InnerException.Message;
                }
                return new
                {
                    res = 0
                };
            }
        }

        [FunctionName("GetEvents")]
        public static async Task<object> GetEvents(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        //public object GetEvents(int? deviceID, int schoopID, int event_id, int languageID)
        {
            string deviceID = req.Query["deviceID"];
            string schoopID = req.Query["schoopID"];
            string event_id = req.Query["event_id"];
            string languageID = req.Query["languageID"];


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            deviceID = deviceID ?? data?.deviceID;
            schoopID = schoopID ?? data?.schoopID;
            event_id = event_id ?? data?.event_id;
            languageID = languageID ?? data?.languageID;

            int dId = 0;
            if (!int.TryParse(deviceID, out dId)) //null device ID
            {
                return setErrorCode(0);
            }
            int schId = 0;
            if (!int.TryParse(schoopID, out schId)) //null device ID
            {
                return setErrorCode(0);
            }
            int eventId = 0;
            if (!int.TryParse(event_id, out eventId)) //null device ID
            {
                return setErrorCode(0);
            }
            int langId = 0;
            if (!int.TryParse(languageID, out langId)) //null device ID
            {
                return setErrorCode(0);
            }
            try
            {
                if (_dataServices == null)
                {
                    _dataServices = new DataServices();
                }

                var events = _dataServices.GetNewEvents(eventId, schId, dId, langId);
                if (events.Count == 0 && _defaultLanguageId != langId)
                {
                    events = _dataServices.GetNewEvents(eventId, schId, dId, _defaultLanguageId);
                }

                if (events.Count == 0)
                {
                    return new
                    {
                        res = 7
                    };
                }

                var customFormat = _dataServices.GetSchoolDateFormat(schId);
                var timeFormat = "dd/MM/yyyy";
                if (!string.IsNullOrEmpty(customFormat))
                {
                    timeFormat = customFormat;
                }

                var validEvents = events.Select(item => new SchoolEvents
                {
                    School_Event_ID = item.Event_id,
                    School_Event_month = item.Event_start_date.ToString("MM"),
                    School_Event_title = item.Event_title,
                    School_Event_start_date = item.Event_start_date.ToString("dd/MM/yyyy"), //for todo: https://freedcamp.com/Development_YJ6/Schoop_BW3/todos/3217050/
                    School_Event_end_date = item.Event_end_date.ToString("dd/MM/yyyy"),
                    School_Event_start_time = item.Event_start_time.ToString("HH:mm"),
                    School_Event_end_time = item.Event_end_time.ToString("HH:mm"),
                    School_Event_location = item.Event_location,
                    //School_Event_contact = item.Event_contact_details,
                    School_Event_cost = (item.Event_cost.HasValue ? item.Event_cost.Value.ToString("0.00") : ""),
                    //School_Event_text = item.Event_text,
                    School_Event_active_years = item.ActiveYears,
                    School_Event_active_groups = item.ActiveGroups.ToActiveGroupNames(item.Language_id),
                    School_Event_type_ID = item.CategoryId.HasValue ? item.CategoryId.Value : 0,
                    displayDate = item.Event_start_date.ToString(timeFormat), //for todo: https://freedcamp.com/Development_YJ6/Schoop_BW3/todos/3217050/
                }).ToList();

                if (validEvents.Count == 0)
                {
                    return new
                    {
                        res = 7
                    };
                }

                return new
                {
                    res = 1,
                    sID = schoopID,
                    data = validEvents.ToArray()
                };
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                if (ex.InnerException != null)
                {
                    message += "-Inner exception: " + ex.InnerException.Message;
                }
                // Schoop.Domain.Utils.Helper.EmailHelper.SendErrorEmail("GetEvents( " + deviceID + ", " + schoopID + ", " + event_id + ", " + languageID + " )", message);
                return new
                {
                    res = 0
                };
            }
        }

        [FunctionName("GetEventByID")]
        public static async Task<object> GetEventByID(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        //public Object GetEventByID(int eventId)
        {
            string eventId = req.Query["eventId"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            eventId = eventId ?? data?.eventId;
            int eId = 0;
            if (!int.TryParse(eventId, out eId)) 
            {
                return null;
            }
            try
            {
                if (_dataServices == null)
                {
                    _dataServices = new DataServices();
                }
                
                StringBuilder eventBody = new StringBuilder();

                var schoolEvent = _dataServices.GetEventById(eId);
                if ((null != schoolEvent) && (schoolEvent.Event_active))
                {
                    var eventYears = schoolEvent.ActiveYears.ToString();
                    var eventTitle = schoolEvent.Event_title.ToString();
                    var schoolDateFormat = _dataServices.GetSchoolDateFormat(schoolEvent.SchoopID);
                    var timeZoneName = _dataServices.GetSchoolTimeZoneName(schoolEvent.SchoopID);
                    var dateFormat = "dd/MM/yyyy";
                    var timeZoneOffsetForJsonTime = 0;
                    if (!string.IsNullOrEmpty(schoolDateFormat) && !string.IsNullOrEmpty(timeZoneName))
                    {
                        dateFormat = schoolDateFormat;

                        var tz = TimeZoneInfo.Local;

                        var systemZones = TimeZoneInfo.GetSystemTimeZones();
                        var query = (from t in systemZones
                                     where t.Id == timeZoneName
                                     select t).FirstOrDefault();
                        if (query != null)
                        {
                            tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneName);
                        }
                        else
                        {
                            query = (from t in systemZones
                                     where t.DisplayName == timeZoneName
                                     select t).FirstOrDefault();
                            if (query != null)
                            {
                                tz = TimeZoneInfo.FindSystemTimeZoneById(query.Id);
                            }
                        }

                        if (tz.IsDaylightSavingTime(schoolEvent.Event_start_date))
                        {
                            timeZoneOffsetForJsonTime = 1;
                        }
                    }


                    //eventBody.Append("<h1>" +  schoolEvent.Event_title + "</h1>");
                    eventBody.Append("<div class=\"boxes\">");
                    //eventBody.Append("<p>" + schoolEvent.ActiveGroups.ToActiveGroupNames(schoolEvent.Language_id) + "</p>");
                    eventBody.Append("<p><strong>Location</strong>:" + schoolEvent.Event_location + "</p>");
                    eventBody.Append("<p><b>Date: </b>" + schoolEvent.Event_start_date.ToString(dateFormat) + " - " + schoolEvent.Event_end_date.ToString(dateFormat) + "</p>");

                    eventBody.Append("<p><strong>Time: </strong>" + schoolEvent.Event_start_time.ToString("HH:mm") + " - " + schoolEvent.Event_end_time.ToString("HH:mm") + "</p>");
                    if (schoolEvent.Event_cost.HasValue && schoolEvent.Event_cost.Value > 0)
                    {
                        eventBody.Append("<p><strong>Ticket price: </strong>&#163;" + (schoolEvent.Event_cost.HasValue ? schoolEvent.Event_cost.Value.ToString("0.00") : "Free") + "</p>");
                    }
                    eventBody.Append("<p><strong>Contact details:</strong></p><p>" + schoolEvent.Event_contact_details + "</p>");
                    eventBody.Append("</div>");
                    eventBody.Append("<div class=\"boxes\">");
                    //Custom: event type 20130325
                    //var eventType = schoolEvent.CategoryId.HasValue ? _eventTypeService.GetById(schoolEvent.CategoryId.Value) : null;
                    //eventBody.Append("<p><strong>Event category:</strong></p>");
                    //eventBody.Append("<p>" + (eventType != null ? eventType.TypeName : "") + "</p>");
                    eventBody.Append("<p><strong>About the event:</strong></p>");
                    eventBody.Append("<p>" + schoolEvent.Event_text + "</p>");
                    eventBody.Append("</div>");

                    return new
                    {
                        res = 1,
                        years = eventYears.ToString(),
                        groups = schoolEvent.ActiveGroups.ToActiveGroupNames(schoolEvent.Language_id),
                        title = eventTitle.ToString(),
                        body = eventBody.ToString(),
                        eStartDate = schoolEvent.Event_start_date.ToString(dateFormat) + " " + schoolEvent.Event_start_time.ToString("HH:mm:ss"),
                        eEndDate = schoolEvent.Event_end_date.ToString(dateFormat) + " " + schoolEvent.Event_end_time.ToString("HH:mm:ss"),
                        eLocation = schoolEvent.Event_location.ToString(),
                        eStartDate2 = schoolEvent.Event_start_time.AddHours(timeZoneOffsetForJsonTime),
                        eEndDate2 = schoolEvent.Event_end_time.AddHours(timeZoneOffsetForJsonTime),
                        //eStartDate2 = schoolEvent.Event_start_date.ToString("yyyy-MM-dd"),
                        //eEndDate2 = schoolEvent.Event_end_date.ToString("yyyy-MM-dd"),
                        //eStartTime = schoolEvent.Event_start_time.ToString("hh:mm:ss"),
                        // eEndTime = schoolEvent.Event_end_time.ToString("hh:mm:ss")
                    };
                }
                else
                {
                    return new

                    {
                        res = 0
                    };
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                if (ex.InnerException != null)
                {
                    message += "-Inner exception: " + ex.InnerException.Message;
                }
                // Schoop.Domain.Utils.Helper.EmailHelper.SendErrorEmail("GetEventByID( "  + eventId + " )", message);
            }
            return null;
        }

        [FunctionName("GetNewsArticleByID")]
        public static async Task<object> GetNewsArticleByID(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        //public object GetNewsArticleByID(int school_news_id)
        {
            string school_news_id = req.Query["school_news_id"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            school_news_id = school_news_id ?? data?.school_news_id;
            int newsId = 0;
            if (!int.TryParse(school_news_id, out newsId))
            {
                return null;
            }
            try
            {
                if (_dataServices == null)
                {
                    _dataServices = new DataServices();
                }
                var schoolNews = _dataServices.GetNewsById(newsId);
                if ((null != schoolNews) && (schoolNews.school_news_active))
                {
                    var dateFormat = "dd/MM/yyyy";
                    var schoolDateFormat = _dataServices.GetSchoolTimeZoneName(schoolNews.schoopID);
                    if(!string.IsNullOrEmpty(schoolDateFormat))
                    {
                        dateFormat = schoolDateFormat;
                    }
                    return new
                    {
                        res = 1,
                        newsyears = schoolNews.school_news_active_years,
                        newsgroups = schoolNews.ActiveGroups.ToActiveGroupNames(schoolNews.Language_id ?? 1),
                        //newsbody = "<h1>" + schoolNews.school_news_headline + "</h1>" + "<p>" + schoolNews.school_news_active_years + "<br />" + schoolNews.ActiveGroups.ToActiveGroupNames(schoolNews.Language_id??1) + "</p><p><strong>" + schoolNews.school_news_date.ToString("dd/MM/yyyy") + "</strong></p>" + "<p>" + schoolNews.school_news_article + "</p>"
                        newsbody = "<h1>" + schoolNews.school_news_headline + "</h1><p><strong>" + schoolNews.school_news_date.ToString(dateFormat) + "</strong></p>" + "<p>" + schoolNews.school_news_article + "</p>"// + googleCode

                    };
                }
                else
                {
                    return new { res = 0 };
                    //return setErrorCode(0);
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                if (ex.InnerException != null)
                {
                    message += "-Inner exception: " + ex.InnerException.Message;
                }
            }
            return null;
        }

        [FunctionName("GetNewAlerts")]
        public static async Task<object> GetNewAlerts(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        //public object GetNewAlerts(int schoop_id, int? device_id, int alert_id)
        {
            string schoop_id = req.Query["schoop_id"];
            string device_id = req.Query["device_id"];
            string alert_id = req.Query["alert_id"];


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            schoop_id = schoop_id ?? data?.schoop_id;
            device_id = device_id ?? data?.device_id;
            alert_id = alert_id ?? data?.alert_id;

            int dId = 0;
            if (!int.TryParse(device_id, out dId)) //null device ID
            {
                return new { res = 5 };
            }
            int schId = 0;
            if (!int.TryParse(schoop_id, out schId)) //null device ID
            {
                return setErrorCode(0);
            }
            int alertId = 0;
            if (!int.TryParse(alert_id, out alertId)) //null device ID
            {
                return setErrorCode(0);
            }
            
            try
            {
                if (_dataServices == null)
                {
                    _dataServices = new DataServices();
                }

                var validAlerts = new List<SchoolAlerts>();
                var device = _dataServices.GetDeviceByDeviceId(dId);
                if (device != null)
                {
                    //update device to active here
                    device.deviceActive = true;
                    //update badge number = 0, update 20141003 by liang
                    device.BadgeNumber = 0;
                    device.deviceLastAccessed = DateTime.Now;
                    _dataServices.InsertReadHistory(alertId, dId); //add alert read history, on 20150913
                    _dataServices.ResetBadgeNumberAndStatus(dId);
                }
                else
                {
                    return new { res = 7 };
                }

                //if alert_id ==0, it will return the highest alert_id record
                var alerts = _dataServices.GetLatestAlertsFromPosted(alertId, dId, schId, 20);
                var isEmpty = alerts == null || alerts.Count == 0;
                if (!isEmpty)
                {
                    foreach (var item in alerts)
                    {

                        var alertText = item.Alert_text;
                        var hasRespond = false;
                        string fid = "0";
                        if (item.SurveyGuid.HasValue) //is survey alert, add on 20141125
                        {
                            //check if device has responsed to the survey
                            hasRespond = _dataServices.CheckIfDeviceRespondSurvey(dId, item.SurveyGuid.Value);
                            
                            if (!hasRespond)
                            {
                                var surveyUrl = "https://schoop.co.uk/survey/?s=" + item.SurveyGuid.Value.ToString() + "&d=" + device.DeviceGuid.ToString();
                                fid = surveyUrl;
                                //var shortUrl = StringHelper.ShortenUrl("https://www.googleapis.com/urlshortener/v1/url", surveyUrl);
                                //cutup if length > 200
                                //if ((alertText + shortUrl).Length > 200)
                                //{
                                //    alertText = alertText.Substring(0, 200 - shortUrl.Length - 2);
                                //}
                                //alertText = alertText + " " + shortUrl;
                            }
                        }
                        if (!hasRespond)
                        {
                            var schoolDateFormat = _dataServices.GetSchoolDateFormat(item.Schoop_id);
                            var dateFormat = "dd/MM/yyyy";
                            if (!string.IsNullOrEmpty(schoolDateFormat))
                            {
                                dateFormat = schoolDateFormat;
                            }
                            var alertTime = item.Alert_date_time.HasValue ? item.Alert_date_time.Value.ToString(dateFormat + " HH:mm") : "";
                            validAlerts.Add(new SchoolAlerts(item.Alert_id, alertTime, item.Activeyears, item.ActiveGroups.ToActiveGroupNames(item.Language_id ?? 1), item.Alert_urgent, alertText, item.NewsId ?? 0, item.EventId ?? 0, fid, item.suggestedSchoopID.HasValue ? item.suggestedSchoopID.Value : 0));
                        }

                    }
                }

                if (validAlerts.Count == 0)
                {
                    return new { res = 7 };//setErrorCode(7);
                }
                else if (alertId == 0)
                {
                    return new
                    {
                        res = 0,
                        schoopID = schoop_id,
                        alert_id = validAlerts[0].Alert_ID
                    };
                }
                else
                {
                    return new
                    {
                        res = 1,
                        schoopID = schoop_id,
                        data = validAlerts.ToArray()
                    };
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                if (ex.InnerException != null)
                {
                    message += "-Inner exception: " + ex.InnerException.Message;
                }
            }
            return new { res = 7 };
        }

        [FunctionName("GetAlertById")]
        public static async Task<object> GetAlertById(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        //public object GetAlertById(int alert_id)
        {
            string alert_id = req.Query["alert_id"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            alert_id = alert_id ?? data?.alert_id;
            int alertId = 0;
            if (!int.TryParse(alert_id, out alertId)) //null device ID
            {
                return setErrorCode(0);
            }
            try
            {
                if (_dataServices == null)
                {
                    _dataServices = new DataServices();
                }

                var schoolAlert = _dataServices.GetAlertById(alertId);
                var strJSON = new List<SchoolAlerts> { };
                if (null == schoolAlert)
                {
                    return setErrorCode(0);
                }
                else
                {
                    var dateFormat = "dd/MM/yyyy";
                    var schoopDateFormat = _dataServices.GetSchoolDateFormat(schoolAlert.Schoop_id);
                    if (!string.IsNullOrEmpty(schoopDateFormat))
                    {
                        dateFormat = schoopDateFormat;
                    }

                    //check if is all groups
                    var isAllGroups = false;
                    if (string.IsNullOrEmpty(schoolAlert.ActiveGroups))
                    {
                        isAllGroups = true;
                    }
                    if (schoolAlert.ActiveGroups == "0" || schoolAlert.ActiveGroups == "|0|")
                    {
                        isAllGroups = true;
                    }

                    var groupsCount = _dataServices.GetGroupsCountBySchoopId(schoolAlert.Schoop_id);
                    if (schoolAlert.ActiveGroups.Trim('|').Split('|').Count() >= groupsCount)
                    {
                        isAllGroups = true;
                    }

                    var activeGroupNames = isAllGroups ? "All Groups" : schoolAlert.ActiveGroups.ToActiveGroupNames(schoolAlert.Language_id ?? 1);
                    var alertTime = schoolAlert.Alert_date_time.HasValue ? schoolAlert.Alert_date_time.Value.ToString(dateFormat + " HH:mm") : "";
                    var fid = "0"; //this service cannot get survey url because we don't know device
                    strJSON.Add(new SchoolAlerts(schoolAlert.Alert_id, alertTime, schoolAlert.Activeyears, activeGroupNames, schoolAlert.Alert_urgent, schoolAlert.Alert_text, schoolAlert.NewsId ?? 0, schoolAlert.EventId ?? 0, fid, schoolAlert.suggestedSchoopID.HasValue ? schoolAlert.suggestedSchoopID.Value : 0));
                }

                return new
                {
                    res = 1,
                    data = strJSON.ToArray()
                };
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                if (ex.InnerException != null)
                {
                    message += "-Inner exception: " + ex.InnerException.Message;
                }
                return setErrorCode(7);
            }
        }

        [FunctionName("GetCollatedAlerts")]
        public static async Task<object> GetCollatedAlerts(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        //public object GetCollatedAlerts(int? device_id)
        {
            string device_id = req.Query["device_id"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            device_id = device_id ?? data?.device_id;
            int deviceId = 0;
            if (!int.TryParse(device_id, out deviceId)) //null device ID
            {
                return new { res = 0 };
            }
            try
            {
                if (_dataServices == null)
                {
                    _dataServices = new DataServices();
                }

                _dataServices.ResetBadgeNumberAndStatus(deviceId);

                var alerts = _dataServices.GetTopNumDeviceAlertsByDeviceID(deviceId).ToList();
                var validAlerts = new List<CollatedAlerts>();
                var device = _dataServices.GetDeviceByDeviceId(deviceId);
                foreach (var item in alerts)
                {
                    var alertIcon = 0;
                    string fid = "0";
                    if (item.Activeyears.Length < 5)
                    {
                        var iconYear = item.Activeyears.Trim('|');
                        int.TryParse(iconYear, out alertIcon);
                    }
                    if (item.Alert_urgent)
                    {
                        alertIcon = 100;
                    }
                    var alertText = item.Alert_text;
                    if (item.SurveyGuid.HasValue) //is survey alert, add on 20141125
                    {
                        var surveyUrl = "https://schoop.co.uk/survey/?s=" + item.SurveyGuid.Value.ToString() + "&d=" + device.DeviceGuid.ToString();
                        fid = surveyUrl;
                    }
                    if (item.IsContactFormAlert)
                    {
                        var surveyUrl = "https://schoop.co.uk/subscriber/contacts?g=" + device.DeviceGuid.ToString() + "&rd=" + Guid.NewGuid().ToString();
                        fid = surveyUrl;
                    }
                    if (item.IsMigrateAlert)
                    {
                        var surveyUrl = "https://schoop.co.uk/migrateandroid/?guid=" + device.DeviceGuid.ToString();
                        fid = surveyUrl;
                    }

                    var dateFormat = "dd/MM/yyyy";
                    var schoolName = item.EstablishmentName;
                    if (!string.IsNullOrEmpty(item.CustomDateFormat))
                    {
                        dateFormat = item.CustomDateFormat;
                    }

                    var alertTime = item.Alert_date_time.HasValue ? item.Alert_date_time.Value.ToString(dateFormat + " HH:mm") : "";

                    var activeYears = item.Activeyears;
                    var activeGroupNames = "";
                    var school = _dataServices.GetSchoolByID(item.Schoop_id);
                    if (school != null && school.hasYears)
                    {
                        if (string.IsNullOrEmpty(activeYears))
                        {
                            activeYears = "Private Message";
                        }
                        if (string.IsNullOrEmpty(item.ActiveGroups) || item.SendByDeviceIds == true)
                        {
                            activeGroupNames = "Private Message";
                        }
                    }
                    else
                    {
                        //check if is all groups
                        if (string.IsNullOrEmpty(item.ActiveGroups) || item.ActiveGroups == "0")
                        {
                            activeGroupNames = "0";
                        }
                        var isAllGroups = false;
                        if (item.ActiveGroups == "|0|")
                        {
                            isAllGroups = true;
                        }
                        var groupsCount = _dataServices.GetGroupsCountBySchoopId(item.Schoop_id);
                        if (string.IsNullOrEmpty(item.ActiveGroups) || item.ActiveGroups.Trim('|').Split('|').Count() >= groupsCount)
                        {
                            isAllGroups = true;
                        }
                        if (isAllGroups)
                        {
                            activeGroupNames = "|0|";
                        }

                        if (item.SendByDeviceIds == true)
                        {
                            activeGroupNames = "Private Message";
                        }
                    }
                    if (string.IsNullOrEmpty(activeGroupNames))
                    {
                        activeGroupNames = item.ActiveGroups.ToActiveGroupNames(item.Language_id ?? 1);
                    }

                    if (string.IsNullOrEmpty(activeGroupNames))
                    {
                        activeGroupNames = "Private Message";
                    }

                    validAlerts.Add(new CollatedAlerts(item.Schoop_id, schoolName, item.Alert_id, alertTime, activeYears, activeGroupNames, item.Alert_urgent, alertText, alertIcon, item.NewsId ?? 0, item.EventId ?? 0, fid, item.suggestedSchoopID.HasValue ? item.suggestedSchoopID.Value : 0));
                }

                if (validAlerts.Count == 0)
                {
                    return new { res = 7 };//setErrorCode(7);
                }
                else
                {
                    return new
                    {
                        res = 1,
                        deviceId = device_id,
                        data = validAlerts.ToArray()
                    };
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                if (ex.InnerException != null)
                {
                    message += "-Inner exception: " + ex.InnerException.Message;
                }
            }
            return null;
        }

        [FunctionName("DeviceLanguageUpdate")]
        public static async Task<object> DeviceLanguageUpdate(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        //public object DeviceLanguageUpdate(int? deviceID, int languageID)
        {
            string deviceID = req.Query["deviceID"];
            string languageID = req.Query["languageID"];
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            deviceID = deviceID ?? data?.deviceID;
            languageID = languageID ?? data?.languageID;

            int dId = 0;
            if (!int.TryParse(deviceID, out dId)) //null device ID
            {
                return setErrorCode(0);
            }
            int langId = 0;
            if (!int.TryParse(languageID, out langId)) //null device ID
            {
                return setErrorCode(0);
            }

            if (_dataServices == null)
            {
                _dataServices = new DataServices();
            }
            var device = _dataServices.GetDeviceByDeviceId(dId);

            if (device != null)
            {
                device.Language_id = langId;
                device.deviceActive = true;
                device.deviceLastAccessed = DateTime.Now;
                //Update tbl_devices
                if (_dataServices.UpdateDeviceLangId(dId, langId))
                {
                    return new
                    {
                        res = 1,
                    };
                }
            }

            return new
            {
                res = 0,
            };
        }

        [FunctionName("GetMySchools")]
        public static async Task<object> GetMySchools(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        //public object GetMySchools(int? device_id)
        {
            string device_id = req.Query["device_id"];
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            device_id = device_id ?? data?.device_id;

            int deviceID = 0;
            if (!int.TryParse(device_id, out deviceID)) //null device ID
            {
                return null;
            }

            try
            {

                if (_dataServices == null)
                {
                    _dataServices = new DataServices();
                }
                var device = _dataServices.GetDeviceByDeviceId(deviceID);
                if (device == null)
                {
                    return null;
                }
                var activeYears = _dataServices.GetActiveYearByDeviceID(deviceID);
                var schools = new List<School>();

                foreach (var activeYear in activeYears)
                {
                    var school = _dataServices.GetSchoolByIDV2(activeYear.schoop_id);
                    if (school != null)
                    {
                        var hasYears = school.hasYears;
                        var hasGroups = school.hasGroups;
                        var orgTypeName = school.organisationTypeName;
                        

                        var scholItem = new School("1", school.schoopID, school.sch1, school.sch2, school.sch3, school.sch4, school.sch5, school.sch6, school.sLow, school.sHigh, school.schTel, school.schEmail, school.schWeb, school.schHead, school.schActive, hasYears, hasGroups, school.activeYears, "0", orgTypeName);
                        scholItem.channelDescription = school.channelDescription;
                        scholItem.channelLogo = school.channelLogo;
                        schools.Add(scholItem);
                    }
                }

                if (schools.Count == 0)
                {
                    return new { res = 7 };//setErrorCode(7);
                }
                else
                {
                    return new
                    {
                        res = 1,
                        deviceId = device_id,
                        data = schools.ToArray()
                    };
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                if (ex.InnerException != null)
                {
                    message += "-Inner exception: " + ex.InnerException.Message;
                }
            }
            return null;
        }

        [FunctionName("GetGroups")]
        public static async Task<object> GetGroups(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        //public object GetGroups(int schoopID)
        {
            string schoopID = req.Query["schoopID"];
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            schoopID = schoopID ?? data?.schoopID;

            int schId = 0;
            if (!int.TryParse(schoopID, out schId)) //null device ID
            {
                return setErrorCode(0);
            }
            try
            {
                if (_dataServices == null)
                {
                    _dataServices = new DataServices();
                }

                var groups = _dataServices.GetUnClosedGroupsBySchoopID(schId);
                var school = _dataServices.GetSchoolByID(schId);
                if (school != null)
                {
                    if (school.hasYears && school.hasYears) //is school, has years
                    {
                        var results = new List<ActiveGroups>();
                        var lowAge = 3;
                        var highAge = 18;
                        int.TryParse(school.sLow, out lowAge);
                        int.TryParse(school.sHigh, out highAge);
                        for (var i = lowAge; i < highAge; i++)
                        {
                            var appValue = i - 2;
                            var activeGroup = new ActiveGroups();
                            activeGroup.res = "1";
                            activeGroup.year = appValue;
                            activeGroup.schoopID = schId;
                            foreach (var group in groups)
                            {
                                var yearHasGroup = false;
                                var activeYears = group.activeYears;
                                if (string.IsNullOrEmpty(activeYears) || activeYears == "0" || activeYears == "|0|")
                                {
                                    yearHasGroup = true;
                                }
                                else if (activeYears.Contains("|" + appValue + "|"))
                                {
                                    yearHasGroup = true;
                                }
                                if (yearHasGroup)
                                {
                                    var updateTime = group.lastUpdatedOn.HasValue ? group.lastUpdatedOn.Value : DateTime.Now;
                                    var groupName = _dataServices.GetDefaultGroupName(group.groupID);
                                    activeGroup.groups.Add(new Models.Group(group.groupID, groupName != null ? groupName.groupName : "", updateTime));
                                }
                            }
                            if (activeGroup.groups.Count > 0)
                            {
                                results.Add(activeGroup);
                            }
                        }
                        return results.ToArray();
                    }
                    else
                    {
                        var resultNoYears = new ActiveGroupsNoYear();
                        resultNoYears.res = "1";
                        resultNoYears.schoopID = schId;
                        foreach (var group in groups)
                        {
                            var updateTime = group.lastUpdatedOn.HasValue ? group.lastUpdatedOn.Value : DateTime.Now;
                            var groupName = _dataServices.GetDefaultGroupName(group.groupID);
                            resultNoYears.groups.Add(new Models.Group(group.groupID, groupName != null ? groupName.groupName : "", updateTime));
                        }
                        return resultNoYears;
                    }
                }
                else
                {
                    return new { res = 7 };//setErrorCode(7);
                }

            }
            catch (Exception ex)
            {
                var message = ex.Message;
                if (ex.InnerException != null)
                {
                    message += "-Inner exception: " + ex.InnerException.Message;
                }

            }
            return null;
        }

        [FunctionName("GetGroups2")]
        public static async Task<object> GetGroups2(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        //public object GetGroups2(int schoopID)
        {
            string schoopID = req.Query["schoopID"];
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            schoopID = schoopID ?? data?.schoopID;

            int schId = 0;
            if (!int.TryParse(schoopID, out schId)) //null device ID
            {
                return setErrorCode(0);
            }
            try
            {
                if (_dataServices == null)
                {
                    _dataServices = new DataServices();
                }

                var groups = _dataServices.GetSchoolGroups2(schId, 500); //max 50 groups, can be more
                var parentGroups = groups.Where(g => g.parentGroupID == 0).ToList();
                if (parentGroups.Count > 0)
                {
                    var result = new ActiveGroupsWithSubGroups();
                    result.schoopID = schId;
                    result.res = "1";
                    foreach (var parentGroup in parentGroups)
                    {
                        var parentItem = new ParentGroups();
                        parentItem.gID = parentGroup.groupID;
                        parentItem.gName = parentGroup.groupName;
                        parentItem.gDescription = string.IsNullOrEmpty(parentGroup.groupDescription) ? "" : parentGroup.groupDescription;
                        var childGroups = groups.Where(g => g.parentGroupID == parentGroup.groupID).ToList();
                        if (childGroups.Count > 0)
                        {
                            foreach (var childGroup in childGroups)
                            {
                                parentItem.subGroups.Add(new SubGroups { gID = childGroup.groupID, parentGID = parentGroup.groupID, gName = childGroup.groupName, gDescription = string.IsNullOrEmpty(childGroup.groupDescription) ? "" : childGroup.groupDescription });
                            }
                        }
                        result.parentGroups.Add(parentItem);
                    }
                    return result;
                }
                else
                {
                    return new { res = 7 };//setErrorCode(7);
                }

            }
            catch (Exception ex)
            {
                var message = ex.Message;
                if (ex.InnerException != null)
                {
                    message += "-Inner exception: " + ex.InnerException.Message;
                }
            }
            return null;
        }

        [FunctionName("GetGroups3")]
        public static async Task<object> GetGroups3(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        //public object GetGroups3(int schoopID, int languageID)
        {
            string schoopID = req.Query["schoopID"];
            string languageID = req.Query["languageID"];
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            schoopID = schoopID ?? data?.schoopID;
            languageID = languageID ?? data?.languageID;
            int schId = 0;
            if (!int.TryParse(schoopID, out schId)) //null device ID
            {
                return setErrorCode(0);
            }
            int langId = 0;
            if (!int.TryParse(languageID, out langId)) //null device ID
            {
                return setErrorCode(0);
            }
            try
            {
                if (_dataServices == null)
                {
                    _dataServices = new DataServices();
                }

                var groups = _dataServices.GetSchoolGroups3(schId, langId, 500); //max 50 groups, can be more
                var parentGroups = groups.Where(g => g.parentGroupID == 0).ToList();
                if (parentGroups.Count > 0)
                {
                    var result = new ActiveGroupsWithSubGroups();
                    result.schoopID = schId;
                    result.res = "1";
                    foreach (var parentGroup in parentGroups)
                    {
                        var parentItem = new ParentGroups();
                        parentItem.gID = parentGroup.groupID;
                        var groupName = parentGroup.groupName;
                        var description = parentGroup.groupDescription;
                        if (string.IsNullOrEmpty(groupName))
                        {
                            var defaultLanguage = _dataServices.GetDefaultGroupName(parentGroup.groupID);
                            if (defaultLanguage != null)
                            {
                                groupName = defaultLanguage.groupName;
                                description = defaultLanguage.groupDescription;
                            }
                        }
                        parentItem.gName = parentGroup.groupName;
                        parentItem.gDescription = string.IsNullOrEmpty(parentGroup.groupDescription) ? "" : parentGroup.groupDescription;
                        var childGroups = groups.Where(g => g.parentGroupID == parentGroup.groupID).ToList();
                        if (childGroups.Count > 0)
                        {
                            foreach (var childGroup in childGroups)
                            {
                                var childGroupName = childGroup.groupName;
                                var childDescription = childGroup.groupDescription;
                                if (string.IsNullOrEmpty(childGroupName))
                                {
                                    var defaultLanguage = _dataServices.GetDefaultGroupName(childGroup.groupID);
                                    if (defaultLanguage != null)
                                    {
                                        childGroupName = defaultLanguage.groupName;
                                        childDescription = defaultLanguage.groupDescription;
                                    }
                                }
                                parentItem.subGroups.Add(new SubGroups { gID = childGroup.groupID, parentGID = parentGroup.groupID, gName = childGroupName, gDescription = string.IsNullOrEmpty(childDescription) ? "" : childDescription });
                            }
                        }
                        result.parentGroups.Add(parentItem);
                    }
                    return result;
                }
                else
                {
                    return new { res = 7 };//setErrorCode(7);
                }

            }
            catch (Exception ex)
            {
                var message = ex.Message;
                if (ex.InnerException != null)
                {
                    message += "-Inner exception: " + ex.InnerException.Message;
                }
                // Schoop.Domain.Utils.Helper.EmailHelper.EmailHelper.SendErrorEmail("GetGroups2( " + schoopID + " )", message);
            }
            return null;
        }

        [FunctionName("SetActiveGroupsByDevice")]
        public static async Task<object> SetActiveGroupsByDevice(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        //public object SetActiveGroupsByDevice(int? deviceID, int schoopID, string activeGroups)
        {
            string deviceID = req.Query["deviceID"];
            string schoopID = req.Query["schoopID"];
            string activeGroups = req.Query["activeGroups"];
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            deviceID = deviceID ?? data?.deviceID;
            schoopID = schoopID ?? data?.schoopID;
            activeGroups = activeGroups ?? data?.activeGroups;
            int schId = 0;
            if (!int.TryParse(schoopID, out schId)) //null device ID
            {
                return setErrorCode(0);
            }
            int dId = 0;
            if (!int.TryParse(deviceID, out dId)) //null device ID
            {
                return setErrorCode(0);
            }
            try
            {
                if (_dataServices == null)
                {
                    _dataServices = new DataServices();
                }
                var groups = new List<int>();
                if (string.IsNullOrEmpty(activeGroups)) //unsubscribe if null
                {
                    _dataServices.SetActiveGroups(dId, schId, groups);

                    return setErrorCode(3);
                }
                groups.AddRange(from @group in activeGroups.Split('|') where !string.IsNullOrEmpty(@group) select int.Parse(@group));

                _dataServices.SetActiveGroups(dId, schId, groups);
                var device = _dataServices.GetDeviceByDeviceId(dId);
                if (device == null)
                {
                    return new { res = 0 };
                }

                if (schId != 9999)
                {
                    Thread.Sleep(4000); //sleep 8 seconds on 20160113
                    return setErrorCode(3);
                }
                //send alert back to device
                var alert = new tbl_school_alerts()
                {
                    Activeyears = activeGroups,
                    Alert_sent_by = 0,
                    Alert_text = "Schoop Engineer. Your Device ID is: " + deviceID,
                    Alert_urgent = false,
                    Schoop_id = schId,
                    Language_id = 1,
                    Alert_date_time = DateTime.Now,
                    ToSendTime = DateTime.Now
                };
                _dataServices.InsertAlert(alert);

                return setErrorCode(3);
            }

            catch (Exception ex)
            {
                if (schId == 9999)
                {
                    var alert = new tbl_school_alerts()
                    {
                        Activeyears = activeGroups,
                        Alert_sent_by = 0,
                        Alert_text = ex.Message.Length > 200 ? ex.Message.Substring(0, 200) : ex.Message,
                        Alert_urgent = false,
                        Schoop_id = schId,
                        Language_id = 1,
                        Alert_date_time = DateTime.Now,
                        ToSendTime = DateTime.Now
                    };

                    _dataServices.InsertAlert(alert);
                }
                return new { res = 0 };
            }
        }

        [FunctionName("GetActiveYears")]
        public static async Task<object> GetActiveYears(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        //public object GetActiveYears(int deviceID, int schoopID)
        {
            string deviceID = req.Query["deviceID"];
            string schoopID = req.Query["schoopID"];
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            deviceID = deviceID ?? data?.deviceID;
            schoopID = schoopID ?? data?.schoopID;
            int schId = 0;
            if (!int.TryParse(schoopID, out schId)) //null device ID
            {
                return setErrorCode(0);
            }
            int dId = 0;
            if (!int.TryParse(deviceID, out dId)) //null device ID
            {
                return setErrorCode(0);
            }
            try
            {
                if (_dataServices == null)
                {
                    _dataServices = new DataServices();
                }
                var school = _dataServices.GetSchoolByID(schId);
                var aybd = _dataServices.GetActiveYearBySchoopIDAndDeviceID(schId, dId);
                if (school == null)
                {
                    return setErrorCode(0);
                }
                var sLow = "";
                var sHigh = "";
                var activeYears = "";
                if (aybd != null)
                {
                    activeYears = aybd.active_years;
                }
                sLow = school.sLow;
                sHigh = school.sHigh;

                return new { sLow = sLow, sHigh = sHigh, activeYears = activeYears };
            }
            catch (Exception)
            {
                return setErrorCode(0);
            }
        }

        [FunctionName("GetMyGroups")]
        public static async Task<object> GetMyGroups(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        //public object GetMyGroups(int deviceID, int schoopID)
        {
            string deviceID = req.Query["deviceID"];
            string schoopID = req.Query["schoopID"];
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            deviceID = deviceID ?? data?.deviceID;
            schoopID = schoopID ?? data?.schoopID;
            int schId = 0;
            if (!int.TryParse(schoopID, out schId)) //null device ID
            {
                return setErrorCode(0);
            }
            int dId = 0;
            if (!int.TryParse(deviceID, out dId)) //null device ID
            {
                return setErrorCode(0);
            }
            try
            {
                if (_dataServices == null)
                {
                    _dataServices = new DataServices();
                }
                var groups = _dataServices.GetActiveGroupsBySchoopIdAndDeviceID(schId, dId);
                if (groups.Count == 0)
                {
                    return setErrorCode(7);
                }
                var result = new List<object>();
                foreach (var g in groups)
                {
                    result.Add(new { groupID = g.active_groupID });
                }

                return result;
            }
            catch (Exception)
            {
                return setErrorCode(0);
            }
        }

        [FunctionName("GetStickyForms")]
        public static async Task<object> GetStickyForms(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        //public object GetStickyForms(int deviceID, int schoopID)
        {
            string deviceID = req.Query["deviceID"];
            string schoopID = req.Query["schoopID"];
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            deviceID = deviceID ?? data?.deviceID;
            schoopID = schoopID ?? data?.schoopID;
            int schId = 0;
            if (!int.TryParse(schoopID, out schId)) //null device ID
            {
                return setErrorCode(0);
            }
            int dId = 0;
            if (!int.TryParse(deviceID, out dId)) //null device ID
            {
                return setErrorCode(0);
            }
            
            try
            {
                if(_dataServices == null)
                {
                    _dataServices = new DataServices();
                }
                var device = _dataServices.GetDeviceByDeviceId(dId);
                if (device == null)
                {
                    return setErrorCode(0);
                }
                var yearStr = "0";
                var groupStr = "0";
                var activeYears = _dataServices.GetActiveYearBySchoopIDAndDeviceID(schId, dId);
                var activeGroups = _dataServices.GetActiveGroupsBySchoopIdAndDeviceID(schId, dId);
                if (activeYears != null && !string.IsNullOrEmpty(yearStr))
                {
                    yearStr = activeYears.active_years;
                }
                if (activeGroups.Count > 0)
                {
                    groupStr = "|";
                    foreach (var group in activeGroups)
                    {
                        groupStr += group.active_groupID + "|";
                    }
                }

                var stickForms = _dataServices.GetLibraryFormsBySchoopIdAndActiveYearsGroups(schId, dId, yearStr, groupStr, device.Language_id ?? 1, 100);
                return new
                {
                    res = 1,
                    data = (from form in stickForms select new Form { fTitle = form.Survey_Name, fDesc = form.Alert_text, fURL = "https://schoop.co.uk/survey/?s=" + form.SurveyGuid.ToString() + "&d=" + device.DeviceGuid.ToString() }).ToArray()
                };
            }
            catch (Exception)
            {
                return setErrorCode(0);
            }

        }

        [FunctionName("SetActiveYearsTest")]
        public static async Task<object> SetActiveYearsTest(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        //public object SetActiveYearsTest(int? deviceID, int schoopID, string activeyears)
        {
            string deviceID = req.Query["deviceID"];
            string schoopID = req.Query["schoopID"];
            string activeyears = req.Query["activeyears"];
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            deviceID = deviceID ?? data?.deviceID;
            schoopID = schoopID ?? data?.schoopID;
            activeyears = activeyears ?? data?.activeyears;
            int schId = 0;
            if (!int.TryParse(schoopID, out schId)) //null device ID
            {
                return setErrorCode(0);
            }
            int dId = 0;
            if (!int.TryParse(deviceID, out dId)) //null device ID
            {
                return setErrorCode(0);
            }

            try
            {
                if (_dataServices == null)
                {
                    _dataServices = new DataServices();
                }
                
                var device = _dataServices.GetDeviceByDeviceId(dId);
                if (device == null)
                {
                    return setErrorCode(0);
                }

                _dataServices.SetActiveYears(dId, schId, activeyears);
                var deviceTypeID = device.deviceTypeID;
                if (deviceTypeID == 1 || deviceTypeID == 2) //only for IOS or Android
                {
                    //UpdateNHRegistration(device);
                }

                if (schId == 9999)
                {
                    //send alert back to device
                    var alert = new tbl_school_alerts()
                    {
                        Activeyears = activeyears,
                        Alert_sent_by = 0,
                        Alert_text = "Schoop Engineer. Your Device ID is: " + deviceID,
                        Alert_urgent = false,
                        Schoop_id = schId,
                        Language_id = 1,
                        Alert_date_time = DateTime.Now,
                        ActiveGroups = "0",
                        ToSendTime = DateTime.Now
                    };
                    _dataServices.InsertAlert(alert);

                }


                //send private alert for contact form, added on 20161018
                if (device.EmailIsAuthed == false && device.PhoneIsAuthed == false)
                {
                    var alert = new tbl_school_alerts()
                    {
                        Activeyears = "0",
                        Alert_sent_by = 0,
                        Alert_text = "Welcome to Schoop. Please provide your up to date contact details. Please click on the pencil icon to complete the form.",
                        Alert_urgent = false,
                        Schoop_id = 2820,
                        IsContactFormAlert = true,
                        Language_id = 1,
                        Alert_date_time = DateTime.Now,
                        SendByDeviceIds = true,
                        DeviceIds = deviceID.ToString(),
                        ActiveGroups = "0",
                        ToSendTime = DateTime.Now
                    };
                    _dataServices.InsertAlert(alert);
                }

                Thread.Sleep(4000);
                return setErrorCode(3);
            }

            catch (Exception ex)
            {
                if (schId == 9999)
                {
                    var alert = new tbl_school_alerts()
                    {
                        Activeyears = activeyears,
                        Alert_sent_by = 0,
                        Alert_text = ex.Message.Length > 200 ? ex.Message.Substring(0, 200) : ex.Message,
                        Alert_urgent = false,
                        Schoop_id = schId,
                        Language_id = 1,
                        Alert_date_time = DateTime.Now,
                        ToSendTime = DateTime.Now
                    };

                    _dataServices.InsertAlert(alert);
                    //SendAlertToDevice(deviceID, ex.Message.Length > 200 ? ex.Message.Substring(0, 200) : ex.Message);
                }
                // Schoop.Domain.Utils.Helper.EmailHelper.EmailHelper.SendErrorEmail("Set Active Years or Groups", ex.Message + "<br>" + ex.StackTrace + " - deviceID: " + deviceID + " - schoopID: " + schoopID + " - activeyears: " + activeyears);
                return setErrorCode(0);
            }
        }

        [FunctionName("GetMySubscription")]
        public static async Task<object> GetMySubscription(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        //public object GetMySubscription(int? deviceID)
        {
            string deviceID = req.Query["deviceID"];
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            deviceID = deviceID ?? data?.deviceID;
            int dId = 0;
            if (!int.TryParse(deviceID, out dId)) //null device ID
            {
                return setErrorCode(0);
            }

            try
            {
                if (_dataServices == null)
                {
                    _dataServices = new DataServices();
                }
                var device = _dataServices.GetDeviceByDeviceId(dId);
                if (device == null)
                {
                    return setErrorCode(0);
                }
                var data1 = new
                {
                    FirstName = device.first_name,
                    LastName = device.last_name,
                    Mobile = device.devicePhone,
                    Email = device.deviceEmail,
                    DeviceGUID = device.DeviceGuid,
                    EmailIsAuthed = device.EmailIsAuthed,
                    PhoneIsAuthed = device.PhoneIsAuthed,
                    Confirmed13 = device.Confirmed13
                };
                return new
                {
                    res = 1,
                    deviceId = deviceID,
                    data = data1
                };
            }
            catch (Exception)
            {
                return setErrorCode(0);
            }
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

        private static string GetDevicePlatform(int deviceTypeID)
        {
            var devicePlatform = "";
            switch (deviceTypeID)
            {
                case 1:
                    devicePlatform = "apns";
                    break;
                case 2:
                    devicePlatform = "gcm";
                    break;
                default:
                    devicePlatform = "";
                    break;
            }
            return devicePlatform;
        }

        #endregion
    }
}
