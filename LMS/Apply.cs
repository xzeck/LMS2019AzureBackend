using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data.SqlClient;

namespace LMS
{
    public static class Apply
    {
        private const string InternalServerError = "Internal server error";

        [FunctionName("Apply")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request. - Apply");

            GenerateResponses Gr = new GenerateResponses();

            string s_From = req.Query["from"];
            string s_To = req.Query["to"];



            string Reason = req.Query["reason"];
            string Session_Token = req.Query["token"];
            string Type = req.Query["type"];

            string uname = null;
            string department = null;


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            s_From = s_From ?? data?.from;
            s_To = s_To ?? data?.to;

            DateTime From = DateTime.ParseExact(s_From, "dd/MM/yyyy", null);
            DateTime To = DateTime.ParseExact(s_To, "dd/MM/yyyy", null);


            Reason = Reason ?? data?.reason;
            Session_Token = Session_Token ?? data?.token;
            Type = Type ?? data?.type;


            DatabaseConnector DBconn = new DatabaseConnector();
            SqlConnection connection = DBconn.connector("Users");
            GenerateHash GH = new GenerateHash();
            SqlDataReader reader;

            Session_Token = Session_Token.Replace(" ", "");

            SqlCommand command_Retrieve_Uname_Dept = new SqlCommand("select username, department from Users where session_token=@token", connection);
            command_Retrieve_Uname_Dept.Parameters.AddWithValue("@token", Session_Token);

            connection.Open();
            Console.WriteLine("From" + From.ToShortDateString());
            reader = command_Retrieve_Uname_Dept.ExecuteReader();

            while (reader.Read())
            {
                uname = reader[0].ToString();
                department = reader[1].ToString();
            }

            connection.Close();

            if (string.IsNullOrEmpty(uname) || string.IsNullOrEmpty(department))
                return Gr.InternalServerError(InternalServerError);


            var LeaveDays = (To - From).TotalDays;
            var LeaveID = department + GH.GenerateSalt();

            connection.Open();

            s_From = From.ToShortDateString();
            s_To = To.ToShortDateString();

            SqlCommand command_Push_Data_Into_Leave = new SqlCommand("insert into leaveapplication(leave_id, from_date, to_date, no_of_days, reason, leave_type, username) values (@LeaveID, @From, @To, @Days, @Reason, @Type, @uname)", connection);
            command_Push_Data_Into_Leave.Parameters.AddWithValue("@LeaveID", LeaveID);
            command_Push_Data_Into_Leave.Parameters.AddWithValue("@From", s_From);
            command_Push_Data_Into_Leave.Parameters.AddWithValue("@To", s_To);
            command_Push_Data_Into_Leave.Parameters.AddWithValue("@Days", LeaveDays);
            command_Push_Data_Into_Leave.Parameters.AddWithValue("@Reason", Reason);
            command_Push_Data_Into_Leave.Parameters.AddWithValue("@Type", Type);
            command_Push_Data_Into_Leave.Parameters.AddWithValue("@uname", uname);

            command_Push_Data_Into_Leave.ExecuteNonQuery();

            connection.Close();

            return Gr.OkResponse("Ok");
        }
    }
}