using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using MimeKit;
using ProtonDesktop.Core.Enums;
using ProtonDesktop.Core.Interfaces;
using ProtonDesktop.Core.Models;
using Serilog;

namespace ProtonDesktop.Infrastructure.Protocols;

public class ImapSyncService : IImapSyncService
{
    private ImapClient? _client;
    private readonly ILogger _logger;
    private readonly ICredentialStore _credentialStore;
    private readonly string _attachmentStoragePath;

    public bool IsConnected => _client?.IsConnected ?? false;

    public ImapSyncService(ILogger logger, ICredentialStore credentialStore)
    {
        _logger = logger;
        _credentialStore = credentialStore;
        _attachmentStoragePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ProtonDesktop",
            "Attachments");
        Directory.CreateDirectory(_attachmentStoragePath);
    }

    public async Task<bool> ConnectAsync(MailAccount account)
    {
        try
        {
            _client = new ImapClient();

            // ProtonMail Bridge uses a self-signed cert on localhost; accept it for local connections
            _client.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
            {
                var isLocalhost = account.ImapHost is "127.0.0.1" or "localhost" or "::1";
                if (isLocalhost)
                {
                    _logger.Warning("Accepting self-signed certificate for local Bridge connection");
                    return true;
                }
                return sslPolicyErrors == System.Net.Security.SslPolicyErrors.None;
            };

            await _client.ConnectAsync(account.ImapHost, account.ImapPort, SecureSocketOptions.StartTls);

            var password = _credentialStore.Decrypt(account.EncryptedPassword);
            await _client.AuthenticateAsync(account.Email, password);

            _logger.Information("Connected to IMAP {Host}:{Port}", account.ImapHost, account.ImapPort);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to connect to IMAP {Host}:{Port} - {Message}", account.ImapHost, account.ImapPort, ex.Message);
            LastError = ex.Message;
            return false;
        }
    }

    public string? LastError { get; private set; }

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
            await imapFolder.OpenAsync(FolderAccess.ReadOnly);

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
        await imapFolder.OpenAsync(FolderAccess.ReadOnly);

        var uids = await imapFolder.SearchAsync(SearchQuery.All);
        if (uids.Count == 0)
        {
            await imapFolder.CloseAsync(false);
            return messages;
        }

        var fetchRequest = new FetchRequest(MessageSummaryItems.Envelope | MessageSummaryItems.Flags | MessageSummaryItems.UniqueId);
        var summaries = await imapFolder.FetchAsync(uids, fetchRequest);

        foreach (var summary in summaries)
        {
            var message = new EmailMessage
            {
                MessageId = summary.Envelope?.MessageId ?? string.Empty,
                Subject = summary.Envelope?.Subject ?? string.Empty,
                FromAddress = summary.Envelope?.From?.Mailboxes.FirstOrDefault()?.Address ?? string.Empty,
                FromName = summary.Envelope?.From?.Mailboxes.FirstOrDefault()?.Name ?? string.Empty,
                ToAddresses = string.Join(",", summary.Envelope?.To?.Mailboxes.Select(m => m.Address) ?? []),
                CcAddresses = summary.Envelope?.Cc != null
                    ? string.Join(",", summary.Envelope.Cc.Mailboxes.Select(m => m.Address))
                    : null,
                ReceivedAt = summary.Envelope?.Date?.UtcDateTime ?? DateTime.UtcNow,
                Uid = summary.UniqueId.Id.ToString(),
                Flags = MapFlags(summary.Flags.Value),
                Size = summary.Size
            };
            messages.Add(message);
        }

        await imapFolder.CloseAsync(false);
        return messages;
    }

    public async Task<IEnumerable<EmailMessage>> SyncNewMessagesAsync(EmailFolder folder, string? lastUid)
    {
        if (_client == null || !_client.IsConnected)
            throw new InvalidOperationException("Not connected to IMAP");

        var messages = new List<EmailMessage>();
        var imapFolder = await _client.GetFolderAsync(folder.Path);
        await imapFolder.OpenAsync(FolderAccess.ReadOnly);

        UniqueId? startUid = null;
        if (!string.IsNullOrEmpty(lastUid) && UniqueId.TryParse(lastUid, out var parsedUid))
        {
            startUid = parsedUid;
        }

        var allUids = await imapFolder.SearchAsync(SearchQuery.All);

        var uids = startUid.HasValue
            ? allUids.Where(u => u.Id >= startUid.Value.Id).ToList()
            : allUids.ToList();

        if (uids.Count == 0)
        {
            await imapFolder.CloseAsync(false);
            return messages;
        }

        var fetchRequest = new FetchRequest(
            MessageSummaryItems.Envelope |
            MessageSummaryItems.Flags |
            MessageSummaryItems.UniqueId |
            MessageSummaryItems.BodyStructure);

        var summaries = await imapFolder.FetchAsync(uids, fetchRequest);

        foreach (var summary in summaries)
        {
            var mimeMessage = await imapFolder.GetMessageAsync(summary.UniqueId);

            var message = new EmailMessage
            {
                MessageId = mimeMessage.MessageId ?? string.Empty,
                InReplyTo = mimeMessage.InReplyTo,
                Subject = mimeMessage.Subject ?? string.Empty,
                FromAddress = mimeMessage.From.Mailboxes.FirstOrDefault()?.Address ?? string.Empty,
                FromName = mimeMessage.From.Mailboxes.FirstOrDefault()?.Name ?? string.Empty,
                ToAddresses = string.Join(",", mimeMessage.To.Mailboxes.Select(m => m.Address)),
                CcAddresses = mimeMessage.Cc.Mailboxes.Any()
                    ? string.Join(",", mimeMessage.Cc.Mailboxes.Select(m => m.Address))
                    : null,
                BccAddresses = mimeMessage.Bcc.Mailboxes.Any()
                    ? string.Join(",", mimeMessage.Bcc.Mailboxes.Select(m => m.Address))
                    : null,
                ReceivedAt = mimeMessage.Date.UtcDateTime,
                SentAt = mimeMessage.Date.UtcDateTime,
                PlainTextBody = mimeMessage.TextBody,
                HtmlBody = mimeMessage.HtmlBody,
                Uid = summary.UniqueId.Id.ToString(),
                Flags = MapFlags(summary.Flags.Value),
                Size = summary.Size,
                HasAttachments = mimeMessage.Attachments.Any()
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
        await imapFolder.OpenAsync(FolderAccess.ReadOnly);

        if (!UniqueId.TryParse(uid, out var uniqueId))
            return null;

        var mimeMessage = await imapFolder.GetMessageAsync(uniqueId);
        await imapFolder.CloseAsync(false);

        return new EmailMessage
        {
            MessageId = mimeMessage.MessageId ?? string.Empty,
            InReplyTo = mimeMessage.InReplyTo,
            Subject = mimeMessage.Subject ?? string.Empty,
            FromAddress = mimeMessage.From.Mailboxes.FirstOrDefault()?.Address ?? string.Empty,
            FromName = mimeMessage.From.Mailboxes.FirstOrDefault()?.Name ?? string.Empty,
            ToAddresses = string.Join(",", mimeMessage.To.Mailboxes.Select(m => m.Address)),
            CcAddresses = mimeMessage.Cc.Mailboxes.Any()
                ? string.Join(",", mimeMessage.Cc.Mailboxes.Select(m => m.Address))
                : null,
            BccAddresses = mimeMessage.Bcc.Mailboxes.Any()
                ? string.Join(",", mimeMessage.Bcc.Mailboxes.Select(m => m.Address))
                : null,
            ReceivedAt = mimeMessage.Date.UtcDateTime,
            SentAt = mimeMessage.Date.UtcDateTime,
            PlainTextBody = mimeMessage.TextBody,
            HtmlBody = mimeMessage.HtmlBody,
            Uid = uid,
            HasAttachments = mimeMessage.Attachments.Any()
        };
    }

    public async Task<byte[]?> FetchAttachmentAsync(EmailAttachment attachment, MailAccount account)
    {
        if (_client == null || !_client.IsConnected)
        {
            if (!await ConnectAsync(account))
                return null;
        }

        if (_client == null)
            return null;

        try
        {
            var messageParts = attachment.ContentId?.Split(':');
            if (messageParts == null || messageParts.Length < 2)
                return null;

            var folderPath = messageParts[0];
            var messageUid = messageParts[1];

            var imapFolder = await _client.GetFolderAsync(folderPath);
            await imapFolder.OpenAsync(FolderAccess.ReadOnly);

            if (!UniqueId.TryParse(messageUid, out var uniqueId))
                return null;

            var mimeMessage = await imapFolder.GetMessageAsync(uniqueId);

            foreach (var mimeAttachment in mimeMessage.Attachments)
            {
                if (mimeAttachment is MimePart mimePart && mimePart.FileName == attachment.FileName)
                {
                    using var memoryStream = new MemoryStream();
                    await mimePart.Content.DecodeToAsync(memoryStream);
                    return memoryStream.ToArray();
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to fetch attachment {FileName}", attachment.FileName);
            return null;
        }
    }

    public async Task<IEnumerable<EmailAttachment>> DownloadAttachmentsAsync(EmailMessage message, EmailFolder folder)
    {
        if (_client == null || !_client.IsConnected)
            throw new InvalidOperationException("Not connected to IMAP");

        var attachments = new List<EmailAttachment>();
        var imapFolder = await _client.GetFolderAsync(folder.Path);
        await imapFolder.OpenAsync(FolderAccess.ReadOnly);

        if (!UniqueId.TryParse(message.Uid!, out var uniqueId))
            return attachments;

        var mimeMessage = await imapFolder.GetMessageAsync(uniqueId);

        foreach (var mimeAttachment in mimeMessage.Attachments)
        {
            if (mimeAttachment is MimePart mimePart)
            {
                var fileName = mimePart.FileName ?? $"attachment_{attachments.Count}";
                var localPath = Path.Combine(_attachmentStoragePath, $"{message.Id}_{fileName}");

                using (var fileStream = File.Create(localPath))
                {
                    await mimePart.Content.DecodeToAsync(fileStream);
                }

                var attachment = new EmailAttachment
                {
                    FileName = fileName,
                    ContentType = mimePart.ContentType.MimeType,
                    Size = new FileInfo(localPath).Length,
                    ContentId = $"{folder.Path}:{message.Uid}:{fileName}",
                    IsInline = mimePart.ContentDisposition?.Disposition == "inline",
                    LocalPath = localPath,
                    EmailMessageId = message.Id
                };

                attachments.Add(attachment);
            }
        }

        await imapFolder.CloseAsync(false);
        return attachments;
    }

    public async Task UpdateFlagsAsync(EmailFolder folder, string uid, EmailFlag flags)
    {
        if (_client == null || !_client.IsConnected)
            throw new InvalidOperationException("Not connected to IMAP");

        var imapFolder = await _client.GetFolderAsync(folder.Path);
        await imapFolder.OpenAsync(FolderAccess.ReadWrite);

        if (!UniqueId.TryParse(uid, out var uniqueId))
            return;

        var messageFlags = MapToMessageFlags(flags);
        await imapFolder.AddFlagsAsync(uniqueId, messageFlags, true);
        await imapFolder.CloseAsync(true);
    }

    private static Core.Enums.FolderType MapFolderType(IMailFolder folder)
    {
        if (folder.Attributes.HasFlag(FolderAttributes.Inbox)) return Core.Enums.FolderType.Inbox;
        if (folder.Attributes.HasFlag(FolderAttributes.Sent)) return Core.Enums.FolderType.Sent;
        if (folder.Attributes.HasFlag(FolderAttributes.Drafts)) return Core.Enums.FolderType.Drafts;
        if (folder.Attributes.HasFlag(FolderAttributes.Trash)) return Core.Enums.FolderType.Trash;
        if (folder.Attributes.HasFlag(FolderAttributes.Junk)) return Core.Enums.FolderType.Junk;
        if (folder.Attributes.HasFlag(FolderAttributes.Archive)) return Core.Enums.FolderType.Archive;
        return Core.Enums.FolderType.Custom;
    }

    private static EmailFlag MapFlags(MessageFlags flags)
    {
        var result = EmailFlag.None;
        if (flags.HasFlag(MessageFlags.Seen)) result |= EmailFlag.Seen;
        if (flags.HasFlag(MessageFlags.Flagged)) result |= EmailFlag.Flagged;
        if (flags.HasFlag(MessageFlags.Answered)) result |= EmailFlag.Answered;
        if (flags.HasFlag(MessageFlags.Draft)) result |= EmailFlag.Draft;
        if (flags.HasFlag(MessageFlags.Deleted)) result |= EmailFlag.Deleted;
        return result;
    }

    private static MessageFlags MapToMessageFlags(EmailFlag flags)
    {
        var result = MessageFlags.None;
        if (flags.HasFlag(EmailFlag.Seen)) result |= MessageFlags.Seen;
        if (flags.HasFlag(EmailFlag.Flagged)) result |= MessageFlags.Flagged;
        if (flags.HasFlag(EmailFlag.Answered)) result |= MessageFlags.Answered;
        if (flags.HasFlag(EmailFlag.Draft)) result |= MessageFlags.Draft;
        if (flags.HasFlag(EmailFlag.Deleted)) result |= MessageFlags.Deleted;
        return result;
    }
}
