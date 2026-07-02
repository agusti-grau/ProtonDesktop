# Architecture Documentation

## Overview

ProtonDesktop follows a clean architecture pattern with clear separation of concerns across four main layers.

## Solution Structure

```
ProtonDesktop/
├── src/
│   ├── ProtonDesktop/                    # Presentation Layer (WPF App)
│   │   ├── Views/                        # XAML views and user controls
│   │   ├── ViewModels/                   # MVVM view models
│   │   ├── Converters/                   # Value converters for XAML
│   │   ├── Resources/                    # Styles, themes, icons
│   │   ├── App.xaml.cs                   # DI container, startup
│   │   └── MainWindow.xaml               # Main application window
│   │
│   ├── ProtonDesktop.Core/               # Domain Layer
│   │   ├── Models/                       # Domain entities
│   │   │   ├── MailAccount.cs
│   │   │   ├── EmailFolder.cs
│   │   │   ├── EmailMessage.cs
│   │   │   ├── EmailAttachment.cs
│   │   │   ├── Contact.cs
│   │   │   ├── Calendar.cs
│   │   │   ├── CalendarEvent.cs
│   │   │   └── CalendarReminder.cs
│   │   ├── Interfaces/                   # Service contracts
│   │   │   ├── IEmailRepository.cs
│   │   │   ├── ICalendarRepository.cs
│   │   │   ├── IImapSyncService.cs
│   │   │   ├── ISmtpService.cs
│   │   │   ├── ICalDavSyncService.cs
│   │   │   ├── IReminderService.cs
│   │   │   └── INavigationService.cs
│   │   └── Enums/                        # Domain enumerations
│   │
│   ├── ProtonDesktop.Services/           # Application Layer
│   │   ├── Email/
│   │   │   ├── EmailSyncService.cs       # Orchestrates email sync
│   │   │   └── EmailSendService.cs       # Compose and send logic
│   │   ├── Calendar/
│   │   │   ├── CalendarSyncService.cs    # Orchestrates calendar sync
│   │   │   └── ReminderService.cs        # Reminder scheduling
│   │   └── Navigation/
│   │       └── NavigationService.cs      # View navigation
│   │
│   └── ProtonDesktop.Infrastructure/     # Infrastructure Layer
│       ├── Data/
│       │   ├── AppDbContext.cs           # EF Core DbContext
│       │   ├── Migrations/             # Database migrations
│       │   └── Configurations/         # Entity configurations
│       ├── Repositories/
│       │   ├── EmailRepository.cs
│       │   └── CalendarRepository.cs
│       ├── Protocols/
│       │   ├── ImapClient.cs           # MailKit IMAP wrapper
│       │   ├── SmtpClient.cs           # MailKit SMTP wrapper
│       │   └── CalDavClient.cs         # CalDAV HTTP client
│       └── Security/
│           └── CredentialStore.cs      # DPAPI credential storage
│
└── tests/
    ├── ProtonDesktop.UnitTests/
    └── ProtonDesktop.IntegrationTests/
```

## Layer Responsibilities

### Presentation Layer (ProtonDesktop)

The WPF application layer handles all UI concerns:

- **Views**: XAML-based user interface with MVVM pattern
- **ViewModels**: Presentation logic using CommunityToolkit.Mvvm
- **Navigation**: View switching and window management
- **Dependency Injection**: Container configuration at startup

### Domain Layer (ProtonDesktop.Core)

Contains business entities and service contracts:

- **Models**: Pure domain entities with no external dependencies
- **Interfaces**: Service contracts for dependency inversion
- **Enums**: Domain-specific enumerations
- **No dependencies** on other projects

### Application Layer (ProtonDesktop.Services)

Orchestrates business logic and coordinates between layers:

- **Sync Services**: Coordinate data synchronization between local cache and remote servers
- **Business Logic**: Email composition, calendar event management
- **Depends on**: Core layer only

### Infrastructure Layer (ProtonDesktop.Infrastructure)

Implements technical concerns:

- **Data Access**: EF Core DbContext, repositories, migrations
- **Protocol Clients**: IMAP, SMTP, CalDAV implementations
- **External Services**: ProtonMail Bridge communication
- **Depends on**: Core layer only

## Data Flow

### Email Sync Flow

```
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────┐
│ ProtonMail      │────▶│ ImapSyncService  │────▶│ EmailRepository │
│ Bridge (IMAP)   │     │ (Infrastructure) │     │ (Infrastructure)│
└─────────────────┘     └──────────────────┘     └────────┬────────┘
                                                          │
                                                          ▼
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────┐
│ EmailListView   │◀────│ EmailListVM      │◀────│   AppDbContext  │
│ (Presentation)  │     │ (Presentation)   │     │ (Infrastructure)│
└─────────────────┘     └──────────────────┘     └─────────────────┘
```

### Email Send Flow

```
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────┐
│ ComposeView     │────▶│ ComposeViewModel │────▶│  SmtpService    │
│ (Presentation)  │     │ (Presentation)   │     │ (Infrastructure)│
└─────────────────┘     └──────────────────┘     └────────┬────────┘
                                                          │
                                                          ▼
                                                  ┌─────────────────┐
                                                  │ ProtonMail      │
                                                  │ Bridge (SMTP)   │
                                                  └─────────────────┘
```

## Database Schema

### Email Tables

- **MailAccounts**: User account configurations
- **EmailFolders**: Folder hierarchy (Inbox, Sent, Drafts, etc.)
- **EmailMessages**: Email messages with metadata
- **EmailAttachments**: Attachment metadata and local paths
- **Contacts**: Address book contacts

### Calendar Tables

- **Calendars**: Calendar collections
- **CalendarEvents**: Event data with recurrence rules
- **CalendarReminders**: Reminder configurations

## Dependency Injection

All services are registered in `App.xaml.cs` at startup:

```csharp
services.AddSingleton<IEmailRepository, EmailRepository>();
services.AddSingleton<ICalendarRepository, CalendarRepository>();
services.AddTransient<IImapSyncService, ImapSyncService>();
services.AddTransient<ISmtpService, SmtpService>();
services.AddTransient<ICalDavSyncService, CalDavSyncService>();
services.AddSingleton<IReminderService, ReminderService>();
services.AddSingleton<INavigationService, NavigationService>();
```

## Offline-First Strategy

1. **Local Cache**: All emails and calendar events are stored in SQLite
2. **Sync on Startup**: Fetch changes from server on app launch
3. **Background Sync**: Periodic sync at configurable intervals
4. **Conflict Resolution**: Server wins for read-only data; local changes queued for upload
5. **Offline Mode**: Full functionality with local data; changes sync when online

## Security Considerations

- **Credentials**: Stored encrypted using Windows DPAPI
- **Connection**: Localhost connections to ProtonMail Bridge (no internet exposure)
- **Data at Rest**: SQLite database in user's AppData folder
- **No Direct API Keys**: All authentication handled by ProtonMail Bridge

## Testing Strategy

- **Unit Tests**: ViewModels, services, business logic
- **Integration Tests**: Repository implementations, protocol clients
- **UI Tests**: Manual testing with ProtonMail Bridge

## Future Considerations

- **Multiple Accounts**: Support for multiple Proton accounts
- **Push Notifications**: Real-time updates via Bridge
- **Advanced Search**: Full-text search with indexing
- **Rules/Filters**: Server-side and client-side email filters
- **Add-ins**: Extensibility for third-party integrations
