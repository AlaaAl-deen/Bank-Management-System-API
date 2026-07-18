using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BankManagementSystem.Configurations;
using BankManagementSystem.Modules.Users.Models;
using BankManagementSystem.Security.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BankManagementSystem.Security
{
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;

        public JwtService(IOptions<JwtSettings> jwtOptions)
        {
            _jwtSettings = jwtOptions.Value;
        }

        public string GenerateToken(User user)
        {
            var claims = new List<Claim>
           {
    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
    new Claim("CustomerNumber", user.CustomerNumber.ToString()),
    new Claim(ClaimTypes.GivenName, user.FirstName),
    new Claim(ClaimTypes.Surname, user.LastName),
    new Claim(ClaimTypes.Role, user.RoleId == 1 ? "Admin" : "Customer")
           };
            //Genrate SymmetricSecurityKey
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(issuer: _jwtSettings.Issuer, audience: _jwtSettings.Audience,  claims: claims,
    expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
    signingCredentials: credentials
);
            return new JwtSecurityTokenHandler().WriteToken(token);



        }
    }
}