using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProtonDesktop.Core.Enums;
using ProtonDesktop.Core.Interfaces;
using ProtonDesktop.Core.Models;
using ProtonDesktop.Services.Email;
using Serilog;

namespace ProtonDesktop.ViewModels;

public partial class ComposeViewModel : ObservableObject
{
    private readonly IAccountRepository _accountRepository;
    private readonly EmailSendService _emailSendService;
    private readonly ILogger _logger;

    [ObservableProperty]
    private string _toAddresses = string.Empty;

    [ObservableProperty]
    private string _ccAddresses = string.Empty;

    [ObservableProperty]
    private string _bccAddresses = string.Empty;

    [ObservableProperty]
    private string _subject = string.Empty;

    [ObservableProperty]
    private string _body = string.Empty;

    [ObservableProperty]
    private bool _showCc;

    [ObservableProperty]
    private bool _showBcc;

    [ObservableProperty]
    private bool _isHtml;

    [ObservableProperty]
    private ObservableCollection<ComposeAttachmentViewModel> _attachments = new();

    [ObservableProperty]
    private bool _isSending;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ComposeMode Mode { get; set; } = ComposeMode.New;
    public EmailMessage? OriginalMessage { get; set; }
    public MailAccount? Account { get; set; }

    public ComposeViewModel(
        IAccountRepository accountRepository,
        EmailSendService emailSendService)
    {
        _accountRepository = accountRepository;
        _emailSendService = emailSendService;
        _logger = Log.ForContext<ComposeViewModel>();
    }

    public async Task InitializeAsync()
    {
        Account = await _accountRepository.GetDefaultAccountAsync();
        if (Account == null)
        {
            StatusMessage = "No account configured";
            return;
        }

        if (Mode == ComposeMode.Reply && OriginalMessage != null)
        {
            ToAddresses = OriginalMessage.FromAddress;
            Subject = OriginalMessage.Subject.StartsWith("Re:", StringComparison.OrdinalIgnoreCase)
                ? OriginalMessage.Subject
                : $"Re: {OriginalMessage.Subject}";

            var replyBody = $"\n\n--- Original Message ---\n" +
                           $"From: {OriginalMessage.FromName} <{OriginalMessage.FromAddress}>\n" +
                           $"To: {OriginalMessage.ToAddresses}\n" +
                           $"Date: {OriginalMessage.ReceivedAt}\n" +
                           $"Subject: {OriginalMessage.Subject}\n\n" +
                           $"{OriginalMessage.PlainTextBody ?? OriginalMessage.HtmlBody ?? string.Empty}";

            Body = replyBody;
        }
        else if (Mode == ComposeMode.Forward && OriginalMessage != null)
        {
            Subject = OriginalMessage.Subject.StartsWith("Fw:", StringComparison.OrdinalIgnoreCase)
                ? OriginalMessage.Subject
                : $"Fw: {OriginalMessage.Subject}";

            var forwardBody = $"\n\n--- Forwarded Message ---\n" +
                             $"From: {OriginalMessage.FromName} <{OriginalMessage.FromAddress}>\n" +
                             $"To: {OriginalMessage.ToAddresses}\n" +
                             $"Date: {OriginalMessage.ReceivedAt}\n" +
                             $"Subject: {OriginalMessage.Subject}\n\n" +
                             $"{OriginalMessage.PlainTextBody ?? OriginalMessage.HtmlBody ?? string.Empty}";

            Body = forwardBody;
        }
    }

    [RelayCommand]
    private void ToggleCc()
    {
        ShowCc = !ShowCc;
    }

    [RelayCommand]
    private void ToggleBcc()
    {
        ShowBcc = !ShowBcc;
    }

