using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace reprodutor_multimidia_api.Database.Migrations
{
    public partial class AddMediasColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Album",
                table: "Medias",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "Medias",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Medias",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Album",
                table: "Medias");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Medias");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Medias");
        }
    }
}
