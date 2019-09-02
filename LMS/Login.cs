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

            
            string uname = req.Query["uname"];// get username
            string pswrd = req.Query["pswrd"];// get password
            string username = null;
            string pswrd_hash = null;
            GenerateResponses Gr = new GenerateResponses();  // Initializing response generator
            SqlDataReader reader; //Sql Data Reaeder
            byte[] Hash; // Store hash bytes
            SHA256 sha256 = SHA256.Create(); // SHA256 generator
            Encoding enc = Encoding.UTF8; // Encoding method
            StringBuilder hashbuilder = new StringBuilder();  // hash string builder


            Hash = sha256.ComputeHash(enc.GetBytes(pswrd)); // Compute SHA256 hash for password
            
            // Convert each value in the hash byte to hex and put inside hashbuilder
            foreach(var h in Hash)
            {
                hashbuilder.Append(h.ToString("x2"));
            }

            // get the SHA256 hex for the hash
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


            connection.Open(); // Open connection to database

            // SQL query to get username
            SqlCommand sqlCommand = new SqlCommand("select username, password_hash from Users where username=@uname", connection);
            sqlCommand.Parameters.AddWithValue("@uname", uname);

            reader = sqlCommand.ExecuteReader(); // Execute query and read data

            // Get username and password
            while (reader.Read())
            {
                username = reader[0].ToString();
                pswrd_hash = reader[1].ToString();
            }

            connection.Close(); // Close connection to database

            // Check if username is empty
            if(string.IsNullOrEmpty(username))
                return Gr.BadRequest("Entered Username doesn't exist");

            // Check the password with password hash and generate token
            if(pswrd == pswrd_hash)
            {
                Token tk = new Token(); // Init token class

                string token = tk.GenerateToken(); // Generate token and store
                bool token_val = tk.IsTokenValid(token); // check if token is valid

                return Gr.OkResponse(token_val.ToString()); // return token value if token is valid

            }
            else
                return Gr.BadRequest("username or password incorrect"); // return if password is incorrect
        }
    }
}
