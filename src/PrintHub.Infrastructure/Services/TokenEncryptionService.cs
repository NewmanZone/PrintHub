using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using PrintHub.Core.Interfaces;
using PrintHub.Core.Interfaces.Services;

namespace PrintHub.Infrastructure.Services;

/// <summary>
/// Encrypts and decrypts tokens at rest.
/// </summary>
public interface ITokenEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}

/// <summary>
/// State store for OAuth state parameter and optional PKCE verifier.
/// </summary>
public interface IOAuthStateStore
{
    void SaveState(string state, string userId, string returnUrl, TimeSpan expiry, string? codeVerifier = null);
    void SaveState(string state, string userId, Guid workspaceId, string returnUrl, TimeSpan expiry, string? codeVerifier = null);
    (string? userId, string? returnUrl, string? codeVerifier) GetState(string state);
    (string? userId, Guid? workspaceId, string? returnUrl, string? codeVerifier) GetWorkspaceState(string state);
    void DeleteState(string state);
}

/// <summary>
/// AES-GCM encryption with a fresh nonce per Encrypt call.
/// Format: base64(nonce(12) || ciphertext || tag(16)).
/// </summary>
public class AesTokenEncryptionService : ITokenEncryptionService
{
    private readonly byte[] _key;

    public AesTokenEncryptionService(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Encryption key is required.", nameof(key));

        var keyBytes = Convert.FromBase64String(key);
        if (keyBytes.Length is not (16 or 24 or 32))
            throw new ArgumentException("Encryption key must be 16, 24, or 32 bytes.", nameof(key));

        _key = keyBytes;
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var nonce = new byte[12];
        RandomNumberGenerator.Fill(nonce);

        var tag = new byte[16];
        var cipherBytes = new byte[plainBytes.Length];

        using var aes = new AesGcm(_key, 16);
        aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

        var result = new byte[nonce.Length + cipherBytes.Length + tag.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, nonce.Length, cipherBytes.Length);
        Buffer.BlockCopy(tag, 0, result, nonce.Length + cipherBytes.Length, tag.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return string.Empty;

        var cipherBytes = Convert.FromBase64String(cipherText);
        if (cipherBytes.Length < 12 + 16)
            throw new CryptographicException("Invalid ciphertext.");

        var nonce = new byte[12];
        var tag = new byte[16];
        var cipher = new byte[cipherBytes.Length - 12 - 16];

        Buffer.BlockCopy(cipherBytes, 0, nonce, 0, 12);
        Buffer.BlockCopy(cipherBytes, cipherBytes.Length - 16, tag, 0, 16);
        Buffer.BlockCopy(cipherBytes, 12, cipher, 0, cipher.Length);

        var plainBytes = new byte[cipher.Length];

        using var aes = new AesGcm(_key, 16);
        aes.Decrypt(nonce, cipher, tag, plainBytes);

        return Encoding.UTF8.GetString(plainBytes);
    }
}

/// <summary>
/// In-memory implementation of OAuth state store (suitable for single-instance deployments).
/// </summary>
/// <summary>
/// Thread-safe in-memory implementation of OAuth state store using ConcurrentDictionary.
/// Suitable for single-instance deployments; for multi-instance, use a distributed cache.
/// </summary>
public class InMemoryOAuthStateStore : IOAuthStateStore
{
    private readonly ConcurrentDictionary<string, (string userId, Guid? workspaceId, string returnUrl, string? codeVerifier, DateTime expiresAt)> _states = new();

    public void SaveState(string state, string userId, string returnUrl, TimeSpan expiry, string? codeVerifier = null)
    {
        _states[state] = (userId, null, returnUrl, codeVerifier, DateTime.UtcNow.Add(expiry));
    }

    public void SaveState(string state, string userId, Guid workspaceId, string returnUrl, TimeSpan expiry, string? codeVerifier = null)
    {
        _states[state] = (userId, workspaceId, returnUrl, codeVerifier, DateTime.UtcNow.Add(expiry));
    }

    public (string? userId, string? returnUrl, string? codeVerifier) GetState(string state)
    {
        if (!_states.TryGetValue(state, out var entry))
            return (null, null, null);

        if (entry.expiresAt < DateTime.UtcNow)
        {
            _states.TryRemove(state, out _);
            return (null, null, null);
        }

        return (entry.userId, entry.returnUrl, entry.codeVerifier);
    }

    public (string? userId, Guid? workspaceId, string? returnUrl, string? codeVerifier) GetWorkspaceState(string state)
    {
        if (!_states.TryGetValue(state, out var entry))
            return (null, null, null, null);

        if (entry.expiresAt < DateTime.UtcNow)
        {
            _states.TryRemove(state, out _);
            return (null, null, null, null);
        }

        return (entry.userId, entry.workspaceId, entry.returnUrl, entry.codeVerifier);
    }

    public void DeleteState(string state)
    {
        _states.TryRemove(state, out _);
    }
}
