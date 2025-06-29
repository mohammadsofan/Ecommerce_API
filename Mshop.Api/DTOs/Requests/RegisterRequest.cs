using Mshop.Api.Data.models;
using Mshop.Api.Validations;
using System.ComponentModel.DataAnnotations;

namespace Mshop.Api.DTOs.Requests
{
    public class RegisterRequest
    {
        [MinLength(3)]
        [MaxLength(50)]
        public string FirstName { get; set; } = null!;
        [MinLength(3)]
        [MaxLength(50)]
        public string LastName { get; set; } = null!;
        [MinLength(4)]
        [MaxLength(50)]
        public string UserName { get; set; } = null!;
        [EmailAddress]
        public string Email { get; set; } = null!;
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = null!;
        [Phone]
        public string PhoneNumber { get; set; } = null!;
        public string City { get; set; } = null!;
        public string Address { get; set; } = null!;
        public ApplicationUserGender Gender { get; set; }
        [OverYears(18)]
        public DateTime BirthDate { get; set; }
    }
}
