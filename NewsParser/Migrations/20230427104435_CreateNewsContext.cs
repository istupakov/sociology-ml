using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsParser.Migrations
{
    /// <inheritdoc />
    public partial class CreateNewsContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "News",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    PublicationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_News", x => x.Id);
                    table.UniqueConstraint("AK_News_Url", x => x.Url);
                });

            migrationBuilder.CreateIndex(
                name: "IX_News_PublicationTime",
                table: "News",
                column: "PublicationTime",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_News_Source",
                table: "News",
                column: "Source");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "News");
        }
    }
}
