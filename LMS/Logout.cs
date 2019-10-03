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
    public static class Logout
    {
        [FunctionName("Logout")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function,"post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request. - Logout");

            string session_token = req.Query["token"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            session_token = session_token ?? data?.session_token;


            GenerateResponses Gr = new GenerateResponses();
            DatabaseConnector DBconn = new DatabaseConnector();
            SqlConnection connection = DBconn.connector("Users");

            SqlCommand command = new SqlCommand("delete from session_tokens where tokens=@token", connection);

            command.Parameters.AddWithValue("@token", session_token);

            connection.Open();
            command.ExecuteNonQuery();
            connection.Close(); 

            return Gr.OkResponse("Token invalidated");    

        }
    }
}
