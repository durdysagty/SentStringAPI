using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MessengerAPI.Models.Migrations
{
    public partial class _3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dialogues",
                columns: table => new
                {
                    IndividualId = table.Column<int>(nullable: false),
                    InterlocutorId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dialogues", x => new { x.IndividualId, x.InterlocutorId });
                    table.ForeignKey(
                        name: "FK_Dialogues_Individuals_IndividualId",
                        column: x => x.IndividualId,
                        principalTable: "Individuals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderId = table.Column<int>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DialoguesMessages",
                columns: table => new
                {
                    DialogueIndividualId = table.Column<int>(nullable: false),
                    DialogueInterlocutorId = table.Column<int>(nullable: false),
                    MessageId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialoguesMessages", x => new { x.DialogueIndividualId, x.DialogueInterlocutorId, x.MessageId });
                    table.ForeignKey(
                        name: "FK_DialoguesMessages_Messages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DialoguesMessages_Dialogues_DialogueIndividualId_DialogueInterlocutorId",
                        columns: x => new { x.DialogueIndividualId, x.DialogueInterlocutorId },
                        principalTable: "Dialogues",
                        principalColumns: new[] { "IndividualId", "InterlocutorId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DialoguesMessages_MessageId",
                table: "DialoguesMessages",
                column: "MessageId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DialoguesMessages");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Dialogues");
        }
    }
}
