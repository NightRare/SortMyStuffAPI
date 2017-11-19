using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SortMyStuffAPI.Migrations
{
    public partial class RemoveUserRootAssetEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Categories_CategoryId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_AspNetUsers_UserId",
                table: "Assets");

            migrationBuilder.DropTable(
                name: "UserRootAssetContracts");

            migrationBuilder.RenameColumn(
                name: "RootAssetContractId",
                table: "AspNetUsers",
                newName: "RootAssetId");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Assets",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CategoryId",
                table: "Assets",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Categories_CategoryId",
                table: "Assets",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_AspNetUsers_UserId",
                table: "Assets",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Categories_CategoryId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_AspNetUsers_UserId",
                table: "Assets");

            migrationBuilder.RenameColumn(
                name: "RootAssetId",
                table: "AspNetUsers",
                newName: "RootAssetContractId");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Assets",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "CategoryId",
                table: "Assets",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "UserRootAssetContracts",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    RootAssetId = table.Column<string>(nullable: false),
                    UserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRootAssetContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRootAssetContracts_Assets_RootAssetId",
                        column: x => x.RootAssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRootAssetContracts_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserRootAssetContracts_RootAssetId",
                table: "UserRootAssetContracts",
                column: "RootAssetId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRootAssetContracts_UserId",
                table: "UserRootAssetContracts",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Categories_CategoryId",
                table: "Assets",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_AspNetUsers_UserId",
                table: "Assets",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
