#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="IWmsEPSTask.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Vladimir G. Nosov</Author>
/// <Date>26.09.2012 10:20:53</Date>
/// <Summary>Интерфейс задачи сервиса экспорта и печати</Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587

using System.Collections.Generic;
using wmsMLC.General.Types;

namespace wmsMLC.EPS.wmsEPS.OutputTasks
{
    public interface IEpsOutputTask
    {
        /// <summary>
        /// Добавление параметра задаче
        /// </summary>
        /// <param name="name">имя парметра</param>
        /// <param name="value">значение параметра</param>
        /// <param name="subvalue">дополнительное значение параметра</param>
        void AddParameter(string name, object value, object subvalue);

        /// <summary>
        /// Выполнить задачу
        /// </summary>
        /// <param name="reports">объект отчета</param>
        void DoTask(EpsFastReport[] reports);

        /// <summary>
        /// Сделать архив и работать с ним
        /// </summary>
        /// <param name="zip">архивировать или нет</param>
        /// <param name="reserve">создание резервной копии</param>
        void Zip(bool zip, bool reserve);

        /// <summary>
        /// Получение списка файлов резерва
        /// </summary>
        /// <returns>список структур файлов</returns>
        List<FileStruct> GetFiles();

        void SetExportType(ExportType et);
    }
}
