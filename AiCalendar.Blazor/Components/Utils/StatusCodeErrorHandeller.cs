using Microsoft.AspNetCore.Components;

namespace AiCalendar.Blazor.Components.Utils
{
    public class StatusCodeErrorHandeller
    {
        private readonly NavigationManager _navigationManager;

        public StatusCodeErrorHandeller(NavigationManager navigationManager)
        {
            _navigationManager = navigationManager;
        }

        public void HandleStatusCode(HttpResponseMessage response)
        {
            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.Unauthorized:
                    _navigationManager.NavigateTo("/status-code/401");
                    break;
                case System.Net.HttpStatusCode.BadRequest:
                    _navigationManager.NavigateTo("/status-code/400");
                    break;
                case System.Net.HttpStatusCode.Forbidden:
                    _navigationManager.NavigateTo("/status-code/403");
                    break;
                case System.Net.HttpStatusCode.NotFound:
                    _navigationManager.NavigateTo("/status-code/404");
                    break;
                case System.Net.HttpStatusCode.InternalServerError:
                    _navigationManager.NavigateTo("/status-code/500");
                    break;
                default:
                    break;
            }
        }
    }
}
