using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using BackOfTheHouse.Data;

#nullable disable

namespace BackOfTheHouse.Data.Migrations
{
    [DbContext(typeof(SandwichContext))]
    [Migration("20251027150000_AddIsPrivate")]
    public partial class AddIsPrivate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPrivate",
                table: "Sandwiches",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPrivate",
                table: "Sandwiches");
        }
    }
}
