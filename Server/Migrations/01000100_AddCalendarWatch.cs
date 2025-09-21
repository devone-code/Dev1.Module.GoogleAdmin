using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Dev1.Module.GoogleAdmin.Migrations.EntityBuilders;
using Dev1.Module.GoogleAdmin.Repository;

namespace Dev1.Module.GoogleAdmin.Migrations
{
    [DbContext(typeof(GoogleAdminContext))]
    [Migration("Dev1.Module.GoogleAdmin.01.00.01.00")]
    public class AddCalendarWatch : MultiDatabaseMigration
    {
        public AddCalendarWatch(IDatabase database) : base(database) { }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var entityBuilder = new CalendarWatchEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilder.Create();
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var entityBuilder = new CalendarWatchEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilder.Drop();
        }
    }
}
