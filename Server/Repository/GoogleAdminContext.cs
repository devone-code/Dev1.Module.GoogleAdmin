using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Oqtane.Modules;
using Oqtane.Repository;
using Oqtane.Infrastructure;
using Oqtane.Repository.Databases.Interfaces;

namespace Dev1.Module.GoogleAdmin.Repository
{
    public class GoogleAdminContext : DBContextBase, ITransientService, IMultiDatabase
    {
        public virtual DbSet<Shared.Models.GoogleAdmin> GoogleAdmin { get; set; }

        public GoogleAdminContext(IDBContextDependencies DBContextDependencies) : base(DBContextDependencies)
        {
            // ContextBase handles multi-tenant database connections
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Shared.Models.GoogleAdmin>().ToTable(ActiveDatabase.RewriteName("Dev1GoogleAdmin"));
        }
    }
}
