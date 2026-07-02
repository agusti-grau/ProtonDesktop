using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProtonDesktop.Core.Enums;
using ProtonDesktop.Core.Interfaces;
using ProtonDesktop.Core.Models;
using Serilog;

namespace ProtonDesktop.ViewModels;

public partial class EmailListViewModel : ObservableObject
{
    private readonly IEmailRepository _emailRepository;
    private readonly ILogger _logger;

    [ObservableProperty]
    private ObservableCollection<EmailMessageViewModel> _messages = new();

    [ObservableProperty]
    private EmailMessageViewModel? _selectedMessage;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _sortBy = "Date";

    [ObservableProperty]
    private bool _sortDescending = true;

    [ObservableProperty]
    private bool _isLoading;

    private int _currentFolderId;

    public EmailListViewModel(IEmailRepository emailRepository)
    {
        _emailRepository = emailRepository;
        _logger = Log.ForContext<EmailListViewModel>();
    }

    public async Task LoadMessagesAsync(int folderId)
    {
        try
        {
            IsLoading = true;
            _currentFolderId = folderId;

            var messages = await _emailRepository.GetMessagesAsync(folderId, 0, 100);
            Messages.Clear();

            foreach (var message in messages)
            {
                Messages.Add(new EmailMessageViewModel(message));
            }

            ApplySorting();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error loading messages for folder {FolderId}", folderId);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            await LoadMessagesAsync(_currentFolderId);
            return;
        }

        try
        {
            IsLoading = true;
            var messages = await _emailRepository.SearchMessagesAsync(0, SearchQuery, 0, 100);
            Messages.Clear();

            foreach (var message in messages)
            {
                Messages.Add(new EmailMessageViewModel(message));
            }

            ApplySorting();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error searching messages");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ToggleSort(string column)
    {
        if (SortBy == column)
        {
            SortDescending = !SortDescending;
        }
        else
        {
            SortBy = column;
            SortDescending = true;
        }

        ApplySorting();
    }

    private void ApplySorting()
    {
        var sorted = SortBy switch
        {
            "From" => SortDescending
                ? Messages.OrderByDescending(m => m.FromName).ThenByDescending(m => m.ReceivedAt)
                : Messages.OrderBy(m => m.FromName).ThenBy(m => m.ReceivedAt),
            "Subject" => SortDescending
                ? Messages.OrderByDescending(m => m.Subject).ThenByDescending(m => m.ReceivedAt)
                : Messages.OrderBy(m => m.Subject).ThenBy(m => m.ReceivedAt),
            "Size" => SortDescending
                ? Messages.OrderByDescending(m => m.Size).ThenByDescending(m => m.ReceivedAt)
                : Messages.OrderBy(m => m.Size).ThenBy(m => m.ReceivedAt),
            _ => SortDescending
                ? Messages.OrderByDescending(m => m.ReceivedAt)
                : Messages.OrderBy(m => m.ReceivedAt)
        };

            var sortedList = sorted.ToList();
            Messages.Clear();
            foreach (var msg in sortedList)
            {
                Messages.Add(msg);
            }
    }

    public void UpdateMessageReadStatus(int messageId, bool isRead)
    {
        var message = Messages.FirstOrDefault(m => m.Id == messageId);
        if (message != null)
        {
            message.IsRead = isRead;
        }
    }
}

public partial class EmailMessageViewModel : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _fromAddress = string.Empty;

    [ObservableProperty]
    private string _fromName = string.Empty;

    [ObservableProperty]
    private string _toAddresses = string.Empty;

    [ObservableProperty]
    private string _subject = string.Empty;

    [ObservableProperty]
    private string _preview = string.Empty;

    [ObservableProperty]
    private DateTime _receivedAt;

    [ObservableProperty]
    private bool _isRead;

    [ObservableProperty]
    private bool _isFlagged;

    [ObservableProperty]
    private bool _hasAttachments;

    [ObservableProperty]
    private long _size;

    [ObservableProperty]
    private EmailFlag _flags;

    public EmailMessageViewModel(EmailMessage message)
    {
        Id = message.Id;
        FromAddress = message.FromAddress;
        FromName = string.IsNullOrEmpty(message.FromName) ? message.FromAddress : message.FromName;
        ToAddresses = message.ToAddresses;
        Subject = message.Subject;
        Preview = GeneratePreview(message);
        ReceivedAt = message.ReceivedAt;
        IsRead = message.Flags.HasFlag(EmailFlag.Seen);
        IsFlagged = message.Flags.HasFlag(EmailFlag.Flagged);
        HasAttachments = message.HasAttachments;
        Size = message.Size ?? 0;
        Flags = message.Flags;
    }

    private string GeneratePreview(EmailMessage message)
    {
        var body = !string.IsNullOrEmpty(message.PlainTextBody)
            ? message.PlainTextBody
            : message.HtmlBody ?? string.Empty;

        body = System.Text.RegularExpressions.Regex.Replace(body, "<.*?>", string.Empty);
        body = body.Trim();

        return body.Length > 100 ? body.Substring(0, 100) + "..." : body;
    }

    public string ReceivedAtDisplay => ReceivedAt.ToString("MMM dd, yyyy HH:mm");

    public string SizeDisplay => FormatSize(Size);

    private static string FormatSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024} KB";
        return $"{bytes / (1024 * 1024)} MB";
    }
}
