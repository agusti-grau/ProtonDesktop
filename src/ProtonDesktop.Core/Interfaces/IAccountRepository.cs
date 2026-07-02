using ProtonDesktop.Core.Models;

namespace ProtonDesktop.Core.Interfaces;

public interface IAccountRepository
{
    Task<MailAccount?> GetAccountByIdAsync(int id);
    Task<MailAccount?> GetDefaultAccountAsync();
    Task<IEnumerable<MailAccount>> GetAllAccountsAsync();
    Task<MailAccount> CreateAccountAsync(MailAccount account);
    Task UpdateAccountAsync(MailAccount account);
    Task DeleteAccountAsync(int id);
    Task SetDefaultAccountAsync(int id);
}
