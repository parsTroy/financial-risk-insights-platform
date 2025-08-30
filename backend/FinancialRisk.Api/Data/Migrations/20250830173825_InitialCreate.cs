using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinancialRisk.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "assets",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    symbol = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    sector = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    industry = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    assettype = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assets", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "portfolios",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    strategy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    targetreturn = table.Column<decimal>(type: "numeric(18,6)", nullable: true),
                    maxrisk = table.Column<decimal>(type: "numeric(18,6)", nullable: true),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_portfolios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prices",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    assetid = table.Column<int>(type: "integer", nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    open = table.Column<decimal>(type: "numeric(18,6)", nullable: true),
                    high = table.Column<decimal>(type: "numeric(18,6)", nullable: true),
                    low = table.Column<decimal>(type: "numeric(18,6)", nullable: true),
                    close = table.Column<decimal>(type: "numeric(18,6)", nullable: true),
                    adjustedclose = table.Column<decimal>(type: "numeric(18,6)", nullable: true),
                    volume = table.Column<long>(type: "BIGINT", nullable: true),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prices", x => x.id);
                    table.ForeignKey(
                        name: "FK_prices_assets_assetid",
                        column: x => x.assetid,
                        principalTable: "assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "portfolioholdings",
                columns: table => new
                {
                    portfolioid = table.Column<int>(type: "integer", nullable: false),
                    assetid = table.Column<int>(type: "integer", nullable: false),
                    weight = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,6)", nullable: true),
                    averagecost = table.Column<decimal>(type: "numeric(18,6)", nullable: true),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_portfolioholdings", x => new { x.portfolioid, x.assetid });
                    table.ForeignKey(
                        name: "FK_portfolioholdings_assets_assetid",
                        column: x => x.assetid,
                        principalTable: "assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_portfolioholdings_portfolios_portfolioid",
                        column: x => x.portfolioid,
                        principalTable: "portfolios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_assets_assettype",
                table: "assets",
                column: "assettype");

            migrationBuilder.CreateIndex(
                name: "ix_assets_sector",
                table: "assets",
                column: "sector");

            migrationBuilder.CreateIndex(
                name: "ix_assets_symbol",
                table: "assets",
                column: "symbol",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_portfolioholdings_assetid",
                table: "portfolioholdings",
                column: "assetid");

            migrationBuilder.CreateIndex(
                name: "ix_portfolios_isactive",
                table: "portfolios",
                column: "isactive");

            migrationBuilder.CreateIndex(
                name: "ix_portfolios_strategy",
                table: "portfolios",
                column: "strategy");

            migrationBuilder.CreateIndex(
                name: "ix_prices_assetid",
                table: "prices",
                column: "assetid");

            migrationBuilder.CreateIndex(
                name: "ix_prices_assetid_date",
                table: "prices",
                columns: new[] { "assetid", "date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_prices_date",
                table: "prices",
                column: "date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "portfolioholdings");

            migrationBuilder.DropTable(
                name: "prices");

            migrationBuilder.DropTable(
                name: "portfolios");

            migrationBuilder.DropTable(
                name: "assets");
        }
    }
}
