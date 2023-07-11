using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AuthetificationWebAPI.Models.Entity;

public enum UserRole
{
    [Display(Name = "Manager")]
    Manager = 0,
    [Display(Name = "Admin")]
    Admin = 1,
    [Display(Name = "SuperAdmin")]
    SuperAdmin = 2
}