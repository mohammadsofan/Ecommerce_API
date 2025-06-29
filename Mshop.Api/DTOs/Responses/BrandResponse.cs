namespace Mshop.Api.DTOs.Responses
{
    public class BrandResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public bool Status { get; set; }
    }
}
