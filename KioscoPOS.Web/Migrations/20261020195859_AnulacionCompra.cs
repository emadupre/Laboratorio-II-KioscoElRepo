using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KioscoPOS.Web.Migrations
{
    /// <inheritdoc />
    public partial class AnulacionCompra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Compras_AspNetUsers_AnuladaPorId",
                table: "Compras");

            migrationBuilder.AddForeignKey(
                name: "FK_Compras_AspNetUsers_AnuladaPorId",
                table: "Compras",
                column: "AnuladaPorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Compras_AspNetUsers_AnuladaPorId",
                table: "Compras");

            migrationBuilder.AddForeignKey(
                name: "FK_Compras_AspNetUsers_AnuladaPorId",
                table: "Compras",
                column: "AnuladaPorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
