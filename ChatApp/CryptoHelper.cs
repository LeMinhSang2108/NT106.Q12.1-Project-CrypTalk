using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

public class CryptoHelper
{
    public static byte[] EncryptRSA(byte[] data, string publicKeyXml)
    {
        using var rsa = RSA.Create();
        rsa.FromXmlString(publicKeyXml);
        return rsa.Encrypt(data, RSAEncryptionPadding.OaepSHA256);
    }

    public static byte[] DecryptRSA(byte[] data, string privateKeyXml)
    {
        using var rsa = RSA.Create();
        rsa.FromXmlString(privateKeyXml);
        return rsa.Decrypt(data, RSAEncryptionPadding.OaepSHA256);
    }

    public static byte[] EncryptAES(byte[] data, byte[] key)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();
        using var encryptor = aes.CreateEncryptor();
        byte[] encrypted = encryptor.TransformFinalBlock(data, 0, data.Length);

        byte[] result = new byte[16 + encrypted.Length];
        Array.Copy(aes.IV, 0, result, 0, 16);
        Array.Copy(encrypted, 0, result, 16, encrypted.Length);
        return result;
    }

    public static byte[] DecryptAES(byte[] data, byte[] key)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = data.Take(16).ToArray();

        using var decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(data, 16, data.Length - 16);
    }

    public static byte[] GenerateAESKey()
    {
        using var aes = Aes.Create();
        aes.GenerateKey();
        return aes.Key;
    }
}
