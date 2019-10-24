using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using Newtonsoft.Json;

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

            string VerifCode = req.Query["code"]; //Get Verification Code
            string email = req.Query["email"]; //Get Email
            string uname = req.Query["uname"];
            string pswrd = req.Query["pswrd"];
            string fname = req.Query["fname"];
            string dptmt = req.Query["dptmt"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            VerifCode = VerifCode ?? data?.code;
            email = email ?? data?.email;
            uname = uname ?? data?.uname;
            pswrd = pswrd ?? data?.pswrd;
            fname = fname ?? data?.fname;
            dptmt = dptmt ?? data?.dptmt;

            string[] Data = { email, uname, pswrd, fname, dptmt };

            GenerateHash GH = new GenerateHash(); //Object for Hash Generating class
            string Hash = GH.Generate(email+VerifCode); // Generate SHA256 hash from email and verification code
            string Hash_val = null; //Initialize Hash_val
            SqlDataReader rdr; //SQL Data Reader
            DatabaseConnector DBConn = new DatabaseConnector(); //Database Connection class object
            SqlConnection connection = DBConn.connector("Users"); //Connect to the Database
            GenerateResponses Gr = new GenerateResponses();



            //Check if connection is null, if null return internal server error since database is not connected
            if (connection == null)
                Gr.InternalServerError("Error connecting to database - Verification");
                // End if database cannot be connected

            
            //Open connection
            connection.Open();
            //Console.WriteLine("Hash:" + Hash);

            //Generate SQL query
            SqlCommand cmd = new SqlCommand("select OTP_Hash from OTP where OTP_hash=@hash", connection);
            cmd.Parameters.AddWithValue("@hash", Hash);

            //Execute query and put data into rdr
            rdr = cmd.ExecuteReader();
            
            //Read data, get the first data in the array since there's only one coloumn
            while(rdr.Read())
            {
                Hash_val = rdr[0].ToString();
            }
            rdr.Close();
            //Console.WriteLine(Hash_val); //Debug


            //If Val is null or empty, it means that specific hash doesn't exist and OTP is invalid for that email
            if (string.IsNullOrEmpty(Hash_val))
                return Gr.NotAcceptable("Invalid OTP");// Ends if OTP is invalid

            connection.Close();

            PushUserData(Data, connection);

            connection.Open();
            //Generate SQL query to delete OTP_Hash from the table
            cmd = new SqlCommand("delete from OTP where OTP_hash=@hash", connection);
            cmd.Parameters.AddWithValue("@hash", Hash);
            cmd.ExecuteNonQuery(); //Execute the command

            connection.Close();
            //Return value
            return Gr.OkResponse("Valid OTP"); 
        }

        static void PushUserData(String[] req_UserData, SqlConnection connection)
        {
            Console.WriteLine("Push User Data");
            GenerateResponses Gr = new GenerateResponses();
            GenerateHash GH = new GenerateHash();

            string email = req_UserData[0];
            string uname = req_UserData[1];
            string pswrd = req_UserData[2];
            string fname = req_UserData[3];
            string dptmt = req_UserData[4];
            string salt = GH.GenerateSalt();
            Console.WriteLine("pswrd" + pswrd);
            pswrd = GH.Generate(pswrd + salt);

            connection.Open(); 

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
