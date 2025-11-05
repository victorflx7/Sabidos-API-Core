using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SabidosAPI_Core.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEventoModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Eventos",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DescriptionEvent",
                table: "Eventos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "Eventos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LocalEvento",
                table: "Eventos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Eventos",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Eventos");

            migrationBuilder.DropColumn(
                name: "DescriptionEvent",
                table: "Eventos");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "Eventos");

            migrationBuilder.DropColumn(
                name: "LocalEvento",
                table: "Eventos");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Eventos");
        }
    }
}
