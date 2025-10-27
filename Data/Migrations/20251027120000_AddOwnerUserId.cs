using Microsoft.EntityFrameworkCore.Migrations;
using BackOfTheHouse.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace BackOfTheHouse.Data.Migrations
{
    [DbContext(typeof(SandwichContext))]
    [Migration("20251027120000_AddOwnerUserId")]
    public partial class AddOwnerUserId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add a nullable integer column to track the owning user's Id
            migrationBuilder.AddColumn<int>(
                name: "OwnerUserId",
                table: "Sandwiches",
                type: "INTEGER",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "Sandwiches");
        }
    }
}
