using AuthetificationWebAPI.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace AuthetificationWebAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
}