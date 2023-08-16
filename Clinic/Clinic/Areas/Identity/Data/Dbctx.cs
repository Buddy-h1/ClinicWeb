using Clinic.Database;
using Microsoft.EntityFrameworkCore;
using Clinic.Database.Dictionaries;
namespace Clinic.Areas.Identity.Data
{
    public class Dbctx :DbContext
    {
        public Dbctx(DbContextOptions<Dbctx> options) : base(options)
        {
        }
        public DbSet<ReceptionRecord> ReceptionRecords { get; set; }

    }
}
