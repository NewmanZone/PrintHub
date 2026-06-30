using System.Security.Cryptography;
using System.Text;

namespace PrintHub.Infrastructure.Services;

/// <summary>
/// Service for encrypting and decrypting sensitive data like OAuth tokens.
/// </summary>
public interface ITokenEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}

/// <summary>
/// Implementation using AES encryption.
/// </summary>
public class AesTokenEncryptionService : ITokenEncryptionService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public AesTokenEncryptionService(string encryptionKey)
    {
        // Derive a 256-bit key from the provided key using SHA256
        using var sha256 = SHA256.Create();
        _key = sha256.ComputeHash(Encoding.UTF8.GetBytes(encryptionKey));
        _iv = new byte[16]; // AES uses 16-byte IV
        RandomNumberGenerator.Fill(_iv);
    }

    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        
        var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        
        // Prepend IV to encrypted data for decryption
        var result = new byte[_iv.Length + encryptedBytes.Length];
        Buffer.BlockCopy(_iv, 0, result, 0, _iv.Length);
        Buffer.BlockCopy(encryptedBytes, 0, result, _iv.Length, encryptedBytes.Length);
        
        return Convert.ToBase64String(result);
    }

    public string Decrypt(string cipherText)
    {
        var cipherBytes = Convert.FromBase64String(cipherText);
        
        // Extract IV from the beginning
        var iv = new byte[16];
        Buffer.BlockCopy(cipherBytes, 0, iv, 0, iv.Length);
        
        var encryptedBytes = new byte[cipherBytes.Length - iv.Length];
        Buffer.BlockCopy(cipherBytes, iv.Length, encryptedBytes, 0, encryptedBytes.Length);
        
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = iv;
        
        var decryptor = aes.CreateDecryptor();
        var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
        
        return Encoding.UTF8.GetString(decryptedBytes);
    }
}

/// <summary>
/// In-memory store for OAuth state (in production, use Redis or similar).
/// </summary>
public interface IOAuthStateStore
{
    void SaveState(string state, string userId, string returnUrl, TimeSpan expiry);
    (string? userId, string? returnUrl) GetState(string state);
    void DeleteState(string state);
}

public class InMemoryOAuthStateStore : IOAuthStateStore
{
    private readonly Dictionary<string, (string userId, string returnUrl, DateTime expiresAt)> _states = new();
    private readonly object _lock = new();

    public void SaveState(string state, string userId, string returnUrl, TimeSpan expiry)
    {
        lock (_lock)
        {
            _states[state] = (userId, returnUrl, DateTime.UtcNow.Add(expiry));
        }
    }

    public (string? userId, string? returnUrl) GetState(string state)
    {
        lock (_lock)
        {
            if (_states.TryGetValue(state, out var data))
            {
                if (data.expiresAt > DateTime.UtcNow)
                {
                    return (data.userId, data.returnUrl);
                }
                _states.Remove(state);
            }
        }
        return (null, null);
    }

    public void DeleteState(string state)
    {
        lock (_lock)
        {
            _states.Remove(state);
        }
    }
}