using Mshop.Api.Data.Interfaces;

namespace Mshop.Api.Data.models
{
    public class ResetPasswordCode:IEntity
    {
        public Guid Id { get;set; }
        public string Code { get; set; } = null!;
        public bool IsUsed { get; set; } = false;
        public string ApplicationUserId { get; set; } = null!;
        public ApplicationUser ApplicationUser { get; set; } = null!;
        public DateTime ExpirationDate { get; set; }
    }
}
