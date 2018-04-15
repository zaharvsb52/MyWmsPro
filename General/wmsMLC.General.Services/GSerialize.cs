#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="GSerialize.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Vladimir G. Nosov</Author>
/// <Date>08.11.2012 20:32:26</Date>
/// <Summary>Бинарная сериализация/десерилизация объектов с использованием ProroBuf от @Google</Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587

using System.IO;
using System.IO.Compression;
using ProtoBuf;

namespace wmsMLC.General.Services
{
    /// <summary>
    /// Бинарная сериализация/десерилизация объектов с использованием ProroBuf от @Google
    /// </summary>
    public static class GSerialize
    {
        public static byte[] SerializeAndCompress(object obj)
        {
            using (var ms = new MemoryStream())
            {
                using (var zs = new GZipStream(ms, CompressionMode.Compress, false))
                {
                    Serializer.Serialize(zs, obj);
                    ProtoBuf.Serializer.FlushPool();
                }
                return ms.ToArray();
            }
        }

        public static object DecompressAndDeserialze<T>(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                using (var zs = new GZipStream(ms, CompressionMode.Decompress, false))
                {
                    var res = Serializer.Deserialize<T>(zs);
                    ProtoBuf.Serializer.FlushPool();
                    return res;
                    //return new BinaryFormatter().Deserialize(zs);
                }
            }
        }

        /// <summary>
        /// Сериализация объекта в массив
        /// </summary>
        /// <typeparam name="T">тип объекта</typeparam>
        /// <param name="target">объект сериализации</param>
        /// <returns>сериализованный объект</returns>
        public static byte[] SerializeBytes<T>(T target)
        {
            return SerializeAndCompress(target);
        }

        /// <summary>
        /// Десерилизация объекта из массива
        /// </summary>
        /// <typeparam name="T">тип объекта</typeparam>
        /// <param name="binary">строка сериализации</param>
        /// <returns>десерилизованный объект</returns>
        public static T DeserializeBytes<T>(byte[] binary)
        {
            return (T)DecompressAndDeserialze<T>(binary);
        }
    }
}