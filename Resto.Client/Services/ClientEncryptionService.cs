using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Resto.Client.Services;

public class ClientEncryptionService
{
    private readonly byte[] _key;

    public ClientEncryptionService(
        IConfiguration configuration)
    {
        var key =
            configuration["Encryption:Key"];

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException(
                "Encryption:Key is missing from configuration.");
        }

        _key = SHA256.HashData(
            Encoding.UTF8.GetBytes(key));
    }

    // ============================================================
    // AES-256 ENCRYPT
    // ============================================================

    public string EncryptField(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            return string.Empty;
        }

        using var aes = Aes.Create();

        aes.Key = _key;

        aes.GenerateIV();

        aes.Mode = CipherMode.CBC;

        aes.Padding = PaddingMode.PKCS7;

        using var encryptor =
            aes.CreateEncryptor();

        byte[] plainBytes =
            Encoding.UTF8.GetBytes(plainText);

        byte[] cipherBytes =
            encryptor.TransformFinalBlock(
                plainBytes,
                0,
                plainBytes.Length);

        byte[] combined =
            new byte[
                aes.IV.Length +
                cipherBytes.Length];

        Buffer.BlockCopy(
            aes.IV,
            0,
            combined,
            0,
            aes.IV.Length);

        Buffer.BlockCopy(
            cipherBytes,
            0,
            combined,
            aes.IV.Length,
            cipherBytes.Length);

        return Convert.ToBase64String(combined);
    }

    // ============================================================
    // AES-256 DECRYPT
    // ============================================================

    public string DecryptField(
        string encryptedText)
    {
        if (string.IsNullOrWhiteSpace(encryptedText))
        {
            return string.Empty;
        }

        try
        {
            byte[] combined =
                Convert.FromBase64String(
                    encryptedText);

            const int ivLength = 16;

            if (combined.Length <= ivLength)
            {
                return string.Empty;
            }

            byte[] iv =
                new byte[ivLength];

            byte[] cipherBytes =
                new byte[
                    combined.Length - ivLength];

            Buffer.BlockCopy(
                combined,
                0,
                iv,
                0,
                ivLength);

            Buffer.BlockCopy(
                combined,
                ivLength,
                cipherBytes,
                0,
                cipherBytes.Length);

            using var aes = Aes.Create();

            aes.Key = _key;

            aes.IV = iv;

            aes.Mode = CipherMode.CBC;

            aes.Padding = PaddingMode.PKCS7;

            using var decryptor =
                aes.CreateDecryptor();

            byte[] plainBytes =
                decryptor.TransformFinalBlock(
                    cipherBytes,
                    0,
                    cipherBytes.Length);

            return Encoding.UTF8.GetString(
                plainBytes);
        }
        catch
        {
            return string.Empty;
        }
    }

    // ============================================================
    // BCRYPT HASH
    // ============================================================

    public string HashPassword(
        string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException(
                "Password cannot be empty.",
                nameof(password));
        }

        return global::BCrypt.Net.BCrypt.HashPassword(
            password,
            workFactor: 12);
    }

    // ============================================================
    // BCRYPT VERIFY
    // ============================================================

    public bool VerifyPassword(
        string password,
        string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        try
        {
            return global::BCrypt.Net.BCrypt.Verify(
                password,
                passwordHash);
        }
        catch
        {
            return false;
        }
    }
}