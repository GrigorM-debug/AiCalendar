using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AiCalendar.WebApi.Controllers
{
    /// <summary>
    /// Handles application-wide error responses.
    /// </summary>
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
    {
        private readonly ILogger<ErrorController> _logger;
        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Handles errors in the development environment and returns detailed exception information.
        /// </summary>
        /// <param name="hostEnvironment">The current hosting environment.</param>
        /// <returns>
        /// A <see cref="ProblemDetails"/> response with detailed error information if in development;
        /// otherwise, a 404 Not Found response.
        /// </returns>
        [Route("/error-development")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Handles errors in non-development environments and returns a generic error response.
        /// </summary>
        /// <returns>
        /// A <see cref="ProblemDetails"/> response with a generic error message.
        /// </returns>
        [Route("/error")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
