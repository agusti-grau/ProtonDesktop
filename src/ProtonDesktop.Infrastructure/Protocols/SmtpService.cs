using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
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
        var mimeMessage = BuildMimeMessage(account, message, attachments);

        using var client = new SmtpClient();
        await client.ConnectAsync(account.SmtpHost, account.SmtpPort, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(account.Email, account.EncryptedPassword);
        await client.SendAsync(mimeMessage);
        await client.DisconnectAsync(true);

        _logger.Information("Email sent to {To}", message.ToAddresses);
    }

    public async Task<EmailMessage> SaveDraftAsync(MailAccount account, EmailMessage message, IEnumerable<EmailAttachment>? attachments = null)
    {
        var mimeMessage = BuildMimeMessage(account, message, attachments);

        using var client = new ImapClient();
        await client.ConnectAsync(account.ImapHost, account.ImapPort, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(account.Email, account.EncryptedPassword);

        var draftsFolder = client.GetFolder(MailKit.SpecialFolder.Drafts);
        await draftsFolder.OpenAsync(MailKit.FolderAccess.ReadWrite);
        var appendRequest = new MailKit.AppendRequest(mimeMessage, MailKit.MessageFlags.Draft);
        await draftsFolder.AppendAsync(appendRequest);
        await draftsFolder.CloseAsync(true);

        await client.DisconnectAsync(true);

        message.Flags = Core.Enums.EmailFlag.Draft;
        return message;
    }

    private static MimeMessage BuildMimeMessage(MailAccount account, EmailMessage message, IEnumerable<EmailAttachment>? attachments)
    {
        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress(message.FromName, message.FromAddress));

        foreach (var to in message.ToAddresses.Split(',', StringSplitOptions.RemoveEmptyEntries))
            mimeMessage.To.Add(MailboxAddress.Parse(to.Trim()));

        if (!string.IsNullOrEmpty(message.CcAddresses))
            foreach (var cc in message.CcAddresses.Split(',', StringSplitOptions.RemoveEmptyEntries))
                mimeMessage.Cc.Add(MailboxAddress.Parse(cc.Trim()));

        mimeMessage.Subject = message.Subject;

        var builder = new BodyBuilder();
        if (!string.IsNullOrEmpty(message.HtmlBody))
            builder.HtmlBody = message.HtmlBody;
        else if (!string.IsNullOrEmpty(message.PlainTextBody))
            builder.TextBody = message.PlainTextBody;

        if (attachments != null)
        {
            foreach (var attachment in attachments)
            {
                if (File.Exists(attachment.LocalPath))
                    builder.Attachments.Add(attachment.LocalPath);
            }
        }

        mimeMessage.Body = builder.ToMessageBody();
        return mimeMessage;
    }
}
