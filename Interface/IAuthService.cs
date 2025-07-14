using chickko.api.Models;

namespace chickko.api.Services
{
    public interface IAuthService
    {
        User? Login(string username, string password);
        string GenerateJwtToken(User user);
        Task<User> Register(RegisterRequest request);
    }
}