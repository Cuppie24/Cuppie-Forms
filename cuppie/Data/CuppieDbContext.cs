using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using cuppie.Models;

namespace cuppie.Data
{
    public class CuppieDbContext : DbContext
    {
        public CuppieDbContext (DbContextOptions<CuppieDbContext> options)
            : base(options)
        {
        }

        public DbSet<cuppie.Models.User> User { get; set; } = default!;
    }
}
