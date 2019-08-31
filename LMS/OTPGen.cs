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


namespace LMS
{
    class OTPGen
    {
        private string OTP;
        private string uname;
        private string email;


        public void GenerateOTP()
        {
            Random rng = new Random();
            StringBuilder sb = new StringBuilder();

            for(int i = 0;i < 5; i++)
            {
                sb.Append(rng.Next(0, 9));
            }

            OTP = sb.ToString();
        }

        public async Task<IActionResult> SendOTP()
        {
            try
            {
                var apiKey = Environment.GetEnvironmentVariable("SENDGRID_APIKEY");
                var client = new SendGridClient(apiKey);

                var from = new EmailAddress("admin@ajaynair.xyz", "Admin");
                var subject = "Email Verification";
                var to = new EmailAddress(email, uname);
                var plainTextContent = "Please use the below code to verify your email";
                var htmlContent = "<strong>Code : " + OTP + "</strong>";
                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                var response = await client.SendEmailAsync(msg);

                return new OkObjectResult(0);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception - Reg : " + e);

                return new BadRequestObjectResult(-1);
            }
        }

        public OTPGen(string un, string em)
        {
            uname = un;
            email = em;
        }
    }
}
