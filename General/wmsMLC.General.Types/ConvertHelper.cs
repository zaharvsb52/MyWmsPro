#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="ConvertHelper.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Vladimir G. Nosov</Author>
/// <Date>16.08.2012 11:04:12</Date>
/// <Summary>Вспомогательный класс для парсинга и конвертаций</Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587

using System;
using System.Linq;

namespace wmsMLC.General.Types
{
    public static class ConvertHelper
    {
        /// <summary>
        /// Переводит строковое значение объема памяти в байты
        /// </summary>
        /// <param name="size">строковое значение объема K,M,G</param>
        /// <returns>количество байт</returns>
        public static ulong SizeToBytes(string size)
        {
            var str = "";
            ulong tmp;
            ulong bytes;
            try
            {
                str = size.TakeWhile(char.IsDigit).Aggregate(str, (current, c) => current + c);
                tmp = ulong.Parse(str);
            }
            catch(Exception ex)
            {
                throw new Exception("ulong.Parse", ex);
            }
            if (size.EndsWith("G"))
            {
                bytes = tmp * 1024 * 1024 * 1024;
            }
            else
            {
                if (size.EndsWith("M"))
                {
                    bytes = tmp * 1024 * 1024;
                }
                else
                {
                    if (size.EndsWith("K"))
                    {
                        bytes = tmp * 1024;
                    }
                    else
                    {
                        bytes = tmp;
                    }
                }
            }
            return bytes;
        }
    }
}
