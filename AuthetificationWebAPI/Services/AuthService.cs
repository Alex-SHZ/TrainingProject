using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using AuthetificationWebAPI.Data;
using AuthetificationWebAPI.Models.Dto;
using AuthetificationWebAPI.Models.Entity;
using AuthetificationWebAPI.Models.Request;
using AuthetificationWebAPI.Models.Response;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Extensions;

namespace AuthetificationWebAPI.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IConfiguration _config;
    private readonly IHttpContextAccessor _httpContext;

    public AuthService(ApplicationDbContext dbContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _config = configuration;
        _httpContext = httpContextAccessor;
    }
    public async Task<User> RegisterUser(UserRequestDto request)
    {
        CreatePasswordHash(request.Password, out byte[] hash, out byte[] salt);
        var user = new User
        {
            UserName = request.UserName,
            PasswordHash = hash,
            PasswordSalt = salt
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return user;
    }

    public async Task<AuthResponseDto> Login(UserRequestDto request)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.UserName == request.UserName);
        if (user == null)
            return new AuthResponseDto
            {
                Message = "User Not Found"
            };

        if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            return new AuthResponseDto
            {
                Message = "Wrong password"
            };

        string token = CreateToken(user);
        var refreshToken = CreateRefreshToken();

        await SetRefreshToken(refreshToken, user);
        
        return new AuthResponseDto
        {
            Success = true,
            Message = "Success",
            Token = token,
            RefreshToken = refreshToken.Token,
            TokenExpires = refreshToken.Expires
        };

    }

    public async Task<AuthResponseDto> RefreshToken()
    {
        var refreshToken = _httpContext?.HttpContext?.Request.Cookies["refreshToken"];
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        if (user == null)
            return new AuthResponseDto
            {
                Message = "Invalid Refresh Token"
            };
        if (user.TokenExpires < DateTime.Now)
            return new AuthResponseDto
            {
                Message = "Token expired"
            };

        string token = CreateToken(user);
        var newRefreshToken = CreateRefreshToken();
        await SetRefreshToken(newRefreshToken, user);

        return new AuthResponseDto
        {
            Success = true,
            Token = token,
            RefreshToken = newRefreshToken.Token,
            TokenExpires = newRefreshToken.Expires
        };
    }

    private bool VerifyPasswordHash(string password, byte[] PasswordHash, byte[] PasswordSalt)
    {
        using (var hmac = new HMACSHA512(PasswordSalt))
        {
            var computeHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return computeHash.SequenceEqual(PasswordHash);
        }
    }

    private void CreatePasswordHash(string password, out byte[] PasswordHash, out byte[] PasswordSalt)
    {
        using (var hmac = new HMACSHA512())
        {
            PasswordSalt = hmac.Key;
            PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }
    }

    private string CreateToken(User user)
    {
        List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, user.Role.GetDisplayName())
        };

        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private RefreshToken CreateRefreshToken()
    {
        return new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            Expires = DateTime.Now.AddDays(7),
            Created = DateTime.Now
        };
    }

    private async Task SetRefreshToken(RefreshToken refreshToken, User user)
    {
        var cookieOptions = new CookieOptions()
        {
            HttpOnly = true,
            Expires = refreshToken.Expires,
        };
        _httpContext?.HttpContext?.Response
            .Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);

        user.RefreshToken = refreshToken.Token;
        user.TokenExpires = refreshToken.Created;
        user.TokenExpires = refreshToken.Expires;

        await _dbContext.SaveChangesAsync();
    }
}