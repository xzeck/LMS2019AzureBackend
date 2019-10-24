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
    public static class GetAppliedLeaves
    {
        [FunctionName("GetAppliedLeaves")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {

            GenerateResponses Gr = new GenerateResponses();


            log.LogInformation("C# HTTP trigger function processed a request.");

            string session_token = req.Query["token"];
            string dptmt = null;


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            session_token = session_token ?? data?.token;

            DatabaseConnector DBConn = new DatabaseConnector();
            SqlConnection connection = DBConn.connector("Users");
            SqlCommand cmd = new SqlCommand("select department from Users where session_token=@token", connection);
            cmd.Parameters.AddWithValue("@token", session_token);

            DataTable dt = new DataTable();

            try
            {
                connection.Open();

                SqlDataReader rdr = cmd.ExecuteReader();
                while(rdr.Read())
                {
                    dptmt = rdr[0].ToString();
                }
                rdr.Close();
                connection.Close();

                //select * from leaveapplication where leave_id LIKE '<department>%'
                Console.WriteLine(dptmt);
                SqlCommand cmd_1 = new SqlCommand("select leave_id, username, from_date, to_date, no_of_days, reason, leave_status from leaveapplication where leave_id LIKE @dpt and leave_status='NA'", connection);
                cmd_1.Parameters.AddWithValue("@dpt", dptmt+"%");
                Console.WriteLine(cmd_1.CommandText.ToString());
                SqlDataAdapter DataAdapter = new SqlDataAdapter(cmd_1);

                connection.Open();

                DataAdapter.Fill(dt);

                connection.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return Gr.InternalServerError(Gr.INTSRE);
            }

            DataSet ds = new DataSet();
            ds.Tables.Add(dt);

            string dsXML = ds.GetXml(); 

            return Gr.OkResponse(dsXML);
        }
    }
}
