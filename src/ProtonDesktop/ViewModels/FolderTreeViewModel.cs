using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ProtonDesktop.Core.Enums;
using ProtonDesktop.Core.Interfaces;
using ProtonDesktop.Core.Models;
using Serilog;

namespace ProtonDesktop.ViewModels;

public partial class FolderTreeViewModel : ObservableObject
{
    private readonly IEmailRepository _emailRepository;
    private readonly int _accountId;
    private readonly ILogger _logger;

    [ObservableProperty]
    private ObservableCollection<FolderViewModel> _folders = new();

    [ObservableProperty]
    private FolderViewModel? _selectedFolder;

    public FolderTreeViewModel(IEmailRepository emailRepository, int accountId)
    {
        _emailRepository = emailRepository;
        _accountId = accountId;
        _logger = Log.ForContext<FolderTreeViewModel>();
    }

    public async Task LoadAsync()
    {
        try
        {
            var folders = await _emailRepository.GetFoldersAsync(_accountId);
            Folders.Clear();

            foreach (var folder in folders.Where(f => f.ParentFolderId == null))
            {
                var folderVm = new FolderViewModel(folder);
                await LoadSubFoldersAsync(folderVm, folders);
                Folders.Add(folderVm);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error loading folders");
        }
    }

    private async Task LoadSubFoldersAsync(FolderViewModel parent, IEnumerable<EmailFolder> allFolders)
    {
        var subFolders = allFolders.Where(f => f.ParentFolderId == parent.Id);
        foreach (var subFolder in subFolders)
        {
            var subFolderVm = new FolderViewModel(subFolder);
            await LoadSubFoldersAsync(subFolderVm, allFolders);
            parent.SubFolders.Add(subFolderVm);
        }
    }
}

public partial class FolderViewModel : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private FolderType _folderType;

    [ObservableProperty]
    private int _unreadCount;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private ObservableCollection<FolderViewModel> _subFolders = new();

    public FolderViewModel(EmailFolder folder)
    {
        Id = folder.Id;
        Name = folder.Name;
        FolderType = folder.FolderType;
        UnreadCount = folder.UnreadCount;
        TotalCount = folder.TotalCount;
    }

    public string DisplayText => UnreadCount > 0 ? $"{Name} ({UnreadCount})" : Name;

    public string IconPath => FolderType switch
    {
        FolderType.Inbox => "/Assets/folder-inbox.png",
        FolderType.Sent => "/Assets/folder-sent.png",
        FolderType.Drafts => "/Assets/folder-drafts.png",
        FolderType.Trash => "/Assets/folder-trash.png",
        FolderType.Spam => "/Assets/folder-spam.png",
        FolderType.Archive => "/Assets/folder-archive.png",
        _ => "/Assets/folder.png"
    };
}
