using System;
using System.Collections.Generic;
using Contracts.DataAnalisysEntities;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Storages.Migrations
{
    /// <inheritdoc />
    public partial class ADD_NEW_GitHubProjects_TABLE : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectClusteringInfos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectName = table.Column<string>(type: "text", nullable: false),
                    CallParametrizationStylesUsages = table.Column<Dictionary<CallParametrizationStyle, double>>(type: "jsonb", nullable: false),
                    MeanCallsPerMethod = table.Column<double>(type: "double precision", nullable: false),
                    TryCatchUsage = table.Column<double>(type: "double precision", nullable: false),
                    MeaningfulClassesUsage = table.Column<double>(type: "double precision", nullable: false),
                    MidParametersCount = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectClusteringInfos", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectClusteringInfos");
        }
    }
}
