using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;

namespace Dev1.Module.GoogleAdmin.Migrations.EntityBuilders
{
    // Follows Oqtane EntityBuilder pattern (see GoogleAdminEntityBuilder)
    public class CalendarWatchEntityBuilder : AuditableBaseEntityBuilder<CalendarWatchEntityBuilder>
    {
        private const string _entityTableName = "Dev1GoogleAdmin.CalendarWatch";

        private readonly PrimaryKey<CalendarWatchEntityBuilder> _primaryKey =
            new("PK_Dev1GoogleCalendarWatch", x => x.CalendarWatchId);

        // Optional FK to Site (common pattern in Oqtane)
        private readonly ForeignKey<CalendarWatchEntityBuilder> _siteForeignKey =
            new("FK_Dev1GoogleCalendarWatch_Site", x => x.SiteId, "Site", "SiteId", ReferentialAction.Cascade);

        public CalendarWatchEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database)
            : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_siteForeignKey);
        }

        protected override CalendarWatchEntityBuilder BuildTable(ColumnsBuilder table)
        {
            CalendarWatchId = AddAutoIncrementColumn(table, "CalendarWatchId");
            SiteId          = AddIntegerColumn(table, "SiteId");

            // Logical identity/scope
            CalendarId      = AddMaxStringColumn(table, "CalendarId");
            UserEmail       = AddMaxStringColumn(table, "UserEmail");

            // Google channel details
            ChannelId       = AddMaxStringColumn(table, "ChannelId");
            ResourceId      = AddMaxStringColumn(table, "ResourceId");
            TokenKey        = AddMaxStringColumn(table, "TokenKey");
            SyncToken       = AddMaxStringColumn(table, "SyncToken");

            // Lifecycle/state
            ExpirationUtc   = AddDateTimeColumn(table, "ExpirationUtc");
            RefCount        = AddIntegerColumn(table, "RefCount");

            // Callback endpoint
            WebhookUrl      = AddMaxStringColumn(table, "WebhookUrl");

            AddAuditableColumns(table);
            return this;
        }

        public OperationBuilder<AddColumnOperation> CalendarWatchId { get; set; }
        public OperationBuilder<AddColumnOperation> SiteId { get; set; }
        public OperationBuilder<AddColumnOperation> CalendarId { get; set; }
        public OperationBuilder<AddColumnOperation> UserEmail { get; set; }
        public OperationBuilder<AddColumnOperation> ChannelId { get; set; }
        public OperationBuilder<AddColumnOperation> ResourceId { get; set; }
        public OperationBuilder<AddColumnOperation> TokenKey { get; set; }
        public OperationBuilder<AddColumnOperation> SyncToken { get; set; }
        public OperationBuilder<AddColumnOperation> ExpirationUtc { get; set; }
        public OperationBuilder<AddColumnOperation> RefCount { get; set; }
        public OperationBuilder<AddColumnOperation> WebhookUrl { get; set; }
    }
}
