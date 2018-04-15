using System;
using System.Security.Cryptography;
using System.Text;

namespace wmsMLC.General
{
    public static class CryptoHelper
    {
        public const string DefualtAlgorithmName = "BarCodeCryptoAlgorithm";

        private static string CryptoTransform(ICryptoTransform transform, string txt, bool compressarray)
        {
            using (transform)
            {
                // Verify that multiple blocks can not be transformed.
                if (!transform.CanTransformMultipleBlocks)
                {
                    // Initializie the offset size.
                    var inputOffset = 0;

                    // Iterate through inputBytes transforming by blockSize.
                    var inputBlockSize = transform.InputBlockSize;

                    //var encoding = Encoding.GetEncoding(1252);
                    //var encoding = Encoding.GetEncoding(1251);
                    var encoding = Encoding.ASCII;
                    var inputBytes = encoding.GetBytes(txt);
                    byte[] outputBytes = null;

                    while (inputBytes.Length - inputOffset > inputBlockSize)
                    {
                        transform.TransformBlock(inputBytes, inputOffset, inputBytes.Length - inputOffset, outputBytes, 0);
                        inputOffset += transform.InputBlockSize;
                    }

                    // Transform the final block of data.
                    outputBytes = transform.TransformFinalBlock(inputBytes, inputOffset, inputBytes.Length - inputOffset);

                    if (compressarray)
                        outputBytes = CompressArray(outputBytes);

                    if (outputBytes == null || outputBytes.Length == 0)
                        return string.Empty;

                    var result = encoding.GetString(outputBytes, 0, outputBytes.Length);
                    return result;
                }
            }

            return null;
        }

        private static byte[] CompressArray(byte[] src)
        {
            if (src == null || src.Length == 0)
                return src;

            var outputBytesLength = src.Length;
            if (outputBytesLength > 0)
            {
                var length = -1;
                for (var i = outputBytesLength - 1; i >= 0; i--)
                {
                    if (src[i] != 0)
                    {
                        length = i + 1;
                        break;
                    }
                }

                if (length < 0)
                    return null;

                if (length < outputBytesLength)
                {
                    var dest = new byte[length];
                    Array.Copy(src, 0, dest, 0, length);
                    return dest;
                }
            }

            return src;
        }

        public static string Encrypt(string txt, byte[] key)
        {
#if WindowsCE
            var encryptor = new Crypto.BarCodeCryptoTransform(Crypto.CryptoMode.Encrypt, key);
#else
            var cryptoTransform = SymmetricAlgorithm.Create(DefualtAlgorithmName);
            var encryptor = cryptoTransform.CreateEncryptor(key, null);
#endif
            return CryptoTransform(encryptor, txt, false);
        }

        public static string Decrypt(string txt, byte[] key)
        {
#if WindowsCE
            var decryptor = new Crypto.BarCodeCryptoTransform(Crypto.CryptoMode.Decrypt, key);
#else
            var cryptoTransform = SymmetricAlgorithm.Create(DefualtAlgorithmName);
            var decryptor = cryptoTransform.CreateDecryptor(key, null);
#endif
            return CryptoTransform(decryptor, txt, true);
        }
    }
}
