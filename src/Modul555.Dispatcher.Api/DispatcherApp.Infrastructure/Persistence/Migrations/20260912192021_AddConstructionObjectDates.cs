using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConstructionObjectDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "EndDate",
                table: "construction_objects",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "StartDate",
                table: "construction_objects",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "construction_objects");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "construction_objects");
        }
    }
}
