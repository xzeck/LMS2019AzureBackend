using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;


namespace LMS
{
    public static class Verification
    {
        [FunctionName("Verification")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.- Verification");

            /*DatabaseConnector DBConn = new DatabaseConnector(); //Database Connection class object
            SqlConnection connection = DBConn.connector(); //Connect to the Database
            GenerateResponses Gr = new GenerateResponses();

            PushUserData(req: req, connection: connection);

            return Gr.OkResponse("Ok");*/

            var VerifCode = req.Query["code"]; //Get Verification Code
            var email = req.Query["email"]; //Get Email
            GenerateHash GH = new GenerateHash(); //Object for Hash Generating class
            string Hash = GH.Generate(email, VerifCode); // Generate SHA256 hash from email and verification code
            string Hash_val = null; //Initialize Hash_val
            SqlDataReader rdr; //SQL Data Reader
            DatabaseConnector DBConn = new DatabaseConnector(); //Database Connection class object
            SqlConnection connection = DBConn.connector(); //Connect to the Database
            GenerateResponses Gr = new GenerateResponses();



            //Check if connection is null, if null return internal server error since database is not connected
            if (connection == null)
            {
                var error_res = new BadRequestObjectResult("Error");
                error_res.StatusCode = StatusCodes.Status500InternalServerError;

                return error_res;
            } // End if database cannot be connected

            
            //Open connection
            connection.Open();
            Console.WriteLine("Hash:" + Hash);

            //Generate SQL query
            SqlCommand cmd = new SqlCommand("select OTP_hash from OTP where OTP_hash=@hash", connection);
            cmd.Parameters.AddWithValue("@hash", Hash);

            //Execute query and put data into rdr
            rdr = cmd.ExecuteReader();
            
            //Read data, get the first data in the array since there's only one coloumn
            while(rdr.Read())
            {
                Hash_val = rdr[0].ToString();
            }
            rdr.Close();
            Console.WriteLine(Hash_val); //Debug


            //If Val is null or empty, it means that specific hash doesn't exist and OTP is invalid for that email
            if (string.IsNullOrEmpty(Hash_val))
                return Gr.NotAcceptable("Invalid OTP");// Ends if OTP is invalid

           

            //Generate SQL query to delete OTP_Hash from the table
            cmd = new SqlCommand("delete from OTP where OTP_hash=@hash", connection);
            cmd.Parameters.AddWithValue("@hash", Hash);
            cmd.ExecuteNonQuery(); //Execute the command

            PushUserData(req: req, connection);

            connection.Close();
            //Return value
            return Gr.OkResponse("Valid OTP"); 
        }

        static void PushUserData(HttpRequest req, SqlConnection connection)
        {
            GenerateResponses Gr = new GenerateResponses();
            GenerateHash GH = new GenerateHash();

            string uname = req.Query["uname"];
            string pswrd = req.Query["pswrd"];
            string fname = req.Query["fname"];
            string dptmt = req.Query["dptmt"];
            string email = req.Query["email"];
            string salt =  GH.GenerateSalt().ToString();

            pswrd = GH.GeneratePasswordHash(pswrd);
            Console.WriteLine(pswrd);

            //connection.Open();

            SqlCommand cmd = new SqlCommand("insert into users (username, password_hash, salt, first_name, department, email)" +
                                             "values(@uname,@pswrd, @salt, @fname, @dptmt, @email)", connection);

            cmd.Parameters.AddWithValue("@uname", uname);
            cmd.Parameters.AddWithValue("@pswrd", pswrd);
            cmd.Parameters.AddWithValue("@salt",  salt );
            cmd.Parameters.AddWithValue("@fname", fname);
            cmd.Parameters.AddWithValue("@dptmt", dptmt);
            cmd.Parameters.AddWithValue("@email", email);

            int rowsaffected = cmd.ExecuteNonQuery();

            Console.WriteLine(rowsaffected);

            connection.Close();
        }

        
    }
}
