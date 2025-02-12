using Microsoft.Data.SqlClient;
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
                        sch6 = reader["Town_name"].ToString(),
                        sLow = reader["StatutoryLowAge"].ToString(),
                        sHigh = reader["StatutoryHighAge"].ToString(),
                        schTel = "0" + reader["TelephoneSTD"].ToString() + " " + reader["TelephoneNum"].ToString(),
                        schEmail = " ",
                        schWeb = reader["WebsiteAddress"].ToString(),
                        schHead = reader["HeadTitle"].ToString() + " " + reader["HeadFirstName"].ToString() + " " + reader["HeadLastName"].ToString(),
                        schActive = (bool)reader["IsActive"],
                        hasYears = false,
                        hasGroups = false,
                        organisationTypeName = "",
                    };
                    return school;
                }
            }

            return null;
        }
    }

}
