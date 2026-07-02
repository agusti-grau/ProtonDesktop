using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProtonDesktop.Core.Enums;
using ProtonDesktop.Core.Interfaces;
using ProtonDesktop.Core.Models;
using Serilog;

namespace ProtonDesktop.ViewModels;

public partial class ReadingPaneViewModel : ObservableObject
{
    private readonly IEmailRepository _emailRepository;
    private readonly ILogger _logger;

    [ObservableProperty]
    private int _messageId;

    [ObservableProperty]
    private string _fromAddress = string.Empty;

    [ObservableProperty]
    private string _fromName = string.Empty;

    [ObservableProperty]
    private string _toAddresses = string.Empty;

    [ObservableProperty]
    private string _ccAddresses = string.Empty;

    [ObservableProperty]
    private string _subject = string.Empty;

    [ObservableProperty]
    private string _body = string.Empty;

    [ObservableProperty]
    private string _htmlBody = string.Empty;

    [ObservableProperty]
    private DateTime _receivedAt;

    [ObservableProperty]
    private bool _hasAttachments;

    [ObservableProperty]
    private ObservableCollection<AttachmentViewModel> _attachments = new();

    [ObservableProperty]
    private bool _isRead;

    [ObservableProperty]
    private bool _isFlagged;

    [ObservableProperty]
    private bool _isEmpty = true;

    public ReadingPaneViewModel(IEmailRepository emailRepository)
    {
        _emailRepository = emailRepository;
        _logger = Log.ForContext<ReadingPaneViewModel>();
    }

    public async Task LoadMessageAsync(int messageId)
    {
        try
        {
            var message = await _emailRepository.GetMessageByIdAsync(messageId);
            if (message == null)
            {
                Clear();
                return;
            }

            MessageId = message.Id;
            FromAddress = message.FromAddress;
            FromName = string.IsNullOrEmpty(message.FromName) ? message.FromAddress : message.FromName;
            ToAddresses = message.ToAddresses;
            CcAddresses = message.CcAddresses ?? string.Empty;
            Subject = message.Subject;
            Body = message.PlainTextBody ?? string.Empty;
            HtmlBody = message.HtmlBody ?? string.Empty;
            ReceivedAt = message.ReceivedAt;
            HasAttachments = message.HasAttachments;
            IsRead = message.Flags.HasFlag(EmailFlag.Seen);
            IsFlagged = message.Flags.HasFlag(EmailFlag.Flagged);
            IsEmpty = false;

            Attachments.Clear();
            if (message.Attachments != null)
            {
                foreach (var attachment in message.Attachments)
                {
                    Attachments.Add(new AttachmentViewModel(attachment));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error loading message {MessageId}", messageId);
            Clear();
        }
    }

    public void Clear()
    {
        MessageId = 0;
        FromAddress = string.Empty;
        FromName = string.Empty;
        ToAddresses = string.Empty;
        CcAddresses = string.Empty;
        Subject = string.Empty;
        Body = string.Empty;
        HtmlBody = string.Empty;
        ReceivedAt = DateTime.MinValue;
        HasAttachments = false;
        Attachments.Clear();
        IsRead = false;
        IsFlagged = false;
        IsEmpty = true;
    }

    [RelayCommand]
    private async Task ToggleFlagAsync()
    {
        try
        {
            var message = await _emailRepository.GetMessageByIdAsync(MessageId);
            if (message == null) return;

            if (IsFlagged)
                message.Flags &= ~EmailFlag.Flagged;
            else
                message.Flags |= EmailFlag.Flagged;

            await _emailRepository.UpdateMessageAsync(message);
            IsFlagged = message.Flags.HasFlag(EmailFlag.Flagged);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error toggling flag");
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        try
        {
            await _emailRepository.SoftDeleteMessageAsync(MessageId);
            Clear();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error deleting message");
        }
    }

    [RelayCommand]
    private void OpenAttachment(AttachmentViewModel attachment)
    {
        try
        {
            if (File.Exists(attachment.LocalPath))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = attachment.LocalPath,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error opening attachment");
        }
    }
}

public partial class AttachmentViewModel : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _fileName = string.Empty;

    [ObservableProperty]
    private string _contentType = string.Empty;

    [ObservableProperty]
    private long _size;

    [ObservableProperty]
    private string _localPath = string.Empty;

    public AttachmentViewModel(EmailAttachment attachment)
    {
        Id = attachment.Id;
        FileName = attachment.FileName;
        ContentType = attachment.ContentType;
        Size = attachment.Size;
        LocalPath = attachment.LocalPath;
    }

    public string SizeDisplay => FormatSize(Size);

    public string IconPath => GetIconForContentType(ContentType);

    private static string FormatSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024} KB";
        return $"{bytes / (1024 * 1024)} MB";
    }

    private static string GetIconForContentType(string contentType)
    {
        return contentType switch
        {
            var ct when ct.StartsWith("image/") => "/Assets/attachment-image.png",
            var ct when ct.StartsWith("application/pdf") => "/Assets/attachment-pdf.png",
            var ct when ct.StartsWith("application/zip") || ct.Contains("compressed") => "/Assets/attachment-zip.png",
            var ct when ct.Contains("word") || ct.Contains("document") => "/Assets/attachment-doc.png",
            var ct when ct.Contains("excel") || ct.Contains("spreadsheet") => "/Assets/attachment-xls.png",
            _ => "/Assets/attachment.png"
        };
    }
}
