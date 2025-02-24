using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SchoopFunctionApp.Entities;
using SchoopFunctionApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoopFunctionApp.Services
{
    public class DataServices
    {
        public School GetSchoolByID(int schoopId)
        {
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                connection.Open();
                var query = @"SELECT * FROM [dbo].[tbl_schools_uk] s left join [dbo].[tbl_towns] t on s.town_id = t.Town_id where SchoopID = " + schoopId;
                SqlCommand command = new SqlCommand(query, connection);
                var reader = command.ExecuteReaderAsync().Result;
                while (reader.Read())
                {
                    var school = new School()
                    {
                        res = "1",
                        schoopID = (int)reader["SchoopID"],
                        sch1 = reader["EstablishmentName"].ToString(),
                        sch2 = reader["Street"].ToString(),
                        sch3 = reader["Locality"].ToString(),
                        sch4 = reader["Address3"].ToString(),
                        sch5 = reader["Locality"].ToString(),
                        sch6 = reader["Town_name"].ToString() + (string.IsNullOrEmpty(reader["State"].ToString()) ? "" : "," + reader["State"].ToString()),
                        sLow = reader["StatutoryLowAge"].ToString(),
                        sHigh = reader["StatutoryHighAge"].ToString(),
                        schTel = "0" + reader["TelephoneSTD"].ToString() + " " + reader["TelephoneNum"].ToString(),
                        schEmail = " ",
                        schWeb = reader["WebsiteAddress"].ToString(),
                        schHead = reader["HeadTitle"].ToString() + " " + reader["HeadFirstName"].ToString() + " " + reader["HeadLastName"].ToString(),
                        schActive = (bool)reader["IsActive"],
                        hasYears = reader["hasYears"] is DBNull ? false : (bool)reader["hasYears"],
                        hasGroups = reader["hasGroups"] is DBNull ? false : (bool)reader["hasGroups"],
                        organisationTypeName = "",
                    };
                    return school;
                }
            }

            return null;
        }

        public School GetSchoolByIDV2(int schoopId)
        {
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                connection.Open();
                var query = @"SELECT * FROM [dbo].[tbl_schools_uk] s left join [dbo].[tbl_towns] t on s.town_id = t.Town_id left join tbl_organisation_types ot on s.organisationType = ot.Org_type_id where SchoopID = " + schoopId;
                SqlCommand command = new SqlCommand(query, connection);
                var reader = command.ExecuteReaderAsync().Result;
                while (reader.Read())
                {
                    var school = new School()
                    {
                        res = "1",
                        schoopID = (int)reader["SchoopID"],
                        sch1 = reader["EstablishmentName"].ToString(),
                        sch2 = reader["Street"].ToString(),
                        sch3 = reader["Locality"].ToString(),
                        sch4 = reader["Address3"].ToString(),
                        sch5 = reader["Locality"].ToString(),
                        sch6 = reader["Town_name"].ToString() + (string.IsNullOrEmpty(reader["State"].ToString()) ? "" : "," + reader["State"].ToString()),
                        sLow = reader["StatutoryLowAge"].ToString(),
                        sHigh = reader["StatutoryHighAge"].ToString(),
                        schTel = "0" + reader["TelephoneSTD"].ToString() + " " + reader["TelephoneNum"].ToString(),
                        schEmail = " ",
                        schWeb = reader["WebsiteAddress"].ToString(),
                        schHead = reader["HeadTitle"].ToString() + " " + reader["HeadFirstName"].ToString() + " " + reader["HeadLastName"].ToString(),
                        schActive = (bool)reader["IsActive"],
                        hasYears = reader["hasYears"] is DBNull ? false : (bool)reader["hasYears"],
                        hasGroups = reader["hasGroups"] is DBNull ? false : (bool)reader["hasGroups"],
                        organisationTypeName = reader["Org_name"].ToString(),
                        channelDescription = reader["channelDescription"].ToString(),
                        channelLogo = reader["LogoUrl"].ToString(),
                        longitude = reader["Longitude"].ToString(),
                        latitude = reader["latitude"].ToString(),
                    };
                    return school;
                }
            }

            return null;
        }


        public int InsertDevice(string deviceToken, int deviceTypeID, string deviceOSVersion, int languageID)
        {
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                var query = @"INSERT INTO[dbo].[tbl_devices]
                               ([deviceToken]
                               , [deviceTypeID]
                               , [deviceOSVersion]
                               , [deviceName]
                               , [deviceEmail]
                               , [devicePhone]
                               , [deviceTwitter]
                               , [deviceFacebook]
                               , [deviceLastAccessed]
                               , [deviceActive]
                               , [Language_id]
                               , [BadgeNumber]
                               , [DeviceGuid]
                               , [first_name]
                               , [last_name]
                               , [deviceNotes]
                               , [EmailIsAuthed]
                               , [PhoneIsAuthed]
                               , [Confirmed13]
                               , [CountryCode]
                               , [AuthToken]
                               , [SendFailedCount]
                               , [NHRegToken]
                               , [AcceptTermsAndPrivacy]
                               , [APIVersion])
                         VALUES
                               (@deviceToken
                               ,@deviceTypeID
                               ,@deviceOSVersion
                               ,null
                               ,null
                               ,null
                               ,null
                               ,null
                               ,getdate()
                               ,1
                               ,@languageID
                               ,0
                               ,NEWID()
                               ,null
                               ,null
                               ,null
                               ,0
                               ,0
                               ,0
                               ,null
                               ,null
                               ,0
                               ,null
                               ,0
                               ,1);
                                SELECT SCOPE_IDENTITY();";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@deviceToken", deviceToken);
                command.Parameters.AddWithValue("@deviceTypeID", deviceTypeID);
                command.Parameters.AddWithValue("@deviceOSVersion", deviceOSVersion);
                command.Parameters.AddWithValue("@languageID", languageID);

                try
                {
                    connection.Open();
                    int newDeviceId = Convert.ToInt32(command.ExecuteScalar());
                    return newDeviceId;
                }
                catch (Exception)
                {
                }
            }

            return 0;
        }

        public void InsertActiveYearsByDevice(tbl_active_years_by_device aybd)
        {
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                var query = @"INSERT INTO [dbo].[tbl_active_years_by_device]
                               ([device_id]
                               ,[schoop_id]
                               ,[active_years])
                         VALUES
                               (@deviceId
                               ,@schoopId
                               ,@activeYears)";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@deviceId", aybd.device_id);
                command.Parameters.AddWithValue("@schoopId", aybd.schoop_id);
                command.Parameters.AddWithValue("@activeYears", aybd.active_years);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (Exception)
                {
                }
            }

        }

        public void SetActiveYears(int deviceID, int schoopID, string activeYears)
        {
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                var query = @"IF EXISTS (select 1 from [dbo].[tbl_active_years_by_device] where device_id = @deviceID and schoop_id = @schoopID)
                              Update [dbo].[tbl_active_years_by_device] set [active_years] = @activeYears where device_id = @deviceID and schoop_id = @schoopID
                              ELSE
                              Insert into [dbo].[tbl_active_years_by_device] ([device_id],[schoop_id],[active_years]) values(@deviceID, @schoopID, @activeYears)";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@deviceID", deviceID);
                command.Parameters.AddWithValue("@schoopID", schoopID);
                command.Parameters.AddWithValue("@activeYears", activeYears);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (Exception)
                {
                }
            }

        }

        public void SetActiveGroups(int deviceID, int schoopID, List<int> activeGroups)
        {
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                var query = @"exec qry_SetActiveGroups_ByDeviceIdAndGroupsAndSchoopId_IgnorePrivated @deviceId,@schoopId,@activeGroups";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@deviceId", deviceID);
                command.Parameters.AddWithValue("@schoopId", schoopID);
                var groups = "";
                foreach (var g in activeGroups)
                {
                    groups += g + ",";
                }
                if (groups.Length > 1)
                {
                    groups = groups.Substring(0, groups.Length - 1);
                }
                command.Parameters.AddWithValue("@activeGroups", groups);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (Exception)
                {
                }
            }

        }

        public bool UpdateDeviceByToken(string deviceToken, int deviceTypeID, string deviceOSVersion, int languageID)
        {
            if (string.IsNullOrEmpty(deviceToken)) 
            {
                return false; //not to update null token
            }
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                var query = @"UPDATE [dbo].[tbl_devices] 
                                SET [deviceTypeID] = @deviceTypeID
                                  ,[deviceOSVersion] = @deviceOSVersion
                                  ,[Language_id] = @languageID
                             WHERE [deviceToken] = @deviceToken";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@deviceTypeID", deviceTypeID);
                command.Parameters.AddWithValue("@deviceOSVersion", deviceOSVersion);
                command.Parameters.AddWithValue("@languageID", languageID);
                command.Parameters.AddWithValue("@deviceToken", deviceToken);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    return true;
                }
                catch (Exception)
                {
                }
            }
            return false;
        }

        public tbl_devices GetDeviceByToken(string token)
        {
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                connection.Open();
                var query = @"SELECT [deviceID]
                              ,[deviceToken]
                              ,[deviceTypeID]
                              ,[deviceOSVersion]
                              ,[deviceActive]
                              ,[Language_id]
                          FROM [dbo].[tbl_devices]
                          WHERE [deviceToken] = @deviceToken and [deviceActive] = 1";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@deviceToken", token);
                var reader = command.ExecuteReaderAsync().Result;
                while (reader.Read())
                {
                    var device = new tbl_devices()
                    {
                        deviceID = (int)reader["deviceID"],
                        deviceTwitter = reader["deviceToken"].ToString(),
                        deviceTypeID = (byte)reader["deviceTypeID"],
                        deviceOSVersion = reader["deviceOSVersion"].ToString(),
                        deviceActive = (bool)reader["deviceActive"],
                        Language_id = (int)reader["Language_id"],
                    };
                    return device;
                }
            }

            return null;
        }

        public IList<tbl_active_years_by_device> GetActiveYearByDeviceID(int deviceId)
        {
            var list = new List<tbl_active_years_by_device>();
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                connection.Open();
                var query = @"SELECT [activeyear_id]
                              ,[device_id]
                              ,[schoop_id]
                              ,[active_years]
                          FROM [dbo].[tbl_active_years_by_device]
                          WHERE device_id = @deviceId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@deviceId", deviceId);
                var reader = command.ExecuteReaderAsync().Result;
                while (reader.Read())
                {
                    var aybd = new tbl_active_years_by_device()
                    {
                        activeyear_id = (int)reader["activeyear_id"],
                        device_id = (int)reader["device_id"],
                        schoop_id = (int)reader["schoop_id"],
                        active_years = reader["active_years"].ToString(),
                    };
                    list.Add( aybd );
                }
                return list;
            }
        }

        public tbl_active_years_by_device GetActiveYearBySchoopIDAndDeviceID(int schoopId, int deviceId)
        {
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                connection.Open();
                var query = @"SELECT [activeyear_id]
                              ,[device_id]
                              ,[schoop_id]
                              ,[active_years]
                          FROM [dbo].[tbl_active_years_by_device]
                          WHERE device_id = @deviceId AND schoop_id = @schoopId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@deviceId", deviceId);
                command.Parameters.AddWithValue("@schoopId", schoopId);
                var reader = command.ExecuteReaderAsync().Result;
                while (reader.Read())
                {
                    var aybd = new tbl_active_years_by_device()
                    {
                        activeyear_id = (int)reader["activeyear_id"],
                        device_id = (int)reader["device_id"],
                        schoop_id = (int)reader["schoop_id"],
                        active_years = reader["active_years"].ToString(),
                    };
                    return aybd;
                }
                return null;
            }
        }

        public tbl_devices GetDeviceByDeviceId(int deviceId) 
        {
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                connection.Open();
                var query = @"SELECT [deviceID]
                              ,[DeviceGuid]
                              ,[deviceToken]
                              ,[deviceTypeID]
                              ,[deviceOSVersion]
                              ,[deviceActive]
                              ,[Language_id]
                          FROM [dbo].[tbl_devices]
                          WHERE [deviceID] = @deviceID and [deviceActive] = 1";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@deviceID", deviceId);
                var reader = command.ExecuteReaderAsync().Result;
                while (reader.Read())
                {
                    var device = new tbl_devices()
                    {
                        deviceID = (int)reader["deviceID"],
                        DeviceGuid = (Guid)reader["DeviceGuid"],
                        deviceTwitter = reader["deviceToken"].ToString(),
                        deviceTypeID = (byte)reader["deviceTypeID"],
                        deviceOSVersion = reader["deviceOSVersion"].ToString(),
                        deviceActive = (bool)reader["deviceActive"],
                        Language_id = (int)reader["Language_id"],
                    };
                    return device;
                }
            }

            return null;
        }

        public bool UpdateDeviceTokenByDeviceID(string deviceToken, int deviceID)
        {
            if (string.IsNullOrEmpty(deviceToken))
            {
                return false; //not to update null token
            }
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                var query = @"UPDATE [dbo].[tbl_devices] 
                                SET [deviceToken] = @deviceToken
                                  ,[deviceActive] = 1
                                  ,[deviceLastAccessed] = getdate()
                             WHERE [deviceID] = @deviceID";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@deviceToken", deviceToken);
                command.Parameters.AddWithValue("@deviceID", deviceID);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    return true;
                }
                catch (Exception)
                {
                }
            }
            return false;
        }

        public bool UpdateDeviceTypeIdByDeviceID(int deviceTypeId, int deviceID)
        {

            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                var query = @"UPDATE [dbo].[tbl_devices] 
                                SET [deviceTypeID] = @deviceTypeID
                             WHERE [deviceID] = @deviceID";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@deviceTypeId", deviceTypeId);
                command.Parameters.AddWithValue("@deviceID", deviceID);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    return true;
                }
                catch (Exception)
                {
                }
            }
            return false;
        }

        public int InsertAlert(tbl_school_alerts alert)
        {
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                var query = @"INSERT INTO [dbo].[tbl_school_alerts]
                               ([Schoop_id]
                               ,[Alert_date_time]
                               ,[Activeyears]
                               ,[Alert_urgent]
                               ,[Alert_text]
                               ,[Alert_sent_by]
                               ,[Language_id]
                               ,[ActiveGroups]
                               ,[IsSent]
                               ,[IsLoginAs]
                               ,[IsTranslatedByGoogle]
                               ,[SurveyGuid]
                               ,[IsScheduled]
                               ,[ToSendTime]
                               ,[ParentAlertId]
                               ,[EventId]
                               ,[NewsId]
                               ,[Deleted]
                               ,[SendByDeviceIds]
                               ,[DeviceIds]
                               ,[suggestedChannel]
                               ,[suggestedSchoopID]
                               ,[IsPrivate]
                               ,[IsContactFormAlert]
                               ,[IsMigrateAlert]
                               ,[IsInDocumentsDB]
                               ,[IsSentForDeviceType5])
                         VALUES
                               (@Schoop_id
                               ,getdate()
                               ,@Activeyears
                               ,@Alert_urgent
                               ,@Alert_text
                               ,@Alert_sent_by
                               ,@Language_id
                               ,@ActiveGroups
                               ,0
                               ,0
                               ,0
                               ,null
                               ,@IsScheduled
                               ,@ToSendTime
                               ,0
                               ,0
                               ,0
                               ,0
                               ,@SendByDeviceIds
                               ,@DeviceIds
                               ,0
                               ,0
                               ,0
                               ,0
                               ,0
                               ,0
                               ,0);
                                SELECT SCOPE_IDENTITY();";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Schoop_id", alert.Schoop_id);
                command.Parameters.AddWithValue("@Activeyears", alert.Activeyears);
                command.Parameters.AddWithValue("@Alert_urgent", alert.Alert_urgent);
                command.Parameters.AddWithValue("@Alert_text", alert.Alert_text);
                command.Parameters.AddWithValue("@Alert_sent_by", alert.Alert_sent_by);
                command.Parameters.AddWithValue("@Language_id", alert.Language_id);
                command.Parameters.AddWithValue("@ActiveGroups", alert.ActiveGroups);
                command.Parameters.AddWithValue("@IsScheduled", alert.IsScheduled);
                command.Parameters.AddWithValue("@ToSendTime", alert.ToSendTime);
                command.Parameters.AddWithValue("@SendByDeviceIds", alert.SendByDeviceIds);
                command.Parameters.AddWithValue("@DeviceIds", alert.DeviceIds);

                try
                {
                    connection.Open();
                    int newDeviceId = Convert.ToInt32(command.ExecuteScalar());
                    return newDeviceId;
                }
                catch (Exception)
                {
                }
            }

            return 0;
        }

        public void DeleteActiveYears(int deviceId, int schoopId)
        {
            //DELETE FROM [dbo].[tbl_active_years_by_device] WHERE [device_id] = @deviceID and [schoop_id] = @schoopID
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                var query = @"DELETE FROM [dbo].[tbl_active_years_by_device] WHERE [device_id] = @deviceID and [schoop_id] = @schoopID";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@deviceID", deviceId);
                command.Parameters.AddWithValue("@schoopID", schoopId);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (Exception)
                {
                }
            }
        }

        public void DeleteActiveGroups(int deviceId, int schoopId)
        {
            //DELETE FROM [dbo].[tbl_active_years_by_device] WHERE [device_id] = @deviceID and [schoop_id] = @schoopID
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                var query = @"DELETE FROM [dbo].[tbl_active_groups_by_device]
                              WHERE deviceID = @deviceID and active_groupID in (select [groupID] from [dbo].[tbl_organisation_groups] where [schoopID] = @schoopId)";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@deviceID", deviceId);
                command.Parameters.AddWithValue("@schoopId", schoopId);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (Exception)
                {
                }
            }
        }

        public IList<tbl_schools_uk> GetSchoolsByTownId(int townId)
        {
            //SELECT [SchoopID],[EstablishmentName] FROM [dbo].[tbl_schools_uk] WHERE [town_id] = @townId
            var list = new List<tbl_schools_uk>();
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                connection.Open();
                var query = @"SELECT [SchoopID],[EstablishmentName] FROM [dbo].[tbl_schools_uk] WHERE [town_id] = @townId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@townId", townId);
                var reader = command.ExecuteReaderAsync().Result;
                while (reader.Read())
                {
                    var school = new tbl_schools_uk()
                    {
                        SchoopID = (int)reader["SchoopID"],
                        EstablishmentName = reader["EstablishmentName"].ToString(),
                    };
                    list.Add(school);
                }
                return list;
            }
        }

        public IList<tbl_school_news> GetNewerNews(int newsId, int schoopId, int deviceId, int languageId)
        {
            //exec qry_FETCH_New_News @schoopId,@deviceId,@newsId,@languageId
            var list = new List<tbl_school_news>();
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                connection.Open();
                var query = @"exec qry_FETCH_New_News @schoopId,@deviceId,@newsId,@languageId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@schoopId", schoopId);
                command.Parameters.AddWithValue("@deviceId", deviceId);
                command.Parameters.AddWithValue("@newsId", newsId);
                command.Parameters.AddWithValue("@languageId", languageId);
                var reader = command.ExecuteReaderAsync().Result;
                while (reader.Read())
                {
                    var news = new tbl_school_news()
                    {
                        school_news_id = (int)reader["school_news_id"],
                        school_news_date = (DateTime)reader["school_news_date"],
                        school_news_headline = reader["school_news_headline"].ToString(),
                        school_news_active_years = reader["school_news_active_years"].ToString(),
                        ActiveGroups = reader["ActiveGroups"].ToString(),
                        Published = (bool)reader["Published"],
                    };
                    list.Add(news);
                }
                return list;
            }
        }

        
        public IList<tbl_school_events> GetNewEvents(int eventId, int schoopId, int deviceId, int languageId)
        {
            //exec qry_FETCH_New_Events @schoopId,@deviceId,@eventId,@languageId
            var list = new List<tbl_school_events>();
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                connection.Open();
                var query = @"exec qry_FETCH_New_Events @schoopId,@deviceId,@eventId,@languageId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@schoopId", schoopId);
                command.Parameters.AddWithValue("@deviceId", deviceId);
                command.Parameters.AddWithValue("@eventId", eventId);
                command.Parameters.AddWithValue("@languageId", languageId);
                var reader = command.ExecuteReaderAsync().Result;
                while (reader.Read())
                {
                    var events = new tbl_school_events()
                    {
                        Event_id = (int)reader["Event_id"],
                        Event_start_date = (DateTime)reader["Event_start_date"],
                        Event_end_date = (DateTime)reader["Event_end_date"],
                        Event_start_time = (DateTime)reader["Event_start_time"],
                        Event_end_time = (DateTime)reader["Event_end_time"],
                        Event_title = reader["Event_title"].ToString(),
                        Event_location = reader["Event_location"].ToString(),
                        Event_text = reader["Event_text"].ToString(),
                        Event_cost = reader["Event_cost"] is DBNull ? null : (decimal?)reader["Event_cost"],
                        ActiveYears = reader["ActiveYears"].ToString(),
                        ActiveGroups = reader["ActiveGroups"].ToString(),

                    };
                    list.Add(events);
                }
                return list;
            }
        }

        public string GetSchoolDateFormat(int schoopId)
        {
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                connection.Open();
                var query = @"SELECT [CustomDateFormat] FROM [dbo].[tbl_schools_uk] where SchoopID = " + schoopId;
                SqlCommand command = new SqlCommand(query, connection);
                var reader = command.ExecuteReaderAsync().Result;
                while (reader.Read())
                {
                    var customeDateFormat = reader["CustomDateFormat"].ToString();
                    return customeDateFormat;
                }
            }

            return null;
        }

        public string GetSchoolTimeZoneName(int schoopId)
        {
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                connection.Open();
                var query = @"SELECT [TimeZoneName] FROM [dbo].[tbl_schools_uk] where SchoopID = " + schoopId;
                SqlCommand command = new SqlCommand(query, connection);
                var reader = command.ExecuteReaderAsync().Result;
                while (reader.Read())
                {
                    var timeZoneName = reader["TimeZoneName"].ToString();
                    return timeZoneName;
                }
            }

            return null;
        }

        public IList<string> GetGroupNamesByGroups(List<int> groups, int languageId)
        {
            var list = new List<string>();
            if(groups.Count == 0)
            {
                return list;
            }
            var strGroups = "";
            foreach (var group in groups)
            {
                strGroups += group + ",";
            }
            if(strGroups.Length > 1)
            {
                strGroups = strGroups.Substring(0, strGroups.Length - 1);
            }

            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                connection.Open();
                var query = @"SELECT [groupName] FROM [dbo].[tbl_group_names] WHERE languID = @languageId and groupId in (" + strGroups + ")";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@languageId", languageId);

                var reader = command.ExecuteReaderAsync().Result;
                while (reader.Read())
                {
                    var groupName = reader["groupName"].ToString();
;
                    list.Add(groupName);
                }
                return list;
            }
        }

        public tbl_school_events GetEventById(int eventId)
        {
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                connection.Open();
                var query = @"SELECT [Event_id]
                              ,[SchoopID]
                              ,[Event_title]
                              ,[Event_start_date]
                              ,[Event_end_date]
                              ,[Event_start_time]
                              ,[Event_end_time]
                              ,[Event_location]
                              ,[Event_cost]
                              ,[Event_text]
                              ,[Event_contact_details]
                              ,[Event_active]
                              ,[Language_id]
                              ,[ActiveYears]
                              ,[ActiveGroups]
                          FROM [dbo].[tbl_school_events]
                          WHERE [Event_id] = @eventId and [Deleted] = 0";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@eventId", eventId);
                var reader = command.ExecuteReaderAsync().Result;
                while (reader.Read())
                {
                    var events = new tbl_school_events()
                    {
                        Event_id = (int)reader["Event_id"],
                        SchoopID = (int)reader["SchoopID"],
                        Event_title = reader["Event_title"].ToString(),
                        Event_start_date = (DateTime)reader["Event_start_date"],
                        Event_end_date = (DateTime)reader["Event_end_date"],
                        Event_start_time = (DateTime)reader["Event_start_time"],
                        Event_end_time = (DateTime)reader["Event_end_time"],
                        Event_location = reader["Event_location"].ToString(),
                        Event_cost = reader["Event_cost"] is DBNull ? null : (decimal?)reader["Event_cost"],
                        Event_text = reader["Event_text"].ToString(),
                        Event_contact_details = reader["Event_contact_details"].ToString(),
                        Event_active = (bool)reader["Event_active"],
                        Language_id = (int)reader["Language_id"],
                        ActiveYears = reader["ActiveYears"].ToString(),
                        ActiveGroups = reader["ActiveGroups"].ToString()
                    };
                    return events;
                }
            }

            return null;
        }


        public tbl_school_news GetNewsById(int newsId)
        {
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                connection.Open();
                var query = @"SELECT [school_news_id]
                              ,[schoopID]
                              ,[school_news_date]
                              ,[school_news_headline]
                              ,[school_news_article]
                              ,[school_news_active_years]
                              ,[school_news_active]
                              ,[ActiveGroups]
                          FROM [dbo].[tbl_school_news]
                          WHERE [school_news_id] = @newsId and [Deleted] = 0";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@newsId", newsId);
                var reader = command.ExecuteReaderAsync().Result;
                while (reader.Read())
                {
                    var news = new tbl_school_news()
                    {
                        school_news_id = (int)reader["school_news_id"],
                        schoopID = (int)reader["schoopID"],
                        school_news_date = (DateTime)reader["school_news_date"],
                        school_news_headline = reader["school_news_headline"].ToString(),
                        school_news_active_years = reader["school_news_active_years"].ToString(),
                        school_news_active = (bool)reader["school_news_active"],
                        ActiveGroups = reader["ActiveGroups"].ToString(),
                        school_news_article = reader["school_news_article"].ToString()
                    };
                    return news;
                }
            }
            return null;
        }

        public void InsertReadHistory(int alertId, int deviceId)
        {
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                var query = @"IF EXISTS (select 1 from [dbo].[tbl_alerts_read] where [Alert_id] = @alertId and [deviceID] = @deviceId)
                              Update [dbo].[tbl_alerts_read] set [ReadTime] = getdate() where [Alert_id] = @alertId and [deviceID] = @deviceId
                              ELSE
                              Insert into [dbo].[tbl_alerts_read] ([Alert_id],[deviceID],[ReadTime]) values(@alertId, @deviceId, getdate())";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@alertId", alertId);
                command.Parameters.AddWithValue("@deviceId", deviceId);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (Exception)
                {
                }
            }

        }

        public bool ResetBadgeNumberAndStatus(int deviceID)
        {
            
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                var query = @"UPDATE [dbo].[tbl_devices] 
                                SET [BadgeNumber] = 0
                                  ,[deviceActive] = 1
                                  ,[deviceLastAccessed] = getdate()
                             WHERE [deviceID] = @deviceID";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@deviceID", deviceID);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    return true;
                }
                catch (Exception)
                {
                }
            }
            return false;
        }

        public IList<tbl_school_alerts> GetLatestAlertsFromPosted(int alertId, int deviceId, int schoopId, int topNum)
        {
            var list = new List<tbl_school_alerts>();
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                connection.Open();
                var query = @"exec qry_FETCH_New_Alerts @schoopId,@deviceId,@alertId,@topNum";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@schoopId", schoopId);
                command.Parameters.AddWithValue("@deviceId", deviceId);
                command.Parameters.AddWithValue("@alertId", alertId);
                command.Parameters.AddWithValue("@topNum", topNum);
                var reader = command.ExecuteReaderAsync().Result;
                while (reader.Read())
                {
                    var alert = new tbl_school_alerts()
                    {
                        Alert_id = (int)reader["Alert_id"],
                        Alert_text = reader["Alert_text"].ToString(),
                        SurveyGuid = reader["SurveyGuid"] is DBNull ? null : (Guid?)reader["SurveyGuid"],
                        Schoop_id = (int)reader["Schoop_id"],
                        Alert_date_time = reader["Alert_date_time"] is DBNull ? null : (DateTime?)reader["Alert_date_time"],
                        Activeyears = reader["Activeyears"].ToString(),
                        ActiveGroups = reader["ActiveGroups"].ToString(),
                        Alert_urgent = (bool)reader["Alert_urgent"],
                        NewsId = reader["NewsId"] is DBNull ? null : (int?)reader["NewsId"],
                        EventId = reader["EventId"] is DBNull ? null : (int?)reader["EventId"],
                        suggestedSchoopID = reader["suggestedSchoopID"] is DBNull ? null : (int?)reader["suggestedSchoopID"],
                    };
                    list.Add(alert);
                }
                return list;
            }
        }


        public bool CheckIfDeviceRespondSurvey(int deviceId, Guid surveyGuid)
        {
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                connection.Open();
                var query = @"SELECT count(0) FROM [dbo].[tbl_surveys] s 
	                        INNER JOIN [dbo].[tbl_questions] q 
	                        ON s.Survey_ID = q.Survey_ID 
	                        INNER JOIN [dbo].[tbl_answers] a
	                        ON q.Question_ID = a.Question_ID
	                        WHERE s.SurveyGuid = @surveyGuid and a.DeviceID = @deviceId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@surveyGuid", surveyGuid.ToString());
                command.Parameters.AddWithValue("@deviceId", deviceId);
                try
                {
                    connection.Open();
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
                catch (Exception)
                {
                }
            }
            return false;
        }

        public tbl_school_alerts GetAlertById(int alertId)
        {
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                connection.Open();
                var query = @"SELECT * FROM [dbo].[tbl_school_alerts] WHERE [Alert_id] = @alertId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@alertId", alertId);
                var reader = command.ExecuteReaderAsync().Result;
                while (reader.Read())
                {
                    var alert = new tbl_school_alerts()
                    {
                        Alert_id = (int)reader["Alert_id"],
                        Alert_text = reader["Alert_text"].ToString(),
                        SurveyGuid = reader["SurveyGuid"] is DBNull ? null : (Guid)reader["SurveyGuid"],
                        Schoop_id = (int)reader["Schoop_id"],
                        Alert_date_time = reader["Alert_date_time"] is DBNull ? null :  (DateTime?)reader["Alert_date_time"],
                        Activeyears = reader["Activeyears"].ToString(),
                        ActiveGroups = reader["ActiveGroups"].ToString(),
                        Alert_urgent = (bool)reader["Alert_urgent"],
                        NewsId = reader["NewsId"] is DBNull ? null : (int)reader["NewsId"],
                        EventId = reader["EventId"] is DBNull ? null : (int)reader["EventId"],
                        suggestedSchoopID = reader["suggestedSchoopID"] is DBNull ? null : (int)reader["suggestedSchoopID"],
                    };
                    return alert;
                }
                return null;
            }
        }

        public int GetGroupsCountBySchoopId(int schoopId)
        {
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                connection.Open();
                var query = @"SELECT count(0) FROM [dbo].[tbl_organisation_groups] 
	                        WHERE [schoopID] = @schoopId and [Deleted] = 0";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@schoopId", schoopId);
                try
                {
                    connection.Open();
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count;
                }
                catch (Exception)
                {
                }
            }
            return 0;
        }

        public IList<device_alert> GetTopNumDeviceAlertsByDeviceID(int deviceId)
        {
            var list = new List<device_alert>();
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                connection.Open();
                var query = @"exec qry_CollatedAlerts_Alerts_V4 @deviceId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@deviceId", deviceId);
                var reader = command.ExecuteReaderAsync().Result;
                while (reader.Read())
                {
                    var alert = new device_alert()
                    {
                        Alert_id = (int)reader["Alert_id"],
                        Alert_text = reader["Alert_text"].ToString(),
                        SurveyGuid = reader["SurveyGuid"] is DBNull ? null : (Guid?)reader["SurveyGuid"],
                        Schoop_id = (int)reader["Schoop_id"],
                        Alert_date_time = reader["Alert_date_time"] is DBNull ? null : (DateTime?)reader["Alert_date_time"],
                        Activeyears = reader["Activeyears"].ToString(),
                        ActiveGroups = reader["ActiveGroups"].ToString(),
                        Alert_urgent = (bool)reader["Alert_urgent"],
                        NewsId = reader["NewsId"] is DBNull ? null : (int?)reader["NewsId"],
                        EventId = reader["EventId"] is DBNull ? null : (int?)reader["EventId"],
                        suggestedSchoopID = reader["suggestedSchoopID"] is DBNull ? null : (int?)reader["suggestedSchoopID"],
                        CustomDateFormat = reader["CustomDateFormat"].ToString(),
                        EstablishmentName = reader["EstablishmentName"].ToString(),
                        SendByDeviceIds = (bool)reader["SendByDeviceIds"],
                        IsContactFormAlert= (bool)reader["IsContactFormAlert"],
                        IsMigrateAlert = (bool)reader["IsMigrateAlert"],
                        Language_id = reader["Language_id"] is DBNull ? null : (int?)reader["Language_id"],
                    };
                    list.Add(alert);
                }
                return list;
            }
        }

        public bool UpdateDeviceLangId(int deviceId, int languageID)
        {

            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                var query = @"UPDATE [dbo].[tbl_devices] 
                                SET [Language_id] = @languageID
                                  ,[deviceActive] = 1
                                  ,[deviceLastAccessed] = getdate()
                             WHERE [deviceID] = @deviceId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@languageID", languageID);
                command.Parameters.AddWithValue("@deviceId", deviceId);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    return true;
                }
                catch (Exception)
                {
                }
            }
            return false;
        }


        public IList<tbl_organisation_groups> GetUnClosedGroupsBySchoopID(int schoopID)
        {
            var list = new List<tbl_organisation_groups>();
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                connection.Open();
                var query = @"SELECT [groupID]
                              ,[schoopID]
                              ,[activeYears]
                              ,[lastUpdatedOn]
                              ,[displayOrder]
                              ,[parentGroupID]
                              ,[IsClosed]
                              ,[Deleted]
                          FROM [dbo].[tbl_organisation_groups]
                          WHERE [schoopID] = @schoopID AND [IsClosed] = 0 AND [Deleted] = 0";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@schoopID", schoopID);
                var reader = command.ExecuteReaderAsync().Result;
                while (reader.Read())
                {
                    var group = new tbl_organisation_groups()
                    {
                        groupID = (int)reader["groupID"],
                        schoopID = (int)reader["schoopID"],
                        activeYears = reader["activeYears"].ToString(),
                        lastUpdatedOn = reader["lastUpdatedOn"] is DBNull ? null : (DateTime?)reader["lastUpdatedOn"],
                        displayOrder = (decimal)reader["displayOrder"],
                        parentGroupID = (int)reader["parentGroupID"],
                        IsClosed = (bool)reader["IsClosed"],
                        Deleted = (bool)reader["Deleted"],
                    };
                    list.Add(group);
                }
                return list;
            }
        }

        public tbl_group_names GetDefaultGroupName(int groupID)
        {
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                connection.Open();
                var query = @"SELECT [nameID]
                              ,[groupID]
                              ,[languID]
                              ,[groupName]
                              ,[groupDescription]
                          FROM [dbo].[tbl_group_names]
                          WHERE [groupID] = @groupID
                          ORDER BY languID";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@groupID", groupID);
                var reader = command.ExecuteReaderAsync().Result;
                while (reader.Read())
                {
                    var group = new tbl_group_names()
                    {
                        nameID = (int)reader["nameID"],
                        groupID = (int)reader["groupID"],
                        languID = (int)reader["languID"],
                        groupName = reader["groupName"].ToString(),
                        groupDescription = reader["groupDescription"].ToString(),
                    };
                    return group;
                }
                return null;
            }
        }

        public IList<school_groups> GetSchoolGroups2(int schoopId, int topNum)
        {
            var list = new List<school_groups>();
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                connection.Open();
                var query = @"exec qry_FETCH_School_Groups2_By_SchoopID @schoopId, @topNum";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@schoopId", schoopId);
                command.Parameters.AddWithValue("@topNum", topNum);

                var reader = command.ExecuteReaderAsync().Result;
                while (reader.Read())
                {
                    var group = new school_groups()
                    {
                        groupID = (int)reader["groupID"],
                        schoopID = (int)reader["schoopID"],
                        activeYears = reader["activeYears"].ToString(),
                        lastUpdatedOn = reader["lastUpdatedOn"] is DBNull ? null : (DateTime?)reader["lastUpdatedOn"],
                        displayOrder = (decimal)reader["displayOrder"],
                        parentGroupID = (int)reader["parentGroupID"],
                        IsClosed = (bool)reader["IsClosed"],
                        groupName = reader["groupName"].ToString(),
                        groupDescription = reader["groupDescription"].ToString(),
                    };
                    list.Add(group);
                }
                return list;
            }
        }

        public IList<school_groups> GetSchoolGroups3(int schoopId, int langId, int topNum)
        {
            //exec qry_FETCH_School_Groups3_By_SchoopIDAndLanguageID @schoopId, @languageId, @topNum
            var list = new List<school_groups>();
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                connection.Open();
                var query = @"exec qry_FETCH_School_Groups3_By_SchoopIDAndLanguageID @schoopId, @languageId, @topNum";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@schoopId", schoopId);
                command.Parameters.AddWithValue("@languageId", langId);
                command.Parameters.AddWithValue("@topNum", topNum);

                var reader = command.ExecuteReaderAsync().Result;
                while (reader.Read())
                {
                    var group = new school_groups()
                    {
                        groupID = (int)reader["groupID"],
                        schoopID = (int)reader["schoopID"],
                        activeYears = reader["activeYears"].ToString(),
                        lastUpdatedOn = reader["lastUpdatedOn"] is DBNull ? null : (DateTime?)reader["lastUpdatedOn"],
                        displayOrder = (decimal)reader["displayOrder"],
                        parentGroupID = (int)reader["parentGroupID"],
                        IsClosed = (bool)reader["IsClosed"],
                        groupName = reader["groupName"].ToString(),
                        groupDescription = reader["groupDescription"].ToString(),
                    };
                    list.Add(group);
                }
                return list;
            }
        }

        public IList<tbl_active_groups_by_device> GetActiveGroupsBySchoopIdAndDeviceID(int schoopId, int deviceId)
        {
            var list = new List<tbl_active_groups_by_device>();
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                connection.Open();
                var query = @"SELECT ag.*
                              FROM [dbo].[tbl_active_groups_by_device] ag
                              INNER JOIN [dbo].[tbl_organisation_groups] og ON ag.active_groupID = og.groupID
                              WHERE ag.[deviceID] = @deviceId AND og.[schoopID] = @schoopId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@deviceId", deviceId);
                command.Parameters.AddWithValue("@schoopId", schoopId);
                var reader = command.ExecuteReaderAsync().Result;
                while (reader.Read())
                {
                    var aybd = new tbl_active_groups_by_device()
                    {
                        activegroup_id = (int)reader["activegroup_id"],
                        deviceID = (int)reader["deviceID"],
                        active_groupID = (int)reader["active_groupID"],
                    };
                    list.Add(aybd);
                }
                return list;
            }
        }

        public IList<StickyForm> GetLibraryFormsBySchoopIdAndActiveYearsGroups(int schoopId, int deviceId, string activeYears, string activeGroups, int languageId, int topNum)
        {
            var list = new List<StickyForm>();
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
            {
                connection.Open();
                var query = @"exec [qry_FETCH_StickyForms_By_Years_And_Groups] @schoopId, @deviceId, @activeYears, @activeGroups, @langId, @topNum";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@schoopId", schoopId);
                command.Parameters.AddWithValue("@deviceId", deviceId);
                command.Parameters.AddWithValue("@activeYears", activeYears);
                command.Parameters.AddWithValue("@activeGroups", activeGroups);
                command.Parameters.AddWithValue("@langId", languageId);
                command.Parameters.AddWithValue("@topNum", topNum);
                var reader = command.ExecuteReaderAsync().Result;
                while (reader.Read())
                {
                    var form = new StickyForm()
                    {
                        Survey_ID = (int)reader["Survey_ID"],
                        Survey_Name = reader["Survey_Name"].ToString(),
                        SurveyGuid = (Guid)reader["SurveyGuid"],
                        Alert_text = reader["Alert_text"].ToString(),
                    };
                    list.Add(form);
                }
                return list;
            }
        }

    }

}
