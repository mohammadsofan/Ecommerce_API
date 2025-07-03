namespace Mshop.Api.DTOs.Requests
{
    public class ConfirmResetPasswordRequest
    {
        public string Email { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string NewPassword { get; set; } = null!;


        //those for ResetPassword - Link with token 
        //public string Email { get; set; } = null!;
        //public string UserId { get; set; } = null!;
        //public string Token { get; set; } = null!;
        //public string NewPassword { get; set; } = null!;

    }
}
