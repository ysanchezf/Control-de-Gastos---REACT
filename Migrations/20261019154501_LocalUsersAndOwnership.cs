using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Control_de_Gastos.Migrations
{
    /// <inheritdoc />
    public partial class LocalUsersAndOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Gastos",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Categorias",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserName = table.Column<string>(type: "TEXT", nullable: false),
                    NormalizedUserName = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    IsAdmin = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    FailedAttempts = table.Column<int>(type: "INTEGER", nullable: false),
                    LockedUntilUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Gastos_UserId",
                table: "Gastos",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_UserId_Nombre",
                table: "Categorias",
                columns: new[] { "UserId", "Nombre" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_NormalizedUserName",
                table: "Users",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Categorias_Users_UserId",
                table: "Categorias",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Gastos_Users_UserId",
                table: "Gastos",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categorias_Users_UserId",
                table: "Categorias");

            migrationBuilder.DropForeignKey(
                name: "FK_Gastos_Users_UserId",
                table: "Gastos");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Gastos_UserId",
                table: "Gastos");

            migrationBuilder.DropIndex(
                name: "IX_Categorias_UserId_Nombre",
                table: "Categorias");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Gastos");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Categorias");
        }
    }
}
