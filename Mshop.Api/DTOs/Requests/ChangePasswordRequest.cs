using System.ComponentModel.DataAnnotations;

namespace Mshop.Api.DTOs.Requests
{
    public class ChangePasswordRequest
    {
        [DataType(DataType.Password)]
        public string OldPassword { get; set; } = null!;
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = null!;
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword))]
        public string ConfirmNewPassword { get; set; } = null!;

    }
}
