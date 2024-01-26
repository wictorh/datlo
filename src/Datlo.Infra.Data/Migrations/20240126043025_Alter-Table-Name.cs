using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datlo.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlterTableName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"DataType\" RENAME TO DataType;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
