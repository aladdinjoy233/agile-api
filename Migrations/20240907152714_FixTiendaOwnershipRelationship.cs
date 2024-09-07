using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace agile_api.Migrations
{
    /// <inheritdoc />
    public partial class FixTiendaOwnershipRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tiendas_Usuarios_DueñoId",
                table: "Tiendas");

            migrationBuilder.DropIndex(
                name: "IX_Tiendas_DueñoId",
                table: "Tiendas");

            migrationBuilder.CreateIndex(
                name: "IX_Tiendas_DueñoId",
                table: "Tiendas",
                column: "DueñoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tiendas_Usuarios_DueñoId",
                table: "Tiendas",
                column: "DueñoId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tiendas_Usuarios_DueñoId",
                table: "Tiendas");

            migrationBuilder.DropIndex(
                name: "IX_Tiendas_DueñoId",
                table: "Tiendas");

            migrationBuilder.CreateIndex(
                name: "IX_Tiendas_DueñoId",
                table: "Tiendas",
                column: "DueñoId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tiendas_Usuarios_DueñoId",
                table: "Tiendas",
                column: "DueñoId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
