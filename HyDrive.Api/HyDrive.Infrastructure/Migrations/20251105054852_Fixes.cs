using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HyDrive.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Fixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Buckets",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Buckets",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "BucketObjects",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Buckets");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Buckets");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "BucketObjects");
        }
    }
}
