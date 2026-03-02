using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafetyCompliance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InspectionPhotos_AspNetUsers_UploadedById",
                table: "InspectionPhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_InspectionRounds_AspNetUsers_InspectedById",
                table: "InspectionRounds");

            migrationBuilder.DropForeignKey(
                name: "FK_InspectionRounds_AspNetUsers_ReviewedById",
                table: "InspectionRounds");

            migrationBuilder.DropIndex(
                name: "IX_InspectionRounds_InspectedById",
                table: "InspectionRounds");

            migrationBuilder.DropIndex(
                name: "IX_InspectionRounds_ReviewedById",
                table: "InspectionRounds");

            migrationBuilder.DropIndex(
                name: "IX_InspectionPhotos_UploadedById",
                table: "InspectionPhotos");

            migrationBuilder.DropColumn(
                name: "Caption",
                table: "InspectionPhotos");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "EquipmentInspections");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "EquipmentInspections");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "InspectionRounds",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AddColumn<int>(
                name: "InspectionScheduleId",
                table: "InspectionRounds",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InspectionSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlantId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Frequency = table.Column<byte>(type: "tinyint", nullable: false),
                    FrequencyInterval = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    NextDueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    LastCompletedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AutoGenerate = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionSchedules_Plants_PlantId",
                        column: x => x.PlantId,
                        principalTable: "Plants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Issues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InspectionRoundId = table.Column<int>(type: "int", nullable: true),
                    EquipmentInspectionId = table.Column<int>(type: "int", nullable: true),
                    EquipmentId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Priority = table.Column<byte>(type: "tinyint", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    AssignedTo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhotoBase64 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhotoFileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Issues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Issues_EquipmentInspections_EquipmentInspectionId",
                        column: x => x.EquipmentInspectionId,
                        principalTable: "EquipmentInspections",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Issues_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Issues_InspectionRounds_InspectionRoundId",
                        column: x => x.InspectionRoundId,
                        principalTable: "InspectionRounds",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Notes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EquipmentId = table.Column<int>(type: "int", nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    PlantId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    IsPinned = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notes_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Notes_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Notes_Plants_PlantId",
                        column: x => x.PlantId,
                        principalTable: "Plants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ServiceBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EquipmentId = table.Column<int>(type: "int", nullable: false),
                    EquipmentInspectionId = table.Column<int>(type: "int", nullable: true),
                    ServiceProvider = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SentDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ExpectedReturnDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ActualReturnDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceBookings_EquipmentInspections_EquipmentInspectionId",
                        column: x => x.EquipmentInspectionId,
                        principalTable: "EquipmentInspections",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServiceBookings_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InspectionRoundId = table.Column<int>(type: "int", nullable: true),
                    IssueId = table.Column<int>(type: "int", nullable: true),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhotoBase64 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhotoFileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_InspectionRounds_InspectionRoundId",
                        column: x => x.InspectionRoundId,
                        principalTable: "InspectionRounds",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Comments_Issues_IssueId",
                        column: x => x.IssueId,
                        principalTable: "Issues",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_InspectionRounds_InspectionScheduleId",
                table: "InspectionRounds",
                column: "InspectionScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_InspectionRoundId",
                table: "Comments",
                column: "InspectionRoundId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_IssueId",
                table: "Comments",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionSchedules_PlantId",
                table: "InspectionSchedules",
                column: "PlantId");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_EquipmentId",
                table: "Issues",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_EquipmentInspectionId",
                table: "Issues",
                column: "EquipmentInspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_InspectionRoundId",
                table: "Issues",
                column: "InspectionRoundId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_Category",
                table: "Notes",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_CompanyId",
                table: "Notes",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_EquipmentId",
                table: "Notes",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_IsPinned",
                table: "Notes",
                column: "IsPinned");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_PlantId",
                table: "Notes",
                column: "PlantId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBookings_EquipmentId",
                table: "ServiceBookings",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBookings_EquipmentInspectionId",
                table: "ServiceBookings",
                column: "EquipmentInspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBookings_Status",
                table: "ServiceBookings",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_InspectionRounds_InspectionSchedules_InspectionScheduleId",
                table: "InspectionRounds",
                column: "InspectionScheduleId",
                principalTable: "InspectionSchedules",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InspectionRounds_InspectionSchedules_InspectionScheduleId",
                table: "InspectionRounds");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "InspectionSchedules");

            migrationBuilder.DropTable(
                name: "Notes");

            migrationBuilder.DropTable(
                name: "ServiceBookings");

            migrationBuilder.DropTable(
                name: "Issues");

            migrationBuilder.DropIndex(
                name: "IX_InspectionRounds_InspectionScheduleId",
                table: "InspectionRounds");

            migrationBuilder.DropColumn(
                name: "InspectionScheduleId",
                table: "InspectionRounds");

            migrationBuilder.AlterColumn<byte>(
                name: "Status",
                table: "InspectionRounds",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Caption",
                table: "InspectionPhotos",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "EquipmentInspections",
                type: "decimal(9,6)",
                precision: 9,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "EquipmentInspections",
                type: "decimal(9,6)",
                precision: 9,
                scale: 6,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InspectionRounds_InspectedById",
                table: "InspectionRounds",
                column: "InspectedById");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionRounds_ReviewedById",
                table: "InspectionRounds",
                column: "ReviewedById");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionPhotos_UploadedById",
                table: "InspectionPhotos",
                column: "UploadedById");

            migrationBuilder.AddForeignKey(
                name: "FK_InspectionPhotos_AspNetUsers_UploadedById",
                table: "InspectionPhotos",
                column: "UploadedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InspectionRounds_AspNetUsers_InspectedById",
                table: "InspectionRounds",
                column: "InspectedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InspectionRounds_AspNetUsers_ReviewedById",
                table: "InspectionRounds",
                column: "ReviewedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
