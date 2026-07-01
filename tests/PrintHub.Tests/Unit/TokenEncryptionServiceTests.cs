using System;
using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using PrintHub.Infrastructure.Services;
using Xunit;

namespace PrintHub.Tests.Unit;

[Collection("Unit Tests")]
public class TokenEncryptionServiceTests
{
    private static AesTokenEncryptionService CreateService()
    {
        // 32-byte AES key encoded as base64 (required for AES-256 GCM)
        var keyBytes = new byte[32];
        RandomNumberGenerator.Fill(keyBytes);
        return new AesTokenEncryptionService(Convert.ToBase64String(keyBytes));
    }

    [Fact]
    public void EncryptDecrypt_RoundTrip_ReturnsOriginalPlainText()
    {
        // Arrange
        var service = CreateService();
        const string plainText = "etsy-access-token-123";

        // Act
        var cipherText = service.Encrypt(plainText);
        var decrypted = service.Decrypt(cipherText);

        // Assert
        cipherText.Should().NotBeNullOrEmpty();
        cipherText.Should().NotBe(plainText);
        decrypted.Should().Be(plainText);
    }

    [Fact]
    public void Encrypt_ProducesDifferentCipherText_ForSamePlainText_DueToFreshNonce()
    {
        // Arrange
        var service = CreateService();
        const string plainText = "repeat-me";

        // Act
        var cipherText1 = service.Encrypt(plainText);
        var cipherText2 = service.Encrypt(plainText);

        // Assert
        cipherText1.Should().NotBe(cipherText2);
    }

    [Fact]
    public void Decrypt_TamperedCipherText_ThrowsCryptographicException()
    {
        // Arrange
        var service = CreateService();
        const string plainText = "sensitive-data";
        var cipherText = service.Encrypt(plainText);

        var tamperedBytes = Convert.FromBase64String(cipherText);
        tamperedBytes[tamperedBytes.Length - 1] ^= 0xFF; // flip tag/tag bytes
        var tamperedCipherText = Convert.ToBase64String(tamperedBytes);

        // Act
        Action act = () => service.Decrypt(tamperedCipherText);

        // Assert
        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void Encrypt_NullOrEmpty_ReturnsEmptyString()
    {
        var service = CreateService();

        service.Encrypt(null!).Should().BeEmpty();
        service.Encrypt(string.Empty).Should().BeEmpty();
    }

    [Fact]
    public void Decrypt_NullOrEmpty_ReturnsEmptyString()
    {
        var service = CreateService();

        service.Decrypt(null!).Should().BeEmpty();
        service.Decrypt(string.Empty).Should().BeEmpty();
    }
}
