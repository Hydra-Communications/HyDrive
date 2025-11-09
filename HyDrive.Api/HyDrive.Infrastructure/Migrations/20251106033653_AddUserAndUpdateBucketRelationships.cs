using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HyDrive.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAndUpdateBucketRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BucketObjects_Buckets_BucketId",
                table: "BucketObjects");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "BucketObjects",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Buckets_UserId",
                table: "Buckets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BucketObjects_UserId",
                table: "BucketObjects",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_Username",
                table: "User",
                column: "Username",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BucketObjects_Buckets_BucketId",
                table: "BucketObjects",
                column: "BucketId",
                principalTable: "Buckets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BucketObjects_User_UserId",
                table: "BucketObjects",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Buckets_User_UserId",
                table: "Buckets",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BucketObjects_Buckets_BucketId",
                table: "BucketObjects");

            migrationBuilder.DropForeignKey(
                name: "FK_BucketObjects_User_UserId",
                table: "BucketObjects");

            migrationBuilder.DropForeignKey(
                name: "FK_Buckets_User_UserId",
                table: "Buckets");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropIndex(
                name: "IX_Buckets_UserId",
                table: "Buckets");

            migrationBuilder.DropIndex(
                name: "IX_BucketObjects_UserId",
                table: "BucketObjects");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "BucketObjects");

            migrationBuilder.AddForeignKey(
                name: "FK_BucketObjects_Buckets_BucketId",
                table: "BucketObjects",
                column: "BucketId",
                principalTable: "Buckets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
