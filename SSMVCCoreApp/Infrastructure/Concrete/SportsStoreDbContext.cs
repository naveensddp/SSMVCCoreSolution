using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using SSMVCCoreApp.Infrastructure.Entities;

namespace SSMVCCoreApp.Infrastructure.Concrete
{
  public class SportsStoreDbContext : DbContext
  {
    public SportsStoreDbContext(DbContextOptions<SportsStoreDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
  }
}
