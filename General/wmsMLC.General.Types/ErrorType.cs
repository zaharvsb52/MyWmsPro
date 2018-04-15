#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="ErrorType.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Vladimir G. Nosov</Author>
/// <Date>14.08.2012 15:18:07</Date>
/// <Summary>Перечисление типов ошибок</Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587

namespace wmsMLC.General.Types
{
    /// <summary>
    /// типы ошибок
    /// </summary>
    public enum ErrorType { INFO = 0, ERROR = 1, LocalERROR = 2 };
}