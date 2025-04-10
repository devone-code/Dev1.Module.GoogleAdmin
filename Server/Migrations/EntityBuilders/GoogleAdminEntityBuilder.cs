using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;

namespace Dev1.Module.GoogleAdmin.Migrations.EntityBuilders
{
    public class GoogleAdminEntityBuilder : AuditableBaseEntityBuilder<GoogleAdminEntityBuilder>
    {
        private const string _entityTableName = "Dev1GoogleAdmin";
        private readonly PrimaryKey<GoogleAdminEntityBuilder> _primaryKey = new("PK_Dev1GoogleAdmin", x => x.GoogleAdminId);
        private readonly ForeignKey<GoogleAdminEntityBuilder> _moduleForeignKey = new("FK_Dev1GoogleAdmin_Module", x => x.ModuleId, "Module", "ModuleId", ReferentialAction.Cascade);

        public GoogleAdminEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_moduleForeignKey);
        }

        protected override GoogleAdminEntityBuilder BuildTable(ColumnsBuilder table)
        {
            GoogleAdminId = AddAutoIncrementColumn(table,"GoogleAdminId");
            ModuleId = AddIntegerColumn(table,"ModuleId");
            Name = AddMaxStringColumn(table,"Name");
            AddAuditableColumns(table);
            return this;
        }

        public OperationBuilder<AddColumnOperation> GoogleAdminId { get; set; }
        public OperationBuilder<AddColumnOperation> ModuleId { get; set; }
        public OperationBuilder<AddColumnOperation> Name { get; set; }
    }
}
