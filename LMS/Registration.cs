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


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            email = email ?? data?.email;
            uname = uname ?? data?.uname;
            pswrd = pswrd ?? data?.pswrd;
            fname = fname ?? data?.fname;
            dptmt = dptmt ?? data?.dptmt;

            //Check if User already exists
            bool DoesUserExist = CheckIfUserAlreadyExist(email);

            if(DoesUserExist) 
                return Gr.Forbidden("An account with the email already  exists"); 


            //Check if Username is taken
            bool IsUserNameTaken = CheckIfUserNameAlreadyExist(uname);

            if(IsUserNameTaken)
                return Gr.Forbidden("User name already exits,  please enter a different one"); 
            

            //Initialize OTPGen Class
            OTPGen gen = new OTPGen(uname,
                                    email);
            gen.GenerateOTP(); //Generate OTP
            var EmailSentAction = gen.SendOTP(); //Send Email
            gen.PushHash(); //Push OTP hash to Database


            if(EmailSentAction.Equals(1))
                return Gr.RequestTimeOut("Error email cannot be sent"); 

            return Gr.OkResponse("Ok");
        }

        public static bool CheckIfUserAlreadyExist(string email)
        {
            DatabaseConnector DBConn = new DatabaseConnector();

            SqlConnection connection = DBConn.connector("Users");

            SqlDataReader reader;

            string em = null;

            SqlCommand cmd = new SqlCommand("select email from Users where email=@email", connection);

            Console.WriteLine("\n" + email);
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

            SqlConnection connection = DBConn.connector("Users");

            SqlDataReader reader;

            string un = null;

            SqlCommand cmd = new SqlCommand("select username from Users where username=@uname", connection);
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
