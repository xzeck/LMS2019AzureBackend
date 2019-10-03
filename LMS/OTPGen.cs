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
using System.Text;
using System.Net.Mail;
using System.Net.Mime;

namespace LMS
{
    class OTPGen
    {
        private string OTP;
        private string uname;
        private string email;


        public void GenerateOTP()
        {
            Random rng = new Random(); //Random Number Generator
            StringBuilder sb = new StringBuilder(); //String builder for the RNG string

            //Loop through n times to generate OTP
            for(int i = 0;i < 5; i++)
            {
                sb.Append(rng.Next(0, 9));
            }

            OTP = sb.ToString(); //Convert to string
        }

        public async Task<IActionResult> SendOTP()
        {
            try
            {
                var apiKey = Environment.GetEnvironmentVariable("SENDGRID_APIKEY"); //Get API Key
                var client = new SendGridClient(apiKey); //Initialize client

                var from = new EmailAddress("admin@ajaynair.xyz", "Admin"); //From email address and recipient name
                var subject = "no reply"; //Email Subject
                var to = new EmailAddress(email, uname); //To emeail address and recipient name
                var plainTextContent = ""; //Plain text


                var htmlContent = Properties.Resources.HTML.ToString();
                htmlContent = htmlContent.Replace("{code}", OTP);
                

                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent); //Generate email

                var response = await client.SendEmailAsync(msg); //Send email

                return new OkObjectResult(0);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception - Reg : " + e);

                return new BadRequestObjectResult(-1);
            }
        }

        public void PushHash()
        {
            GenerateHash GH = new GenerateHash(); // Init GenerateHash

            string hash = GH.Generate(email+OTP); // Generate hash from email and OTP

            DatabaseConnector DBConn = new DatabaseConnector(); // connect to database
            SqlConnection conn = DBConn.connector("Users"); // start an SQL connection
            

            conn.Open(); // Open connection to database

            // Push OTP value to database
            SqlCommand cmd = new SqlCommand("insert into OTP ([OTP_HASH]) values(@hash)", conn);
            cmd.Parameters.AddWithValue("@hash", hash); 
            int affectedrows = cmd.ExecuteNonQuery(); // Execute non query

            Console.WriteLine(affectedrows);
            
        }

        public OTPGen(string un, string em)
        {
            uname = un;
            email = em;
        }
    }
}
