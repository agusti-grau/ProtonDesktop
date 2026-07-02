# Development Phase Plan

## Overview

This document outlines the development phases for ProtonDesktop, from initial setup to a fully-featured email and calendar client.

## Phase 0: Project Setup (Completed)

**Status**: ✅ Complete

**Deliverables**:
- Solution structure with four projects (Core, Services, Infrastructure, App)
- Test projects (UnitTests, IntegrationTests)
- NuGet package references configured
- Basic WPF application shell

**Notes**:
- Phase 0 established the foundation with clean architecture
- All dependencies selected and added to projects
- Solution builds successfully

---

## Phase 1: Email + Calendar MVP

**Status**: 🔄 In Progress

**Goal**: Deliver a functional email and calendar client with core features.

### Phase 1.0: Foundation

**Status**: 🔄 In Progress

**Deliverables**:
- [ ] Core domain models (MailAccount, EmailFolder, EmailMessage, CalendarEvent, etc.)
- [ ] Service interfaces (IEmailRepository, ICalendarRepository, IImapSyncService, etc.)
- [ ] EF Core DbContext with entity configurations
- [ ] Initial database migration
- [ ] DI container setup in App.xaml.cs
- [ ] Serilog logging configuration
- [ ] Main window shell with navigation structure
- [ ] INavigationService implementation

**Acceptance Criteria**:
- App launches and shows empty main window
- Database is created on first run
- Navigation between placeholder views works
- All services resolvable via DI

**Estimated Effort**: 2-3 days

---

### Phase 1.1: Email Backend

**Status**: ⏳ Pending

**Deliverables**:
- [ ] ImapSyncService: Connect to ProtonMail Bridge, list folders, fetch messages
- [ ] SmtpService: Compose and send emails via Bridge
- [ ] EmailRepository: CRUD operations for emails in SQLite
- [ ] Background sync timer (configurable interval)
- [ ] Delta sync logic (UIDNEXT for incremental updates)
- [ ] Attachment download and storage

**Acceptance Criteria**:
- Can connect to Bridge and list folders
- Can fetch emails and store in local database
- Can send emails via SMTP
- Sync runs periodically in background
- Offline mode works with cached data

**Dependencies**: Phase 1.0

**Estimated Effort**: 3-4 days

---

### Phase 1.2: Email UI

**Status**: ⏳ Pending

**Deliverables**:
- [ ] Folder tree view (left pane) with unread counts
- [ ] Email list view (middle pane) with sorting and filtering
- [ ] Reading pane (right pane) with HTML rendering
- [ ] Compose window with To/Cc/Bcc, Subject, Body, Attachments
- [ ] Reply/Forward functionality
- [ ] Delete/Move/Flag operations
- [ ] Search bar with full-text search
- [ ] Keyboard shortcuts (Ctrl+N, Ctrl+R, Delete, etc.)

**Acceptance Criteria**:
- Three-pane Outlook-style layout
- Can read emails with HTML formatting
- Can compose and send new emails
- Can reply to and forward emails
- Search works across subject/from/body
- UI is responsive and smooth

**Dependencies**: Phase 1.1

**Estimated Effort**: 4-5 days

---

### Phase 1.3: Calendar Backend

**Status**: ⏳ Pending

**Deliverables**:
- [ ] CalDavSyncService: Sync calendars and events from Bridge
- [ ] Event CRUD operations (create, read, update, delete)
- [ ] Recurrence rule expansion (daily, weekly, monthly, yearly)
- [ ] ReminderService: Background scheduler for reminders
- [ ] CalendarRepository: CRUD operations in SQLite
- [ ] Conflict resolution for sync

**Acceptance Criteria**:
- Can sync calendars from ProtonMail Bridge
- Can create/edit/delete events
- Recurring events display correctly
- Reminders fire at scheduled times
- Offline mode works with cached events

**Dependencies**: Phase 1.0

**Estimated Effort**: 3-4 days

---

### Phase 1.4: Calendar UI

**Status**: ⏳ Pending

**Deliverables**:
- [ ] Month view: Calendar grid with event indicators
- [ ] Week view: 7-column time grid
- [ ] Day view: Single column time grid
- [ ] View switcher toolbar
- [ ] Event editor dialog (title, location, start/end, recurrence, reminders)
- [ ] Drag-and-drop event rescheduling
- [ ] Navigation (previous/next, today button)
- [ ] Calendar selector (show/hide calendars)

**Acceptance Criteria**:
- All three views functional and switchable
- Can create/edit/delete events from UI
- Recurring events display correctly
- Navigation works smoothly
- UI is responsive with many events

