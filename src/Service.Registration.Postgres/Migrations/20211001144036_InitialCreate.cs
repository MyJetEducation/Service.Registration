using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Service.Registration.Postgres.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "authorization");

            migrationBuilder.CreateTable(
                name: "accounts",
                schema: "authorization",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Auth = table.Column<string>(type: "text", nullable: true),
                    PersonalData = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "externaldata",
                schema: "authorization",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    TraderId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_externaldata", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "registerlogs",
                schema: "authorization",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TraderId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Email = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RegistrationCountryCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Phone = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Ip = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Owner = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    RedirectedFrom = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    LandingPageId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Language = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CountryCodeByIp = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Registered = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_registerlogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_externaldata_TraderId",
                schema: "authorization",
                table: "externaldata",
                column: "TraderId");

            migrationBuilder.CreateIndex(
                name: "IX_externaldata_TraderId_Key",
                schema: "authorization",
                table: "externaldata",
                columns: new[] { "TraderId", "Key" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accounts",
                schema: "authorization");

            migrationBuilder.DropTable(
                name: "externaldata",
                schema: "authorization");

            migrationBuilder.DropTable(
                name: "registerlogs",
                schema: "authorization");
        }
    }
}
