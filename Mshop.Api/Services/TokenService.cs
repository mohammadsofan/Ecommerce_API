
using Microsoft.IdentityModel.Tokens;
using Mshop.Api.Data.models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Mshop.Api.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration configuration;

        public TokenService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public string GetToken(string id, string userName, IEnumerable<string> roles)
        {
            List<Claim> claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name,userName),
                            new Claim(ClaimTypes.NameIdentifier,id),
                        };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            //var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET_KEY")!));
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:SecretKey"]!));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var jwtToken = new JwtSecurityToken(
                claims: claims,
                signingCredentials: cred,
                expires: DateTime.Now.AddMinutes(30));
            var token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            return token;
        }

    }
}
