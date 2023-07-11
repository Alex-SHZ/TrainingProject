namespace AuthetificationWebAPI.Models.Request;

public class UserRequestDto
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}