    [RelayCommand]
    private async Task SendAsync()
    {
        if (Account == null)
        {
            StatusMessage = "No account configured";
            return;
        }

        if (string.IsNullOrWhiteSpace(ToAddresses))
        {
            StatusMessage = "Please enter recipient addresses";
            return;
        }

        try
        {
            IsSending = true;
            StatusMessage = "Sending...";

            var message = new EmailMessage
            {
                FromAddress = Account.Email,
                FromName = Account.DisplayName,
                ToAddresses = ToAddresses,
                CcAddresses = string.IsNullOrWhiteSpace(CcAddresses) ? null : CcAddresses,
                BccAddresses = string.IsNullOrWhiteSpace(BccAddresses) ? null : BccAddresses,
                Subject = Subject,
                PlainTextBody = IsHtml ? null : Body,
                HtmlBody = IsHtml ? Body : null,
                ReceivedAt = DateTime.UtcNow
            };

            var attachmentList = Attachments.Select(a => new EmailAttachment
            {
                FileName = a.FileName,
                ContentType = a.ContentType,
                Size = a.Size,
                LocalPath = a.LocalPath
            }).ToList();

            if (Mode == ComposeMode.Reply && OriginalMessage != null)
            {
                await _emailSendService.ReplyAsync(Account, OriginalMessage, message, attachmentList);
            }
            else if (Mode == ComposeMode.Forward && OriginalMessage != null)
            {
                await _emailSendService.ForwardAsync(Account, OriginalMessage, message, attachmentList);
            }
            else
            {
                await _emailSendService.SendAsync(Account, message, attachmentList);
            }

            StatusMessage = "Email sent successfully";
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error sending email");
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsSending = false;
        }
    }

    [RelayCommand]
    private async Task SaveDraftAsync()
    {
        if (Account == null)
        {
            StatusMessage = "No account configured";
            return;
        }

        try
        {
            StatusMessage = "Saving draft...";

            var message = new EmailMessage
            {
                FromAddress = Account.Email,
                FromName = Account.DisplayName,
                ToAddresses = ToAddresses,
                CcAddresses = string.IsNullOrWhiteSpace(CcAddresses) ? null : CcAddresses,
                BccAddresses = string.IsNullOrWhiteSpace(BccAddresses) ? null : BccAddresses,
                Subject = Subject,
                PlainTextBody = IsHtml ? null : Body,
                HtmlBody = IsHtml ? Body : null,
                ReceivedAt = DateTime.UtcNow
            };

            var attachmentList = Attachments.Select(a => new EmailAttachment
            {
                FileName = a.FileName,
                ContentType = a.ContentType,
                Size = a.Size,
                LocalPath = a.LocalPath
            }).ToList();

            await _emailSendService.SaveDraftAsync(Account, message, attachmentList);

            StatusMessage = "Draft saved";
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error saving draft");
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private void AddAttachment()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Multiselect = true,
            Title = "Select files to attach"
        };

        if (dialog.ShowDialog() == true)
        {
            foreach (var fileName in dialog.FileNames)
            {
                var fileInfo = new FileInfo(fileName);
                Attachments.Add(new ComposeAttachmentViewModel
                {
                    FileName = fileInfo.Name,
                    ContentType = GetContentType(fileName),
                    Size = fileInfo.Length,
                    LocalPath = fileInfo.FullName
                });
            }
        }
    }

    [RelayCommand]
    private void RemoveAttachment(ComposeAttachmentViewModel attachment)
    {
        Attachments.Remove(attachment);
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".txt" => "text/plain",
            ".html" => "text/html",
            ".htm" => "text/html",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/msword",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.ms-excel",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.ms-powerpoint",
            ".zip" => "application/zip",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };
    }

    public event EventHandler? RequestClose;
}

public partial class ComposeAttachmentViewModel : ObservableObject
{
    [ObservableProperty]
    private string _fileName = string.Empty;

    [ObservableProperty]
    private string _contentType = string.Empty;

    [ObservableProperty]
    private long _size;

    [ObservableProperty]
    private string _localPath = string.Empty;

    public string SizeDisplay
    {
        get
        {
            if (Size < 1024) return $"{Size} B";
            if (Size < 1024 * 1024) return $"{Size / 1024} KB";
            return $"{Size / (1024 * 1024)} MB";
        }
    }
}

public enum ComposeMode
{
    New,
    Reply,
    Forward
}
