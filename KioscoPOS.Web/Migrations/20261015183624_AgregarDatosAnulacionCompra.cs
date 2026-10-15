using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KioscoPOS.Web.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDatosAnulacionCompra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnuladaPorId",
                table: "Compras",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAnulacion",
                table: "Compras",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoAnulacion",
                table: "Compras",
                type: "varchar(250)",
                maxLength: 250,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Compras_AnuladaPorId",
                table: "Compras",
                column: "AnuladaPorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Compras_AspNetUsers_AnuladaPorId",
                table: "Compras",
                column: "AnuladaPorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Compras_AspNetUsers_AnuladaPorId",
                table: "Compras");

            migrationBuilder.DropIndex(
                name: "IX_Compras_AnuladaPorId",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "AnuladaPorId",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "FechaAnulacion",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "MotivoAnulacion",
                table: "Compras");
        }
    }
}
