using System;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;

namespace Task_3
{
    public class Hash
    {
        public byte[] GenerateSecureRandomKey()
        {
            var rng = RandomNumberGenerator.Create();
            var key = new byte[32];
            rng.GetBytes(key);
            return key;
        }

        public int GenerateSecureRandomNumber(int min, int max)
        {
            return RandomNumberGenerator.GetInt32(min, max + 1);
        }

        public string CalculateHMAC(byte[] key, int message)
        {
            var digest = new Sha3Digest(256);
            var hmac = new HMac(digest);
            hmac.Init(new KeyParameter(key));
            var messageBytes = BitConverter.GetBytes(message);
            hmac.BlockUpdate(messageBytes, 0, messageBytes.Length);
            var result = new byte[hmac.GetMacSize()];
            hmac.DoFinal(result, 0);
            return BitConverter.ToString(result).Replace("-", "").ToLower();
        }
    }
}