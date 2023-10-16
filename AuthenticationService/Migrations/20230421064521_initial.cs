using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthenticationService.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "AuthenticationInfo",
                schema: "public",
                columns: table => new
                {
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<byte[]>(type: "bytea", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    IsThirdPartyAccount = table.Column<bool>(type: "boolean", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    ValidateEmailString = table.Column<string>(type: "text", nullable: true),
                    ResetPasswordString = table.Column<string>(type: "text", nullable: true),
                    IsValidated = table.Column<bool>(type: "boolean", nullable: true),
                    RefreshToken = table.Column<string>(type: "text", nullable: true),
                    PasswordSalt = table.Column<string>(type: "text", nullable: true),
                    RefreshTokenExpiredDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthenticationInfo", x => x.AccountId);
                });

            migrationBuilder.CreateTable(
                name: "EmailSender",
                schema: "dbo",
                columns: table => new
                {
                    usr = table.Column<string>(type: "text", nullable: false),
                    pwd = table.Column<string>(type: "text", nullable: false),
                    EmailSended = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailSender", x => x.usr);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthenticationInfo",
                schema: "public");

            migrationBuilder.DropTable(
                name: "EmailSender",
                schema: "dbo");
        }
    }
}
