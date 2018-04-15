using System;
using System.Collections.Generic;

namespace wmsMLC.General
{
    public class CryptoKeyProvider : ICryptoKeyProvider
    {
        private readonly Dictionary<int, byte[]> _key = new Dictionary<int, byte[]>();

        public void AddOrChangeKey(int number, byte[] key)
        {
            _key[number] = key;
        }

        public byte[] GetKey(int keyNumber)
        {
            if (!_key.ContainsKey(keyNumber))
                throw new Exception("Key not found.");

            return _key[keyNumber];
        }
    }

    public interface ICryptoKeyProvider
    {
        byte[] GetKey(int keyNumber);
    }
}
