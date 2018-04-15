#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="wmsEPSTaskStatus.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Vladimir G. Nosov</Author>
/// <Date>26.09.2012 10:59:06</Date>
/// <Summary>Статус задачи сервиса экспорта и печати</Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587

namespace wmsMLC.EPS.wmsEPS
{
    /// <summary>
    /// Типы статусов задач сервиса экспорта и печати
    /// </summary>
// ReSharper disable InconsistentNaming
    public enum EpsTaskStatus { OTS_COMPLETED, OTS_ERROR }
// ReSharper restore InconsistentNaming
}
