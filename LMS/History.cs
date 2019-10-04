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
    public static class History
    {
        [FunctionName("History")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string token = req.Query["token"];
            string uname = null;
            string[] Data = null;

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            token = token ?? data?.token;

            Console.WriteLine(token); 

            DatabaseConnector conn = new DatabaseConnector();
            SqlConnection connection = conn.connector("Users");
            SqlDataReader reader;
            GenerateResponses Gr = new GenerateResponses();

            if (connection == null)
                Gr.InternalServerError("Internal Server Error");

            try
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand("select username from Users where session_token=@token", connection);
                cmd.Parameters.AddWithValue("@token", token);


                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    uname = reader[0].ToString();
                }

                connection.Close();
            }catch(Exception e)
            {
                Gr.InternalServerError(e.ToString());
            }


            SqlCommand cmd_1 = new SqlCommand("select leave_id from_date, to_date, no_of_days, reason, leave_type from leaveapplication where username=@uname", connection);
            cmd_1.Parameters.AddWithValue("@uname", uname);

            try
            {
                connection.Open();

                reader = cmd_1.ExecuteReader();

                int affectedrows = reader.RecordsAffected;

                Console.WriteLine(affectedrows);


                Data = new string[6];

                while (reader.Read())
                {
                   
                        for (int i = 0; i < 6; i++)
                        {

                            Data[i] = reader[i].ToString();
                        }
                    
                }

                connection.Close();
            }catch(Exception e)
            {
                Gr.InternalServerError(e.ToString());
            }
            var json = JsonConvert.SerializeObject(Data);

            return Gr.OkResponse(json);
        }
    }
}
