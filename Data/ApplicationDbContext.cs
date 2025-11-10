using Microsoft.EntityFrameworkCore;
using NSN.Models;

namespace NSN.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Token> Tokens { get; set; }
    }
}