**Dependencies**: Phase 1.3

**Estimated Effort**: 4-5 days

---

### Phase 1.5: Settings & Auth

**Status**: ⏳ Pending

**Deliverables**:
- [ ] Settings view with Bridge connection configuration
- [ ] Account management (add/remove Proton accounts)
- [ ] Credential storage with DPAPI encryption
- [ ] Sync interval configuration
- [ ] Notification preferences
- [ ] Theme selection (light/dark)
- [ ] Startup options (launch on boot, start minimized)

**Acceptance Criteria**:
- Can configure Bridge connection (host/port)
- Can add/remove accounts
- Credentials stored securely
- Settings persist across sessions
- UI reflects theme changes

**Dependencies**: Phase 1.0

**Estimated Effort**: 2-3 days

---

### Phase 1.6: Polish

**Status**: ⏳ Pending

**Deliverables**:
- [ ] System tray icon with context menu
- [ ] Minimize to tray functionality
- [ ] Toast notifications for new emails
- [ ] Toast notifications for calendar reminders
- [ ] Badge notifications (unread count)
- [ ] Keyboard shortcut overlay
- [ ] About dialog with version info
- [ ] Error handling and user-friendly messages
- [ ] Loading states and progress indicators

**Acceptance Criteria**:
- App minimizes to system tray
- Notifications appear for new mail and reminders
- Keyboard shortcuts work throughout app
- Error messages are clear and actionable
- UI is polished and professional

**Dependencies**: Phase 1.2, 1.4, 1.5

**Estimated Effort**: 2-3 days

---

## Phase 1 Summary

**Total Estimated Effort**: 20-27 days

**MVP Features**:
- ✅ Email: Read, compose, send, reply, forward, search, folders
- ✅ Calendar: View (day/week/month), create/edit/delete events, reminders
- ✅ Offline-first with local SQLite cache
- ✅ Outlook-style three-pane UI
- ✅ Settings and account management
- ✅ System tray and notifications

**Out of Scope for Phase 1**:
- Multiple account support (single account only)
- Advanced filters and rules
- Contact management (beyond email addresses)
- Task/to-do list
- Notes
- Add-ins/extensibility
- Advanced search (operators, saved searches)

---

## Phase 2: Advanced Features (Future)

**Status**: 📋 Planned

**Potential Features**:
- Multiple Proton account support
- Advanced email filters and rules
- Contact management with groups
- Task/to-do list integration
- Notes feature
- Advanced search with operators
- Saved searches
- Email templates
- Quick steps / automation
- Add-in system for extensibility
- Integration with other Proton services (Drive, Pass)

---

## Phase 3: Enterprise Features (Future)

**Status**: 📋 Planned

**Potential Features**:
- Shared mailboxes
- Delegation
- Exchange ActiveSync support
- Compliance features (eDiscovery, retention policies)
- Advanced security (S/MIME, hardware keys)
- Admin console integration
- Audit logging

---

## Development Principles

1. **Incremental Delivery**: Each phase delivers working software
2. **Test-Driven**: Write tests alongside features
3. **Clean Code**: Follow SOLID principles, maintainable code
4. **User-Focused**: Prioritize user experience and usability
5. **Performance**: Optimize for responsiveness and efficiency
6. **Security**: Secure credential storage, no data leaks

---

## Risk Mitigation

### Technical Risks

| Risk | Mitigation |
|------|-----------|
| ProtonMail Bridge compatibility | Test early, document Bridge version requirements |
| CalDAV complexity | Start with simple sync, iterate |
| HTML email rendering | Use WebView2, test with various email clients |
| Offline sync conflicts | Server-wins for read-only data, clear conflict resolution |

### Schedule Risks

| Risk | Mitigation |
|------|-----------|
| Underestimated complexity | Buffer time in estimates, prioritize MVP features |
| Scope creep | Strict phase boundaries, defer features to Phase 2 |
| Technical blockers | Spike on risky areas early (CalDAV, HTML rendering) |

---

## Success Criteria

Phase 1 is complete when:
- ✅ All Phase 1.0-1.6 deliverables complete
- ✅ App can replace Outlook for basic email/calendar workflows
- ✅ Stable and performant (no crashes, <1s response time)
- ✅ Tested with real ProtonMail Bridge
- ✅ Documentation complete
- ✅ Ready for beta testing

---

## Next Steps

1. Complete Phase 1.0 (Foundation)
2. Push to GitHub for collaboration
3. Begin Phase 1.1 (Email Backend)
4. Iterate through remaining Phase 1 sub-phases
5. Beta testing and feedback
6. Phase 2 planning based on user feedback
