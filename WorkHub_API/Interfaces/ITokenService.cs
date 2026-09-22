using WorkHub.API.Models;

namespace WorkHub.API.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}