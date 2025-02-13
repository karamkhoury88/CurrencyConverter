using CurrencyConverter.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace CurrencyConverter.Data
{
    public class CurrencyConverterDbContext : IdentityDbContext<CurrencyConverterUser>
    {
        public CurrencyConverterDbContext(DbContextOptions<CurrencyConverterDbContext> options)
        : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
