using ProtonDesktop.Core.Interfaces;
using ProtonDesktop.Core.Models;
using Serilog;

namespace ProtonDesktop.Services.Email;

public class EmailSendService
{
    private readonly ISmtpService _smtpService;
    private readonly IEmailRepository _emailRepository;
    private readonly ICredentialStore _credentialStore;
    private readonly ILogger _logger;

    public EmailSendService(
        ISmtpService smtpService,
        IEmailRepository emailRepository,
        ICredentialStore credentialStore,
        ILogger logger)
    {
        _smtpService = smtpService;
        _emailRepository = emailRepository;
        _credentialStore = credentialStore;
        _logger = logger;
    }

    public async Task SendAsync(MailAccount account, EmailMessage message, IEnumerable<EmailAttachment>? attachments = null)
    {
        try
        {
            _logger.Information("Sending email to {To}", message.ToAddresses);

            var decryptedPassword = _credentialStore.Decrypt(account.EncryptedPassword);
            var accountWithPassword = new MailAccount
            {
                Id = account.Id,
                Email = account.Email,
                DisplayName = account.DisplayName,
                ImapHost = account.ImapHost,
                ImapPort = account.ImapPort,
                SmtpHost = account.SmtpHost,
                SmtpPort = account.SmtpPort,
                CalDavHost = account.CalDavHost,
                CalDavPort = account.CalDavPort,
                EncryptedPassword = decryptedPassword
            };

            await _smtpService.SendAsync(accountWithPassword, message, attachments);

            message.Flags |= Core.Enums.EmailFlag.Seen;
            message.SentAt = DateTime.UtcNow;

            _logger.Information("Email sent successfully");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error sending email");
            throw;
        }
    }

    public async Task<EmailMessage> SaveDraftAsync(MailAccount account, EmailMessage message, IEnumerable<EmailAttachment>? attachments = null)
    {
        try
        {
            _logger.Information("Saving draft");

            var decryptedPassword = _credentialStore.Decrypt(account.EncryptedPassword);
            var accountWithPassword = new MailAccount
            {
                Id = account.Id,
                Email = account.Email,
                DisplayName = account.DisplayName,
                ImapHost = account.ImapHost,
                ImapPort = account.ImapPort,
                SmtpHost = account.SmtpHost,
                SmtpPort = account.SmtpPort,
                CalDavHost = account.CalDavHost,
                CalDavPort = account.CalDavPort,
                EncryptedPassword = decryptedPassword
            };

            var savedMessage = await _smtpService.SaveDraftAsync(accountWithPassword, message, attachments);

            _logger.Information("Draft saved with UID {Uid}", savedMessage.Uid);
            return savedMessage;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error saving draft");
            throw;
        }
    }

    public async Task ReplyAsync(MailAccount account, EmailMessage originalMessage, EmailMessage replyMessage, IEnumerable<EmailAttachment>? attachments = null)
    {
        try
        {
            _logger.Information("Replying to email {Subject}", originalMessage.Subject);

            var decryptedPassword = _credentialStore.Decrypt(account.EncryptedPassword);
            var accountWithPassword = new MailAccount
            {
                Id = account.Id,
                Email = account.Email,
                DisplayName = account.DisplayName,
                ImapHost = account.ImapHost,
                ImapPort = account.ImapPort,
                SmtpHost = account.SmtpHost,
                SmtpPort = account.SmtpPort,
                CalDavHost = account.CalDavHost,
                CalDavPort = account.CalDavPort,
                EncryptedPassword = decryptedPassword
            };

            await _smtpService.SendReplyAsync(accountWithPassword, originalMessage, replyMessage, attachments);

            originalMessage.Flags |= Core.Enums.EmailFlag.Answered;
            await _emailRepository.UpdateMessageAsync(originalMessage);

            _logger.Information("Reply sent successfully");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error sending reply");
            throw;
        }
    }

    public async Task ForwardAsync(MailAccount account, EmailMessage originalMessage, EmailMessage forwardMessage, IEnumerable<EmailAttachment>? attachments = null)
    {
        try
        {
            _logger.Information("Forwarding email {Subject}", originalMessage.Subject);

            var decryptedPassword = _credentialStore.Decrypt(account.EncryptedPassword);
            var accountWithPassword = new MailAccount
            {
                Id = account.Id,
                Email = account.Email,
                DisplayName = account.DisplayName,
                ImapHost = account.ImapHost,
                ImapPort = account.ImapPort,
                SmtpHost = account.SmtpHost,
                SmtpPort = account.SmtpPort,
                CalDavHost = account.CalDavHost,
                CalDavPort = account.CalDavPort,
                EncryptedPassword = decryptedPassword
            };

            await _smtpService.SendForwardAsync(accountWithPassword, originalMessage, forwardMessage, attachments);

            originalMessage.Flags |= Core.Enums.EmailFlag.Forwarded;
            await _emailRepository.UpdateMessageAsync(originalMessage);

            _logger.Information("Forward sent successfully");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error sending forward");
            throw;
        }
    }
}
