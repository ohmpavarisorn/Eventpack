using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventPack.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add Role column if it doesn't already exist
            migrationBuilder.Sql(@"
                ALTER TABLE userinformation 
                ADD COLUMN Role varchar(50) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT 'customer' AFTER UserPassword
            ", suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop Role column
            migrationBuilder.Sql(@"
                ALTER TABLE userinformation DROP COLUMN IF EXISTS Role
            ", suppressTransaction: true);
        }
    }
}
