using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace reprodutor_multimidia_api.Database.Migrations
{
    public partial class AddMediasColumnThumbName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ThumbName",
                table: "Medias",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ThumbName",
                table: "Medias");
        }
    }
}
