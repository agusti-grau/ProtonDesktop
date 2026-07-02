# Technology Stack

## Overview

This document explains the technology choices for ProtonDesktop and the rationale behind each decision.

## Core Technologies

### .NET 8 + WPF

**Choice**: .NET 8 with Windows Presentation Foundation (WPF)

**Rationale**:
- Native Windows desktop application with full system integration
- WPF provides mature, powerful UI framework with XAML
- .NET 8 is LTS (Long Term Support) release
- Excellent performance and modern C# features
- Rich ecosystem of libraries and tools

**Alternatives Considered**:
- **WinUI 3**: Newer but less mature ecosystem
- **Avalonia**: Cross-platform not needed for Windows-only target
- **Electron**: Higher resource usage, not native feel

### CommunityToolkit.Mvvm

**Choice**: Microsoft's official MVVM toolkit

**Rationale**:
- Lightweight, source-generator based
- Official Microsoft support
- Reduces boilerplate with `[ObservableProperty]`, `[RelayCommand]`
- No runtime overhead from reflection
- Well-documented and widely adopted

**Alternatives Considered**:
- **Prism**: Heavier, more features than needed
- **MVVM Light**: No longer maintained
- **Custom MVVM**: Reinventing the wheel

### Microsoft.Extensions.DependencyInjection

**Choice**: Microsoft's DI container

**Rationale**:
- Standard .NET DI abstraction
- Familiar API for .NET developers
- Well-tested and maintained
- Integrates seamlessly with other Microsoft libraries

**Alternatives Considered**:
- **Autofac**: More features but unnecessary complexity
- **DryIOC**: Faster but less familiar

## Data Layer

### Entity Framework Core 8

**Choice**: EF Core with SQLite provider

**Rationale**:
- Industry-standard ORM for .NET
- Code-first migrations for schema evolution
- LINQ queries for type-safe data access
- SQLite provides offline-first capability
- Well-documented and widely used

**Alternatives Considered**:
- **Dapper**: More manual work, less maintainable
- **LiteDB**: Less features than EF Core
- **SQL Server LocalDB**: Heavier, requires SQL Server installation

### SQLite

**Choice**: SQLite for local data storage

**Rationale**:
- Zero configuration, embedded database
- Single file storage (easy backup/migration)
- Excellent performance for read-heavy workloads
- Cross-platform if needed in future
- Perfect for offline-first desktop apps

**Alternatives Considered**:
- **SQL Server LocalDB**: Requires installation
- **LiteDB**: No LINQ provider, less mature
- **Realm**: Overkill for this usecase

## Email Protocol

### MailKit

**Choice**: MailKit for IMAP and SMTP

**Rationale**:
- Modern, actively maintained email library
- Full IMAP4rev2 support with IDLE for push notifications
- Complete MIME support for complex emails
- SMTP with authentication and TLS
- Async/await support throughout
- Works perfectly with ProtonMail Bridge

**Alternatives Considered**:
- **Mail.dll**: Commercial license
- **OpenPop**: No longer maintained, limited features
- **System.Net.Mail**: No IMAP support, limited MIME

## Calendar Protocol

### Ical.Net

**Choice**: Ical.Net for iCalendar parsing and generation

**Rationale**:
- Full RFC 5545 (iCalendar) compliance
- Recurrence rule expansion (RRULE)
- Timezone handling
- Active development
- Works with CalDAV for ProtonMail Bridge

**Alternatives Considered**:
- **DDay.iCal**: Original library, no longer maintained
- **Custom parser**: Reinventing the wheel, error-prone

### CalDAV (HTTP)

**Choice**: Raw HTTP client for CalDAV protocol

**Rationale**:
- CalDAV is HTTP-based (RFC 4791)
- No mature .NET CalDAV library exists
- MailKit doesn't support CalDAV
- HTTP requests with Ical.Net for serialization is straightforward
- Full control over sync logic

**Alternatives Considered**:
- **Third-party CalDAV libraries**: None mature enough
- **Google Calendar API**: Proprietary, not compatible with Proton

## Logging

### Serilog

**Choice**: Serilog for structured logging

**Rationale**:
- Structured logging for better queryability
- Multiple sinks (debug, file, etc.)
- Performance-optimized
- Widely used in .NET ecosystem
- Easy to configure

**Alternatives Considered**:
- **NLog**: Similar features, but Serilog is more modern
- **Microsoft.Extensions.Logging**: Less features than Serilog
- **log4net**: Older, less flexible

## UI/UX Considerations

### Three-Pane Layout

**Choice**: Outlook-style folder list | message list | reading pane

**Rationale**:
- Familiar to Outlook users (target audience)
- Efficient use of screen space
- Standard email client pattern
- Supports wide and narrow window sizes

### WebView2 for HTML Email

**Choice**: Microsoft Edge WebView2 for rendering HTML emails

**Rationale**:
- Modern HTML/CSS/JS rendering
- Consistent with Edge browser
- Better security than IE-based WebView
- Smaller runtime than full browser
- Required for complex HTML emails

**Note**: WebView2 runtime must be installed (included in Windows 11, available for Windows 10)

## Build & Tooling

### MSBuild / dotnet CLI

**Choice**: Standard .NET build tools

**Rationale**:
- Industry standard for .NET projects
- Cross-platform build support
- Excellent IDE integration (Visual Studio, Rider, VS Code)

### Visual Studio 2022 / Rider

**Choice**: Full-featured IDEs for development

**Rationale**:
- Excellent XAML designer support
- Advanced debugging features
- Refactoring tools
- Git integration

## Package Management

### NuGet

**Choice**: NuGet for package management

**Rationale**:
- Standard .NET package manager
- Large ecosystem of packages
- Integrated with Visual Studio and dotnet CLI

## Version Control

### Git

**Choice**: Git for version control

**Rationale**:
- Industry standard
- Excellent tooling support
- GitHub integration for collaboration

## Future Technology Considerations

### Potential Additions

- **WebView2**: For HTML email rendering (if needed)
- **Windows Community Toolkit**: For additional UI controls
- **Microsoft.Extensions.Hosting**: For background service management
- **Polly**: For retry policies and resilience
- **MediatR**: For CQRS pattern if complexity grows

### Monitoring & Diagnostics

- **Application Insights**: If cloud monitoring needed
- **MiniProfiler**: For performance profiling
- **BenchmarkDotNet**: For performance testing

## Technology Risk Assessment

### Low Risk
- .NET 8, WPF, EF Core, MailKit: Mature, well-supported
- CommunityToolkit.Mvvm: Official Microsoft, widely adopted

### Medium Risk
- CalDAV implementation: No mature library, custom code required
- WebView2: Requires runtime installation (but widely available)

### Mitigations
- CalDAV: Start with simple sync, iterate
- WebView2: Provide fallback or installation instructions

## Conclusion

The technology stack prioritizes:
1. **Maturity**: Proven, well-supported libraries
2. **Performance**: Native Windows app with efficient data access
3. **Maintainability**: Clean architecture, standard patterns
4. **Developer Experience**: Familiar tools and libraries
5. **Offline-First**: SQLite for local data storage

All choices align with the goal of building a robust, Outlook-like email and calendar client for Proton services.
