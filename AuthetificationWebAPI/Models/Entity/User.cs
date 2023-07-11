namespace AuthetificationWebAPI.Models.Entity;

public class User
{
   public int Id { get; set; }
   public string UserName { get; set; } = string.Empty;
   public byte[] PasswordHash { get; set; } = new byte[32];
   public byte[] PasswordSalt { get; set; } = new byte[32];

   public string RefreshToken { get; set; } = string.Empty;
   
   public DateTime TokenCreated { get; set; }
   
   public DateTime TokenExpires { get; set; }
   
   public UserRole Role { get; set; }
}