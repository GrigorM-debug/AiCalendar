using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AiCalendar.WebApi.Models;
using AiCalendar.WebApi.Services.Users.Interfaces;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace AiCalendar.WebApi.Services.Users
{
    public class TokenProvider : ITokenProvider
    {
        public readonly IConfiguration _configuration;

        public TokenProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(User user)
        {
            string secretKey = _configuration["Jwt:Secret"];

            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));

            SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                [
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), 
                        new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim("username", user.UserName)
                ]),
                Expires = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("Jwt:ExpirationInDays")),
                SigningCredentials = credentials,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            JsonWebTokenHandler handler = new JsonWebTokenHandler();
            
            string token = handler.CreateToken(tokenDescriptor);

            return token;
        }
    }
}
