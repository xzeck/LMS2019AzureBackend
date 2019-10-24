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
    public static class Grant
    {
        [FunctionName("Grant")]
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
            token = token ?? data?.token;
            Leave_ID = Leave_ID ?? data?.LeaveID;
           

            Console.WriteLine(Leave_ID);
            //update leavapplication set leave_status='A' where leave_id in ($values);

            DatabaseConnector DBConn = new DatabaseConnector();
            SqlConnection connection = DBConn.connector("Users");
            
            connection.Open();

            SqlCommand UpdateLeaveStatus = new SqlCommand("update leaveapplication set leave_status='A' where leave_id=@Leave_ID" , connection);
            UpdateLeaveStatus.Parameters.AddWithValue("@Leave_ID", Leave_ID);

            SqlCommand GetDaysAndType = new SqlCommand("select no_of_days, leave_type from leaveapplication where leave_id=@Leave_ID", connection);
            GetDaysAndType.Parameters.AddWithValue("@Leave_ID", Leave_ID);

            SqlCommand GetUserName = new SqlCommand("select username from leaveapplication where leave_id=@LeaveID", connection);
            GetUserName.Parameters.AddWithValue("@LeaveID", Leave_ID);

           

            UpdateLeaveStatus.ExecuteNonQuery();

            SqlDataReader reader = GetDaysAndType.ExecuteReader();
            string days, type;
            days = type = null; 

            while(reader.Read())
            {
                days = reader[0].ToString();
                type = reader[1].ToString();
            }
            reader.Close();
            Console.WriteLine("Days and Types Done");


            reader = GetUserName.ExecuteReader();

            string uname, email;
            uname = email = null;

            while(reader.Read())
            {
                uname = reader[0].ToString();
            }

            reader.Close();

            SqlCommand getEmailOfUser = new SqlCommand("select email from Users where username=@uname", connection);
            getEmailOfUser.Parameters.AddWithValue("@uname", uname);

            reader = getEmailOfUser.ExecuteReader();

            while(reader.Read())
            {
                email = reader[0].ToString();
            }

            reader.Close();

            switch (type)
            {
                case "CA":
                   

                    SqlCommand CACmd = new SqlCommand("update Users set CA=CA-@days where username=@uname", connection);
                    CACmd.Parameters.AddWithValue("@days", days);
                    CACmd.Parameters.AddWithValue("@uname", uname);
                    Console.WriteLine(CACmd.CommandText.ToString());
                    CACmd.ExecuteNonQuery();
                    break;

                case "SI":
                    SqlCommand SICmd = new SqlCommand("update Users set SI=SI-@days where username=@uname", connection);
                    SICmd.Parameters.AddWithValue("@days", days);
                    SICmd.Parameters.AddWithValue("@uname", uname);
                    Console.WriteLine("SI");
                    SICmd.ExecuteNonQuery();
                    break;

                case "ER":
                    SqlCommand ERCmd = new SqlCommand("update Users set ER=ER-@days where username=@uname", connection);
                    ERCmd.Parameters.AddWithValue("@days", days);
                    ERCmd.Parameters.AddWithValue("@uname", uname);
                    Console.WriteLine("ER");
                    ERCmd.ExecuteNonQuery();
                    break;

                default:
                    Gr.InternalServerError(Gr.INTSRE);
                    break;

            }
            connection.Close();

            _ = await SendEmail(email, uname);
            

            return Gr.OkResponse(Gr.OKRESP);
        }

        public static async Task<IActionResult>  SendEmail(string email, string uname)
        {
            try
            {
                var apiKey = Environment.GetEnvironmentVariable("SENDGRID_APIKEY");
                var client = new SendGridClient(apiKey);
                var from = new EmailAddress("noreply@ajaynair.xyz", "Admin");
                var subject = "no reply";
                var to = new EmailAddress(email, uname);
                var plainTextContent = "Your application has been evaluated and leave has been granted to you for the applied period";
                var htmlContent = "<h3> Leave Applied Status : Approved <h3>";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

                var response = await client.SendEmailAsync(msg);

                return new OkObjectResult(0);
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);

                return new OkObjectResult(e.Message); 
            }

        }
    }
}
