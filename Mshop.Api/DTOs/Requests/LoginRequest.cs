using System.ComponentModel.DataAnnotations;

namespace Mshop.Api.DTOs.Requests
{
    public class LoginRequest
    {
        [EmailAddress]
        public string Email { get; set; } = null!;
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
    }
}
