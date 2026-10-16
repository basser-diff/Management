using System.Security.Cryptography;
using System.Text;

namespace Resto.API.Services;

public class EncryptionService
{
    private readonly byte[] _key;

    public EncryptionService(IConfiguration configuration)
    {
        string? key = configuration["Encryption:Key"];

        if (string.IsNullOrWhiteSpace(key))
            throw new InvalidOperationException(
                "Encryption:Key is missing from appsettings.json.");

        _key = Convert.FromBase64String(key);

        if (_key.Length != 32)
            throw new InvalidOperationException(
                "Encryption key must be exactly 32 bytes.");
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        using Aes aes = Aes.Create();

        aes.Key = _key;
        aes.GenerateIV();

        using ICryptoTransform encryptor =
            aes.CreateEncryptor();

        byte[] plainBytes =
            Encoding.UTF8.GetBytes(plainText);

        byte[] encryptedBytes =
            encryptor.TransformFinalBlock(
                plainBytes,
                0,
                plainBytes.Length);

        byte[] result =
            new byte[aes.IV.Length + encryptedBytes.Length];

        Buffer.BlockCopy(
            aes.IV,
            0,
            result,
            0,
            aes.IV.Length);

        Buffer.BlockCopy(
            encryptedBytes,
            0,
            result,
            aes.IV.Length,
            encryptedBytes.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
            return string.Empty;

        byte[] data =
            Convert.FromBase64String(encryptedText);

        using Aes aes = Aes.Create();

        aes.Key = _key;

        byte[] iv = new byte[16];

        Buffer.BlockCopy(
            data,
            0,
            iv,
            0,
            iv.Length);

        aes.IV = iv;

        byte[] cipherText =
            new byte[data.Length - iv.Length];

        Buffer.BlockCopy(
            data,
            iv.Length,
            cipherText,
            0,
            cipherText.Length);

        using ICryptoTransform decryptor =
            aes.CreateDecryptor();

        byte[] decrypted =
            decryptor.TransformFinalBlock(
                cipherText,
                0,
                cipherText.Length);

        return Encoding.UTF8.GetString(decrypted);
    }
}