using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AthensWorkspace.Migrations.MHWs
{
    /// <inheritdoc />
    public partial class AddMHWsAmulet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Amulet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Rare = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    SkillId1 = table.Column<short>(type: "smallint", nullable: false),
                    Level1 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    SkillId2 = table.Column<short>(type: "smallint", nullable: false),
                    Level2 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    SkillId3 = table.Column<short>(type: "smallint", nullable: true),
                    Level3 = table.Column<byte>(type: "tinyint unsigned", nullable: true),
                    Slot1 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Slot2 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Slot3 = table.Column<byte>(type: "tinyint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Amulet", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AmuletPattern",
                columns: table => new
                {
                    Rare = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Group1 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Group2 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Group3 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Slot1 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Slot2 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Slot3 = table.Column<byte>(type: "tinyint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AmuletPattern", x => new { x.Rare, x.Group1, x.Group2, x.Group3, x.Slot1, x.Slot2, x.Slot3 });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AmuletSkillGroup",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    SkillId = table.Column<short>(type: "smallint", nullable: false),
                    Level = table.Column<byte>(type: "tinyint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AmuletSkillGroup", x => new { x.Id, x.SkillId });
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Amulet");

            migrationBuilder.DropTable(
                name: "AmuletPattern");

            migrationBuilder.DropTable(
                name: "AmuletSkillGroup");
        }
    }
}
