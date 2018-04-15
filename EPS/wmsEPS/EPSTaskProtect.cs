#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="wmsEPSTaskProtect.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Olga Marysheva</Author>
/// <Date>15.10.2012 9:03:00</Date>
/// <Summary>Защита файла во время записи</Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587

namespace wmsMLC.EPS.wmsEPS
{
    /// <summary>
    /// Типы защиты файлов при записи.
    /// </summary>
    public enum EpsTaskProtect
    {
        None,
// ReSharper disable InconsistentNaming
        FOLDER, 
        EXT, 
        CRC
// ReSharper restore InconsistentNaming
    }
}
