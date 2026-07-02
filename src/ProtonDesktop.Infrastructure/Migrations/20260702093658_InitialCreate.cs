using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProtonDesktop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MailAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    ImapHost = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    ImapPort = table.Column<int>(type: "INTEGER", nullable: false),
                    SmtpHost = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    SmtpPort = table.Column<int>(type: "INTEGER", nullable: false),
                    CalDavHost = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    CalDavPort = table.Column<int>(type: "INTEGER", nullable: false),
                    EncryptedPassword = table.Column<string>(type: "TEXT", nullable: false),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastSyncAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Calendars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Color = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    IsVisible = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    MailAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    SyncToken = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastSyncAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Calendars", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Calendars_MailAccounts_MailAccountId",
                        column: x => x.MailAccountId,
                        principalTable: "MailAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Company = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    MailAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contacts_MailAccounts_MailAccountId",
                        column: x => x.MailAccountId,
                        principalTable: "MailAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmailFolders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Path = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    FolderType = table.Column<int>(type: "INTEGER", nullable: false),
                    ParentFolderId = table.Column<int>(type: "INTEGER", nullable: true),
                    MailAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    UidNext = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UidValidity = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UnreadCount = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastSyncAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailFolders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailFolders_EmailFolders_ParentFolderId",
                        column: x => x.ParentFolderId,
                        principalTable: "EmailFolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmailFolders_MailAccounts_MailAccountId",
                        column: x => x.MailAccountId,
                        principalTable: "MailAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CalendarEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Uid = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Location = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    StartUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsAllDay = table.Column<bool>(type: "INTEGER", nullable: false),
                    Recurrence = table.Column<int>(type: "INTEGER", nullable: false),
                    RecurrenceRule = table.Column<string>(type: "TEXT", nullable: true),
                    RecurrenceParentId = table.Column<int>(type: "INTEGER", nullable: true),
                    RecurrenceExceptionDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CalendarId = table.Column<int>(type: "INTEGER", nullable: false),
                    ETag = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalendarEvents_CalendarEvents_RecurrenceParentId",
                        column: x => x.RecurrenceParentId,
                        principalTable: "CalendarEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CalendarEvents_Calendars_CalendarId",
                        column: x => x.CalendarId,
                        principalTable: "Calendars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmailMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MessageId = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    InReplyTo = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Subject = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    FromAddress = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    FromName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    ToAddresses = table.Column<string>(type: "TEXT", nullable: false),
                    CcAddresses = table.Column<string>(type: "TEXT", nullable: true),
                    BccAddresses = table.Column<string>(type: "TEXT", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SentAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PlainTextBody = table.Column<string>(type: "TEXT", nullable: true),
                    HtmlBody = table.Column<string>(type: "TEXT", nullable: true),
                    Flags = table.Column<int>(type: "INTEGER", nullable: false),
                    HasAttachments = table.Column<bool>(type: "INTEGER", nullable: false),
                    Size = table.Column<long>(type: "INTEGER", nullable: true),
                    Uid = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    FolderId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailMessages_EmailFolders_FolderId",
                        column: x => x.FolderId,
                        principalTable: "EmailFolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CalendarReminders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReminderType = table.Column<int>(type: "INTEGER", nullable: false),
                    MinutesBefore = table.Column<int>(type: "INTEGER", nullable: false),
                    IsSent = table.Column<bool>(type: "INTEGER", nullable: false),
                    SentAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CalendarEventId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarReminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalendarReminders_CalendarEvents_CalendarEventId",
                        column: x => x.CalendarEventId,
                        principalTable: "CalendarEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmailAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Size = table.Column<long>(type: "INTEGER", nullable: false),
                    ContentId = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    IsInline = table.Column<bool>(type: "INTEGER", nullable: false),
                    LocalPath = table.Column<string>(type: "TEXT", nullable: false),
                    EmailMessageId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailAttachments_EmailMessages_EmailMessageId",
                        column: x => x.EmailMessageId,
                        principalTable: "EmailMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_CalendarId_Uid",
                table: "CalendarEvents",
                columns: new[] { "CalendarId", "Uid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_DeletedAt",
                table: "CalendarEvents",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_EndUtc",
                table: "CalendarEvents",
                column: "EndUtc");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_RecurrenceParentId",
                table: "CalendarEvents",
                column: "RecurrenceParentId");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_StartUtc",
                table: "CalendarEvents",
                column: "StartUtc");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_Uid",
                table: "CalendarEvents",
                column: "Uid");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarReminders_CalendarEventId",
                table: "CalendarReminders",
                column: "CalendarEventId");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarReminders_IsSent",
                table: "CalendarReminders",
                column: "IsSent");

            migrationBuilder.CreateIndex(
                name: "IX_Calendars_MailAccountId_Name",
                table: "Calendars",
                columns: new[] { "MailAccountId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_Email",
                table: "Contacts",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_MailAccountId_Email",
                table: "Contacts",
                columns: new[] { "MailAccountId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailAttachments_EmailMessageId",
                table: "EmailAttachments",
                column: "EmailMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailFolders_FolderType",
                table: "EmailFolders",
                column: "FolderType");

            migrationBuilder.CreateIndex(
                name: "IX_EmailFolders_MailAccountId_Path",
                table: "EmailFolders",
                columns: new[] { "MailAccountId", "Path" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailFolders_ParentFolderId",
                table: "EmailFolders",
                column: "ParentFolderId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessages_DeletedAt",
                table: "EmailMessages",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessages_FolderId_Uid",
                table: "EmailMessages",
                columns: new[] { "FolderId", "Uid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessages_FromAddress",
                table: "EmailMessages",
                column: "FromAddress");

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessages_MessageId",
                table: "EmailMessages",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessages_ReceivedAt",
                table: "EmailMessages",
                column: "ReceivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MailAccounts_Email",
                table: "MailAccounts",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_MailAccounts_IsDefault",
                table: "MailAccounts",
                column: "IsDefault");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalendarReminders");

            migrationBuilder.DropTable(
                name: "Contacts");

            migrationBuilder.DropTable(
                name: "EmailAttachments");

            migrationBuilder.DropTable(
                name: "CalendarEvents");

            migrationBuilder.DropTable(
                name: "EmailMessages");

            migrationBuilder.DropTable(
                name: "Calendars");

            migrationBuilder.DropTable(
                name: "EmailFolders");

            migrationBuilder.DropTable(
                name: "MailAccounts");
        }
    }
}
