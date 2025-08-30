namespace AiCalendar.Blazor.Services.UserSession
{
    public interface IUserSessionManager
    {
        Task SignInUserAsync(string jwtToken);

        void SignOutUser();
    }
}
