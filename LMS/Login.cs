using System;
using System.IO;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;


namespace LMS
{
    public static class Login
    {
        [FunctionName("Login")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            //getting username
            string uname = req.Query["uname"];
            string pswrd = req.Query["pswrd"];
            string username = null;
            string pswrd_hash = null;
            GenerateResponses Gr = new GenerateResponses();
            SqlDataReader reader;
            byte[] Hash;
            SHA256 sha256 = SHA256.Create();
            Encoding enc = Encoding.UTF8;
            StringBuilder hashbuilder = new StringBuilder();
            Hash = sha256.ComputeHash(enc.GetBytes(pswrd));

            foreach(var h in Hash)
            {
                hashbuilder.Append(h.ToString("x2"));
            }

            pswrd = hashbuilder.ToString();
            

            //check if uname or pswrd is null
            if (string.IsNullOrEmpty(uname) || string.IsNullOrEmpty(pswrd))
            {
                return new BadRequestObjectResult("Password or Username Empty");
            }

            DatabaseConnector DB_Con = new DatabaseConnector(); //Object for database connector
            SqlConnection connection = DB_Con.connector(); //Returns a DB connection

            //If connection is not established, send internal server error
            if(connection == null)
                return Gr.InternalServerError("Internal server error cannot connect to Database"); // Ends if connection to database cannot be established


            connection.Open();

            SqlCommand sqlCommand = new SqlCommand("select username, password_hash from Users where username='"+uname+"'", connection);

            reader = sqlCommand.ExecuteReader();

            while (reader.Read())
            {
                username = reader[0].ToString();
                pswrd_hash = reader[1].ToString();
            }

            connection.Close();

            if(string.IsNullOrEmpty(username))
                return Gr.BadRequest("Entered Username doesn't exist");

            if(pswrd == pswrd_hash)
            {
                Token tk = new Token();

                string token = tk.GenerateToken();
                bool token_val = tk.IsTokenValid(token);

                return Gr.OkResponse(token_val.ToString());

            }
            else
                return Gr.BadRequest("username or password incorrect");
        }
    }
}
