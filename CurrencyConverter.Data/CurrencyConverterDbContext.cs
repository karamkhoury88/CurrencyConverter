using CurrencyConverter.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CurrencyConverter.Data
{
    public class CurrencyConverterDbContext : IdentityDbContext<CurrencyConverterUser>
    {
        public CurrencyConverterDbContext(DbContextOptions<CurrencyConverterDbContext> options)
        : base(options) { }

    }
}
