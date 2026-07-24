using System.Windows.Input;
using Serilog;

namespace ProtonDesktop.Services;

public interface IKeyboardShortcutService
{
    void RegisterShortcut(Key key, ModifierKeys modifiers, Action action);
    void UnregisterShortcut(Key key, ModifierKeys modifiers);
}

public class KeyboardShortcutService : IKeyboardShortcutService
{
    private readonly ILogger _logger;
    private readonly Dictionary<(Key, ModifierKeys), Action> _shortcuts = new();

    public KeyboardShortcutService(ILogger logger)
    {
        _logger = logger;
    }

    public void RegisterShortcut(Key key, ModifierKeys modifiers, Action action)
    {
        var shortcutKey = (key, modifiers);
        _shortcuts[shortcutKey] = action;
        _logger.Debug("Registered keyboard shortcut: {Modifiers}+{Key}", modifiers, key);
    }

    public void UnregisterShortcut(Key key, ModifierKeys modifiers)
    {
        var shortcutKey = (key, modifiers);
        _shortcuts.Remove(shortcutKey);
        _logger.Debug("Unregistered keyboard shortcut: {Modifiers}+{Key}", modifiers, key);
    }

    public bool HandleKeyDown(Key key, ModifierKeys modifiers)
    {
        var shortcutKey = (key, modifiers);
        if (_shortcuts.TryGetValue(shortcutKey, out var action))
        {
            try
            {
                action();
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error executing keyboard shortcut");
            }
        }
        return false;
    }
}
