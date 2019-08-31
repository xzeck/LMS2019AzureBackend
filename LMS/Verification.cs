using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using Newtonsoft.Json;
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

            var VerifCode = req.Query["code"];
            var uname = req.Query["uname"];
            byte[] Hash = null;
            StringBuilder sb = new StringBuilder();
            StringBuilder HashString = new StringBuilder();

            
            try
            {
                SHA256 sha256 = SHA256.Create();
                Encoding enc = Encoding.UTF8;
                foreach(var un in uname)
                {
                    sb.Append(un);
                }

                foreach(var vc in VerifCode)
                {
                    sb.Append(vc);
                }

                Hash = sha256.ComputeHash(enc.GetBytes(sb.ToString()));

            }catch(Exception e)
            {
                Console.WriteLine("Error while generating hash :" + e);
            }

            foreach(var h in Hash)
            {
                HashString.Append(h.ToString("x2"));
            }

            var res = new OkObjectResult(HashString.ToString());
            res.StatusCode = StatusCodes.Status200OK;

            return res;
        }

        
    }
}
