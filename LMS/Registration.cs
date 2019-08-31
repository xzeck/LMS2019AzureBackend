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

            string uname = req.Query["uname"];
            string pswrd = req.Query["pswrd"];
            string email = req.Query["email"];
            string fname = req.Query["fname"];
            string dptmt = req.Query["dptmt"];


            OTPGen gen = new OTPGen(uname,
                                    email);
            gen.GenerateOTP();
            var EmailSentAction = gen.SendOTP();



            if (EmailSentAction.Equals(1))
            {
                var BadRes = new ObjectResult("Error");
                BadRes.StatusCode = StatusCodes.Status419AuthenticationTimeout;

                return BadRes;
            }

            var res = new ObjectResult("Ok");
            res.StatusCode = StatusCodes.Status200OK;
            return res;
        }
        /*
        public static async Task<IActionResult> Verification(string email, string uname)
        {
            try
            {
                var apiKey = Environment.GetEnvironmentVariable("SENDGRID_APIKEY");
                var client = new SendGridClient(apiKey);

                var code = OTPGen();

                var from = new EmailAddress("admin@ajaynair.xyz", "Admin");
                var subject = "Email Verification";
                var to = new EmailAddress(email, uname);
                var plainTextContent = "Please use the below code to verify your email";
                var htmlContent = "<strong>Code : " + code + "</strong>";
                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                var response = await client.SendEmailAsync(msg);

                return new OkObjectResult(0);
            }catch(Exception e)
            {
                Console.WriteLine("Exception - Reg : " + e);

                return new BadRequestObjectResult(-1);
            }
        }*/
    }
}
