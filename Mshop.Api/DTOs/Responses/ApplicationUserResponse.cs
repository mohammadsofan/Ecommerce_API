using Mshop.Api.Data.models;

namespace Mshop.Api.DTOs.Responses
{
    public class ApplicationUserResponse
    {
        public string Id { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string City { get; set; } = null!;
        public string Address { get; set; } = null!;
        public ApplicationUserGender Gender { get; set; }
        public DateTime BirthDate { get; set; }
    }
}
