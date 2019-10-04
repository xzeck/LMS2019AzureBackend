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
    public static class Check
    {
        [FunctionName("Check")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string token = req.Query["token"]; // Get token from post data, token from post data
            string token_in_database = null; //token in database
            

            //deserialize and read
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            token = token ?? data?.token;
            
            DatabaseConnector conn = new DatabaseConnector();
            SqlConnection connection = conn.connector("Users");
            GenerateResponses Gr = new GenerateResponses();


            connection.Open(); 

            //check if token exists
            SqlCommand cmd = new SqlCommand("select session_token from Users where session_token=@token", connection);
            cmd.Parameters.AddWithValue("@token", token);

            SqlDataReader rdr = cmd.ExecuteReader();

            while(rdr.Read())
            {
                token_in_database = rdr[0].ToString(); //get token value in database
            }

            connection.Close();

            if (token_in_database == token) //check if the tokens match
            {
                //If tokens match, check if the token is valid
                Token tk = new Token();
                bool isValid = tk.IsTokenValid(token);

                if (!isValid) //If Token isn't valid remove token 
                {
                    if (DeleteTokenFromDataBase(token, connection))
                        return Gr.NotAcceptable("Token Expired");
                    else
                        return Gr.InternalServerError("Internal Server error");
                }
                else //Valid token
                    return Gr.OkResponse("Valid Token");
            }
            else //If token doesn't match remove the token from database 
            {
                if (DeleteTokenFromDataBase(token, connection))
                    return Gr.NotAcceptable("Invalid Token");
                else
                    return Gr.InternalServerError("Internal Server error"); //If Internal server error occurs
            }
        }

        static bool DeleteTokenFromDataBase(string Token, SqlConnection conn)
        {
            SqlCommand cmd = new SqlCommand("update table Users set session_token=null where session_token=@token", conn);
            cmd.Parameters.AddWithValue("@token", Token);

            try
            {
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }catch(Exception e)
            {
                return false;
            }

            return true;
        }
    }
}
