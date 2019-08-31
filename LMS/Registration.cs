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
using SendGrid;
using SendGrid.Helpers;
using SendGrid.Helpers.Mail;


namespace LMS
{
    public static class Registration
    {

        [FunctionName("Registration")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request. - Registration");

            GenerateResponses Gr = new GenerateResponses();

            string uname = req.Query["uname"];
            string pswrd = req.Query["pswrd"];
            string email = req.Query["email"];
            string fname = req.Query["fname"];
            string dptmt = req.Query["dptmt"];

            //Check if User already exists
            bool DoesUserExist = CheckIfUserAlreadyExist(email);

            if(DoesUserExist) 
                return Gr.Forbidden("An account with the email already  exists");


            //Check if Username is taken
            bool IsUserNameTaken = CheckIfUserNameAlreadyExist(uname);

            if(IsUserNameTaken)
                return Gr.Forbidden("User name already exits,  please enter a different one");
            

            //Generate OTP
            OTPGen gen = new OTPGen(uname,
                                    email);
            gen.GenerateOTP();
            var EmailSentAction = gen.SendOTP();
            gen.PushHash();


            if (EmailSentAction.Equals(1))
                return Gr.RequestTimeOut("Error email cannot be sent");

            return Gr.OkResponse("Ok");
        }

        public static bool CheckIfUserAlreadyExist(string email)
        {
            DatabaseConnector DBConn = new DatabaseConnector();

            SqlConnection connection = DBConn.connector();

            SqlDataReader reader;

            string em = null; 

            SqlCommand cmd = new SqlCommand("select email from users where email=@email", connection);

            cmd.Parameters.AddWithValue("@email", email);

            connection.Open();

            reader = cmd.ExecuteReader();

            while(reader.Read())
            {
                em = reader[0].ToString();
            }
            connection.Close();

            if(!string.IsNullOrEmpty(em))
            {
                return true;
            }

            return false;
        }

        public static bool  CheckIfUserNameAlreadyExist(string uname)
        {
            DatabaseConnector DBConn = new DatabaseConnector();

            SqlConnection connection = DBConn.connector();

            SqlDataReader reader;

            string un = null;

            SqlCommand cmd = new SqlCommand("select username from users where username=@uname", connection);

            cmd.Parameters.AddWithValue("@uname", uname);

            connection.Open();

            reader = cmd.ExecuteReader();


            while (reader.Read())
            {
                un = reader[0].ToString();
            }

            connection.Close();
            if (!string.IsNullOrEmpty(un))
                return true;
           
            return false;
        }
    }
}
