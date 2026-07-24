using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using ProtonDesktop.Core.Enums;
using ProtonDesktop.Core.Interfaces;
using ProtonDesktop.Core.Models;
using Serilog;

namespace ProtonDesktop.Infrastructure.Protocols;

public class SmtpService : ISmtpService
{
    private readonly ILogger _logger;

    public SmtpService(ILogger logger)
    {
        _logger = logger;
    }

    public async Task SendAsync(MailAccount account, EmailMessage message, IEnumerable<EmailAttachment>? attachments = null)
    {
        try
        {
            var mimeMessage = BuildMimeMessage(account, message, attachments);

            using var client = new SmtpClient();

            var isLocalhost = account.SmtpHost is "127.0.0.1" or "localhost" or "::1";
            if (isLocalhost)
            {
                client.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            }

            await client.ConnectAsync(account.SmtpHost, account.SmtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(account.Email, account.EncryptedPassword);
            await client.SendAsync(mimeMessage);
            await client.DisconnectAsync(true);

            message.Flags |= EmailFlag.Seen;
            message.SentAt = DateTime.UtcNow;

            _logger.Information("Email sent to {To}", message.ToAddresses);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to send email to {To}", message.ToAddresses);
            throw;
        }
    }

    public async Task<EmailMessage> SaveDraftAsync(MailAccount account, EmailMessage message, IEnumerable<EmailAttachment>? attachments = null)
    {
        try
        {
            var mimeMessage = BuildMimeMessage(account, message, attachments);

            using var client = new ImapClient();

            var isLocalhost = account.ImapHost is "127.0.0.1" or "localhost" or "::1";
            if (isLocalhost)
            {
                client.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            }

            await client.ConnectAsync(account.ImapHost, account.ImapPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(account.Email, account.EncryptedPassword);

            var draftsFolder = client.GetFolder(SpecialFolder.Drafts);
            await draftsFolder.OpenAsync(FolderAccess.ReadWrite);
            var appendRequest = new AppendRequest(mimeMessage, MessageFlags.Draft);
            var appendedUid = await draftsFolder.AppendAsync(appendRequest);
            await draftsFolder.CloseAsync(true);

            await client.DisconnectAsync(true);

            message.Flags = EmailFlag.Draft;
            message.Uid = appendedUid.Value.Id.ToString();

            _logger.Information("Draft saved with UID {Uid}", message.Uid);
            return message;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to save draft");
            throw;
        }
    }

    public async Task SendReplyAsync(MailAccount account, EmailMessage originalMessage, EmailMessage replyMessage, IEnumerable<EmailAttachment>? attachments = null)
    {
        replyMessage.InReplyTo = originalMessage.MessageId;
        replyMessage.Subject = originalMessage.Subject.StartsWith("Re:", StringComparison.OrdinalIgnoreCase)
            ? originalMessage.Subject
            : $"Re: {originalMessage.Subject}";

        await SendAsync(account, replyMessage, attachments);
    }

    public async Task SendForwardAsync(MailAccount account, EmailMessage originalMessage, EmailMessage forwardMessage, IEnumerable<EmailAttachment>? attachments = null)
    {
        forwardMessage.Subject = originalMessage.Subject.StartsWith("Fw:", StringComparison.OrdinalIgnoreCase)
            ? originalMessage.Subject
            : $"Fw: {originalMessage.Subject}";

        var originalAttachments = originalMessage.Attachments?.ToList();
        var allAttachments = (attachments ?? []).Concat(originalAttachments ?? []).ToList();

        await SendAsync(account, forwardMessage, allAttachments);
    }

    private static MimeMessage BuildMimeMessage(MailAccount account, EmailMessage message, IEnumerable<EmailAttachment>? attachments)
    {
        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress(
            string.IsNullOrEmpty(message.FromName) ? account.DisplayName : message.FromName,
            message.FromAddress));

        foreach (var to in message.ToAddresses.Split(',', StringSplitOptions.RemoveEmptyEntries))
            mimeMessage.To.Add(MailboxAddress.Parse(to.Trim()));

        if (!string.IsNullOrEmpty(message.CcAddresses))
            foreach (var cc in message.CcAddresses.Split(',', StringSplitOptions.RemoveEmptyEntries))
                mimeMessage.Cc.Add(MailboxAddress.Parse(cc.Trim()));

        if (!string.IsNullOrEmpty(message.BccAddresses))
            foreach (var bcc in message.BccAddresses.Split(',', StringSplitOptions.RemoveEmptyEntries))
                mimeMessage.Bcc.Add(MailboxAddress.Parse(bcc.Trim()));

        mimeMessage.Subject = message.Subject;
        mimeMessage.MessageId = message.MessageId;
        mimeMessage.Date = new DateTimeOffset(DateTime.UtcNow);

        var builder = new BodyBuilder();

        if (!string.IsNullOrEmpty(message.HtmlBody))
        {
            builder.HtmlBody = message.HtmlBody;
        }
        else if (!string.IsNullOrEmpty(message.PlainTextBody))
        {
            builder.TextBody = message.PlainTextBody;
        }

        if (attachments != null)
        {
            foreach (var attachment in attachments)
            {
                if (!string.IsNullOrEmpty(attachment.LocalPath) && File.Exists(attachment.LocalPath))
                {
                    builder.Attachments.Add(attachment.LocalPath);
                }
            }
        }

        mimeMessage.Body = builder.ToMessageBody();
        return mimeMessage;
    }
}
