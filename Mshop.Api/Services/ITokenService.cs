namespace Mshop.Api.Services
{
    public interface ITokenService
    {
        string GetToken(string id, string userName, IEnumerable<string> roles);
    }
}
