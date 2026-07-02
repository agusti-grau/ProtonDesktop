using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using ProtonDesktop.Core.Interfaces;
using ProtonDesktop.Core.Models;
using Serilog;

namespace ProtonDesktop.Infrastructure.Protocols;

public class ImapSyncService : IImapSyncService
{
    private ImapClient? _client;
    private readonly ILogger _logger;

    public bool IsConnected => _client?.IsConnected ?? false;

    public ImapSyncService(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<bool> ConnectAsync(MailAccount account)
    {
        try
        {
            _client = new ImapClient();
            await _client.ConnectAsync(account.ImapHost, account.ImapPort, SecureSocketOptions.StartTls);
            await _client.AuthenticateAsync(account.Email, account.EncryptedPassword);
            _logger.Information("Connected to IMAP {Host}:{Port}", account.ImapHost, account.ImapPort);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to connect to IMAP");
            return false;
        }
    }

    public Task DisconnectAsync()
    {
        _client?.Disconnect(true);
        _client = null;
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<EmailFolder>> SyncFoldersAsync(MailAccount account)
    {
        if (_client == null || !_client.IsConnected)
            throw new InvalidOperationException("Not connected to IMAP");

        var folders = new List<EmailFolder>();
        var personalNamespace = _client.PersonalNamespaces[0];
        var folderList = await _client.GetFoldersAsync(personalNamespace);

        foreach (var imapFolder in folderList)
        {
            await imapFolder.OpenAsync(MailKit.FolderAccess.ReadOnly);

            var folder = new EmailFolder
            {
                Name = imapFolder.Name,
                Path = imapFolder.FullName,
                FolderType = MapFolderType(imapFolder),
                UidNext = imapFolder.UidNext.ToString(),
                UidValidity = imapFolder.UidValidity.ToString(),
                TotalCount = imapFolder.Count,
                UnreadCount = imapFolder.Unread
            };

            folders.Add(folder);
            await imapFolder.CloseAsync(false);
        }

        return folders;
    }

    public async Task<IEnumerable<EmailMessage>> SyncMessagesAsync(EmailFolder folder)
    {
        if (_client == null || !_client.IsConnected)
            throw new InvalidOperationException("Not connected to IMAP");

        var messages = new List<EmailMessage>();
        var imapFolder = await _client.GetFolderAsync(folder.Path);
        await imapFolder.OpenAsync(MailKit.FolderAccess.ReadOnly);

        var uids = await imapFolder.SearchAsync(SearchQuery.All);
        var fetchRequest = new MailKit.FetchRequest { Items = MailKit.MessageSummaryItems.Envelope | MailKit.MessageSummaryItems.Flags };
        var messageSummary = await imapFolder.FetchAsync(uids, fetchRequest);
        foreach (var summary in messageSummary)
        {
            var message = new EmailMessage
            {
                MessageId = summary.Envelope?.MessageId ?? string.Empty,
                Subject = summary.Envelope?.Subject ?? string.Empty,
                FromAddress = summary.Envelope?.From?.Mailboxes.FirstOrDefault()?.Address ?? string.Empty,
                FromName = summary.Envelope?.From?.Mailboxes.FirstOrDefault()?.Name ?? string.Empty,
                ToAddresses = string.Join(",", summary.Envelope?.To?.Mailboxes.Select(m => m.Address) ?? Enumerable.Empty<string>()),
                ReceivedAt = summary.Envelope?.Date?.UtcDateTime ?? DateTime.UtcNow,
                Uid = summary.UniqueId.ToString(),
                Flags = MapFlags(summary.Flags ?? MailKit.MessageFlags.None)
            };
            messages.Add(message);
        }

        await imapFolder.CloseAsync(false);
        return messages;
    }

    public async Task<EmailMessage?> FetchMessageAsync(EmailFolder folder, string uid)
    {
        if (_client == null || !_client.IsConnected)
            throw new InvalidOperationException("Not connected to IMAP");

        var imapFolder = await _client.GetFolderAsync(folder.Path);
        await imapFolder.OpenAsync(MailKit.FolderAccess.ReadOnly);

        if (!MailKit.UniqueId.TryParse(uid, out var uniqueId))
            return null;

        var mimeMessage = await imapFolder.GetMessageAsync(uniqueId);
        await imapFolder.CloseAsync(false);

        return new EmailMessage
        {
            MessageId = mimeMessage.MessageId ?? string.Empty,
            Subject = mimeMessage.Subject ?? string.Empty,
            FromAddress = mimeMessage.From.Mailboxes.FirstOrDefault()?.Address ?? string.Empty,
            FromName = mimeMessage.From.Mailboxes.FirstOrDefault()?.Name ?? string.Empty,
            ToAddresses = string.Join(",", mimeMessage.To.Mailboxes.Select(m => m.Address)),
            CcAddresses = string.Join(",", mimeMessage.Cc.Mailboxes.Select(m => m.Address)),
            ReceivedAt = mimeMessage.Date.UtcDateTime,
            PlainTextBody = mimeMessage.TextBody,
            HtmlBody = mimeMessage.HtmlBody,
            Uid = uid
        };
    }

    public Task<byte[]?> FetchAttachmentAsync(EmailAttachment attachment, MailAccount account)
    {
        return Task.FromResult<byte[]?>(null);
    }

    private static Core.Enums.FolderType MapFolderType(MailKit.IMailFolder folder)
    {
        if (folder.Attributes.HasFlag(MailKit.FolderAttributes.Inbox)) return Core.Enums.FolderType.Inbox;
        if (folder.Attributes.HasFlag(MailKit.FolderAttributes.Sent)) return Core.Enums.FolderType.Sent;
        if (folder.Attributes.HasFlag(MailKit.FolderAttributes.Drafts)) return Core.Enums.FolderType.Drafts;
        if (folder.Attributes.HasFlag(MailKit.FolderAttributes.Trash)) return Core.Enums.FolderType.Trash;
        if (folder.Attributes.HasFlag(MailKit.FolderAttributes.Junk)) return Core.Enums.FolderType.Junk;
        if (folder.Attributes.HasFlag(MailKit.FolderAttributes.Archive)) return Core.Enums.FolderType.Archive;
        return Core.Enums.FolderType.Custom;
    }

    private static Core.Enums.EmailFlag MapFlags(MailKit.MessageFlags flags)
    {
        var result = Core.Enums.EmailFlag.None;
        if (flags.HasFlag(MailKit.MessageFlags.Seen)) result |= Core.Enums.EmailFlag.Seen;
        if (flags.HasFlag(MailKit.MessageFlags.Flagged)) result |= Core.Enums.EmailFlag.Flagged;
        if (flags.HasFlag(MailKit.MessageFlags.Answered)) result |= Core.Enums.EmailFlag.Answered;
        if (flags.HasFlag(MailKit.MessageFlags.Draft)) result |= Core.Enums.EmailFlag.Draft;
        if (flags.HasFlag(MailKit.MessageFlags.Deleted)) result |= Core.Enums.EmailFlag.Deleted;
        return result;
    }
}
