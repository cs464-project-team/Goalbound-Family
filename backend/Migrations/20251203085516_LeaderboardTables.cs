using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoalboundFamily.Api.Migrations
{
    /// <inheritdoc />
    public partial class LeaderboardTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Avatar",
                table: "HouseholdMembers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "HouseholdMemberId",
                table: "HouseholdMembers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuestsCompleted",
                table: "HouseholdMembers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Streak",
                table: "HouseholdMembers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Xp",
                table: "HouseholdMembers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Badges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Icon = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Badges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Quests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    XpReward = table.Column<int>(type: "integer", nullable: false),
                    Target = table.Column<int>(type: "integer", nullable: false),
                    Difficulty = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    TimeLimitSeconds = table.Column<int>(type: "integer", nullable: true),
                    IsRepeatable = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MemberBadges",
                columns: table => new
                {
                    HouseholdMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    BadgeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EarnedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberBadges", x => new { x.HouseholdMemberId, x.BadgeId });
                    table.ForeignKey(
                        name: "FK_MemberBadges_Badges_BadgeId",
                        column: x => x.BadgeId,
                        principalTable: "Badges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MemberBadges_HouseholdMembers_HouseholdMemberId",
                        column: x => x.HouseholdMemberId,
                        principalTable: "HouseholdMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MemberQuests",
                columns: table => new
                {
                    HouseholdMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Progress = table.Column<int>(type: "integer", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClaimedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberQuests", x => new { x.HouseholdMemberId, x.QuestId });
                    table.ForeignKey(
                        name: "FK_MemberQuests_HouseholdMembers_HouseholdMemberId",
                        column: x => x.HouseholdMemberId,
                        principalTable: "HouseholdMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MemberQuests_Quests_QuestId",
                        column: x => x.QuestId,
                        principalTable: "Quests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HouseholdMembers_HouseholdMemberId",
                table: "HouseholdMembers",
                column: "HouseholdMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberBadges_BadgeId",
                table: "MemberBadges",
                column: "BadgeId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberQuests_QuestId",
                table: "MemberQuests",
                column: "QuestId");

            migrationBuilder.AddForeignKey(
                name: "FK_HouseholdMembers_HouseholdMembers_HouseholdMemberId",
                table: "HouseholdMembers",
                column: "HouseholdMemberId",
                principalTable: "HouseholdMembers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HouseholdMembers_HouseholdMembers_HouseholdMemberId",
                table: "HouseholdMembers");

            migrationBuilder.DropTable(
                name: "MemberBadges");

            migrationBuilder.DropTable(
                name: "MemberQuests");

            migrationBuilder.DropTable(
                name: "Badges");

            migrationBuilder.DropTable(
                name: "Quests");

            migrationBuilder.DropIndex(
                name: "IX_HouseholdMembers_HouseholdMemberId",
                table: "HouseholdMembers");

            migrationBuilder.DropColumn(
                name: "Avatar",
                table: "HouseholdMembers");

            migrationBuilder.DropColumn(
                name: "HouseholdMemberId",
                table: "HouseholdMembers");

            migrationBuilder.DropColumn(
                name: "QuestsCompleted",
                table: "HouseholdMembers");

            migrationBuilder.DropColumn(
                name: "Streak",
                table: "HouseholdMembers");

            migrationBuilder.DropColumn(
                name: "Xp",
                table: "HouseholdMembers");
        }
    }
}
