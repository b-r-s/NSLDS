using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SharpRaven;
using SharpRaven.Data;
using NSLDS.Common;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace NSLDS.API
{
    public class CustomExceptionFilter: ExceptionFilterAttribute
    {
        #region Private Fields

        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private readonly RavenClient _ravenclient;
        #endregion

        public CustomExceptionFilter(ILoggerFactory loggerfactory, IConfiguration config)
        {
            _config = config;
            _ravenclient = new RavenClient(_config["Logging:SharpRavenDSN"]);
            _logger = loggerfactory.CreateLogger<CustomExceptionFilter>();
        }

        public override void OnException(ExceptionContext context)
        {
            // global exception handler with status code 500 & exception message
            // log entry
            _logger.LogError(context.Exception.Message, context.Exception);
            _ravenclient.Capture(new SentryEvent(context.Exception));

            var result = new ObjectResult(new ErrorResult { Message = context.Exception.Message });

            result.StatusCode = StatusCodes.Status500InternalServerError;

            context.Result = result;
        }
    }
}
