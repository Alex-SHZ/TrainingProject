using AuthetificationWebAPI.Models.Entity;
using AuthetificationWebAPI.Models.Request;
using AuthetificationWebAPI.Models.Response;
using AuthetificationWebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthetificationWebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;

    public AuthController(IAuthService service)
    {
        _service = service;
    }
    [HttpPost("register")]
    public async Task<ActionResult<User>> RegisterUser(UserRequestDto request)
    {
        var response = await _service.RegisterUser(request);
        return Ok(response);
    }
    
    [HttpPost("refresh-token")]
    public async Task<ActionResult<string>> RefreshToken()
    {
        var response = await _service.RefreshToken();
        if (response.Success)
            return Ok(response);
        return BadRequest(response.Message);
    }
    
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(UserRequestDto request)
    {
        var response = await _service.Login(request);
        if (response.Success)
            return Ok(response);
        return BadRequest(response.Message);

    }
    
    [HttpGet("admin"), Authorize(Roles = "Admin")]
    public  ActionResult<string> AlohaAdmin()
    {
        return Ok("Aloha, Admin");
    }
    
    [HttpGet("manager"), Authorize(Roles = "Manager")]
    public  ActionResult<string> AlohaManager()
    {
        return Ok("Aloha, Manager");
    }
    
    [HttpGet, Authorize]
    public  ActionResult<string> Aloha()
    {
        return Ok("Aloha, Hyi");
    }
    
    [HttpGet("all"), Authorize(Roles = "User,Admin")]
    public  ActionResult<string> AlohaAll()
    {
        return Ok("Aloha, All");
    }
}
