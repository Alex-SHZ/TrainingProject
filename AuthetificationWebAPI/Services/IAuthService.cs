using AuthetificationWebAPI.Models.Entity;
using AuthetificationWebAPI.Models.Request;
using AuthetificationWebAPI.Models.Response;

namespace AuthetificationWebAPI.Services;

public interface IAuthService
{
    Task<User> RegisterUser(UserRequestDto request);
    Task<AuthResponseDto> Login(UserRequestDto request);
    Task<AuthResponseDto> RefreshToken();
}