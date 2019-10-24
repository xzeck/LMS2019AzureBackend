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
using SendGrid;
using SendGrid.Helpers.Mail;

namespace LMS
{
    public static class Reject
    {
        [FunctionName("Reject")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string token = req.Query["token"];
            string Leave_ID = req.Query["LeaveID"];

            GenerateResponses Gr = new GenerateResponses();

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            token = token ?? data?.name;
            Leave_ID = Leave_ID ?? data?.LeaveID;


            Console.WriteLine(Leave_ID);
            //update leavapplication set leave_status='A' where leave_id in ($values);

            DatabaseConnector DBConn = new DatabaseConnector();
            SqlConnection connection = DBConn.connector("Users");

            connection.Open();

            SqlCommand UpdateLeaveStatus = new SqlCommand("update leaveapplication set leave_status='R' where leave_id=@Leave_ID", connection);
            UpdateLeaveStatus.Parameters.AddWithValue("@Leave_ID", Leave_ID);

            SqlCommand GetUserName = new SqlCommand("select username from Users where session_token=@token", connection);
            GetUserName.Parameters.AddWithValue("@token", token);

            UpdateLeaveStatus.ExecuteNonQuery();

            SqlDataReader reader = GetUserName.ExecuteReader();
            reader.Close();
            Console.WriteLine("Days and Types Done");

            reader = GetUserName.ExecuteReader();

            string uname, email;
            uname = email = null;

            while (reader.Read())
            {
                uname = reader[0].ToString();

            }


            reader.Close();

            _ = await SendEmail(email, uname);

            connection.Close();

            return Gr.OkResponse(Gr.OKRESP);
        }

        public static async Task<IActionResult> SendEmail(string email, string uname)
        {
            try
            {
                var apiKey = Environment.GetEnvironmentVariable("SENDGRID_APIKEY");
                var client = new SendGridClient(apiKey);
                var from = new EmailAddress("noreply@ajaynair.xyz", "Admin");
                var subject = "no reply";
                var to = new EmailAddress(email, uname);
                var plainTextContent = "Your application has been evaluated and leave has been rejected to you for the applied period";
                var htmlContent = "<h3> Leave Applied Status : Rejected <h3>" + plainTextContent;

                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

                var response = await client.SendEmailAsync(msg);

                return new OkObjectResult(0);
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);

                return new OkObjectResult(-1);
            }

        }
    }
}
