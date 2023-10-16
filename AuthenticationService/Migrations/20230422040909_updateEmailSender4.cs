using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthenticationService.Migrations
{
    /// <inheritdoc />
    public partial class updateEmailSender4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_emailsender",
                schema: "dbo",
                table: "emailsender");

            migrationBuilder.RenameTable(
                name: "emailsender",
                schema: "dbo",
                newName: "EmailSender",
                newSchema: "dbo");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmailSender",
                schema: "dbo",
                table: "EmailSender",
                column: "usr");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_EmailSender",
                schema: "dbo",
                table: "EmailSender");

            migrationBuilder.RenameTable(
                name: "EmailSender",
                schema: "dbo",
                newName: "emailsender",
                newSchema: "dbo");

            migrationBuilder.AddPrimaryKey(
                name: "PK_emailsender",
                schema: "dbo",
                table: "emailsender",
                column: "usr");
        }
    }
}
