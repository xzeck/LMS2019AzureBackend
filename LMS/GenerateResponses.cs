using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;


namespace LMS
{
    class GenerateResponses
    {
        public ObjectResult OkResponse(string message)
        {
            var response = new ObjectResult(message); 
            response.StatusCode = StatusCodes.Status200OK;

            return response;
        }

        public ObjectResult InternalServerError(string message)
        {
            var response = new ObjectResult(message);
            response.StatusCode = StatusCodes.Status500InternalServerError;

            return response;
        }

        public ObjectResult NotAcceptable(string message)
        {
            var response = new ObjectResult(message);
            response.StatusCode = StatusCodes.Status406NotAcceptable;

            return response;
        }

        public ObjectResult Forbidden(string message)
        {
            var response = new ObjectResult(message);
            response.StatusCode = StatusCodes.Status403Forbidden;

            return response;
        }

        public ObjectResult BadRequest(string message)
        {
            var response = new ObjectResult(message);
            response.StatusCode = StatusCodes.Status400BadRequest;

            return response;
        }

        public ObjectResult RequestTimeOut(string message)
        {
            var response = new ObjectResult(message);
            response.StatusCode = StatusCodes.Status419AuthenticationTimeout;

            return response;
        }
    }
}
