using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcurementSystem.Migrations
{
    /// <inheritdoc />
    public partial class Fixcascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_PurchaseRequests",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Users",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseRequests_Users",
                table: "PurchaseRequests");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseRequests_CreatedAt",
                table: "PurchaseRequests");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseRequests_Status",
                table: "PurchaseRequests");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_ActionDate",
                table: "AuditLogs");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "PurchaseRequests",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ActionDate",
                table: "AuditLogs",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_PurchaseRequests_PurchaseRequestId",
                table: "AuditLogs",
                column: "PurchaseRequestId",
                principalTable: "PurchaseRequests",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Users",
                table: "AuditLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseRequests_Users",
                table: "PurchaseRequests",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_PurchaseRequests_PurchaseRequestId",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Users",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseRequests_Users",
                table: "PurchaseRequests");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "PurchaseRequests",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ActionDate",
                table: "AuditLogs",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequests_CreatedAt",
                table: "PurchaseRequests",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequests_Status",
                table: "PurchaseRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ActionDate",
                table: "AuditLogs",
                column: "ActionDate");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_PurchaseRequests",
                table: "AuditLogs",
                column: "PurchaseRequestId",
                principalTable: "PurchaseRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Users",
                table: "AuditLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseRequests_Users",
                table: "PurchaseRequests",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
