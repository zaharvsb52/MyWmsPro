#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="HashCode.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Olga Marysheva</Author>
/// <Date>18.10.2012 9:18:27</Date>
/// <Summary>Вычисление хешкода файла</Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587

using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace wmsMLC.EPS.wmsEPS.Helpers
{
    public class HashCode
    {
        /// <summary>
        /// Вычисляет хэш SHA1 для файла
        /// </summary>
        /// <param name="fileName">полное имя файла</param>
        /// <returns>строка - хеш код</returns>
        public string GetSHA1(string fileName)
        {
            using (SHA1 sha1 = new SHA1CryptoServiceProvider())
            {
                byte[] sha1HashValue;
                using (var fs = File.Open(fileName, FileMode.Open))
                {
                    sha1HashValue = sha1.ComputeHash(fs);
                }
                //string result = BitConverter.ToString(sha1HashValue).Replace("-", String.Empty);
                return HashValueToString(sha1HashValue);
            }
        }

        /// <summary>
        /// Вычисляет хэш SHA1 для массива данных
        /// </summary>
        /// <param name="bytes">массив байт</param>
        /// <returns>строка - хеш код</returns>
        public string GetSHA1(byte[] bytes)
        {
            using (SHA1 sha1 = new SHA1CryptoServiceProvider())
            {
                byte[] sha1HashValue = sha1.ComputeHash(bytes);
                return HashValueToString(sha1HashValue);
            }
        }

        /// <summary>
        /// Вычисляет хэш MD5 для файла.
        /// </summary>
        /// <param name="fileName">полное имя файла</param>
        /// <returns>строка - хеш код</returns>
        public string GetMD5(string fileName)
        {
            using (MD5 md5 = new MD5CryptoServiceProvider())
            {
                using (var fs = File.Open(fileName, FileMode.Open))
                {
                    byte[] md5HashValue = md5.ComputeHash(fs);
                    return HashValueToString(md5HashValue);
                }
            }
        }

        /// <summary>
        /// Вычисляет хэш MD5 для массива данных
        /// </summary>
        /// <param name="bytes">массив байт</param>
        /// <returns>строка - хеш код</returns>
        public string GetMD5(byte[] bytes)
        {
            using (MD5 md5 = new MD5CryptoServiceProvider())
            {
                byte[] md5HashValue = md5.ComputeHash(bytes);
                return HashValueToString(md5HashValue);
            }
        }

        /// <summary>
        /// Приведение массива byte к строковому виду
        /// </summary>
        /// <param name="hash">массив байт</param>
        /// <returns>строковое представление</returns>
        public string HashValueToString(byte[] hash)
        {
            return hash.Aggregate("", (current, b) => current + b.ToString("x2"));
        }
    }
}
