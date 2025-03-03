using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SocialMedialPlatformAPI.Interface;
using SocialMedialPlatformAPI.Models;

namespace SocialMedialPlatformAPI.BLL
{
    public class JwtService : IJwtService
    {
        private string secretkey;
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            secretkey = configuration.GetValue<string>("Jwt:Key") ?? string.Empty;
            _configuration = configuration;
        }
        public string GetJWTToken(User user)
        {
            JwtSecurityTokenHandler tokenHandler = new();
            byte[] key = Encoding.UTF8.GetBytes(secretkey);

            SecurityTokenDescriptor tokenDestriptor = new()
            {
                Subject = new ClaimsIdentity(new List<Claim>
                {
                new Claim("UserId", user.UserId.ToString()),
                new Claim("UserName", user.UserName ?? string.Empty),
                }),
                Expires = DateTime.UtcNow.AddMinutes(120),
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            SecurityToken token = tokenHandler.CreateToken(tokenDestriptor);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("UserId", user.UserId.ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddDays(7);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidateIssuer"],
                audience: _configuration["JWt:ValidateAudience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
