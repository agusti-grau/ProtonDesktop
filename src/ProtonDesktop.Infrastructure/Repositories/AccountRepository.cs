using Microsoft.EntityFrameworkCore;
using ProtonDesktop.Core.Interfaces;
using ProtonDesktop.Core.Models;
using ProtonDesktop.Infrastructure.Data;

namespace ProtonDesktop.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public AccountRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<MailAccount?> GetAccountByIdAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.MailAccounts.FindAsync(id);
    }

    public async Task<MailAccount?> GetDefaultAccountAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.MailAccounts.FirstOrDefaultAsync(x => x.IsDefault);
    }

    public async Task<IEnumerable<MailAccount>> GetAllAccountsAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.MailAccounts.ToListAsync();
    }

    public async Task<MailAccount> CreateAccountAsync(MailAccount account)
    {
        using var context = _contextFactory.CreateDbContext();
        context.MailAccounts.Add(account);
        await context.SaveChangesAsync();
        return account;
    }

    public async Task UpdateAccountAsync(MailAccount account)
    {
        using var context = _contextFactory.CreateDbContext();
        context.MailAccounts.Update(account);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAccountAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        var account = await context.MailAccounts.FindAsync(id);
        if (account != null)
        {
            context.MailAccounts.Remove(account);
            await context.SaveChangesAsync();
        }
    }

    public async Task SetDefaultAccountAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        var accounts = await context.MailAccounts.ToListAsync();
        foreach (var account in accounts)
        {
            account.IsDefault = account.Id == id;
        }
        await context.SaveChangesAsync();
    }
}
