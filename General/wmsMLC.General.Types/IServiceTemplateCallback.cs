#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="IServiceTemplateCallback.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Vladimir G. Nosov</Author>
/// <Date>10.09.2012 13:46:05</Date>
/// <Summary>Интерфейс обратной связи службы</Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587

using System.ServiceModel;

namespace wmsMLC.General.Types
{
    /// <summary>
    /// Интерфейс обратной связи службы
    /// </summary>
    public interface IServiceTemplateCallback
    {
        /// <summary>
        /// какое-либо событие для передачи клиенту
        /// </summary>
        /// <param name="e">любой объект</param>
        [OperationContract(IsOneWay = true)]
        void EventCallback(object e);
    }
}
