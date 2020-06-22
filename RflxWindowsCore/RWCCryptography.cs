using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace RflxWindowsCore
{
    public class RWCCryptography
    {
        private static string encryptKey = "ZrnR@pg1aBAwnD@!jZFx7JLYbooJUIBrdIl";
        public static String EncryptString(string value)
        {
            try
            {
                var hashKey = GetHashKey();
                var encryptBuffer = CryptographicBuffer.ConvertStringToBinary(value, BinaryStringEncoding.Utf8);
                var AES = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesEcbPkcs7);
                var symmetricKey = AES.CreateSymmetricKey(hashKey);
                var encryptedBuffer = CryptographicEngine.Encrypt(symmetricKey, encryptBuffer, null);
                var encryptedString = CryptographicBuffer.EncodeToBase64String(encryptBuffer);
                return encryptedString;
            }
            catch (Exception)
            {
                return "";
            }
        }
        public static String DecryptString(string value)
        {
            try
            {
                var hashKey = GetHashKey();
                IBuffer decryptBuffer = CryptographicBuffer.DecodeFromBase64String(value);
                var AES = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesEcbPkcs7);
                var symmetricKey = AES.CreateSymmetricKey(hashKey);
                var decryptedBuffer = CryptographicEngine.Decrypt(symmetricKey, decryptBuffer, null);
                var decryptedString = CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, decryptBuffer);
                return decryptedString;
            }
            catch (Exception e)
            {
                RWCLogManager.logDebugMessage(e.Message);
                return "";
            }
        }
        private static IBuffer GetHashKey()
        {
            try
            {
                IBuffer utf8Buffer = CryptographicBuffer.ConvertStringToBinary(encryptKey, BinaryStringEncoding.Utf8);
                HashAlgorithmProvider hashAlgo = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
                IBuffer hashBuffer = hashAlgo.HashData(utf8Buffer);
                if (hashBuffer.Length != hashAlgo.HashLength)
                {
                    throw new Exception("Could not create hash buffer");
                }
                return hashBuffer;
            }
            catch (Exception e)
            {
                RWCLogManager.logDebugMessage(e.Message);
            }
            return null;
        }
    }
}
