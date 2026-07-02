# Development Diary

This diary tracks the development progress of ProtonDesktop, documenting decisions, challenges, and milestones.

---

## 2026-07-02: Project Inception

### What Happened
- Project initialized with clean architecture structure
- Technology stack selected and documented
- Phase plan created for Email + Calendar MVP
- Documentation structure established

### Decisions Made

**Architecture**: Clean Architecture with four layers
- **Core**: Domain models and interfaces (no dependencies)
- **Services**: Business logic and orchestration
- **Infrastructure**: Data access and protocol implementations
- **Presentation**: WPF UI with MVVM pattern

**Technology Choices**:
- .NET 8 + WPF for native Windows desktop
- MailKit for IMAP/SMTP (works with ProtonMail Bridge)
- Ical.Net + CalDAV HTTP for calendar
- EF Core + SQLite for offline-first storage
- CommunityToolkit.Mvvm for MVVM
- Serilog for logging

**ProtonMail Bridge Strategy**:
- Use Bridge's local IMAP/SMTP (localhost:1143, localhost:1025)
- Use Bridge's CalDAV for calendar sync
- No direct Proton API integration (simpler, leverages Bridge's encryption)

**UI Design**:
- Outlook-style three-pane layout
- Folder tree | Email list | Reading pane
- Day/Week/Month calendar views
- System tray integration

### Challenges
- CalDAV has no mature .NET library → will implement raw HTTP client
- HTML email rendering → will use WebView2
- ProtonMail Bridge compatibility → need to test early

### Next Steps
1. Push documentation to GitHub
2. Begin Phase 1.0: Foundation
   - Create domain models
   - Define service interfaces
   - Set up EF Core DbContext
   - Configure DI container
   - Build main window shell

---

## 2026-07-02: Phase 0 Complete

### Status
✅ **Phase 0: Project Setup - COMPLETE**

### What Was Done
- Solution structure created with four projects
- Test projects added (UnitTests, IntegrationTests)
- NuGet packages configured:
  - CommunityToolkit.Mvvm 8.4.2
  - Microsoft.Extensions.DependencyInjection 10.0.9
  - Serilog 4.3.1
  - MailKit 4.17.0
  - Ical.Net 5.2.3
  - EF Core 8.0 + SQLite
- Basic WPF application shell (MainWindow)
- All projects build successfully

### Files Created
```
ProtonDesktop.sln
src/
├── ProtonDesktop/
│   ├── ProtonDesktop.csproj
│   ├── App.xaml
│   ├── App.xaml.cs
│   ├── MainWindow.xaml
│   └── MainWindow.xaml.cs
├── ProtonDesktop.Core/
│   └── ProtonDesktop.Core.csproj
├── ProtonDesktop.Services/
│   └── ProtonDesktop.Services.csproj
└── ProtonDesktop.Infrastructure/
    └── ProtonDesktop.Infrastructure.csproj
tests/
├── ProtonDesktop.UnitTests/
│   └── ProtonDesktop.UnitTests.csproj
└── ProtonDesktop.IntegrationTests/
    └── ProtonDesktop.IntegrationTests.csproj
```

### Notes
- Phase 0 was completed in a previous session
- Foundation is solid, ready for Phase 1
- Documentation created to guide development

---

## 2026-07-02: Documentation Created

### Status
📝 **Documentation - COMPLETE**

### What Was Done
Created comprehensive documentation:
- **README.md**: Project overview, tech stack, setup instructions
- **docs/ARCHITECTURE.md**: Detailed architecture documentation
- **docs/TECH_STACK.md**: Technology choices and rationale
- **docs/PHASE_PLAN.md**: Development phases and milestones
- **docs/DEVELOPMENT_DIARY.md**: This file

### Purpose
- Guide development process
- Onboard new contributors
- Document decisions for future reference
- Provide context for AI assistants

### Next Steps
- Push to GitHub
- Begin Phase 1.0 implementation

---

## 2026-07-02: Phase 1.0 Complete - Foundation

### Status
✅ **Phase 1.0: Foundation - COMPLETE**

### What Was Done

**Core Domain Models** (ProtonDesktop.Core/Models/):
- `MailAccount` - Account configuration with IMAP/SMTP/CalDAV connection settings
- `EmailFolder` - Folder hierarchy with UID tracking for sync
- `EmailMessage` - Email messages with flags, attachments, HTML/plain body
- `EmailAttachment` - Attachment metadata with local file paths
- `Contact` - Address book contacts
- `Calendar` - Calendar collections with sync tokens
- `CalendarEvent` - Events with recurrence rules and reminders
- `CalendarReminder` - Reminder configurations

**Enums** (ProtonDesktop.Core/Enums/):
- `EmailFlag` - Flags (Seen, Flagged, Answered, Forwarded, Draft, Deleted)
- `FolderType` - Folder types (Inbox, Sent, Drafts, Trash, Spam, Archive, Junk, Custom)
- `EventRecurrence` - Recurrence patterns (None, Daily, Weekly, Monthly, Yearly)
- `ReminderType` - Reminder types (Popup, Email, Sound)

**Service Interfaces** (ProtonDesktop.Core/Interfaces/):
- `IEmailRepository` - Email CRUD operations
- `ICalendarRepository` - Calendar and event CRUD operations
- `IAccountRepository` - Account management
- `IImapSyncService` - IMAP sync operations
- `ISmtpService` - SMTP send operations
- `ICalDavSyncService` - CalDAV sync operations
- `IReminderService` - Reminder scheduling
- `INavigationService` - View navigation

**Infrastructure** (ProtonDesktop.Infrastructure/):
- `AppDbContext` - EF Core DbContext with all entity configurations
- `DesignTimeDbContextFactory` - Design-time factory for migrations
- Entity configurations for all 8 entities with proper indexes and relationships
- `AccountRepository` - Account CRUD implementation
- `EmailRepository` - Email CRUD with search and unread counts
- `CalendarRepository` - Calendar/event CRUD with reminder queries
- `ImapSyncService` - MailKit IMAP client wrapper
- `SmtpService` - MailKit SMTP client wrapper with draft saving
- `CalDavSyncService` - CalDAV stub (to be fully implemented in Phase 1.3)

**Services** (ProtonDesktop.Services/):
- `NavigationService` - View navigation implementation
- `EmailSyncService` - Orchestrates email sync from IMAP to local DB
- `EmailSendService` - Orchestrates email sending via SMTP
- `CalendarSyncService` - Orchestrates calendar sync from CalDAV to local DB
- `ReminderService` - Background reminder checking with timer

**Application** (ProtonDesktop/):
- `App.xaml.cs` - DI container setup with all services registered
- Serilog logging configuration (Debug + File sinks)
- Automatic database migration on startup

**Database**:
- Initial migration created and applied
- SQLite database at `protondesktop.db`

### Challenges Resolved
- MailKit API changes: `FetchAsync` requires `FetchRequest` object (not `MessageSummaryItems`)
- MailKit API changes: `AppendAsync` requires `AppendRequest` object (not `MimeMessage`)
- EF Core design-time: Required `IDesignTimeDbContextFactory` for migrations
- Package version compatibility: EF Core Design 10.x incompatible with .NET 8, used 8.0.*
- Serilog package: Needed to add to Services and Infrastructure projects separately

### Build Status
✅ All 6 projects build successfully with 0 errors, 0 warnings

### Files Created/Modified
```
src/ProtonDesktop.Core/
├── Enums/
│   ├── EmailFlag.cs
│   ├── FolderType.cs
│   ├── EventRecurrence.cs
│   └── ReminderType.cs
├── Interfaces/
│   ├── IEmailRepository.cs
│   ├── ICalendarRepository.cs
│   ├── IAccountRepository.cs
│   ├── IImapSyncService.cs
│   ├── ISmtpService.cs
│   ├── ICalDavSyncService.cs
│   ├── IReminderService.cs
│   └── INavigationService.cs
└── Models/
    ├── MailAccount.cs
    ├── EmailFolder.cs
    ├── EmailMessage.cs
    ├── EmailAttachment.cs
    ├── Contact.cs
    ├── Calendar.cs
    ├── CalendarEvent.cs
    └── CalendarReminder.cs

src/ProtonDesktop.Infrastructure/
├── Data/
│   ├── AppDbContext.cs
│   ├── DesignTimeDbContextFactory.cs
│   ├── Configurations/
│   │   ├── MailAccountConfiguration.cs
│   │   ├── EmailFolderConfiguration.cs
│   │   ├── EmailMessageConfiguration.cs
│   │   ├── EmailAttachmentConfiguration.cs
│   │   ├── ContactConfiguration.cs
│   │   ├── CalendarConfiguration.cs
│   │   ├── CalendarEventConfiguration.cs
│   │   └── CalendarReminderConfiguration.cs
│   └── Migrations/
│       └── (InitialCreate migration files)
├── Repositories/
│   ├── AccountRepository.cs
│   ├── EmailRepository.cs
│   └── CalendarRepository.cs
└── Protocols/
    ├── ImapSyncService.cs
    ├── SmtpService.cs
    └── CalDavSyncService.cs

src/ProtonDesktop.Services/
├── Navigation/
│   └── NavigationService.cs
├── Email/
│   ├── EmailSyncService.cs
│   └── EmailSendService.cs
└── Calendar/
    ├── CalendarSyncService.cs
    └── ReminderService.cs

src/ProtonDesktop/
└── App.xaml.cs (updated with DI + Serilog)
```

### Next Steps
- Phase 1.1: Email Backend - Complete IMAP/SMTP implementation with full message body fetching
- Phase 1.2: Email UI - Build the three-pane Outlook-style interface

---

## Upcoming: Phase 1.1 - Email Backend

### Planned Work
- [ ] Implement ImapSyncService: Connect to Bridge, list folders, fetch messages
- [ ] Implement SmtpService: Compose and send emails via Bridge
- [ ] Background sync timer with configurable interval
- [ ] Delta sync logic (UIDNEXT for incremental updates)
- [ ] Attachment download and storage

### Expected Duration
3-4 days

### Success Criteria
- Can connect to Bridge and list folders
- Can fetch emails and store in local database
- Can send emails via SMTP
- Sync runs periodically in background

---

## Template for Future Entries

```markdown
## YYYY-MM-DD: [Title]

### Status
[🔄 In Progress / ✅ Complete / ⏳ Blocked]

### What Happened
[Description of work done]

### Decisions Made
[Key decisions and rationale]

### Challenges
[Problems encountered and solutions]

### Code Changes
[Summary of files changed/added]

### Next Steps
[What to work on next]

### Notes
[Any other relevant information]
```

---

## Progress Summary

| Phase | Status | Started | Completed |
|-------|--------|---------|-----------|
| Phase 0: Project Setup | ✅ Complete | - | 2026-07-02 |
| Phase 1.0: Foundation | ✅ Complete | 2026-07-02 | 2026-07-02 |
| Phase 1.1: Email Backend | 🔄 In Progress | - | - |
| Phase 1.2: Email UI | ⏳ Pending | - | - |
| Phase 1.3: Calendar Backend | ⏳ Pending | - | - |
| Phase 1.4: Calendar UI | ⏳ Pending | - | - |
| Phase 1.5: Settings & Auth | ⏳ Pending | - | - |
| Phase 1.6: Polish | ⏳ Pending | - | - |

---

## Lessons Learned

*(To be updated as development progresses)*

---

## Open Questions

1. **WebView2 Runtime**: Should we bundle it or require users to install?
   - Decision: Require installation (included in Windows 11, available for Windows 10)

2. **Database Location**: Where to store SQLite database?
   - Decision: `%LOCALAPPDATA%\ProtonDesktop\protondesktop.db`

3. **Credential Storage**: How to securely store Bridge credentials?
   - Decision: Use Windows DPAPI for encryption

4. **Sync Strategy**: How to handle sync conflicts?
   - Decision: Server wins for read-only data (emails), merge for calendar events

---

## Resources

- [ProtonMail Bridge Documentation](https://proton.me/support/protonmail-bridge)
- [MailKit Documentation](https://github.com/jstedfast/MailKit)
- [EF Core Documentation](https://docs.microsoft.com/ef/core/)
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet)
- [WPF Documentation](https://docs.microsoft.com/dotnet/desktop/wpf/)
