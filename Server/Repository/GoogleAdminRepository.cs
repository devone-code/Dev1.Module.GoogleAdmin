using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using Oqtane.Modules;

namespace Dev1.Module.GoogleAdmin.Repository
{
    public class GoogleAdminRepository : IGoogleAdminRepository, ITransientService
    {
        private readonly IDbContextFactory<GoogleAdminContext> _factory;

        public GoogleAdminRepository(IDbContextFactory<GoogleAdminContext> factory)
        {
            _factory = factory;
        }

        public IEnumerable<Shared.Models.GoogleAdmin> GetGoogleAdmins(int ModuleId)
        {
            using var db = _factory.CreateDbContext();
            return db.GoogleAdmin.Where(item => item.ModuleId == ModuleId).ToList();
        }

        public Shared.Models.GoogleAdmin GetGoogleAdmin(int GoogleAdminId)
        {
            return GetGoogleAdmin(GoogleAdminId, true);
        }

        public Shared.Models.GoogleAdmin GetGoogleAdmin(int GoogleAdminId, bool tracking)
        {
            using var db = _factory.CreateDbContext();
            if (tracking)
            {
                return db.GoogleAdmin.Find(GoogleAdminId);
            }
            else
            {
                return db.GoogleAdmin.AsNoTracking().FirstOrDefault(item => item.GoogleAdminId == GoogleAdminId);
            }
        }

        public Shared.Models.GoogleAdmin AddGoogleAdmin(Shared.Models.GoogleAdmin GoogleAdmin)
        {
            using var db = _factory.CreateDbContext();
            db.GoogleAdmin.Add(GoogleAdmin);
            db.SaveChanges();
            return GoogleAdmin;
        }

        public Shared.Models.GoogleAdmin UpdateGoogleAdmin(Shared.Models.GoogleAdmin GoogleAdmin)
        {
            using var db = _factory.CreateDbContext();
            db.Entry(GoogleAdmin).State = EntityState.Modified;
            db.SaveChanges();
            return GoogleAdmin;
        }

        public void DeleteGoogleAdmin(int GoogleAdminId)
        {
            using var db = _factory.CreateDbContext();
            Shared.Models.GoogleAdmin GoogleAdmin = db.GoogleAdmin.Find(GoogleAdminId);
            db.GoogleAdmin.Remove(GoogleAdmin);
            db.SaveChanges();
        }
    }
}
