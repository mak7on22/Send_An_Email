using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TDList.Models
{
    public class TDLContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Goal> Goals { get; set; }
        public TDLContext(DbContextOptions<TDLContext> options) : base(options) { }
    }
}
