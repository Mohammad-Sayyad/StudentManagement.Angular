using Microsoft.EntityFrameworkCore;
using StudentManagement.API.Model;

namespace StudentManagement.API.Data
{
    public class AppDbContext : DbContext
    {

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

        public DbSet<Student> Students { get; set; }
        public DbSet<Product> Products { get; set; }

    }
}
