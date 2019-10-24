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
using System.Data;

namespace LMS
{
    public static class RemainingLeaves
    {
        [FunctionName("RemainingLeaves")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string session_token = req.Query["token"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            session_token = session_token ?? data?.token;

            DatabaseConnector DBConn = new DatabaseConnector();
            SqlConnection connection = DBConn.connector("Users");
            GenerateResponses Gr = new GenerateResponses();



            SqlCommand cmd = new SqlCommand("select CA, SI, ER from Users where session_token=@token", connection);
            cmd.Parameters.AddWithValue("@token", session_token);
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();

            try
            {
                connection.Open();

                adapter.Fill(dt);

                connection.Close();
            }
            catch (Exception e)
            {

                Gr.InternalServerError(e.ToString());
            }

            DataSet ds = new DataSet();
            ds.Tables.Add(dt);
            string dsXML = ds.GetXml();

            return Gr.OkResponse(dsXML);
        }
    }
}
