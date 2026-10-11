using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TaskFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedRolesAndPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Packages",
                column: "Id",
                values: new object[]
                {
                    "General",
                    "Management",
                    "Projects",
                    "Tasks",
                    "Teams"
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "DateTimeCreated", "DateTimeDeleted", "DateTimeModified", "Description", "IsDefault", "Name" },
                values: new object[,]
                {
                    { new Guid("039d8370-1848-4397-b389-34fff78a9ec0"), new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "Full access, including users and roles", false, "Admin" },
                    { new Guid("50b3a389-3edf-4444-89c3-5e96b8887da0"), new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "Works in teams, projects and tasks", true, "User" },
                    { new Guid("f12cc9d1-75eb-4735-ba0e-1b126f39cbd3"), new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "Read-only access to teams, projects and tasks", false, "ReadOnlyUser" }
                });

            migrationBuilder.InsertData(
                table: "Claims",
                columns: new[] { "Id", "FeatureId", "PackageId" },
                values: new object[,]
                {
                    { "Projects.Create", null, "Projects" },
                    { "Projects.Manage", null, "Projects" },
                    { "Projects.View.All", null, "Projects" },
                    { "Tasks.Assign", null, "Tasks" },
                    { "Tasks.Create", null, "Tasks" },
                    { "Tasks.Manage", null, "Tasks" },
                    { "Tasks.View.All", null, "Tasks" },
                    { "Teams.Manage", null, "Teams" },
                    { "Teams.Members.Manage", null, "Teams" },
                    { "Teams.View.All", null, "Teams" }
                });

            migrationBuilder.InsertData(
                table: "Features",
                columns: new[] { "Id", "IsEnabled", "PackageId" },
                values: new object[] { "Tasks.Board", true, "Tasks" });

            migrationBuilder.InsertData(
                table: "RolePackages",
                columns: new[] { "PackageId", "RoleId" },
                values: new object[,]
                {
                    { "General", new Guid("039d8370-1848-4397-b389-34fff78a9ec0") },
                    { "Management", new Guid("039d8370-1848-4397-b389-34fff78a9ec0") },
                    { "Projects", new Guid("039d8370-1848-4397-b389-34fff78a9ec0") },
                    { "Tasks", new Guid("039d8370-1848-4397-b389-34fff78a9ec0") },
                    { "Teams", new Guid("039d8370-1848-4397-b389-34fff78a9ec0") },
                    { "General", new Guid("50b3a389-3edf-4444-89c3-5e96b8887da0") },
                    { "Projects", new Guid("50b3a389-3edf-4444-89c3-5e96b8887da0") },
                    { "Tasks", new Guid("50b3a389-3edf-4444-89c3-5e96b8887da0") },
                    { "Teams", new Guid("50b3a389-3edf-4444-89c3-5e96b8887da0") },
                    { "General", new Guid("f12cc9d1-75eb-4735-ba0e-1b126f39cbd3") },
                    { "Projects", new Guid("f12cc9d1-75eb-4735-ba0e-1b126f39cbd3") },
                    { "Tasks", new Guid("f12cc9d1-75eb-4735-ba0e-1b126f39cbd3") },
                    { "Teams", new Guid("f12cc9d1-75eb-4735-ba0e-1b126f39cbd3") }
                });

            migrationBuilder.InsertData(
                table: "Claims",
                columns: new[] { "Id", "FeatureId", "PackageId" },
                values: new object[] { "Tasks.Board.View", "Tasks.Board", "Tasks" });

            migrationBuilder.InsertData(
                table: "RoleClaims",
                columns: new[] { "ClaimId", "RoleId" },
                values: new object[,]
                {
                    { "Projects.Create", new Guid("039d8370-1848-4397-b389-34fff78a9ec0") },
                    { "Projects.Manage", new Guid("039d8370-1848-4397-b389-34fff78a9ec0") },
                    { "Projects.View.All", new Guid("039d8370-1848-4397-b389-34fff78a9ec0") },
                    { "Tasks.Assign", new Guid("039d8370-1848-4397-b389-34fff78a9ec0") },
                    { "Tasks.Create", new Guid("039d8370-1848-4397-b389-34fff78a9ec0") },
                    { "Tasks.Manage", new Guid("039d8370-1848-4397-b389-34fff78a9ec0") },
                    { "Tasks.View.All", new Guid("039d8370-1848-4397-b389-34fff78a9ec0") },
                    { "Teams.Manage", new Guid("039d8370-1848-4397-b389-34fff78a9ec0") },
                    { "Teams.Members.Manage", new Guid("039d8370-1848-4397-b389-34fff78a9ec0") },
                    { "Teams.View.All", new Guid("039d8370-1848-4397-b389-34fff78a9ec0") },
                    { "Projects.Create", new Guid("50b3a389-3edf-4444-89c3-5e96b8887da0") },
                    { "Projects.Manage", new Guid("50b3a389-3edf-4444-89c3-5e96b8887da0") },
                    { "Tasks.Assign", new Guid("50b3a389-3edf-4444-89c3-5e96b8887da0") },
                    { "Tasks.Create", new Guid("50b3a389-3edf-4444-89c3-5e96b8887da0") },
                    { "Tasks.Manage", new Guid("50b3a389-3edf-4444-89c3-5e96b8887da0") },
                    { "Projects.View.All", new Guid("f12cc9d1-75eb-4735-ba0e-1b126f39cbd3") },
                    { "Tasks.View.All", new Guid("f12cc9d1-75eb-4735-ba0e-1b126f39cbd3") },
                    { "Teams.View.All", new Guid("f12cc9d1-75eb-4735-ba0e-1b126f39cbd3") },
                    { "Tasks.Board.View", new Guid("039d8370-1848-4397-b389-34fff78a9ec0") },
                    { "Tasks.Board.View", new Guid("50b3a389-3edf-4444-89c3-5e96b8887da0") },
                    { "Tasks.Board.View", new Guid("f12cc9d1-75eb-4735-ba0e-1b126f39cbd3") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumns: new[] { "ClaimId", "RoleId" },
                keyValues: new object[] { "Projects.Create", new Guid("039d8370-1848-4397-b389-34fff78a9ec0") });

            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumns: new[] { "ClaimId", "RoleId" },
                keyValues: new object[] { "Projects.Manage", new Guid("039d8370-1848-4397-b389-34fff78a9ec0") });

            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumns: new[] { "ClaimId", "RoleId" },
                keyValues: new object[] { "Projects.View.All", new Guid("039d8370-1848-4397-b389-34fff78a9ec0") });

            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumns: new[] { "ClaimId", "RoleId" },
                keyValues: new object[] { "Tasks.Assign", new Guid("039d8370-1848-4397-b389-34fff78a9ec0") });

            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumns: new[] { "ClaimId", "RoleId" },
                keyValues: new object[] { "Tasks.Board.View", new Guid("039d8370-1848-4397-b389-34fff78a9ec0") });

            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumns: new[] { "ClaimId", "RoleId" },
                keyValues: new object[] { "Tasks.Create", new Guid("039d8370-1848-4397-b389-34fff78a9ec0") });

            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumns: new[] { "ClaimId", "RoleId" },
                keyValues: new object[] { "Tasks.Manage", new Guid("039d8370-1848-4397-b389-34fff78a9ec0") });

            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumns: new[] { "ClaimId", "RoleId" },
                keyValues: new object[] { "Tasks.View.All", new Guid("039d8370-1848-4397-b389-34fff78a9ec0") });

            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumns: new[] { "ClaimId", "RoleId" },
                keyValues: new object[] { "Teams.Manage", new Guid("039d8370-1848-4397-b389-34fff78a9ec0") });

            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumns: new[] { "ClaimId", "RoleId" },
                keyValues: new object[] { "Teams.Members.Manage", new Guid("039d8370-1848-4397-b389-34fff78a9ec0") });

            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumns: new[] { "ClaimId", "RoleId" },
                keyValues: new object[] { "Teams.View.All", new Guid("039d8370-1848-4397-b389-34fff78a9ec0") });

            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumns: new[] { "ClaimId", "RoleId" },
                keyValues: new object[] { "Projects.Create", new Guid("50b3a389-3edf-4444-89c3-5e96b8887da0") });

            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumns: new[] { "ClaimId", "RoleId" },
                keyValues: new object[] { "Projects.Manage", new Guid("50b3a389-3edf-4444-89c3-5e96b8887da0") });

            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumns: new[] { "ClaimId", "RoleId" },
                keyValues: new object[] { "Tasks.Assign", new Guid("50b3a389-3edf-4444-89c3-5e96b8887da0") });

            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumns: new[] { "ClaimId", "RoleId" },
                keyValues: new object[] { "Tasks.Board.View", new Guid("50b3a389-3edf-4444-89c3-5e96b8887da0") });

            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumns: new[] { "ClaimId", "RoleId" },
                keyValues: new object[] { "Tasks.Create", new Guid("50b3a389-3edf-4444-89c3-5e96b8887da0") });

            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumns: new[] { "ClaimId", "RoleId" },
                keyValues: new object[] { "Tasks.Manage", new Guid("50b3a389-3edf-4444-89c3-5e96b8887da0") });

            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumns: new[] { "ClaimId", "RoleId" },
                keyValues: new object[] { "Projects.View.All", new Guid("f12cc9d1-75eb-4735-ba0e-1b126f39cbd3") });

            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumns: new[] { "ClaimId", "RoleId" },
                keyValues: new object[] { "Tasks.Board.View", new Guid("f12cc9d1-75eb-4735-ba0e-1b126f39cbd3") });

            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumns: new[] { "ClaimId", "RoleId" },
                keyValues: new object[] { "Tasks.View.All", new Guid("f12cc9d1-75eb-4735-ba0e-1b126f39cbd3") });

            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumns: new[] { "ClaimId", "RoleId" },
                keyValues: new object[] { "Teams.View.All", new Guid("f12cc9d1-75eb-4735-ba0e-1b126f39cbd3") });

            migrationBuilder.DeleteData(
                table: "RolePackages",
                keyColumns: new[] { "PackageId", "RoleId" },
                keyValues: new object[] { "General", new Guid("039d8370-1848-4397-b389-34fff78a9ec0") });

            migrationBuilder.DeleteData(
                table: "RolePackages",
                keyColumns: new[] { "PackageId", "RoleId" },
                keyValues: new object[] { "Management", new Guid("039d8370-1848-4397-b389-34fff78a9ec0") });

            migrationBuilder.DeleteData(
                table: "RolePackages",
                keyColumns: new[] { "PackageId", "RoleId" },
                keyValues: new object[] { "Projects", new Guid("039d8370-1848-4397-b389-34fff78a9ec0") });

            migrationBuilder.DeleteData(
                table: "RolePackages",
                keyColumns: new[] { "PackageId", "RoleId" },
                keyValues: new object[] { "Tasks", new Guid("039d8370-1848-4397-b389-34fff78a9ec0") });

            migrationBuilder.DeleteData(
                table: "RolePackages",
                keyColumns: new[] { "PackageId", "RoleId" },
                keyValues: new object[] { "Teams", new Guid("039d8370-1848-4397-b389-34fff78a9ec0") });

            migrationBuilder.DeleteData(
                table: "RolePackages",
                keyColumns: new[] { "PackageId", "RoleId" },
                keyValues: new object[] { "General", new Guid("50b3a389-3edf-4444-89c3-5e96b8887da0") });

            migrationBuilder.DeleteData(
                table: "RolePackages",
                keyColumns: new[] { "PackageId", "RoleId" },
                keyValues: new object[] { "Projects", new Guid("50b3a389-3edf-4444-89c3-5e96b8887da0") });

            migrationBuilder.DeleteData(
                table: "RolePackages",
                keyColumns: new[] { "PackageId", "RoleId" },
                keyValues: new object[] { "Tasks", new Guid("50b3a389-3edf-4444-89c3-5e96b8887da0") });

            migrationBuilder.DeleteData(
                table: "RolePackages",
                keyColumns: new[] { "PackageId", "RoleId" },
                keyValues: new object[] { "Teams", new Guid("50b3a389-3edf-4444-89c3-5e96b8887da0") });

            migrationBuilder.DeleteData(
                table: "RolePackages",
                keyColumns: new[] { "PackageId", "RoleId" },
                keyValues: new object[] { "General", new Guid("f12cc9d1-75eb-4735-ba0e-1b126f39cbd3") });

            migrationBuilder.DeleteData(
                table: "RolePackages",
                keyColumns: new[] { "PackageId", "RoleId" },
                keyValues: new object[] { "Projects", new Guid("f12cc9d1-75eb-4735-ba0e-1b126f39cbd3") });

            migrationBuilder.DeleteData(
                table: "RolePackages",
                keyColumns: new[] { "PackageId", "RoleId" },
                keyValues: new object[] { "Tasks", new Guid("f12cc9d1-75eb-4735-ba0e-1b126f39cbd3") });

            migrationBuilder.DeleteData(
                table: "RolePackages",
                keyColumns: new[] { "PackageId", "RoleId" },
                keyValues: new object[] { "Teams", new Guid("f12cc9d1-75eb-4735-ba0e-1b126f39cbd3") });

            migrationBuilder.DeleteData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: "Projects.Create");

            migrationBuilder.DeleteData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: "Projects.Manage");

            migrationBuilder.DeleteData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: "Projects.View.All");

            migrationBuilder.DeleteData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: "Tasks.Assign");

            migrationBuilder.DeleteData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: "Tasks.Board.View");

            migrationBuilder.DeleteData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: "Tasks.Create");

            migrationBuilder.DeleteData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: "Tasks.Manage");

            migrationBuilder.DeleteData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: "Tasks.View.All");

            migrationBuilder.DeleteData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: "Teams.Manage");

            migrationBuilder.DeleteData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: "Teams.Members.Manage");

            migrationBuilder.DeleteData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: "Teams.View.All");

            migrationBuilder.DeleteData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: "General");

            migrationBuilder.DeleteData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: "Management");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("039d8370-1848-4397-b389-34fff78a9ec0"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("50b3a389-3edf-4444-89c3-5e96b8887da0"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("f12cc9d1-75eb-4735-ba0e-1b126f39cbd3"));

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "Id",
                keyValue: "Tasks.Board");

            migrationBuilder.DeleteData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: "Projects");

            migrationBuilder.DeleteData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: "Teams");

            migrationBuilder.DeleteData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: "Tasks");
        }
    }
}
