using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace reprodutor_multimidia_api.Database.Migrations
{
    public partial class AddMediaFileName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Medias",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Medias");
        }
    }
}
