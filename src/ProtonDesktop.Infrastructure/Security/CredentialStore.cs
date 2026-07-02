using System.Security.Cryptography;
using System.Text;
using ProtonDesktop.Core.Interfaces;
using Serilog;

namespace ProtonDesktop.Infrastructure.Security;

public class CredentialStore : ICredentialStore
{
    private readonly ILogger _logger;
    private readonly byte[] _additionalEntropy;

    public CredentialStore(ILogger logger)
    {
        _logger = logger;
        _additionalEntropy = Encoding.UTF8.GetBytes("ProtonDesktop_v1_Salt_2026");
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        try
        {
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var encryptedBytes = ProtectedData.Protect(plainBytes, _additionalEntropy, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedBytes);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to encrypt credential");
            throw new CryptographicException("Failed to encrypt credential", ex);
        }
    }

    public string Decrypt(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
            return string.Empty;

        try
        {
            var encryptedBytes = Convert.FromBase64String(encryptedText);
            var plainBytes = ProtectedData.Unprotect(encryptedBytes, _additionalEntropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(plainBytes);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to decrypt credential");
            throw new CryptographicException("Failed to decrypt credential", ex);
        }
    }
}
