using ProtonDesktop.Core.Interfaces;
using ProtonDesktop.Core.Models;
using Serilog;

namespace ProtonDesktop.Services.Email;

public class EmailSendService
{
    private readonly ISmtpService _smtpService;
    private readonly IEmailRepository _emailRepository;
    private readonly ILogger _logger;

    public EmailSendService(
        ISmtpService smtpService,
        IEmailRepository emailRepository,
        ILogger logger)
    {
        _smtpService = smtpService;
        _emailRepository = emailRepository;
        _logger = logger;
    }

    public async Task SendAsync(MailAccount account, EmailMessage message, IEnumerable<EmailAttachment>? attachments = null)
    {
        try
        {
            _logger.Information("Sending email to {To}", message.ToAddresses);
            await _smtpService.SendAsync(account, message, attachments);
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
            var savedMessage = await _smtpService.SaveDraftAsync(account, message, attachments);
            _logger.Information("Draft saved");
            return savedMessage;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error saving draft");
            throw;
        }
    }
}
