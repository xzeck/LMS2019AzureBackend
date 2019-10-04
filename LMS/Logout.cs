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
            [HttpTrigger(AuthorizationLevel.Anonymous,"post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request. - Logout");

            string session_token = req.Query["token"]; //Get Session token from POST

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            session_token = session_token ?? data?.token; //get the token if serialized
            GenerateResponses Gr = new GenerateResponses();

            if (string.IsNullOrEmpty(session_token))
            {
                Gr.BadRequest("Token Empty");
            }
            
            try
            {  
                DatabaseConnector DBconn = new DatabaseConnector(); //Create object to connect to Database
                SqlConnection connection = DBconn.connector("Users"); //connect to User database

                //Command to delete tokens which are expired
                SqlCommand command = new SqlCommand("update Users set session_token=null where session_token=@token", connection);

                //Add value to the token
                command.Parameters.AddWithValue("@token", session_token);

                connection.Open(); 
                command.ExecuteNonQuery();
                connection.Close();

                return Gr.OkResponse("Token invalidated"); //Invalidate Token on logout
            }catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                return Gr.BadRequest("Token Not invalidated");
            }

        }
    }
}
