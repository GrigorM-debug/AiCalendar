using System.ComponentModel;
using System.Text.Json;
using AiCalendar.WebApi.DTOs.Event;
using AiCalendar.WebApi.DTOs.Users;
using ModelContextProtocol.Server;

namespace AiCalendar.MCPServer
{
    [McpServerToolType]
    public static class UserTools
    {
        [McpServerTool, Description("Register a new user")]
        public static async Task<string> RegisterUserAsync(
            UserService userService,
            [Description("The user data in JSON format")]
            string userJson)
        {
            if (string.IsNullOrEmpty(userJson))
            {
                return "User data can't be null or empty!";
            }

            try
            {
                var userData = JsonSerializer.Deserialize<LoginAndRegisterInputDto>(userJson);

                if (userData == null)
                {
                    return "Invalid user data format.";
                }

                var response = await userService.Register(userData);

                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                return $"Error registering user: {ex.Message}";
            }
        }

        [McpServerTool, Description("Login a user")]
        public static async Task<string> LoginUserAsync(
            UserService userService,
            [Description("The user data in JSON format")]
            string userJson)
        {
            if (string.IsNullOrEmpty(userJson))
            {
                return "User data can't be null or empty!";
            }

            try
            {
                var userData = JsonSerializer.Deserialize<LoginAndRegisterInputDto>(userJson);

                if (userData == null)
                {
                    return "Invalid user data format.";
                }

                var response = await userService.Login(userData);

                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                return $"Error logging in user: {ex.Message}";
            }
        }

        [McpServerTool, Description("Get users")]
        public static async Task<string> GetUsersAsync(
            UserService userService,
            [Description("Filter options")] string? filterDataJson)
        {
            try
            {
                var filterObj = default(UserFilterCriteriaDto);

                if (!string.IsNullOrEmpty(filterDataJson))
                {
                    filterObj = JsonSerializer.Deserialize<UserFilterCriteriaDto>(filterDataJson);
                }

                var response = await userService.GetUsers(filterObj);

                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                return $"Error retrieving users: {ex.Message}";
            }
        }

        [McpServerTool, Description("Delete user")]
        public static async Task<string> DeleteUserAsync(
            UserService userService,
            [Description("The id of the user to delete")]
            string userId,
            [Description("User JWT token")] string jwtToken)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return "UserId can't be null or empty!";
            }

            if (string.IsNullOrEmpty(jwtToken))
            {
                return "JWT token can't be null or empty!";
            }

            try
            {
                var response = await userService.DeleteUser(userId, jwtToken);

                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                return $"Error deleting user: {ex.Message}";
            }
        }

        [McpServerTool, Description("Update user")]
        public static async Task<string> UpdateUser(
            UserService userService,
            [Description("The id of the user to update")]
            string userId,
            [Description("The JWT token for authentication")]
            string jwtToken,
            [Description("The new user data")] string userDataJson)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return "UserId can't be null or empty!";
            }

            if (string.IsNullOrEmpty(jwtToken))
            {
                return "JWT token can't be null or empty!";
            }

            if (string.IsNullOrEmpty(userDataJson))
            {
                return "User data can't be null or empty!";
            }

            try
            {
                var userData = JsonSerializer.Deserialize<UpdateUserDto>(userDataJson);

                if (userData == null)
                {
                    return "Invalid user data format.";
                }

                var response = await userService.UpdateUser(userId, userData, jwtToken);

                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                return $"Error updating user: {ex.Message}";
            }
        }

        [McpServerTool, Description("Get user created events")]
        public static async Task<string> GetUserEvents(
            UserService userService,
            [Description("User JWT token")] string jwtToken,
            [Description("Event filter criteria in JSON format")] string? filterString
            )
        {
            if (string.IsNullOrEmpty(jwtToken))
            {
                return "JWT token can't be null or empty!";
            }

            try
            {
                var filterObj = default(EventFilterCriteriaDto);
                if (!string.IsNullOrEmpty(filterString))
                {
                    filterObj = JsonSerializer.Deserialize<EventFilterCriteriaDto>(filterString);
                }

                var response = await userService.GetUserEvents(jwtToken, filterObj);
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                return $"Error retrieving user events: {ex.Message}";
            }
        }
    }
}
