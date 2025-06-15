using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AiCalendar.WebApi.Controllers
{
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
    {
        private readonly ILogger<ErrorController> _logger;
        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [Route("/error-development")]
        public IActionResult ErrorDevelopment([FromServices] IHostEnvironment hostEnvironment)
        {
            //This endpoint is only for development environment.
            if (!hostEnvironment.IsDevelopment())
            {
                return NotFound();
            }

            IExceptionHandlerFeature  exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();

            Exception exception = exceptionHandlerFeature.Error;

            //Log the exception
            _logger.LogError(exception, "An unhandled exception occurred.");

            return Problem(
                instance: HttpContext.TraceIdentifier,
                detail: exception.StackTrace,
                title: exception.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );  
        }

        [Route("/error")]
        public IActionResult Error()
        {
            IExceptionHandlerFeature exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();

            Exception exception = exceptionHandlerFeature.Error;

            //Log the exception
            _logger.LogError(exception, "An unhandled exception occurred.");

            return Problem(
                title: "Internal Server Error",
                detail: "An unexpected error occurred. Please try again later.",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}
