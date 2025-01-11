using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System.Net;
using System.Security.Authentication;

namespace AuthService.Exceptions
{
    public class BackOfficeExceptionHandler : AbstractExceptionHandler
    {
        public BackOfficeExceptionHandler(RequestDelegate next) : base(next) { }
        public override (HttpStatusCode code, string message) GetResponse(Exception exception)
        {
            HttpStatusCode code;
            switch (exception)
            {
                case KeyNotFoundException:
                case FileNotFoundException:
                    code = HttpStatusCode.NotFound;
                    break;
                case UnauthorizedAccessException:
                    code = HttpStatusCode.Unauthorized;
                    break;
                case ArgumentException:
                case InvalidOperationException:
                    code = HttpStatusCode.BadRequest;
                    break;
                default:
                    code = HttpStatusCode.InternalServerError;
                    break;
            }
            return (code, JsonConvert.SerializeObject(new SimpleResponse(exception.Message)));
        }
    }
    public class SimpleResponse 
    {
        public string Message { get; set; }

        public SimpleResponse(string message) 
        {
            Message = message;
        }
    }
}

