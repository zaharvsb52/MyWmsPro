#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="wmsEPSTypeStream.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Olga Marysheva</Author>
/// <Date>26.09.2012 13:46:53</Date>
/// <Summary>Класс - имя типа и поток данных</Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587

using System.IO;

namespace wmsMLC.EPS.wmsEPS
{
    /// <summary>
    /// Класс, для передачи данных (имя типа и поток данных) для экпорт отчета FastReport
    /// </summary>
    public class EpsStreamType
    {
        public string TypeName { get; set; }

        public byte[] Bytes { get; set; }

        /// <summary>
        /// Данных для экпорт отчета FastReport
        /// </summary>
        /// <param name="nameType">название типа</param>
        /// <param name="stream">поток данных</param>
        public EpsStreamType(string nameType, byte[] bytes)
        {
            TypeName = nameType;
            Bytes = bytes;
        }
    }
}
