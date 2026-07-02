namespace ProtonDesktop.Core.Interfaces;

public interface ICredentialStore
{
    string Encrypt(string plainText);
    string Decrypt(string encryptedText);
}
