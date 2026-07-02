using Microsoft.EntityFrameworkCore;
using ProtonDesktop.Core.Interfaces;
using ProtonDesktop.Core.Models;
using ProtonDesktop.Infrastructure.Data;

namespace ProtonDesktop.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly AppDbContext _context;

    public AccountRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MailAccount?> GetAccountByIdAsync(int id)
    {
        return await _context.MailAccounts.FindAsync(id);
    }

    public async Task<MailAccount?> GetDefaultAccountAsync()
    {
        return await _context.MailAccounts.FirstOrDefaultAsync(x => x.IsDefault);
    }

    public async Task<IEnumerable<MailAccount>> GetAllAccountsAsync()
    {
        return await _context.MailAccounts.ToListAsync();
    }

    public async Task<MailAccount> CreateAccountAsync(MailAccount account)
    {
        _context.MailAccounts.Add(account);
        await _context.SaveChangesAsync();
        return account;
    }

    public async Task UpdateAccountAsync(MailAccount account)
    {
        _context.MailAccounts.Update(account);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAccountAsync(int id)
    {
        var account = await _context.MailAccounts.FindAsync(id);
        if (account != null)
        {
            _context.MailAccounts.Remove(account);
            await _context.SaveChangesAsync();
        }
    }

    public async Task SetDefaultAccountAsync(int id)
    {
        var accounts = await _context.MailAccounts.ToListAsync();
        foreach (var account in accounts)
        {
            account.IsDefault = account.Id == id;
        }
        await _context.SaveChangesAsync();
    }
}
