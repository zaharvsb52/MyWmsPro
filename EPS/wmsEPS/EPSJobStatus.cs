#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="wmsEPSJobStatus.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Vladimir G. Nosov</Author>
/// <Date>05.10.2012 14:53:25</Date>
/// <Summary>TypeDescriptionHere</Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587

using System;

namespace wmsMLC.EPS.wmsEPS
{    
    /// <summary>
    /// Типы статусов задания сервиса экспорта и печати
    /// </summary>
    [Flags]
// ReSharper disable InconsistentNaming
    public enum EpsJobStatus { OS_COMPLETED, OS_ERROR }
// ReSharper restore InconsistentNaming
}
