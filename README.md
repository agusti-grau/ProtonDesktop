# ProtonDesktop

A native Windows desktop client for Proton services, providing Outlook-like email and calendar functionality through ProtonMail Bridge.

## Overview

ProtonDesktop is a full-featured email and calendar client for Windows that connects to Proton services via ProtonMail Bridge. It provides a familiar Outlook-style three-pane interface with offline-first data storage.

### Key Features

- **Email Client**: Full IMAP/SMTP integration via ProtonMail Bridge
- **Calendar Client**: CalDAV integration for events and reminders
- **Offline-First**: SQLite local cache for working without connection
- **Outlook-like UI**: Three-pane layout (folders | list | reader)
- **Calendar Views**: Day, Week, and Month views
- **Search**: Full-text search across emails and calendar events
- **System Tray**: Minimize to tray with notifications
- **Keyboard Shortcuts**: Outlook-compatible shortcuts

## Tech Stack

| Layer | Technology |
|-------|-----------|
| UI Framework | WPF (.NET 8) |
| MVVM | CommunityToolkit.Mvvm |
| Dependency Injection | Microsoft.Extensions.DependencyInjection |
| Email Protocol | MailKit (IMAP/SMTP) |
| Calendar Protocol | Ical.Net + CalDAV HTTP |
| Local Storage | Entity Framework Core + SQLite |
| Logging | Serilog |

## Architecture

```
ProtonDesktop/
├── ProtonDesktop/              # WPF Application (UI layer)
├── ProtonDesktop.Core/         # Domain models & interfaces
├── ProtonDesktop.Services/     # Business logic & sync services
├── ProtonDesktop.Infrastructure/ # Data access, IMAP/SMTP/CalDAV clients
└── tests/
    ├── ProtonDesktop.UnitTests/
    └── ProtonDesktop.IntegrationTests/
```

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for detailed architecture documentation.

## Prerequisites

- **Windows 10/11**
- **.NET 8 SDK**
- **ProtonMail Bridge** running locally
  - Default IMAP: `localhost:1143`
  - Default SMTP: `localhost:1025`
  - CalDAV: configured via Bridge

## Getting Started

### 1. Install ProtonMail Bridge

Download and install [ProtonMail Bridge](https://proton.me/mail/bridge) from Proton's website. Configure your Proton account and ensure the Bridge is running.

### 2. Clone and Build

```bash
git clone <repository-url>
cd ProtonDesktop
dotnet build
```

### 3. Run

```bash
dotnet run --project src/ProtonDesktop
```

### 4. Configure

On first launch, configure your ProtonMail Bridge connection settings (defaults should work if Bridge is running on standard ports).

## Development

### Build

```bash
dotnet build
```

### Run Tests

```bash
dotnet test
```

### Project Structure

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for the complete project structure and layer responsibilities.

## Documentation

- [Architecture](docs/ARCHITECTURE.md) - System architecture and design decisions
- [Tech Stack](docs/TECH_STACK.md) - Technology choices and rationale
- [Phase Plan](docs/PHASE_PLAN.md) - Development phases and milestones
- [Development Diary](docs/DEVELOPMENT_DIARY.md) - Development progress log

## License

This project is proprietary software.
