
using Microsoft.IdentityModel.Tokens;
using Mshop.Api.Data.models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Mshop.Api.Services
{
    public class TokenService : ITokenService
    {
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
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YfV8j2joL4NuSomujF20c6F8EqUMKe1r"));
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
